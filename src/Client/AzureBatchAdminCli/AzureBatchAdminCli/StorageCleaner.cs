namespace AzureBatchAdminCli
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Queue;

    using Serilog;

    internal class StorageCleaner
    {
        private CloudStorageAccount account;

        private static Regex MessageBlobContainerNamePattern { get; } = new Regex(@"\bhpcsoa-\d+-\d+-.*");

        private const string MessageBlobPrefix = "hpcsoa-";

        private static Regex BrokerWorkerControllerRequestQueueNamePattern { get; } = new Regex(@"\bbrokerworkerctrlreq-.+");

        private static Regex BrokerWorkerControllerResponseQueueNamePattern { get; } = new Regex(@"\bbrokerworkerctrlres-.+");

        private static Regex MessageQueueNamePattern { get; } = new Regex(@"\bhpcsoa-\d+-\d+-.*");

        private const string BrokerWorkerControllerRequestQueuePrefix = "brokerworkerctrlreq-";

        private const string BrokerWorkerControllerResponseQueuePrefix = "brokerworkerctrlres-";

        private const string MessageQueuePrefix = "hpcsoa-";

        public StorageCleaner(string connectionstring)
        {
            this.account = CloudStorageAccount.Parse(connectionstring);
        }

        public async Task CleanBlobContainerAsync()
        {
            var blobClient = this.account.CreateCloudBlobClient();
            var candidates = blobClient.ListContainers(MessageBlobPrefix).Where(c => MessageBlobContainerNamePattern.IsMatch(c.Name)).ToArray();
            foreach (var container in candidates)
            {
                Log.Information($"About to delete container {container.Name}");
            }

            Log.Information($"Total number of deletion is {candidates.Count()}.");

            var tasks = candidates.Select(c => c.DeleteIfExistsAsync());
            await Task.WhenAll(tasks);
            Log.Information($"Cleaning done.");
        }

        public async Task CleanQueueAsync()
        {
            var queueClient = this.account.CreateCloudQueueClient();
            var candidates = queueClient.ListQueues(BrokerWorkerControllerRequestQueuePrefix)
                .Where(c => BrokerWorkerControllerRequestQueueNamePattern.IsMatch(c.Name))
                .Union(queueClient.ListQueues(BrokerWorkerControllerResponseQueuePrefix).Where(c => BrokerWorkerControllerResponseQueueNamePattern.IsMatch(c.Name)))
                .Union(queueClient.ListQueues(MessageQueuePrefix).Where(c => MessageQueueNamePattern.IsMatch(c.Name))).ToArray();

            foreach (var queue in candidates)
            {
                Log.Information($"About to delete queue {queue.Name}");
            }

            Log.Information($"Total number of deletion is {candidates.Count()}.");

            var tasks = candidates.Select(c => c.DeleteIfExistsAsync());
            await Task.WhenAll(tasks);
            Log.Information($"Cleaning done.");
        }

        public async Task CleanAsync()
        {
             await this.CleanBlobContainerAsync();
             await this.CleanQueueAsync();
        }
    }
}