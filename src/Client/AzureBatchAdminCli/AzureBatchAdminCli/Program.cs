using System;

namespace AzureBatchAdminCli
{
    using System.Threading.Tasks;

    using Serilog;

    class Program
    {
        private const string ConnectionString = "";

        static async Task Main(string[] args)
        {
            
            var logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Log.Logger = logger;

            Console.WriteLine("Hello World!");

            StorageCleaner cleaner = new StorageCleaner(ConnectionString);
            await cleaner.CleanAsync();
        }
    }
}
