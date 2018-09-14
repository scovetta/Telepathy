using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Store
{
    public interface IClusterTaskGroup : IClusterStoreObject
    {
        IClusterTaskGroup CreateChild(string name);
        
        void AddParent(IClusterTaskGroup group);
        
        IClusterTaskGroup[] GetChildren();
        
        IClusterTaskGroup[] GetParents();

        bool IsCompleted { get; set; }

        int[] GetParentIds();

        bool ParentTasksFailedOrCancelled { get; set; }
        bool TasksFailedOrCancelled { get; set; }
    }
}
