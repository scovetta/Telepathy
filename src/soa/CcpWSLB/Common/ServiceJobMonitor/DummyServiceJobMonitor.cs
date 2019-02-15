//-----------------------------------------------------------------------
// <copyright file="ServiceJobMonitor.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Dummy Monitor service job</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.Hpc.ServiceBroker.Common;

    using System.Net.Http;

    using Microsoft.Hpc.RESTServiceModel;

    class DummyServiceJobMonitor : ServiceJobMonitorBase
    {
        /// <summary>
        /// Stores some uri informations for REST server
        /// </summary>
        private string prefix = "http://";

        private int port = 80;

        private string serverName = "SvcHost";

        private string endPoint = "svchostserver";

        private const int taskIdStart = 9300;


        /// <summary>
        /// Initializes a new instance of the DummyServiceJobMonitor class
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="stateManager">indicating the state manager</param>
        public DummyServiceJobMonitor(SharedData sharedData, BrokerStateManager stateManager, NodeMappingData nodeMappingData, IHpcContext context)
            : base(sharedData, stateManager, nodeMappingData, context)
        {
        }

        /// <summary>
        /// Override Start method and start dummy service job monitor
        /// </summary>
        /// <param name="startInfo"></param>
        /// <param name="dispatcherManager"></param>
        /// <param name="observer"></param>
        /// <returns></returns>
        public override async Task Start(SessionStartInfoContract startInfo, DispatcherManager dispatcherManager, BrokerObserver observer)
        {
            BrokerTracing.TraceVerbose("[ServiceJobMonitor].Start: Enter");

            this.dispatcherManager = dispatcherManager;
            this.observer = observer;

            this.gracefulPreemptionHandler = new GracefulPreemptionHandler(
                this.dispatcherManager,
                this.sharedData.BrokerInfo.SessionId,
                taskId => this.FinishTask(taskId, isRunAwayTask: true));

            this.schedulerAdapterClientFactory = new SchedulerAdapterClientFactory(sharedData, this, dispatcherManager, this.context);

            if (this.sharedData.Config.Monitor.AllocationAdjustInterval != Timeout.Infinite && this.sharedData.StartInfo.EprList == null)
            {
                BrokerTracing.TraceVerbose(
                    "[ServiceJobMonitor].Start: start the allocation adjust thread, interval={0}",
                    this.sharedData.Config.Monitor.AllocationAdjustInterval);

                this.allocationAdjustThread.Start();
            }

            List<TaskInfo> taskInfoList = new List<TaskInfo>();
            Task<TaskInfo>[] tl = new Task<TaskInfo>[startInfo.IpAddress.Length];

            for (int i = 0; i < startInfo.IpAddress.Length; i++)
            {
                tl[i] = this.OpenSvcHostsAsync(i, startInfo.IpAddress[i], startInfo.RegPath, startInfo.ServiceName, startInfo.Environments, startInfo.DependFilesStorageInfo);
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
            await((ISchedulerNotify)this).TaskStateChanged(taskInfoList);
        }

        /// <summary>
        /// Asyn open service host.
        /// </summary>
        /// <param name="num"></param>
        /// <param name="ipAddress"></param>
        /// <param name="regPath"></param>
        /// <param name="svcName"></param>
        /// <returns></returns>
        private async Task<TaskInfo> OpenSvcHostsAsync(int num, string ipAddress, string regPath, string svcName, Dictionary<string, string> environment, Dictionary<string, string> dependFilesInfo)
        {
            TaskInfo ti = new TaskInfo();
            ti.Id = taskIdStart + num;
            ti.Capacity = 1;
            ti.FirstCoreIndex = 3;
            ti.Location = Scheduler.Session.Data.NodeLocation.OnPremise;
            ti.MachineName = ipAddress;
            ti.State = Scheduler.Session.Data.TaskState.Dispatching;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(prefix + ipAddress + ":" + port + "/" + serverName + "/api/");
                //HTTP POST
                var serviceInfo = new ServiceInfo(this.sharedData.BrokerInfo.SessionId, ti.Id, ti.FirstCoreIndex, regPath + "\\", svcName + ".config", environment, dependFilesInfo);
                try
                {
                    var result = await client.PostAsJsonAsync<ServiceInfo>(endPoint, serviceInfo);
                    BrokerTracing.TraceVerbose("[OpenSvcHost].result:{0}", result);
                    if (result.IsSuccessStatusCode)
                        return ti;
                    else
                        return null;
                }
                catch (Exception e) //for the romote host closed 
                {
                    BrokerTracing.TraceVerbose("[OpenSvcHost].post: Exception: {0}", e.ToString());
                    return null;
                }
            }
        }
    }
}
