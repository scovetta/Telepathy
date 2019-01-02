//------------------------------------------------------------------------------
// <copyright file="IHpcSchedulerAdapterInternal.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service contract of scheduler adapter for broker launcher
//      and the soa diag service.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading.Tasks;
    /// <summary>
    /// Service contract of scheduler adapter for broker launcher
    /// </summary>
    [ServiceContract(Namespace = "http://hpc.microsoft.com")]
    public interface IHpcSchedulerAdapterInternal
    {
        /// <summary>
        /// Get ACL string from a job template
        /// </summary>
        /// <param name="jobTemplate">the job template name</param>
        /// <returns>ACL string</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<string> GetJobTemlpateACL(string jobTemplate);

        /// <summary>
        /// Update the job's properties
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="properties">the properties table</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> UpdateBrokerInfo(int jobid, Dictionary<string, object> properties);

        /// <summary>
        /// Get all the running and queued service jobs whose broker node is machinename.
        /// </summary>
        /// <param name="machineName">Node Name</param>
        /// <returns>sessionid, sessionstart info table</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<BrokerRecoverInfo[]> GetRecoverInfoFromJobs(string machineName);

        /// <summary>
        /// Create the sessionstartinfo from job properties.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>the sessionstart info</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<BrokerRecoverInfo> GetRecoverInfoFromJob(int jobid);

        /// <summary>
        /// Check if the job id is valid
        /// </summary>
        /// <param name="jobid">jobid</param>
        /// <returns>true if the job id is valid</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<bool> IsValidJob(int jobid);

        /// <summary>
        /// Get the job's owner SID
        /// </summary>
        /// <param name="jobid"></param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<string> GetJobOwnerSID(int jobid);

        /// <summary>
        /// Fail a service job with the reason
        /// </summary>
        /// <param name="jobid">the job id</param>
        /// <param name="reason">the reason string</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task FailJob(int jobid, string reason);

        /// <summary>
        /// Get the job's allocated node name.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<List<Tuple<string, bool>>> GetJobAllocatedNodeName(int jobid);

        /// <summary>
        /// Get the task's allocated node name.
        /// </summary>
        /// <param name="jobId">job id</param>
        /// <param name="taskId">task id</param>
        /// <returns>list of the node name and location flag (on premise or not)</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<List<Tuple<string, bool>>> GetTaskAllocatedNodeName(int jobid, int taskid);

        /// <summary>
        /// Get all the exist session id list
        /// </summary>
        /// <returns>session id list</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<List<int>> GetAllSessionId();

        /// <summary>
        /// Get all the non terminated session id and requeue count
        /// </summary>
        /// <returns>
        /// dictionary
        /// key: session Id
        /// value: requeue count
        /// </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Dictionary<int, int>> GetNonTerminatedSession();

        /// <summary>
        /// Get specified job requeue count
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <returns>requeue count</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<int> GetJobRequeueCount(int jobid);

        /// <summary>
        /// Get the soa session's broker node name.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>broker node name</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<string> GetSessionBrokerNodeName(int jobid);

        /// <summary>
        /// Get the broker node name list.
        /// </summary>
        /// <returns>broker node name list</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<List<string>> GetBrokerNodeName();

        /// <summary>
        /// Check if the soa diag trace enabled for the specified session.
        /// </summary>
        /// <param name="jobid">job id of the session</param>
        /// <returns>soa diag trace is enabled or disabled </returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        bool IsDiagTraceEnabled(int jobid);

        /// <summary>
        /// Dump the event log onto a target file
        /// </summary>
        /// <param name="targetFolder">indicating the target folder to put the dumped file</param>
        /// <param name="logName">indicating the log name</param>
        /// <returns>returns the dumped file name</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<string> DumpEventLog(string targetFolder, string logName);

        /// <summary>
        /// Get specified job's customized properties.
        /// </summary>
        /// <param name="propNames">customized property names</param>
        /// <returns>customized properties</returns>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task<Dictionary<string, string>> GetJobCustomizedProperties(int jobid, string[] propNames);

        /// <summary>
        /// Set job's progress message.
        /// </summary>
        /// <param name="jobid">job id</param>
        /// <param name="message">progress message</param>
        [OperationContract]
        [FaultContract(typeof(SessionFault), Action = SessionFault.Action)]
        Task SetJobProgressMessage(int jobid, string message);
    }
}
