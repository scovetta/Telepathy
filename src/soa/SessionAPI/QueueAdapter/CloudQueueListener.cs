namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueListener<T>
    {
        private CloudQueue queue;

        private Func<T, Task> messageReceivedCallback;

        private CloudQueueSerializer serializer;

        private static readonly TimeSpan QueryDelay = TimeSpan.FromMilliseconds(500);

        public CloudQueueListener(string connectionString, string queueName, CloudQueueSerializer serializer, Func<T, Task> callback)
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
            this.messageReceivedCallback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.queue = account.CreateCloudQueueClient().GetQueueReference(queueName);
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        private T Deserialize(string json)
        {
            return this.serializer.Deserialize<T>(json);
        }

        public void StartListen() => Task.Run(this.StartListenAsync);

        public async Task StartListenAsync()
        {
            while (true)
            {
                await this.queue.CreateIfNotExistsAsync();
                var messages = await this.queue.GetMessagesAsync(10);
                if (!messages.Any())
                {
                    await Task.Delay(QueryDelay);
                }
                else
                {
                    var dtos = messages.Select(m => this.Deserialize(m.AsString));

                    // Proceed message in sequence 
                    foreach (var dto in dtos)
                    {
                        await this.messageReceivedCallback(dto);
                    }
                }
            }
        }
    }
}