//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="DataMoveTaskQueue.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Data move task queue
// </summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using TraceHelper = DataServiceTraceHelper;

    /// <summary>
    ///  Data move task queue. Usage: 
    ///  1. Data service generates DataMoveTask and adds it to the queue
    ///     via AddDataMoveTask
    ///  2. DataMoveWorkers gets a task via GetDataMoveTask, processes the
    ///     task, and deletes it via DeleteDataMoveTask when finished
    /// </summary>
    internal class DataMoveTaskQueue : DisposableObject
    {
        /// <summary>
        /// waiting tasks queue
        /// </summary>
        private Queue<string> waitingTasks = new Queue<string>();

        /// <summary>
        /// all waiting and running tasks
        /// </summary>
        private Dictionary<string, DataMoveTask> allTasks = new Dictionary<string, DataMoveTask>();

        /// <summary>
        /// semaphore object for accessing waiting tasks
        /// </summary>
        private Semaphore semWaitingTasks = new Semaphore(0, int.MaxValue);

        /// <summary>
        /// lock object for this instance
        /// </summary>
        private object lockTasks = new object();

        /// <summary>
        /// Add a DataMoveTask to the waiting queue if the task doesn't exist yet.
        /// </summary>
        /// <param name="task">task to be added</param>
        public void AddDataMoveTask(DataMoveTask task)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataMoveTaskQueue].AddDataMoveTask: try add task, id={0}", task.Id);

            DataMoveTask existingTask = null;
            lock (this.lockTasks)
            {
                if (this.allTasks.TryGetValue(task.Id, out existingTask))
                {
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataMoveTaskQueue].AddDataMoveTask: task with the same id already exist={0}", task.Id);
                    if (string.Equals(task.Destination, existingTask.Destination, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    else
                    {
                        // remove the old task with the same id but different destination
                        this.allTasks.Remove(task.Id);
                    }
                }

                this.allTasks.Add(task.Id, task);
                this.waitingTasks.Enqueue(task.Id);
            }

            this.semWaitingTasks.Release();

            if (existingTask != null)
            {
                existingTask.Cancel(/*waitForCompletion =*/false);
            }
        }

        /// <summary>
        /// Get next DataMoveTask from the waiting queue. This is a blocking call
        /// </summary>
        /// <returns>the next DataMoveTask in the waiting queue</returns>
        public DataMoveTask GetDataMoveTask()
        {
            while (true)
            {
                this.semWaitingTasks.WaitOne();

                lock (this.lockTasks)
                {
                    if (this.waitingTasks.Count <= 0)
                    {
                        break;
                    }

                    string taskId = this.waitingTasks.Dequeue();

                    DataMoveTask task = null;
                    if (this.allTasks.TryGetValue(taskId, out task))
                    {
                        return task;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Cancel a DataMoveTask and remove it from the queue
        /// </summary>
        /// <param name="taskId">data move task id</param>
        public void CancelDataMoveTask(string taskId)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataMoveTaskQueue].CancelDataMoveTask: id={0}", taskId);

            DataMoveTask task = null;
            lock (this.lockTasks)
            {
                if (this.allTasks.TryGetValue(taskId, out task))
                {
                    this.allTasks.Remove(taskId);
                }
            }

            if (task != null)
            {
                task.Cancel(/*waitForCompletion =*/true);
                task.Close();
            }
        }

        /// <summary>
        /// Delete a DataMoveTask from the queue. 
        /// </summary>
        /// <param name="task">data move task to be deleted</param>
        public void DeleteDataMoveTask(DataMoveTask task)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[DataMoveTaskQueue].DeleteDataMoveTask: id={0}", task.Id);

            bool removeFlag = false;
            lock (this.lockTasks)
            {
                removeFlag = this.allTasks.Remove(task.Id);
            }

            if (removeFlag)
            {
                task.Close();
            }
        }

        /// <summary>
        /// Check if there is already a data move task with the specified task id
        /// </summary>
        /// <param name="taskId">data move task id</param>
        /// <param name="sourceDataPath">source data path</param>
        /// <param name="destDataPath">destination data path</param>
        /// <returns>true if a data move task with the specified source and destination already exists, false otherwise</returns>
        public bool ContainsDataMoveTask(string taskId, string sourceDataPath, string destDataPath)
        {
            DataMoveTask task;
            lock (this.lockTasks)
            {
                return this.allTasks.TryGetValue(taskId, out task) &&
                    string.Equals(task.Source, sourceDataPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(task.Destination, destDataPath, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Get a list of all DataMoveTasks currently in the queue
        /// </summary>
        /// <returns>a list of all DataMoveTasks in the queue</returns>
        public List<DataMoveTask> GetAllDataMoveTasks()
        {
            lock (this.lockTasks)
            {
                List<DataMoveTask> tasks = new List<DataMoveTask>(this.allTasks.Values);
                return tasks;
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is called directly or indirectly by user's code</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.semWaitingTasks.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}