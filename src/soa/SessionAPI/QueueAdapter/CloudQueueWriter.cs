namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Module;
    using Microsoft.WindowsAzure.Storage.Queue;

    public class CloudQueueWriter<T> : IQueueWriter<T>
    {
        private readonly CloudQueueSerializer serializer;

        private readonly CloudQueue queue;

        public CloudQueueWriter(string connectionString, string queueName, CloudQueueSerializer serializer) : this(
            CloudQueueCreationModule.GetCloudQueueReference(connectionString, queueName),
            serializer,
            true)
        {
        }

        public CloudQueueWriter(string sasUri, CloudQueueSerializer serializer) : this(
            CloudQueueCreationModule.GetCloudQueueReference(sasUri),
            serializer,
            false)
        {
        }

        private CloudQueueWriter(CloudQueue queue, CloudQueueSerializer serializer, bool haveCreateQueuePermission)
        {
            this.queue = queue;
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            this.HaveCreateQueuePermission = haveCreateQueuePermission;
        }

        private string Serialize(T item)
        {
            return this.serializer.Serialize(item);
        }

        public bool HaveCreateQueuePermission { get; }

        public async Task WriteAsync(T item)
        {
            try
            {
                if (this.HaveCreateQueuePermission)
                {
                    await this.queue.CreateIfNotExistsAsync();
                }

                string payload = this.Serialize(item);
                CloudQueueMessage msg = new CloudQueueMessage(payload);
                await this.queue.AddMessageAsync(msg);
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Exception when Write Cloud Queue Message: {ex.ToString()}");
                throw;
            }
        }
    }
}