namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public class ThreadPoolMonitor
    {
        private int lastWorker = 0;

        private int lastIo = 0;

        private int lastThread = 0;

        private System.Timers.Timer timer;

        private static readonly Lazy<ThreadPoolMonitor> Instance = new Lazy<ThreadPoolMonitor>(() => new ThreadPoolMonitor());

        private ThreadPoolMonitor()
        {
            this.timer = new System.Timers.Timer();
            this.timer.Interval = 100;
            this.timer.Elapsed += (sender, args) => this.WriteThreadPoolStatus();
        }

        private void StartInternal()
        {
            this.timer.Enabled = true;
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

            int threadNumber = Process.GetCurrentProcess().Threads.Count;

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
    }
}
