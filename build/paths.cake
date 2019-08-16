public static class Paths
{
	public static FilePath[] SolutionFiles => new FilePath[] { 
		@"Telepathy.sln"
	};

	public static FilePath[] TestFiles => new FilePath[]{
	};

	public static IEnumerable<FilePath> AllFiles => SolutionFiles.Union(TestFiles);
}