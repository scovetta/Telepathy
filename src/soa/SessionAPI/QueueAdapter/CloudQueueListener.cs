namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueListener<T> : IQueueListener<T>
    {
        private readonly CloudQueue queue;

        public Func<T, Task> MessageReceivedCallback { get; set; }

        private readonly Func<bool> shouldListenPredicate = () => true;

        private readonly CloudQueueSerializer serializer;

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
            var message = await this.queue.GetMessageAsync();

            if (message == null)
            {
                return false;
            }

            var dto = this.Deserialize(message.AsString);

            // Proceed message in sequence 
            await this.MessageReceivedCallback(dto);
            await this.queue.DeleteMessageAsync(message);

            return true;
        }
    }
}