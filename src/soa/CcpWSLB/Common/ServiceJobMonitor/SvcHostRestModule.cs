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
            List<TaskInfo> taskInfoList = new List<TaskInfo>();
            Task<TaskInfo>[] tl = new Task<TaskInfo>[startInfo.IpAddress.Length];

            for (int i = 0; i < startInfo.IpAddress.Length; i++)
            {
                tl[i] = OpenSvcHostAsync(sessionId, i, startInfo.IpAddress[i], startInfo.RegPath, startInfo.ServiceName, startInfo.Environments, startInfo.DependFilesStorageInfo);
            }

            await Task.WhenAll(tl);
            for (int i = 0; i < tl.Length; i++)
            {
                TaskInfo ti = tl[i].Result;
                if (ti != null)
                {
                    taskInfoList.Add(ti);
                }
            }

            Debug.Assert(taskInfoList.Count != 0, "No available endpoint.");
            if (taskStateChangedCallBack != null)
            {
                await taskStateChangedCallBack(taskInfoList);
            }
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

            // HTTP POST
            var serviceInfo = new ServiceInfo(sessionId, ti.Id, ti.FirstCoreIndex, regPath + "\\", svcName + ".config", environment, dependFilesInfo);
            try
            {
                var result = await SvcHostHttpClient.PostAsJsonAsync<ServiceInfo>(new Uri($"{Prefix}{ipAddress}:{Port}/{EndPointName}/api/{ApiName}"), serviceInfo);
                BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
                if (result.IsSuccessStatusCode)
                    return ti;
                else
                    return null;
            }
            catch (Exception e)
            {
                // for the romote host closed 
                BrokerTracing.TraceVerbose("[OpenSvcHost].post: Exception: {0}", e.ToString());
                return null;
            }
        }
    }
}