//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataMoveWorker.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Data move worker
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System.Threading;

    /// <summary>
    /// Worker that execute data move tasks
    /// </summary>
    internal class DataMoveWorker
    {
        /// <summary>
        /// Data move task queue
        /// </summary>
        private DataMoveTaskQueue taskQueue;

        /// <summary>
        /// Worker thread
        /// </summary>
        private Thread workerThread;

        /// <summary>
        /// A flag indicating whether the worker is running
        /// </summary>
        private volatile bool runningFlag;

        /// <summary>
        /// Initializes a new instance of the DataMoveWorker class
        /// </summary>
        /// <param name="taskQueue">data move task queue that this worker consumes</param>
        public DataMoveWorker(DataMoveTaskQueue taskQueue)
        {
            this.taskQueue = taskQueue;
            this.workerThread = new Thread(this.WorkerThreadProc);
            this.workerThread.IsBackground = true;
            this.workerThread.Start();
        }

        /// <summary>
        /// Finalizes an instance of the DataMoveWorker class
        /// </summary>
        ~DataMoveWorker()
        {
            if (this.workerThread == null)
            {
                return;
            }

            this.runningFlag = false;
            this.workerThread.Join();
        }

        /// <summary>
        /// Worker thread that process DataMoveTask from data move task queue
        /// </summary>
        private void WorkerThreadProc()
        {
            this.runningFlag = true;
            while (this.runningFlag)
            {
                DataMoveTask task = this.taskQueue.GetDataMoveTask();
                if (task != null)
                {
                    task.Run();
                    this.taskQueue.DeleteDataMoveTask(task);
                }
            }
        }
    }
}
#endif