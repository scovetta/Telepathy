namespace AzureBatchAdminCli
{
    using System.CommandLine.Invocation;
    using System.Threading.Tasks;

    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var rootCommand = new CommandBuilder().BuildRootCommand();

            return await rootCommand.InvokeAsync(args);
        }
    }
}