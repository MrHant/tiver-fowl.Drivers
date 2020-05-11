const string project = "Tiver.Fowl.Drivers";
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var projects = Argument("projects", project + ";" + project + ".Tests;" + project + ".TestsWithoutConfigFile");

var projectDirectories = projects.Split(';');

GitVersion versionInfo;
string version;

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
    Information("Restoring nuget packages");
    DotNetCoreRestore();
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
    Information("Building with configuration {0}", configuration);
    var settings = new DotNetCoreBuildSettings
     {
         Framework = "netcoreapp3.1",
         Configuration = configuration,
     };

     DotNetCoreBuild("./"+project+"/"+project+".csproj", settings);
});

Task("RunUnitTests")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest();
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
    version = versionInfo.LegacySemVerPadded;
});
   
Task("CreateNuGetPackage")
    .IsDependentOn("RunUnitTests")
    .Does(() =>
{
    Information("Packing version {0}", version);
    var nuGetPackSettings = new NuGetPackSettings {
        Version = version,
        OutputDirectory = "./package"
    };

    NuGetPack("./package/Package.nuspec", nuGetPackSettings);
});

Task("PushNuGetPackage")
    .IsDependentOn("CreateNuGetPackage")
    .Does(() =>
{
    var package = "./package/" + project + "."  + version +".nupkg";

    NuGetPush(package, new NuGetPushSettings {
        Source = "https://nuget.org/",
        ApiKey = Environment.GetEnvironmentVariable("NuGet_API_KEY")
    });
});

Task("Default")
    .IsDependentOn("PushNuGetPackage");

RunTarget(target);
