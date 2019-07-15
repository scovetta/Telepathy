namespace Microsoft.Hpc.ServiceBroker.Common.ServiceJobMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RESTServiceModel;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;

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

        public static async Task OpenSvcHostsAsync(int sessionId, SessionStartInfoContract startInfo, Func<List<TaskInfo>, Task> taskStateChangedCallBack)
        {
            for (int i = 0; i < startInfo.IpAddress.Length; i++)
            {
                OpenSvcHostWithRetryAsync(sessionId, i, startInfo.IpAddress[i], startInfo.RegPath, startInfo.ServiceName, startInfo.ServiceVersion, startInfo.Environments, startInfo.DependFilesStorageInfo)
                    .ContinueWith(t => taskStateChangedCallBack(new List<TaskInfo> { t.Result }));
            }
        }

        private static async Task<TaskInfo> OpenSvcHostWithRetryAsync(
            int sessionId,
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
            return await mgr.InvokeWithRetryAsync(() => OpenSvcHostAsync(sessionId, num, ipAddress, regPath, svcName, svcVersion, environment, dependFilesInfo),  ex => true);
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
            int sessionId,
            int num,
            string ipAddress,
            string regPath,
            string svcName,
            Version svcVersion,
            Dictionary<string, string> environment,
            Dictionary<string, string> dependFilesInfo)
        {
            TaskInfo ti = new TaskInfo();
            ti.Id = TaskIdStart + num;
            ti.Capacity = 1;
            ti.FirstCoreIndex = 3;
            ti.Location = Scheduler.Session.Data.NodeLocation.OnPremise;
            ti.MachineName = ipAddress;
            ti.State = Scheduler.Session.Data.TaskState.Dispatching;
            string fileName = SoaRegistrationAuxModule.GetRegistrationFileName(svcName, svcVersion);
            // HTTP POST
            var serviceInfo = new ServiceInfo(sessionId, ti.Id, ti.FirstCoreIndex, regPath + "\\", fileName, environment, dependFilesInfo);
            var result = await SvcHostHttpClient.PostAsJsonAsync<ServiceInfo>(new Uri($"{Prefix}{ipAddress}:{Port}/{EndPointName}/api/{ApiName}"), serviceInfo);
            BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
            result.EnsureSuccessStatusCode();
            return ti;
        }
    }
}