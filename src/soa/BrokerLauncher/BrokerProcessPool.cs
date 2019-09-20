// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.RuntimeTrace;

    /// <summary>
    /// Provide broker process pool
    /// </summary>
    internal class BrokerProcessPool : IDisposable
    {
        /// <summary>
        /// Stores the pool size
        /// </summary>
        private int poolSize = BrokerLauncherSettings.Default.BrokerPoolSize;

        /// <summary>
        /// Stores the timeout to wait for new process
        /// </summary>
        private const int WaitForNewProcessTimeout = 3000;

        /// <summary>
        /// Stores the timeout to get broker process
        /// </summary>
        private const int GetBrokerProcessTimeout = 60 * 1000;

        /// <summary>
        /// Stores the period between create new processes
        /// </summary>
        private const int CreateNewProcessPeriod = 3000;

        /// <summary>
        /// Stores the broker process pool
        /// </summary>
        private List<BrokerProcess> pool;

        /// <summary>
        /// Stores the create new broker process callback
        /// </summary>
        private WaitCallback createNewBrokerProcessCallback;

        /// <summary>
        /// Stores the new broker process ready event
        /// </summary>
        private AutoResetEvent newBrokerProcessReadyEvent;

        /// <summary>
        /// Stores the disposing flag
        /// </summary>
        private volatile bool disposed;

        /// <summary>
        /// Stores the current pool size
        /// </summary>
        private int currentPoolSize;

        /// <summary>
        /// Initializes a new instance of the BrokerProcessPool class
        /// </summary>
        /// <param name="job">indicating the job object</param>
        public BrokerProcessPool()
        {
            this.currentPoolSize = this.poolSize;
            this.pool = new List<BrokerProcess>(this.poolSize);
            this.createNewBrokerProcessCallback = new ThreadHelper<object>(new WaitCallback(this.CreateNewBrokerProcess)).CallbackRoot;
            this.newBrokerProcessReadyEvent = new AutoResetEvent(false);
            for (int i = 0; i < this.poolSize; i++)
            {
                ThreadPool.QueueUserWorkItem(this.createNewBrokerProcessCallback);
            }
        }

        /// <summary>
        /// Finalizes an instance of the BrokerProcessPool class
        /// </summary>
        ~BrokerProcessPool()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets a broker process from the pool
        /// </summary>
        /// <returns>returns a broker process</returns>
        /// <remarks>this method is thread safe</remarks>
        public BrokerProcess GetBrokerProcess()
        {
            // Create a new broker process in another thread
            ThreadPool.QueueUserWorkItem(this.createNewBrokerProcessCallback);
            DateTime st = DateTime.Now;

            while (true)
            {
                if (this.pool.Count > 0)
                {
                    lock (this.pool)
                    {
                        if (this.pool.Count > 0)
                        {
                            // Gets the last process
                            BrokerProcess process = this.pool[this.pool.Count - 1];
                            process.Exited -= this.BrokerProcess_Exited;
                            this.pool.RemoveAt(this.pool.Count - 1);
                            TraceHelper.TraceEvent(TraceEventType.Information, "[BrokerProcessPool] Fetched broker process, PID = {0}", process.Id);
                            return process;
                        }
                    }
                }

                if (!this.newBrokerProcessReadyEvent.WaitOne(WaitForNewProcessTimeout, false))
                {
                    if ((int)DateTime.Now.Subtract(st).TotalMilliseconds > GetBrokerProcessTimeout)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.TimeoutToGetBrokerWorkerProcess, Hpc.Scheduler.Session.SR.TimeoutToGetBrokerWorkerProcess);
                    }
                }
            }
        }

        /// <summary>
        /// Close the broker process pool
        /// </summary>
        public void Close()
        {
            ((IDisposable)this).Dispose();
        }

        /// <summary>
        /// Dispose the broker process pool
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the broker process pool
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        private void Dispose(bool disposing)
        {
            // Kill all broker processes in pool
            lock (this.pool)
            {
                foreach (BrokerProcess process in this.pool)
                {
                    process.Close();
                }

                if (this.newBrokerProcessReadyEvent != null)
                {
                    this.newBrokerProcessReadyEvent.Dispose();
                    this.newBrokerProcessReadyEvent = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Create new broker process
        /// </summary>
        private void CreateNewBrokerProcess(object state)
        {
            BrokerProcess process = new BrokerProcess();
            process.Ready += new EventHandler<BrokerProcessReadyEventArgs>(this.BrokerProcess_Ready);
            process.Exited += new EventHandler(this.BrokerProcess_Exited);
            process.Start();
        }

        /// <summary>
        /// Event triggered when broker process is exited
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void BrokerProcess_Exited(object sender, EventArgs e)
        {
            BrokerProcess process = (BrokerProcess)sender;
            Debug.Assert(process != null, "[BrokerProcessPool] Sender should be an instance of BrokerProcess class.");
            TraceHelper.RuntimeTrace.LogBrokerWorkerProcessFailedToInitialize(process.Id);
            lock (this.pool)
            {
                if (this.disposed)
                {
                    return;
                }

                this.pool.Remove(process);
            }

            int count = Interlocked.Decrement(ref this.currentPoolSize);
            Debug.Assert(count >= 0, "[BrokerProcessPool] Current pool size should always be non-negative.");
        }

        /// <summary>
        /// Event triggered when broker process is ready
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event args</param>
        private void BrokerProcess_Ready(object sender, BrokerProcessReadyEventArgs e)
        {
            BrokerProcess process = (BrokerProcess)sender;
            Debug.Assert(process != null, "[BrokerProcessPool] Sender should be an instance of BrokerProcess class.");
            if (e.TimedOut)
            {
                TraceHelper.RuntimeTrace.LogBrokerWorkerProcessFailedToInitialize(process.Id);
                int count = Interlocked.Decrement(ref this.currentPoolSize);
                Debug.Assert(count >= 0, "[BrokerProcessPool] Current pool size should always be non-negative.");
                process.Close();
            }
            else
            {
                lock (this.pool)
                {
                    if (this.disposed)
                    {
                        return;
                    }

                    //
                    // Insert into the list in descending order
                    // This way we can always tell which broker process will be used - it will be always the one with lowest pid
                    // I didn't use SortedList because SortedList needs to be unique in key and there might be a chance where old 
                    // process died but is not yet removed from the list while a new process with the exact same PID goes into the 
                    // list and lead to exception.
                    //
                    int indexToInsert = 0;
                    int id = process.Id;
                    while (indexToInsert < this.pool.Count)
                    {
                        if (id >= this.pool[indexToInsert].Id)
                            break;

                        indexToInsert++;
                    }

                    this.pool.Insert(indexToInsert, process);
                }

                this.newBrokerProcessReadyEvent.Set();
                TraceHelper.RuntimeTrace.LogBrokerWorkerProcessReady(process.Id);

                int count = Interlocked.Increment(ref this.currentPoolSize);
                if (count <= this.poolSize)
                {
                    ThreadPool.QueueUserWorkItem(this.createNewBrokerProcessCallback);
                }
                else
                {
                    Interlocked.Decrement(ref this.currentPoolSize);
                }
            }
        }
    }
}
