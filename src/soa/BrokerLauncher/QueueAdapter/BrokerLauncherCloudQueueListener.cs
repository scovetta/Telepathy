namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;

    using Newtonsoft.Json;

    internal class BrokerLauncherCloudQueueListener<T>
        where T : class
    {
        private CloudQueue queue;

        private Func<T, Task> messageReceivedCallback;

        private BrokerLauncherCloudQueueSerializer serializer;

        private static readonly TimeSpan QueryDelay = TimeSpan.FromMilliseconds(500);

        internal BrokerLauncherCloudQueueListener(string connectionString, string queueName, BrokerLauncherCloudQueueSerializer serializer, Func<T, Task> callback)
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

        internal void StartListen() => Task.Run(this.StartListenAsync);

        internal async Task StartListenAsync()
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