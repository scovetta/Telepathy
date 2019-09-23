// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.QueueAdapter.Interface;

    public class LocalQueueWriter<T> : IQueueWriter<T>
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