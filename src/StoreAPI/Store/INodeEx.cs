using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterNode : IClusterStoreObject
    {
        void TakeOffline();

        void PutOnline();

        void SetReachable();

        void SetUnreachable();
        
    }
}
