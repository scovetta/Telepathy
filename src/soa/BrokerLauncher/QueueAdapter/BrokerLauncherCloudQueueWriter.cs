namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    using Newtonsoft.Json;

    public class BrokerLauncherCloudQueueWriter<T>
    {
        private BrokerLauncherCloudQueueSerializer serializer;

        private CloudQueue queue;

        public BrokerLauncherCloudQueueWriter(string connectionString, string queueName, BrokerLauncherCloudQueueSerializer serializer)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            var account = CloudStorageAccount.Parse(connectionString);

            this.queue = account.CreateCloudQueueClient().GetQueueReference(queueName);
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        private string Serialize(T item)
        {
            return this.serializer.Serialize(item);
        }

        public async Task WriteAsync(T item)
        {
            await this.queue.CreateIfNotExistsAsync();
            string payload = this.Serialize(item);
            CloudQueueMessage msg = new CloudQueueMessage(payload);
            await this.queue.AddMessageAsync(msg);
        }
    }
}