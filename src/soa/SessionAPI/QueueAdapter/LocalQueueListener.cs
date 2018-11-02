namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class LocalQueueListener<T> : IQueueListener<T>
    {
        private Queue<string> queue;

        private IQueueSerializer serializer;

        public LocalQueueListener(Queue<string> queue, IQueueSerializer serializer) : this(queue, null, serializer)
        {
        }

        public LocalQueueListener(Queue<string> queue, Func<T, Task> callback, IQueueSerializer serializer)
        {
            this.queue = queue;
            this.serializer = serializer;
            this.MessageReceivedCallback = callback;
        }

        public Func<T, Task> MessageReceivedCallback { get; set; }

        public void StartListen()
        {
            throw new NotImplementedException();
        }

        public async Task StartListenAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CheckAsync()
        {
            if (this.queue.Any())
            {
                var item = this.queue.Dequeue();
                var ditem = this.serializer.Deserialize<T>(item);
                await this.MessageReceivedCallback(ditem);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void StopListen()
        {
            throw new NotImplementedException();
        }
    }
}