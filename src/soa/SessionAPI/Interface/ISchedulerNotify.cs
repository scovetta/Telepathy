//------------------------------------------------------------------------------
// <copyright file="ISchedulerNotify.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The event handler for the job and task events
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;
    using System.Threading.Tasks;

    /// <summary>
    /// The event handler for the job and task events
    /// </summary>
    [ServiceContract(Namespace = "http://hpc.microsoft.com")]
    public interface ISchedulerNotify
    {
        /// <summary>
        /// Tigger when the job state changed.
        /// </summary>
        /// <param name="state">indicating the job state</param>
        [OperationContract]
        Task JobStateChanged(JobState state);

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="taskInfoList">
        ///   <para />
        /// </param>
        [OperationContract]
        Task TaskStateChanged(List<TaskInfo> taskInfoList);
    }
}
