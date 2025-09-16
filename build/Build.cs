using System;
using System.Collections.Generic;
using System.Linq;
using Candoumbe.Pipelines.Components;
using Candoumbe.Pipelines.Components.GitHub;
using Candoumbe.Pipelines.Components.NuGet;
using Candoumbe.Pipelines.Components.Workflows;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;

[GitHubActions("integration", GitHubActionsImage.Ubuntu2204,
                  AutoGenerate = true,
                  FetchDepth = 0,
                  InvokedTargets = [nameof(IUnitTest.Compile), nameof(IUnitTest.UnitTests), nameof(IPack.Pack), nameof(IPushNugetPackages.Publish)
                  ],
                  CacheKeyFiles =
                  [
                      "src/**/*.csproj",
                      "test/**/*.csproj",
                      "stryker-config.json",
                      "test/**/*/xunit.runner.json"
                  ],
                  OnPushBranchesIgnore = [IHaveMainBranch.MainBranchName],
                  EnableGitHubToken = true,
                  ImportSecrets =
                  [
                      nameof(NugetApiKey),
                      nameof(IReportCoverage.CodecovToken)
                  ],
                  PublishArtifacts = true,
                  OnPullRequestExcludePaths =
                  [
                      "docs/*",
                      "README.md",
                      "CHANGELOG.md",
                      "LICENSE"
                  ]
              )]
[GitHubActions("delivery", GitHubActionsImage.Ubuntu2204,
                  AutoGenerate = true,
                  FetchDepth = 0,
                  InvokedTargets = [nameof(IUnitTest.Compile), nameof(IPack.Pack), nameof(IPushNugetPackages.Publish)],
                  CacheKeyFiles =
                  [
                      "src/**/*.csproj",
                      "test/**/*.csproj",
                      "stryker-config.json",
                      "test/**/*/xunit.runner.json"
                  ],
                  OnPushBranches = [IHaveMainBranch.MainBranchName],
                  EnableGitHubToken = true,
                  ImportSecrets =
                  [
                      nameof(NugetApiKey),
                      nameof(IReportCoverage.CodecovToken)
                  ],
                  PublishArtifacts = true,
                  OnPullRequestExcludePaths =
                  [
                      "docs/*",
                      "README.md",
                      "CHANGELOG.md",
                      "LICENSE"
                  ]
              )]
[DotNetVerbosityMapping]
[ShutdownDotNetAfterServerBuild]
public class Build : EnhancedNukeBuild,
    IHaveSourceDirectory,
    IHaveTestDirectory,
    IClean,
    IRestore,
    IUnitTest,
    IMutationTest,
    IReportCoverage,
    IPushNugetPackages,
    ICreateGithubRelease,
    IGitFlowWithPullRequest
{
    [Required] [Solution] public Solution Solution;

    ///<inheritdoc/>
    Solution IHaveSolution.Solution => Solution;

    IEnumerable<AbsolutePath> IClean.DirectoriesToDelete => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobDirectories("**/bin", "**/obj")
        .Concat(this.Get<IHaveTestDirectory>().TestDirectory.GlobDirectories("**/bin", "**/obj"));

    /// <summary>
    /// Token used to interact with GitHub API
    /// </summary>
    [Parameter("Token used to interact with Nuget API")] [Secret]
    public readonly string NugetApiKey;


    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => ( (ICompile)x ).Compile);

    ///<inheritdoc/>
    IEnumerable<AbsolutePath> IPack.PackableProjects => this.Get<IHaveSourceDirectory>().SourceDirectory.GlobFiles("**/*.csproj");


    ///<inheritdoc/>
    IEnumerable<PushNugetPackageConfiguration> IPushNugetPackages.PublishConfigurations =>
    [
        new NugetPushConfiguration(
                                   apiKey: NugetApiKey,
                                   source: new Uri("https://api.nuget.org/v3/index.json"),
                                   canBeUsed: () => NugetApiKey is not null
                                  ),
        new GitHubPushNugetConfiguration(
                                         githubToken: this.Get<ICreateGithubRelease>()?.GitHubToken,
                                         source: new Uri($"https://nuget.pkg.github.com/{this.Get<IHaveGitHubRepository>().GitRepository.GetGitHubOwner()}/index.json"),
                                         canBeUsed: () => this is ICreateGithubRelease { GitHubToken: not null })
    ];

    ///<inheritdoc/>
    IEnumerable<Project> IUnitTest.UnitTestsProjects => this.Get<IHaveSolution>().Solution.GetAllProjects("*.UnitTests");

    ///<inheritdoc/>
    IEnumerable<MutationProjectConfiguration> IMutationTest.MutationTestsProjects
        => Projects.Select(projectName => new MutationProjectConfiguration(Solution.GetProject(projectName),
                                                                           this.Get<IUnitTest>().UnitTestsProjects.Where(csproj => csproj.Name == $"{projectName}.UnitTests")));

    ///<inheritdoc/>
    bool IReportCoverage.ReportToCodeCov => this.Get<IReportCoverage>().CodecovToken is not null;

    private string[] Projects => [ .. Solution.AllProjects
                                                  .Where(x => this.Get<IHaveSourceDirectory>().SourceDirectory.Contains(x.Path))
                                                  .Select(x => x.Name)
    ];
}