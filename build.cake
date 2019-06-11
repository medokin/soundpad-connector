#addin Cake.Git
#tool GitVersion.CommandLine

var target = Argument("target", "Default");
var config = Argument("configuration", "Release");
var nugetKey = Argument<string>("nugetKey", null) ?? EnvironmentVariable("nuget_key");
var rootDir = Directory(".");
var srcDir = rootDir + Directory("src");
var libDir = srcDir + Directory("SoundpadConnector");
var pkgDir = libDir + Directory($"bin/{config}");
GitVersion semVer = null;

Task("SemVer").Does(() => {
    semVer = GitVersion();
    Information($"{semVer.FullSemVer}");
});

Task("Clean").Does(() =>
    DotNetCoreClean(srcDir, new DotNetCoreCleanSettings {
        Configuration = config,
        Verbosity = DotNetCoreVerbosity.Minimal
    }));

Task("Build").Does(() =>
    DotNetCoreBuild(srcDir, new DotNetCoreBuildSettings {
        Configuration = config
    }));

Task("Pack")
    .IsDependentOn("SemVer")
    .Does(() => {
        Information($"Packing {semVer.NuGetVersion}");

        var msbuildSettings = new DotNetCoreMSBuildSettings();
        msbuildSettings.Properties["PackageVersion"] = new[] { semVer.NuGetVersion };

        DotNetCorePack(libDir, new DotNetCorePackSettings {
            Configuration = config,
            OutputDirectory = pkgDir,
            NoBuild = true,
            NoDependencies = false,
            MSBuildSettings = msbuildSettings
        });
    });

Task("Release")
    .IsDependentOn("Pack")
    .Does(() => {
        Information($"Releasing {semVer.NuGetVersion} to nuget.org");

        DotNetCoreNuGetPush(
            pkgDir + File($"SoundpadConnector.{semVer.NuGetVersion}.nupkg"),
            new DotNetCoreNuGetPushSettings {
                Source = "nuget.org",
                ApiKey = nugetKey
            });
    });

Task("Default")
    .IsDependentOn("SemVer")
    .IsDependentOn("Clean")
    .IsDependentOn("Build");

RunTarget(target);