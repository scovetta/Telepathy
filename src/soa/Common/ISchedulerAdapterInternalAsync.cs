//------------------------------------------------------------------------------
// <copyright file="SchedulerAdapterInternalAsync.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The async version of ISchedulerAdapterInternal
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// AsyncVersion of ISchedulerAdapterInternal
    /// </summary>
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://hpc.microsoft.com", ConfigurationName = "ISchedulerAdapterInternal")]
    internal interface ISchedulerAdapterInternalAsync
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobTemlpateACL", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobTemlpateACLResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        string GetJobTemlpateACL(string jobTemplate);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobTemlpateACL", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobTemlpateACLResponse")]
        System.IAsyncResult BeginGetJobTemlpateACL(string jobTemplate, System.AsyncCallback callback, object asyncState);

        string EndGetJobTemlpateACL(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/UpdateBrokerInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/UpdateBrokerInfoRespon" +
            "se")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool UpdateBrokerInfo(int jobid, System.Collections.Generic.Dictionary<string, object> properties);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/UpdateBrokerInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/UpdateBrokerInfoRespon" +
            "se")]
        System.IAsyncResult BeginUpdateBrokerInfo(int jobid, System.Collections.Generic.Dictionary<string, object> properties, System.AsyncCallback callback, object asyncState);

        bool EndUpdateBrokerInfo(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobs", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobsResponse" +
            "")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        BrokerRecoverInfo[] GetRecoverInfoFromJobs(string machineName);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobs", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobsResponse" +
            "")]
        System.IAsyncResult BeginGetRecoverInfoFromJobs(string machineName, System.AsyncCallback callback, object asyncState);

        BrokerRecoverInfo[] EndGetRecoverInfoFromJobs(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        BrokerRecoverInfo GetRecoverInfoFromJob(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetRecoverInfoFromJobResponse")]
        System.IAsyncResult BeginGetRecoverInfoFromJob(int jobid, System.AsyncCallback callback, object asyncState);

        BrokerRecoverInfo EndGetRecoverInfoFromJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsValidJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsValidJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool IsValidJob(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsValidJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsValidJobResponse")]
        System.IAsyncResult BeginIsValidJob(int jobid, System.AsyncCallback callback, object asyncState);

        bool EndIsValidJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobOwnerSID", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobOwnerSIDResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        string GetJobOwnerSID(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobOwnerSID", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobOwnerSIDResponse")]
        System.IAsyncResult BeginGetJobOwnerSID(int jobid, System.AsyncCallback callback, object asyncState);

        string EndGetJobOwnerSID(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/FailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/FailJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        void FailJob(int jobid, string reason);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/FailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/FailJobResponse")]
        System.IAsyncResult BeginFailJob(int jobid, string reason, System.AsyncCallback callback, object asyncState);

        void EndFailJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobAllocatedNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobAllocatedNodeNameResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        List<Tuple<string, bool>> GetJobAllocatedNodeName(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobAllocatedNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobAllocatedNodeNameResponse")]
        System.IAsyncResult BeginGetJobAllocatedNodeName(int jobid, System.AsyncCallback callback, object asyncState);

        List<Tuple<string, bool>> EndGetJobAllocatedNodeName(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetTaskAllocatedNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetTaskAllocatedNodeNameResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        List<Tuple<string, bool>> GetTaskAllocatedNodeName(int jobid, int taskid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetTaskAllocatedNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetTaskAllocatedNodeNameResponse")]
        System.IAsyncResult BeginGetTaskAllocatedNodeName(int jobid, int taskid, System.AsyncCallback callback, object asyncState);

        List<Tuple<string, bool>> EndGetTaskAllocatedNodeName(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetAllSessionId", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetAllSessionIdResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        List<int> GetAllSessionId();

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetAllSessionId", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetAllSessionIdResponse")]
        System.IAsyncResult BeginGetAllSessionId(System.AsyncCallback callback, object asyncState);

        List<int> EndGetAllSessionId(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetNonTerminatedSession", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetNonTerminatedSessionResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        Dictionary<int, int> GetNonTerminatedSession();

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetNonTerminatedSession", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetNonTerminatedSessionResponse")]
        System.IAsyncResult BeginGetNonTerminatedSession(System.AsyncCallback callback, object asyncState);

        Dictionary<int, int> EndGetNonTerminatedSession(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobRequeueCount", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobRequeueCountResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        int GetJobRequeueCount(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobRequeueCount", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobRequeueCountResponse")]
        System.IAsyncResult BeginGetJobRequeueCount(int jobid, System.AsyncCallback callback, object asyncState);

        int EndGetJobRequeueCount(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetSessionBrokerNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetSessionBrokerNodeNameResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        string GetSessionBrokerNodeName(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetSessionBrokerNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetSessionBrokerNodeNameResponse")]
        System.IAsyncResult BeginGetSessionBrokerNodeName(int jobid, System.AsyncCallback callback, object asyncState);

        string EndGetSessionBrokerNodeName(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetBrokerNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetBrokerNodeNameResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        List<string> GetBrokerNodeName();

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetBrokerNodeName", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetBrokerNodeNameResponse")]
        System.IAsyncResult BeginGetBrokerNodeName(System.AsyncCallback callback, object asyncState);

        List<string> EndGetBrokerNodeName(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsDiagTraceEnabled", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsDiagTraceEnabledResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool IsDiagTraceEnabled(int jobid);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsDiagTraceEnabled", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/IsDiagTraceEnabledResponse")]
        System.IAsyncResult BeginIsDiagTraceEnabled(int jobid, System.AsyncCallback callback, object asyncState);

        bool EndIsDiagTraceEnabled(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/DumpEventLog", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/DumpEventLogResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        string DumpEventLog(string targetFolder, string logName);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/DumpEventLog", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/DumpEventLogResponse")]
        System.IAsyncResult BeginDumpEventLog(string targetFolder, string logName, System.AsyncCallback callback, object asyncState);

        string EndDumpEventLog(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobCustomizedProperties", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobCustomizedPropertiesResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        Dictionary<string, string> GetJobCustomizedProperties(int jobid, string[] propNames);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobCustomizedProperties", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/GetJobCustomizedPropertiesResponse")]
        System.IAsyncResult BeginGetJobCustomizedProperties(int jobid, string[] propNames, System.AsyncCallback callback, object asyncState);

        Dictionary<string, string> EndGetJobCustomizedProperties(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/SetJobProgressMessage", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/SetJobProgressMessageResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        void SetJobProgressMessage(int jobid, string message);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapterInternal/SetJobProgressMessage", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapterInternal/SetJobProgressMessageResponse")]
        System.IAsyncResult BeginSetJobProgressMessage(int jobid, string message, System.AsyncCallback callback, object asyncState);

        void EndSetJobProgressMessage(System.IAsyncResult result);
    }
}
