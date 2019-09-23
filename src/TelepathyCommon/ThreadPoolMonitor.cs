// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Timer = System.Timers.Timer;

    public class ThreadPoolMonitor
    {
        private static readonly Lazy<ThreadPoolMonitor> Instance = new Lazy<ThreadPoolMonitor>(() => new ThreadPoolMonitor());

        private readonly Timer timer;

        private int lastIo;

        private int lastThread;

        private int lastWorker;

        private ThreadPoolMonitor()
        {
            this.timer = new Timer();
            this.timer.Interval = 100;
            this.timer.Elapsed += (sender, args) => this.WriteThreadPoolStatus();
        }

        public static void Start()
        {
            Instance.Value.StartInternal();
        }

        [Conditional("DEBUG")]
        public static void StartOnlyInDebug()
        {
            Instance.Value.StartInternal();
        }

        public void WriteThreadPoolStatus()
        {
            int workerMin;
            int ioMin;

            int workerMax;
            int ioMax;

            int worker;
            int io;

            ThreadPool.GetMinThreads(out workerMin, out ioMin);
            ThreadPool.GetMaxThreads(out workerMax, out ioMax);
            ThreadPool.GetAvailableThreads(out worker, out io);

            var threadNumber = Process.GetCurrentProcess().Threads.Count;

            if (worker == this.lastWorker && io == this.lastIo && threadNumber == this.lastThread)
            {
                // Reduce spam
                return;
            }

            Trace.TraceInformation(
                $@"[{nameof(ThreadPoolMonitor)}] Available Worker Thread:{worker}/{workerMax}({workerMin}), Available IO Thread:{io}/{ioMax}({ioMin}), Threads in process:{threadNumber} delta {worker - this.lastWorker}/{
                        io - this.lastIo
                    }/{threadNumber - this.lastThread}");

            this.lastWorker = worker;
            this.lastIo = io;
            this.lastThread = threadNumber;
        }

        private void StartInternal()
        {
            this.timer.Enabled = true;
        }
    }
}