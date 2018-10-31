namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueListener<T> : IQueueListener<T>
    {
        private CloudQueue queue;

        public Func<T, Task> MessageReceivedCallback { get; set; }

        private Func<bool> shouldListenPredicate = () => true;

        private CloudQueueSerializer serializer;

        private static readonly TimeSpan QueryDelay = TimeSpan.FromMilliseconds(500);

        public CloudQueueListener(string connectionString, string queueName, CloudQueueSerializer serializer, Func<T, Task> callback) : this(connectionString, queueName, serializer, callback, null)
        {
        }

        public CloudQueueListener(string connectionString, string queueName, CloudQueueSerializer serializer, Func<T, Task> callback, Func<bool> predicate)
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
            this.MessageReceivedCallback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.queue = account.CreateCloudQueueClient().GetQueueReference(queueName);
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            if (predicate != null)
            {
                this.shouldListenPredicate = predicate;
            }
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

                if (!this.shouldListenPredicate())
                {
                    await Task.Delay(QueryDelay);
                    continue;
                }

                if (!await this.CheckAsync())
                {
                    await Task.Delay(QueryDelay);
                }
            }
        }

        public async Task<bool> CheckAsync()
        {
            var messages = await this.queue.GetMessagesAsync(10);
            if (!messages.Any())
            {
                return false;
            }
            else
            {
                var dtos = messages.Select(m => this.Deserialize(m.AsString));

                // Proceed message in sequence 
                foreach (var dto in dtos)
                {
                    await this.MessageReceivedCallback(dto);
                }

                return true;
            }
        }
    }
}