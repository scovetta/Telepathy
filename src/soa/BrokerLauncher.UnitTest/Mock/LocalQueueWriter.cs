namespace BrokerLauncher.UnitTest.Mock
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    internal class LocalQueueWriter<T> : IQueueWriter<T>
    {
        private Queue<string> queue;

        private IQueueSerializer serializer;

        public LocalQueueWriter(Queue<string> queue, IQueueSerializer serializer)
        {
            this.queue = queue;
            this.serializer = serializer;
        }

        public async Task WriteAsync(T item)
        {
            this.queue.Enqueue(this.serializer.Serialize(item));
        }
    }
}