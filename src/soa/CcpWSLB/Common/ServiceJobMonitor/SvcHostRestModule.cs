// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ServiceJobMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.CcpServiceHost.Rest;
    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Common.ServiceRegistrationStore;
    using Microsoft.Telepathy.ServiceBroker.BackEnd;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Data;
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Controls service host using its management rest service when
    /// the service host is not started by HPC Pack Scheduler Service
    /// </summary>
    public static class SvcHostRestModule
    {
        private static HttpClient SvcHostHttpClient = new HttpClient();

        private const int TaskIdStart = 9300;

        private const string Prefix = "http://";

        private const int Port = 80;

        private const string EndPointName = "SvcHost";

        private const string ApiName = "svchostserver";

        //TODO: int should be changed to string when taskid is type of string
        private static HashSet<string> invalidIds = new HashSet<string>();

        public static async Task OpenSvcHostsAsync(string sessionId, SessionStartInfoContract startInfo, Func<List<TaskInfo>, Task> taskStateChangedCallBack)
        {
            for (int i = 0; i < startInfo.IpAddress.Length; i++)
            {
                OpenSvcHostWithRetryAsync(sessionId, i, startInfo.IpAddress[i], startInfo.RegPath, startInfo.ServiceName, startInfo.ServiceVersion, startInfo.Environments, startInfo.DependFilesStorageInfo)
                    .ContinueWith(t => taskStateChangedCallBack(new List<TaskInfo> { t.Result }));
            }
        }

        internal static async Task<ServiceTaskDispatcherInfo> OpenSvcHostsAsync(string sessionId, SessionStartInfoContract startInfo, ServiceTaskDispatcherInfo taskDispatcherInfo)
        {
            return await OpenSvcHostWithRetryAsync(sessionId, taskDispatcherInfo, startInfo.RegPath, startInfo.ServiceName, startInfo.ServiceVersion, startInfo.Environments, startInfo.DependFilesStorageInfo);
        }

        public static void StopOpenSvcHostAsync(List<string> cancelledIds)
        {
            invalidIds = new HashSet<string>(invalidIds.Union(cancelledIds));
        }

        private static async Task<TaskInfo> OpenSvcHostWithRetryAsync(
            string sessionId,
            int num,
            string ipAddress,
            string regPath,
            string svcName,
            Version svcVersion,
            Dictionary<string, string> environment,
            Dictionary<string, string> dependFilesInfo)
        {
            BrokerTracing.TraceVerbose("[OpenSvcHostWithRetryAsync] Started open service host {0} for session {1}", ipAddress, sessionId);
            RetryManager mgr = new RetryManager(new ExponentialRandomBackoffRetryTimer(1 * 1000, 10 * 1000));
            return await mgr.InvokeWithRetryAsync(() => OpenSvcHostAsync(sessionId, num, ipAddress, regPath, svcName, svcVersion, environment, dependFilesInfo), ex => true);
        }

        private static async Task<ServiceTaskDispatcherInfo> OpenSvcHostWithRetryAsync(
            string sessionId,
             ServiceTaskDispatcherInfo taskDispatcherInfo,
             string regPath,
             string svcName,
             Version svcVersion,
             Dictionary<string, string> environment,
             Dictionary<string, string> dependFilesInfo)
        {
            BrokerTracing.TraceVerbose("[OpenSvcHostWithRetryAsync] Started open service host {0} for session {1}", taskDispatcherInfo.MachineName, sessionId);
            RetryManager mgr = new RetryManager(new ExponentialRandomBackoffRetryTimer(1 * 1000, 10 * 1000), RetryManager.InfiniteRetries);
            return await mgr.InvokeWithRetryAsync(() => OpenSvcHostAsync(sessionId, taskDispatcherInfo, regPath, svcName, svcVersion, environment, dependFilesInfo), ex => true);
        }

        /// <summary>
        /// Async open service host.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="taskInfo"></param>
        /// <param name="regPath"></param>
        /// <param name="svcName"></param>
        /// <returns></returns>
        private static async Task<ServiceTaskDispatcherInfo> OpenSvcHostAsync(
            string sessionId,
             ServiceTaskDispatcherInfo taskDispatcherInfo,
             string regPath,
             string svcName,
             Version svcVersion,
             Dictionary<string, string> environment,
             Dictionary<string, string> dependFilesInfo)
        {
            var invalidList = invalidIds;
            if (invalidList.Contains(taskDispatcherInfo.TaskId))
            {
                return null;
            }
            string fileName = SoaRegistrationAuxModule.GetRegistrationFileName(svcName, svcVersion);
            BrokerTracing.TraceVerbose("[SvcHostHttpClient] Started send request, taskId is {0} and session id is {1}", taskDispatcherInfo.TaskId, sessionId);
            // HTTP POST
            var serviceInfo = new ServiceInfo(sessionId, taskDispatcherInfo.TaskId, taskDispatcherInfo.FirstCoreId, regPath + "\\", fileName, environment, dependFilesInfo);
            var result = await SvcHostHttpClient.PostAsJsonAsync<ServiceInfo>(new Uri($"{Prefix}{taskDispatcherInfo.MachineName}:{Port}/{EndPointName}/api/{ApiName}"), serviceInfo);
            BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
            result.EnsureSuccessStatusCode();
            return taskDispatcherInfo;
        }

        /// <summary>
        /// Async open service host.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="ipAddress"></param>
        /// <param name="regPath"></param>
        /// <param name="svcName"></param>
        /// <returns></returns>
        private static async Task<TaskInfo> OpenSvcHostAsync(
            string sessionId,
            int num,
            string ipAddress,
            string regPath,
            string svcName,
            Version svcVersion,
            Dictionary<string, string> environment,
            Dictionary<string, string> dependFilesInfo)
        {
            var ti = CreateDummyTaskInfo(num, ipAddress);
            string fileName = SoaRegistrationAuxModule.GetRegistrationFileName(svcName, svcVersion);
            // HTTP POST
            var serviceInfo = new ServiceInfo(sessionId, ti.Id, ti.FirstCoreIndex, regPath + "\\", fileName, environment, dependFilesInfo);
            var result = await SvcHostHttpClient.PostAsJsonAsync<ServiceInfo>(new Uri($"{Prefix}{ipAddress}:{Port}/{EndPointName}/api/{ApiName}"), serviceInfo);
            BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
            result.EnsureSuccessStatusCode();
            return ti;
        }

        public static TaskInfo CreateDummyTaskInfo(int idx, string ipAddress)
        {
            TaskInfo ti = new TaskInfo();
            ti.Id = (TaskIdStart + idx).ToString();
            ti.Capacity = 1;
            ti.FirstCoreIndex = 3;
            ti.Location = NodeLocation.OnPremise;
            ti.MachineName = ipAddress;
            ti.State = TaskState.Dispatching;
            return ti;
        }

        public static List<TaskInfo> CreateDummyTaskInfos(string[] ipAddresses)
        {
            List<TaskInfo> res = new List<TaskInfo>();
            for (int i = 0; i < ipAddresses.Length; ++i)
            {
                res.Add(CreateDummyTaskInfo(i, ipAddresses[i]));
            }

            return res;
        }
    }
}