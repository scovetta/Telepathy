#load build/paths.cake

var target = Argument("Target", "Build");
var configuration = Argument("Configuration", "Debug");

Task("Restore")
.Does(() =>
{
	NuGetRestore(Paths.SolutionFile);
});

Task("Build")
.IsDependentOn("Restore")
.Does(() =>
{
	DotNetBuild(Paths.SolutionFile,
	settings => settings.SetConfiguration(configuration).WithTarget("Build").WithProperty("AllowUnsafeBlocks", "true"));
});

RunTarget(target);
