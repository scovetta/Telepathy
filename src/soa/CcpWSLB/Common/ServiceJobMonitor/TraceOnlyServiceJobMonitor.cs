// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Microsoft.Hpc.Scheduler.Session.Data;
using Microsoft.Hpc.Scheduler.Session.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Hpc.ServiceBroker.Common.ServiceJobMonitor
{
    public class TraceOnlyServiceJobMonitor : ISchedulerNotify
    {
        public TraceOnlyServiceJobMonitor()
        {
        }
        Task ISchedulerNotify.JobStateChanged(JobState state)
        {
            BrokerTracing.TraceWarning("[TraceOnlyServiceJobMonitor].JobStateChangeStart: Enter");
            return Task.CompletedTask;
        }

        Task ISchedulerNotify.TaskStateChanged(List<TaskInfo> taskInfoList)
        {
            BrokerTracing.TraceWarning("[TraceOnlyServiceJobMonitor].TaskStateChangeStart: Enter");
            return Task.CompletedTask;
        }
    }
}
