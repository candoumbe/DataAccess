using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.Workflows;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

using System.Collections.Generic;
using System.Linq;

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
            nameof(IReportCoverage.CodecovToken)
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
        IHaveSolution,
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
        IReportCoverage,
        IPack,
        IPublish,
        ICreateGithubRelease,
        IGitFlowWithPullRequest
    {
        [Required]
        [Solution]
        public readonly Solution Solution;

        ///<inheritdoc/>
        Solution IHaveSolution.Solution => Solution;

        /// <summary>
        /// Token to interact with GitHub's API
        /// </summary>
        [Parameter]
        [Secret]
        public readonly string GitHubToken;

        /// <summary>
        /// Token to interact with Nuget's API
        /// </summary>
        [Parameter("Token to interact with Nuget's API")]
        [Secret]
        public readonly string NugetApiKey;

        [Parameter]
        [Secret]
        public readonly string CodecovToken;

        ///<inheritdoc/>
        string IReportCoverage.CodecovToken => CodecovToken;


        [Parameter("API Key used to submit Stryker dashboard")]
        [Secret]
        public readonly string StrykerDashboardApiKey;

        [GitVersion(NoFetch = true, Framework = "net5.0")]
        public readonly GitVersion GitVersion;

        [GitRepository]
        public readonly GitRepository GitRepository;

        ///<inheritdoc/>
        GitRepository IHaveGitRepository.GitRepository => GitRepository;

        ///<inheritdoc/>
        GitVersion IHaveGitVersion.GitVersion => GitVersion;

        [CI] public readonly GitHubActions GitHubActions;

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IClean.DirectoriesToDelete => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
                                                                .Concat(this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IClean.DirectoriesToClean => new[] { this.Get<IPack>().ArtifactsDirectory, this.Get<IReportCoverage>().CoverageReportDirectory };

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IClean.DirectoriesToEnsureExistance => new[]
        {
            this.Get<IReportCoverage>().CoverageReportHistoryDirectory
        };

        ///<inheritdoc/>
        IEnumerable<Project> IUnitTest.UnitTestsProjects => Partition.GetCurrent(this.Get<IHaveSolution>().Solution.GetProjects("*.UnitTests"));

        ///<inheritdoc/>
        IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");

        ///<inheritdoc/>
        bool IReportCoverage.ReportToCodeCov => CodecovToken is not null;

        public static int Main() => Execute<Build>(x => ((ICompile)x).Compile);
    }
}