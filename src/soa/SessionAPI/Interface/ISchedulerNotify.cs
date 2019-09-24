// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.Data;

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
