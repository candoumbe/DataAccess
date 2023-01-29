using Candoumbe.Pipelines;
using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.Workflows;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using System;
using System.Collections.Generic;
using System.Linq;

namespace ContinuousIntegration
{
    [GitHubActions("integration", GitHubActionsImage.UbuntuLatest,
        AutoGenerate = true,
        FetchDepth = 0,
        InvokedTargets = new[] { nameof(IUnitTest.Compile), nameof(IUnitTest.UnitTests), nameof(IPublish.Pack), nameof(IPublish.Publish) },
        CacheKeyFiles = new[] {
            "src/**/*.csproj",
            "test/**/*.csproj",
            "stryker-config.json",
            "test/**/*/xunit.runner.json" },
        OnPushBranchesIgnore = new[] { IHaveMainBranch.MainBranchName },
        EnableGitHubToken = true,
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken)
        },
        PublishArtifacts = true,
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
        }
    )]
    [GitHubActions("delivery", GitHubActionsImage.UbuntuLatest,
        AutoGenerate = true,
        FetchDepth = 0,
        InvokedTargets = new[] { nameof(IUnitTest.Compile), nameof(IPublish.Pack), nameof(IPublish.Publish) },
        CacheKeyFiles = new[] {
            "src/**/*.csproj",
            "test/**/*.csproj",
            "stryker-config.json",
            "test/**/*/xunit.runner.json" },
        OnPushBranches = new[] { IHaveMainBranch.MainBranchName },
        EnableGitHubToken = true,
        ImportSecrets = new[]
        {
            nameof(NugetApiKey),
            nameof(IReportCoverage.CodecovToken)
        },
        PublishArtifacts = true,
        OnPullRequestExcludePaths = new[]
        {
            "docs/*",
            "README.md",
            "CHANGELOG.md",
            "LICENSE"
        }
    )]

    [DotNetVerbosityMapping]
    [ShutdownDotNetAfterServerBuild]
    public class Build : NukeBuild,
        IHaveSolution,
        IHaveSourceDirectory,
        IHaveTestDirectory,
        IHaveConfiguration,
        IHaveGitVersion,
        IHaveGitHubRepository,
        IHaveChangeLog,
        IHaveMainBranch,
        IHaveDevelopBranch,
        IClean,
        IRestore,
        ICompile,
        IUnitTest,
        IMutationTest,
        IReportCoverage,
        IPack,
        IPublish,
        ICreateGithubRelease,
        IGitFlowWithPullRequest,
        IHaveArtifacts
    {
        [CI]
        public GitHubActions GitHubActions;

        [Required]
        [Solution]
        public Solution Solution;

        ///<inheritdoc/>
        Solution IHaveSolution.Solution => Solution;

        ///<inheritdoc/>
        AbsolutePath IHaveSourceDirectory.SourceDirectory => RootDirectory / "src";

        IEnumerable<AbsolutePath> IClean.DirectoriesToClean => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
            .Concat(this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

        /// <summary>
        /// Token used to interact with GitHub API
        /// </summary>
        [Parameter("Token used to interact with Nuget API")]
        [Secret]
        public readonly string NugetApiKey;


        /// Support plugins are available for:
        ///   - JetBrains ReSharper        https://nuke.build/resharper
        ///   - JetBrains Rider            https://nuke.build/rider
        ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
        ///   - Microsoft VSCode           https://nuke.build/vscode
        public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");


        ///<inheritdoc/>
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
        )};

        ///<inheritdoc/>
        IEnumerable<Project> IUnitTest.UnitTestsProjects => Partition.GetCurrent(this.Get<IHaveSolution>().Solution.GetProjects("*.UnitTests"));

        ///<inheritdoc/>
        IEnumerable<Project> IMutationTest.MutationTestsProjects => this.Get<IUnitTest>().UnitTestsProjects;

        ///<inheritdoc/>
        bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;

        ///<inheritdoc/>
        protected override void OnBuildCreated()
        {
            if (IsServerBuild)
            {
                Environment.SetEnvironmentVariable("DOTNET_ROLL_FORWARD", "LatestMajor");
            }
        }
    }
}