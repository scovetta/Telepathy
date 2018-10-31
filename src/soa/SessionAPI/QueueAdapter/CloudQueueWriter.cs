namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueWriter<T> : IQueueWriter<T>
    {
        private CloudQueueSerializer serializer;

        private CloudQueue queue;

        public CloudQueueWriter(string connectionString, string queueName, CloudQueueSerializer serializer)
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