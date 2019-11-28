#load build/paths.cake
#tool nuget:?package=vswhere

 // Find MSBuild for Visual Studio 2019 and newer
DirectoryPath vsLatest = VSWhereLatest();
FilePath msBuildPath = vsLatest?.CombineWithFilePath("./MSBuild/Current/Bin/MSBuild.exe");

// Find MSBuild for Visual Studio 2017
if (msBuildPath != null && !FileExists(msBuildPath))
	msBuildPath = vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/MSBuild.exe");

// Have we found MSBuild yet?
if (!FileExists(msBuildPath))
{
	throw new Exception($"Failed to find MSBuild: {msBuildPath}");
}

Information("Building using MSBuild at " + msBuildPath);

var target = Argument("Target", "Build");
var configuration = Argument("Configuration", "Debug");


Task("Restore")
.Does(() =>
{   
    var settings = new NuGetRestoreSettings { ArgumentCustomization = args => args.Append("-recursive") };
    settings.Source.Add("https://pkgs.dev.azure.com/bc-telepathy/telepathy/_packaging/telepathy-sdk-preview/nuget/v3/index.json");
    NuGetRestore(Paths.AllFiles, settings);
});


Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
    foreach (var path in Paths.AllFiles)
    {
        MSBuild(path, settings => {
            settings.SetConfiguration(configuration).WithTarget("Build").SetPlatformTarget(PlatformTarget.MSIL);
            settings.ToolPath = msBuildPath;
            });
    }    
});


Task("ReBuild")
.IsDependentOn("Restore")
.Does(() =>
{
    foreach (var path in Paths.AllFiles)
    {
        MSBuild(path, settings => {
            settings.SetConfiguration(configuration).WithTarget("ReBuild").SetPlatformTarget(PlatformTarget.MSIL);
            settings.ToolPath = msBuildPath;
            });
    }    
});


Task("UnitTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    VSTest("./src/**/bin/" + configuration + "/*UnitTest.dll");
});

RunTarget(target);
