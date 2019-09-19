// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface
{
    using System;
    using System.Threading.Tasks;

    public interface IQueueListener<T>
    {
        Func<T, Task> MessageReceivedCallback { get; set; }

        void StartListen();

        Task StartListenAsync();

        Task<bool> CheckAsync();

        void StopListen();
    }
}
