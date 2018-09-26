var target = Argument("target", "Default");
var automated = Argument("automated", "false");
var configuration = Argument("configuration", "Release");
var buildversion = Argument("buildversion", "0.9.0");

var artifactsPath = "./artifacts";
var solutionPath = "./PatchMap.sln";
var testProjectPath = "./PatchMap.Tests/PatchMap.Tests.csproj";
var nugetProjectPath = "./PatchMap/PatchMap.csproj";
Setup(ctx =>
{
   Information("Running tasks...");
});

Teardown(ctx =>
{
   Information("Finished running tasks.");
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new[] { artifactsPath });
    DotNetCoreClean(solutionPath);
});

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore(solutionPath);
});

Task("Test")
    .Does(() =>
{
     DotNetCoreTest(testProjectPath, new DotNetCoreTestSettings { Logger = (automated == "true") ? "Appveyor" : "trx", ResultsDirectory = artifactsPath });
});

Task("Package")
    .Does(() =>
{
	DotNetCorePack(nugetProjectPath, new DotNetCorePackSettings 
    { 
        OutputDirectory = artifactsPath, 
        Configuration = configuration, 
        ArgumentCustomization = args=>args.Append("/p:Version=" + buildversion)
    });
});

Task("Default")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Test")
    .IsDependentOn("Package");

RunTarget(target);