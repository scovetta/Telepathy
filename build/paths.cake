public static class Paths
{
	public static FilePath[] SolutionFiles => new FilePath[] { 
		@"src\soa\EchoClient\EchoClient.csproj",
		@"src\soa\CcpServiceHost\CcpServiceHost.csproj",
		@"src\soa\EchoSvcLib\EchoSvcLib.csproj",
		@"src\soa\BrokerLauncher\BrokerLauncher.csproj",
		@"src\soa\HpcBrokerWorker\HpcBrokerWorker.csproj" };

	public static FilePath[] TestFiles => new FilePath[]{
		@"src\soa\BrokerLauncher.UnitTest\BrokerLauncher.UnitTest.csproj",
		@"src\soa\SessionAPI.UnitTest\SessionAPI.UnitTest.csproj"
	};

	public static IEnumerable<FilePath> AllFiles => SolutionFiles.Union(TestFiles);
}