//------------------------------------------------------------------------------
// <copyright file="ISchedulerNotifyAsync.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The async version of ISchedulerNotify interface
// </summary>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    /// <summary>
    /// The async version of ISchedulerNotify interface
    /// </summary>
    [ServiceContract(Namespace = "http://hpc.microsoft.com")]
    public interface ISchedulerNotifyAsync
    {
        /// <summary>
        /// Trigger when the job state changed.
        /// </summary>
        /// <param name="state">indicating the job state</param>
        [OperationContract]
        void JobStateChanged(JobState state);

        /// <summary>
        /// Async version of JobStateChanged
        /// </summary>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginJobStateChanged(JobState jobState, AsyncCallback callback, object state);

        /// <summary>
        /// End of async JobStateChanged
        /// </summary>
        /// <param name="ar">
        ///   <para />
        /// </param>
        void EndJobStateChanged(IAsyncResult ar);

        /// <summary>
        /// Trigger when task state changed
        /// </summary>
        /// <param name="taskInfoList">
        ///   <para />
        /// </param>
        [OperationContract]
        void TaskStateChanged(List<TaskInfo> taskInfoList);

        /// <summary>
        /// Async version of TaskStateChanged
        /// </summary>
        /// <param name="taskInfoList">
        ///   <para />
        /// </param>
        /// <param name="callback">
        ///   <para />
        /// </param>
        /// <param name="state">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginTaskStateChanged(List<TaskInfo> taskInfoList, AsyncCallback callback, object state);

        /// <summary>
        /// End of async TaskStateChanged
        /// </summary>
        /// <param name="ar">
        ///   <para />
        /// </param>
        void EndTaskStateChanged(IAsyncResult ar);
    }
}
