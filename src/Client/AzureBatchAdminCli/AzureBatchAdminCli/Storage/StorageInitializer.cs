namespace AzureBatchAdminCli
{
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    using Serilog;

    internal class StorageInitializer
    {
        private CloudStorageAccount account;

        private static string[] RequiredBlobContainerNames { get; } = new[] { "runtime", "service-assembly", "service-registration" };

        public StorageInitializer(string connectionString)
        {
            this.account = CloudStorageAccount.Parse(connectionString);
        }

        public async Task CreateContainersAsync()
        {
            var containerClient = account.CreateCloudBlobClient();
            var tasks = RequiredBlobContainerNames.Select(containerClient.GetContainerReference).Select(c => c.CreateIfNotExistsAsync());
            await Task.WhenAll(tasks);
            Log.Information($"Done creating containers.");
        }

        public static Task CreateContainersAsync(string connectionString)
        {
            return new StorageInitializer(connectionString).CreateContainersAsync();
        }
    }
}
