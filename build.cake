#load build/paths.cake

var target = Argument("Target", "Build");
var configuration = Argument("Configuration", "Debug");

Task("Restore")
.Does(() =>
{
	NuGetRestore(Paths.SolutionFiles);
});

Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
	foreach (var path in Paths.SolutionFiles)
	{
		DotNetBuild(path, settings => settings.SetConfiguration(configuration).WithTarget("Build").WithProperty("AllowUnsafeBlocks", "true"));
	}	
});

Task("ReBuild")
.IsDependentOn("Restore")
.Does(() =>
{
	foreach (var path in Paths.SolutionFiles)
	{
		DotNetBuild(path, settings => settings.SetConfiguration(configuration).WithTarget("ReBuild").WithProperty("AllowUnsafeBlocks", "true"));
	}	
});

RunTarget(target);
