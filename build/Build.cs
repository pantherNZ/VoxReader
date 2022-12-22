using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.StaticFiles;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.ChangeLog;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;

[GitHubActions(
    "test",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushBranches = new[] { "**" },
    InvokedTargets = new[] { nameof(Test) },
    EnableGitHubToken = true,
    ImportSecrets = new[] { nameof(NuGetApiKey) })]
[GitHubActions(
    "release",
    GitHubActionsImage.UbuntuLatest,
    AutoGenerate = false,
    FetchDepth = 0,
    OnPushTags = new[] { "v[0-9]+.[0-9]+.[0-9]+" },
    InvokedTargets = new[] { nameof(Pack) },
    EnableGitHubToken = true,
    ImportSecrets = new[] { nameof(NuGetApiKey) })]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.Pack);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    [Parameter("NuGet API Key"), Secret] readonly string NuGetApiKey;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    readonly AbsolutePath PublishDirectory = (AbsolutePath)Path.Combine(RootDirectory, "publish");
    
    [GitRepository] readonly GitRepository GitRepository;

    Target Clean => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(PublishDirectory);
            DotNetTasks.DotNetClean(s => s
                .SetProject(Solution));
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            SemanticVersion version = GitRepository.GetLatestVersionTag();

            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetVersion(version.ToString())
                .SetAssemblyVersion($"{version.Major}.0.0.0") // See https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning
                .SetInformationalVersion(version.ToString())
                .SetFileVersion(version.ToString())
                .SetCopyright($"Copyright {DateTime.UtcNow.Year} (c) Sandro Figo")
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetLoggers("trx;logfilename=test-results.trx")
                .EnableNoRestore()
                .EnableNoBuild());
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .OnlyWhenStatic(() => GitRepository.CurrentCommitHasVersionTag())
        .Produces(PublishDirectory / "*.nupkg")
        .Executes(() =>
        {
            SemanticVersion version = GitRepository.GetLatestVersionTag();

            Log.Information("Version: {Version}", version);
            
            DotNetTasks.DotNetPack(s => s
                .SetProject(Solution.VoxReader)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PublishDirectory)
                .SetVersion(version.ToString())
                .EnableNoRestore()
                .EnableNoBuild());
        });

    // Target GitHubRelease => _ => _
    //     .Requires(() => GitHubAccessToken)
    //     .DependsOn(Pack)
    //     .OnlyWhenDynamic(() => IsOnVersionTag)
    //     .Executes(() =>
    //     {
    //         GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue("VoxReader"))
    //         {
    //             Credentials = new Credentials(GitHubAccessToken)
    //         };
    //
    //         var release = new NewRelease($"v{PackageVersion}")
    //         {
    //             Body = "Changes:\n - TODO",
    //             Draft = true,
    //             Name = PackageVersion.ToString(),
    //             TargetCommitish = "master"
    //         };
    //
    //         Release createdRelease = GitHubTasks.GitHubClient.Repository.Release.Create("sandrofigo", "VoxReader", release).Result;
    //
    //         // Add artifacts to release
    //         foreach (AbsolutePath artifact in PackOutputPath.GlobFiles("*"))
    //         {
    //             if (!FileSystemTasks.FileExists(artifact))
    //                 continue;
    //
    //             if (!new FileExtensionContentTypeProvider().TryGetContentType(artifact, out string assetContentType))
    //             {
    //                 assetContentType = "application/x-binary";
    //             }
    //
    //             var releaseAssetUpload = new ReleaseAssetUpload
    //             {
    //                 ContentType = assetContentType,
    //                 FileName = Path.GetFileName(artifact),
    //                 RawData = File.OpenRead(artifact)
    //             };
    //
    //             ReleaseAsset createdReleaseAsset = GitHubTasks.GitHubClient.Repository.Release.UploadAsset(createdRelease, releaseAssetUpload).Result;
    //
    //             Logger.Info($"Added '{releaseAssetUpload.FileName}' to '{release.Name}'.");
    //         }
    //     });

    // Target PublishNuGetPackage => _ => _
    //     .DependsOn(GitHubRelease)
    //     .Executes(() =>
    //     {
    //     });
}