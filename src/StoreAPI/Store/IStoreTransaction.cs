using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterStoreTransaction : IDisposable
    {
        void Commit();

        void Detach();

        void Attach();
    }
}
