// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.QueueAdapter.Interface;
    using Microsoft.Telepathy.Session.QueueAdapter.Module;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueListener<T> : IQueueListener<T>
    {
        private volatile bool enabled;

        private readonly CloudQueue queue;

        public Func<T, Task> MessageReceivedCallback { get; set; }

        private readonly Func<bool> shouldListenPredicate = () => true;

        private readonly CloudQueueSerializer serializer;

        private readonly bool haveCreateQueuePermission;

        private TimeSpan QueryDelay => TimeSpan.FromMilliseconds(500);

        public CloudQueueListener(string connectionString, string queueName, CloudQueueSerializer serializer, Func<T, Task> callback) : this(connectionString, queueName, serializer, callback, null)
        {
        }

        public CloudQueueListener(string connectionString, string queueName, CloudQueueSerializer serializer, Func<T, Task> callback, Func<bool> predicate) : this(
            CloudQueueCreationModule.GetCloudQueueReference(connectionString, queueName),
            serializer,
            callback,
            predicate,
            true)
        {
        }

        public CloudQueueListener(string sasUri, CloudQueueSerializer serializer, Func<T, Task> callback) : this(sasUri, serializer, callback, null)
        {
        }

        public CloudQueueListener(string sasUri, CloudQueueSerializer serializer, Func<T, Task> callback, Func<bool> predicate) : this(
            CloudQueueCreationModule.GetCloudQueueReference(sasUri),
            serializer,
            callback,
            predicate,
            false)
        {
        }

        private CloudQueueListener(CloudQueue queue, CloudQueueSerializer serializer, Func<T, Task> callback, Func<bool> predicate, bool haveCreateQueuePermission)
        {
            this.queue = queue;
            this.MessageReceivedCallback = callback ?? throw new ArgumentNullException(nameof(callback));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            if (predicate != null)
            {
                this.shouldListenPredicate = predicate;
            }

            this.haveCreateQueuePermission = haveCreateQueuePermission;
        }

        private T Deserialize(string json)
        {
            return this.serializer.Deserialize<T>(json);
        }

        public void StartListen() => Task.Run(this.StartListenAsync);

        public async Task StartListenAsync()
        {
            this.enabled = true;

            while (this.enabled)
            {
                if (this.haveCreateQueuePermission)
                {
                    await this.queue.CreateIfNotExistsAsync();
                }

                if (!this.shouldListenPredicate())
                {
                    await Task.Delay(this.QueryDelay);
                    continue;
                }

                if (!await this.CheckAsync())
                {
                    await Task.Delay(this.QueryDelay);
                }
            }
        }

        public async Task<bool> CheckAsync()
        {
            try
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
            catch (Exception ex)
            {
                Trace.TraceError($"Exception when Checking Cloud Queue Message: {ex.ToString()}");
                throw;
            }
        }

        public void StopListen()
        {
            this.enabled = false;
        }
    }
}