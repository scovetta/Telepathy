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
        private JsonSerializerSettings serializerSettings;

        private CloudQueue queue;

        public BrokerLauncherCloudQueueWriter(string connectionString, string queueName, SerializationBinder serializationBinder)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName));
            }

            if (serializationBinder == null)
            {
                throw new ArgumentNullException(nameof(serializationBinder));
            }

            var account = CloudStorageAccount.Parse(connectionString);

            this.queue = account.CreateCloudQueueClient().GetQueueReference(queueName);
            this.serializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Binder = serializationBinder };
        }

        public async Task WriteAsync(T item)
        {
            await this.queue.CreateIfNotExistsAsync();
            string payload = JsonConvert.SerializeObject(item, this.serializerSettings);
            CloudQueueMessage msg = new CloudQueueMessage(payload);
            await this.queue.AddMessageAsync(msg);
        }
    }
}