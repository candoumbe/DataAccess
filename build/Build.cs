using Candoumbe.Pipelines;
using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.Workflows;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

using System;
using System.Collections.Generic;

namespace ContinuousIntegration
{
    [GitHubActions("integration", GitHubActionsImage.UbuntuLatest,
        OnPushBranchesIgnore = new[] { nameof(IHaveMainBranch.MainBranchName) },
        EnableGitHubToken = true,
        FetchDepth = 0,
        InvokedTargets = new[] { nameof(IUnitTest.UnitTests), nameof(IPublish.Publish), nameof(ICreateGithubRelease.AddGithubRelease) },
        CacheKeyFiles = new[] { "global.json", "src/**/*.csproj", "test/**/*.csproj" },
        PublishArtifacts = true,
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken),
            nameof(IMutationTest.StrykerDashboardApiKey)
        },
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md"
        })]
    [DotNetVerbosityMapping]
    [HandleVisualStudioDebugging]
    [ShutdownDotNetAfterServerBuild]
    public class Build : NukeBuild,
        IHaveSourceDirectory,
        IHaveTestDirectory,
        IHaveConfiguration,
        IHaveGitVersion,
        IHaveChangeLog,
        IHaveMainBranch,
        IHaveDevelopBranch,
        IClean,
        IRestore,
        ICompile,
        IUnitTest,
        IPublish,
        IReportCoverage,
        ICreateGithubRelease,
        IGitFlowWithPullRequest,
        IHaveArtifacts
    {
        [Solution]
        [Required]
        public readonly Solution Solution;

        [CI]
        public readonly GitHubActions GitHubActions;

        ///<inheritdoc/>
        Solution IHaveSolution.Solution => Solution;

        [Parameter]
        [Secret]
        public readonly string NugetApiKey;

        ///<inheritdoc/>
        AbsolutePath IHaveTestDirectory.TestDirectory => RootDirectory / "tests";

        ///<inheritdoc/>
        IEnumerable<Project> IUnitTest.UnitTestsProjects => this.Get<IHaveSolution>().Solution.GetProjects("*.Tests");

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

        /// <inheritdoc/>
        IEnumerable<PublishConfiguration> IPublish.PublishConfigurations => new PublishConfiguration[]
        {
            new NugetPublishConfiguration(
                apiKey: NugetApiKey,
                source: new Uri("https://api.nuget.org/v3/index.json"),
                canBeUsed: () => NugetApiKey is not null
            ),
            new GitHubPublishConfiguration(
                githubToken: this.Get<ICreateGithubRelease>()?.GitHubToken,
                source: new Uri($"https://nuget.pkg.github.com/{GitHubActions?.RepositoryOwner}/index.json"),
                canBeUsed: () => this is ICreateGithubRelease createRelease && createRelease.GitHubToken is not null
            ),
        };

        /// <inheritdoc/>
        bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>()?.CodecovToken is not null;

        /// <summary>
        /// Defines the default target called when running the pipeline with no args
        /// </summary>
        public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);
    }
}