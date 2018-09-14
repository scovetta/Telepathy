using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterTask : IClusterStoreObject
    {
        int TaskId { get; }
        
        void SetEnvironmentVariable(string name, string value);

        Dictionary<string, string> GetEnvironmentVariables();
        
        IClusterTask Clone(StoreProperty[] taskProperties);

        IAsyncResult BeginSubmit(AsyncCallback callback, object param);            
        TaskState EndSubmit(IAsyncResult result);

        void SubmitTask();

        void Configure();

        void ServiceConclude(bool fCancelSubTasks);
    }
}
