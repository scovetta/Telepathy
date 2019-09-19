// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK
namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using System;

    /// <summary>
    /// Stores job informations
    /// </summary>
    internal struct JobInfo
    {
        /// <summary>
        /// Stores job id
        /// </summary>
        private int id;

        /// <summary>
        /// Stores number of running tasks
        /// </summary>
        private int runningTasks;

        /// <summary>
        /// Stores number of failed tasks
        /// </summary>
        private int failedTasks;

        /// <summary>
        /// Stores number of canceled tasks
        /// </summary>
        private int canceledTasks;

        /// <summary>
        /// Stores the number of total tasks
        /// </summary>
        private int totalTasks;

        /// <summary>
        /// Stores the number of finished tasks
        /// </summary>
        private int finishedTasks;

        /// <summary>
        /// Initializes a new instance of the JobInfo struct
        /// </summary>
        /// <param name="id">indicating job id</param>
        /// <param name="runningTasks">indicating number of running tasks</param>
        /// <param name="failedTasks">indicating number of failed tasks</param>
        /// <param name="canceledTasks">indicating number of canceled tasks</param>
        /// <param name="finishedTasks">indicating the number of finished tasks</param>
        /// <param name="totalTasks">indicating the number of total tasks</param>
        public JobInfo(int id, int runningTasks, int failedTasks, int canceledTasks, int finishedTasks, int totalTasks)
        {
            this.id = id;
            this.runningTasks = runningTasks;
            this.failedTasks = failedTasks;
            this.canceledTasks = canceledTasks;
            this.totalTasks = totalTasks;
            this.finishedTasks = finishedTasks;
        }

        /// <summary>
        /// Initializes a new instance of the JobInfo struct by job counters
        /// </summary>
        /// <param name="id">indicating the job id</param>
        /// <param name="counters">indicating the job counters</param>
        public JobInfo(int id, ISchedulerJobCounters counters)
        {
            this.id = id;
            this.runningTasks = counters.RunningTaskCount;
            this.failedTasks = counters.FailedTaskCount;
            this.canceledTasks = counters.CanceledTaskCount;
            this.totalTasks = counters.TaskCount;
            this.finishedTasks = counters.FinishedTaskCount;
        }

        /// <summary>
        /// Initializes a new instance of the JobInfo struct by default values
        /// </summary>
        /// <param name="id"></param>
        public JobInfo(int id)
        {
            this.id = id;
            this.runningTasks = -1;
            this.failedTasks = -1;
            this.canceledTasks = -1;
            this.totalTasks = -1;
            this.finishedTasks = -1;
        }

        /// <summary>
        /// Gets the job id
        /// </summary>
        public int Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Override Equals function
        /// </summary>
        /// <param name="obj">indicating the object to be compared</param>
        /// <returns>returns a value indicating whether they are equal</returns>
        public bool Equals(JobInfo obj)
        {
            return
                obj.id == this.id &&
                obj.canceledTasks == this.canceledTasks &&
                obj.failedTasks == this.failedTasks &&
                obj.finishedTasks == this.finishedTasks &&
                obj.runningTasks == this.runningTasks &&
                obj.totalTasks == this.totalTasks;
        }

        /// <summary>
        /// Override ToString function for writing trace
        /// </summary>
        /// <returns>returns a string representing all counters</returns>
        public override string ToString()
        {
            return String.Format("Id = {0}, Running = {1}, Failed = {2}, Canceled = {3}, Finished = {4}, Total = {5}",
                this.id, this.runningTasks, this.failedTasks, this.canceledTasks, this.finishedTasks, this.totalTasks);
        }
    }
}
#endif