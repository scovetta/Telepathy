#load build/paths.cake

var target = Argument("Target", "Build");
var configuration = Argument("Configuration", "Debug");

Task("Restore")
.Does(() =>
{
	NuGetRestore(Paths.AllFiles, new NuGetRestoreSettings { ArgumentCustomization = args => args.Append("-recursive") });
});

Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
	foreach (var path in Paths.AllFiles)
	{
		MSBuild(path, settings => settings.SetConfiguration(configuration).WithTarget("Build").SetPlatformTarget(PlatformTarget.MSIL));
	}	
});

Task("ReBuild")
.IsDependentOn("Restore")
.Does(() =>
{
	foreach (var path in Paths.AllFiles)
	{
		MSBuild(path, settings => settings.SetConfiguration(configuration).WithTarget("ReBuild").SetPlatformTarget(PlatformTarget.MSIL));
	}	
});

Task("UnitTest")
    .IsDependentOn("Build")
    .Does(() =>
{
    VSTest("./src/**/bin/" + configuration + "/*UnitTest.dll");
});

RunTarget(target);
