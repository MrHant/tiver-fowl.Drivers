var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var solutionFilename = Argument("solutionFilename", "Tiver.Fowl.Drivers.sln");
var testsProject = Argument("testsProject", "Tiver.Fowl.Drivers.Tests");
var testsFile = Argument("testsFile", "Tiver.Fowl.Drivers.Tests.dll");
var projects = Argument("projects", "Tiver.Fowl.Drivers;Tiver.Fowl.Drivers.Tests");

var projectDirectories = projects.Split(';');

DirectoryPath vsLatest  = VSWhereLatest();
var msBuildPath = (vsLatest==null)
                            ? null
                            : vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

GitVersion versionInfo;

Setup(_ =>
{
    Information("");
    Information(@"   _______ _                      ______            _       _____       _                      ");
    Information(@"  |__   __(_)                    |  ____|          | |     |  __ \     (_)                     ");
    Information(@"     | |   ___   _____ _ __      | |__ _____      _| |     | |  | |_ __ ___   _____ _ __ ___   ");
    Information(@"     | |  | \ \ / / _ \ '__|     |  __/ _ \ \ /\ / / |     | |  | | '__| \ \ / / _ \ '__/ __|  ");
    Information(@"     | |  | |\ V /  __/ |     _  | | | (_) \ V  V /| |  _  | |__| | |  | |\ V /  __/ |  \__ \  ");
    Information(@"     |_|  |_| \_/ \___|_|    (_) |_|  \___/ \_/\_/ |_| (_) |_____/|_|  |_| \_/ \___|_|  |___/  ");
    Information(@"                                                                                               ");
    Information("");
});

Teardown(_ =>
{
    Information("Finished running tasks.");
});

Task("RestoreNuGetPackages")
    .Does(() =>
{
    Information("Restoring nuget packages for {0}", solutionFilename);
    NuGetRestore("./" + solutionFilename);
});

Task("Clean")
    .IsDependentOn("RestoreNuGetPackages")
    .Does(() =>
{
    Information("Cleaning project directories");
    foreach (var dir in projectDirectories) {
        CleanDirectories("./" + dir + "/bin");
        CleanDirectories("./" + dir + "/obj");
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Version")
    .Does(() =>
{
    Information("Building {0} with configuration {1}", solutionFilename, configuration);
    MSBuild("./" + solutionFilename, new MSBuildSettings {
        ToolVersion = MSBuildToolVersion.VS2017,
        Configuration = configuration,
        ToolPath = msBuildPath
    });
});

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit("./Tiver.Fowl.Drivers.Tests/bin/" + configuration + "/" + testsFile, new NUnitSettings {
        ToolPath = "./tools/NUnit.ConsoleRunner/tools/nunit3-console.exe"
    });
});

Task("Version")
    .Does(() => 
{
    GitVersion(new GitVersionSettings{
        UpdateAssemblyInfo = true,
        OutputType = GitVersionOutput.BuildServer,
    });

    versionInfo = GitVersion(new GitVersionSettings{ 
        OutputType = GitVersionOutput.Json,
    });
    Information("GitVersion_NuGetVersion - " + versionInfo.NuGetVersion + " - " + EnvironmentVariable("GitVersion_NuGetVersion"));
    Information("GitVersion_LegacySemVerPadded - " + versionInfo.LegacySemVerPadded + " - " + EnvironmentVariable("GitVersion_LegacySemVerPadded"));
});
   
Task("CreateNuGetPackage")
    .IsDependentOn("RunUnitTests")
    .Does(() =>
{
    string version;
    if (AppVeyor.IsRunningOnAppVeyor) {
        version = EnvironmentVariable("GitVersion_LegacySemVerPadded");
    } else {
        version = versionInfo.LegacySemVerPadded;
    }

    Information("Packing version {0}", version);
    var nuGetPackSettings = new NuGetPackSettings {
        Version = version,
        OutputDirectory = "./package"
    };

    NuGetPack("./package/Package.nuspec", nuGetPackSettings);
});

Task("Default")
    .IsDependentOn("CreateNuGetPackage");

RunTarget(target);
