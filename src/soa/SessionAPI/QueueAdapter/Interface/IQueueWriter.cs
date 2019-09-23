// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.Interface
{
    using System.Threading.Tasks;

    public interface IQueueWriter<in T>
    {
        Task WriteAsync(T item);
    }
}
