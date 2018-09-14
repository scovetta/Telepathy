using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterResource : IClusterStoreObject
    {
        void ScheduleJob(int jobId, StoreProperty[] jobProperties);

        void ReserveForJob(int jobId, DateTime reserveLimitTime, StoreProperty[] jobProperties);

        void ClearJob();
        
        void ClearReservedJob();
        
    }
}
