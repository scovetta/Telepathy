// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Async version of ISchedulerAdapter
    /// Callback is sync version of ISchedulerNotify
    /// </summary>
    [System.ServiceModel.ServiceContractAttribute(Name = "ISchedulerAdapter", Namespace = "http://hpc.microsoft.com", CallbackContract = typeof(ISchedulerNotify))]
    internal interface ISchedulerAdapterAsync
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/RegisterJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/RegisterJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        JobState RegisterJob(int jobid, out int autoMax, out int autoMin);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/RegisterJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/RegisterJobResponse")]
        System.IAsyncResult BeginRegisterJob(int jobid, System.AsyncCallback callback, object asyncState);

        [return: System.ServiceModel.MessageParameterAttribute(Name = "autoMax")]
        JobState EndRegisterJob(out int autoMax, out int autoMin, System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/FinishJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FinishJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        void FinishJob(int jobid, string reason);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/FinishJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FinishJobResponse")]
        System.IAsyncResult BeginFinishJob(int jobid, string reason, System.AsyncCallback callback, object asyncState);

        void EndFinishJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/FailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FailJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        void FailJob(int jobid, string reason);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/FailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FailJobResponse")]
        System.IAsyncResult BeginFailJob(int jobid, string reason, System.AsyncCallback callback, object asyncState);

        void EndFailJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/RequeueOrFailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/RequeueOrFailJobResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        void RequeueOrFailJob(int jobid, string reason);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/RequeueOrFailJob", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/RequeueOrFailJobResponse")]
        System.IAsyncResult BeginRequeueOrFailJob(int jobid, string reason, System.AsyncCallback callback, object asyncState);

        void EndRequeueOrFailJob(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/ExcludeNode", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/ExcludeNodeResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool ExcludeNode(int jobid, string nodeName);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/ExcludeNode", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/ExcludeNodeResponse")]
        System.IAsyncResult BeginExcludeNode(int jobid, string nodeName, System.AsyncCallback callback, object asyncState);

        bool EndExcludeNode(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/UpdateBrokerInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/UpdateBrokerInfoResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool UpdateBrokerInfo(int jobid, System.Collections.Generic.Dictionary<string, object> properties);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/UpdateBrokerInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/UpdateBrokerInfoResponse")]
        System.IAsyncResult BeginUpdateBrokerInfo(int jobid, System.Collections.Generic.Dictionary<string, object> properties, System.AsyncCallback callback, object asyncState);

        bool EndUpdateBrokerInfo(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/GetTaskErrorCode", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/GetTaskErrorCodeResponse")]
        System.IAsyncResult BeginGetTaskErrorCode(int jobId, int globalTaskId, System.AsyncCallback callback, object asyncState);

        int? EndGetTaskErrorCode(System.IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/GetGracefulPreemptionInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/GetGracefulPreemptionInfoResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool GetGracefulPreemptionInfo(int jobId, out BalanceInfo balanceInfo, out List<int> taskIds, out List<int> runningTaskIds);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/GetGracefulPreemptionInfo", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/GetGracefulPreemptionInfoResponse")]
        System.IAsyncResult BeginGetGracefulPreemptionInfo(int jobid, System.AsyncCallback callback, object asyncState);

        [return: System.ServiceModel.MessageParameterAttribute(Name = "succeeded")]
        bool EndGetGracefulPreemptionInfo(out BalanceInfo balanceInfo, out List<int> taskIds, out List<int> runningTaskIds, IAsyncResult result);

        [System.ServiceModel.OperationContractAttribute(Action = "http://hpc.microsoft.com/ISchedulerAdapter/FinishTask", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FinishTaskResponse")]
        [System.ServiceModel.FaultContractAttribute(typeof(SessionFault), Action = "http://hpc.microsoft.com/session/SessionFault", Name = "SessionFault", Namespace = "http://hpc.microsoft.com/session/")]
        bool FinishTask(int jobId, int taskUniqueId);

        [System.ServiceModel.OperationContractAttribute(AsyncPattern = true, Action = "http://hpc.microsoft.com/ISchedulerAdapter/FinishTask", ReplyAction = "http://hpc.microsoft.com/ISchedulerAdapter/FinishTaskResponse")]
        System.IAsyncResult BeginFinishTask(int jobid, int taskUniqueId, System.AsyncCallback callback, object asyncState);

        [return: System.ServiceModel.MessageParameterAttribute(Name = "succeeded")]
        bool EndFinishTask(System.IAsyncResult result);
    }
}
