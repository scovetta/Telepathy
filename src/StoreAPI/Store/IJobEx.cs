using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    [Serializable]
    [Flags]
    public enum TaskRowSetOptions
    {
        None                        = 0,
        NoParametricExpansion       = 0x001,
        ParametricMasterOnly        = 0x002,
        FullParametric              = 0x004,
    }

    
    public interface IClusterJob : IClusterStoreObject
    {
        // Job Control

        void Configure();

        void Submit(params StoreProperty[] props);

        IAsyncResult BeginSubmit(StoreProperty[] props, AsyncCallback callback, object state);

        JobState EndSubmit(IAsyncResult result);

        void Cancel(string message);

        void Cancel(string message, bool isForced, bool isGraceful);

        void Finish(string message, bool isForced, bool isGraceful);

        void Requeue();

        IAsyncResult BeginCancel(AsyncCallback callback, object state, string message);

        IAsyncResult BeginCancel(AsyncCallback callback, object state, CancelRequest request, string message);
        
        JobState EndCancel(IAsyncResult result);
        
        IClusterJob Clone();

        void SetHoldUntil(DateTime holdUntil);
        
        // Task related methods

        IClusterTask CreateTask(params StoreProperty[] taskProperties);

        List<IClusterTask> CreateTasks(List<StoreProperty[]> taskPropertyList);
    
        IClusterTask OpenTask(Int32 taskSystemId);

        IClusterTask OpenTask(TaskId taskId);

        int DeleteTask(Int32 taskSystemId);
        
        IAsyncResult BeginCancelTask(TaskId taskId, AsyncCallback callback, object param, string message);

        IAsyncResult BeginCancelTask(TaskId taskId, CancelRequest request, AsyncCallback callback, object param, string message);

        IAsyncResult BeginCancelTask(Int32 taskSystemId, AsyncCallback callback, object param, string message);

        IAsyncResult BeginCancelTask(Int32 taskSystemId, CancelRequest request, AsyncCallback callback, object param, string message);
        
        TaskState EndCancelTask(IAsyncResult result);

        void CancelTask(int taskId, string message);

        void CancelTask(int taskId, string message, bool isForced);

        void FinishTask(int taskId, string message);

        void FinishTaskByNiceId(int taskNiceId, string message);

        ITaskRowSet OpenTaskRowSet();

        ITaskRowSet OpenTaskRowSet(RowSetType type);
        
        ITaskRowSet OpenTaskRowSet(RowSetType type, TaskRowSetOptions options);
        
        IRowEnumerator OpenTaskRowEnumerator();

        IRowEnumerator OpenTaskRowEnumerator(TaskRowSetOptions options);

        ITaskGroupRowSet OpenTaskGroupRowSet();

        void SubmitTasks(int[] taskIds);

        // Task Groups
        
        IClusterTaskGroup GetRootTaskGroup();
        
        void DeleteTaskGroup(Int32 groupId);

        void UpdateTaskGroup(Int32 groupId);

        void UpdateTaskGroups(IList<int> groupIds);
        
        // Allocation history
        
        IAllocationRowSet OpenAllocationRowSet();

        IRowEnumerator OpenAllocationEnumerator();

        // Job Messages

        IJobMessageRowSet OpenMessageRowset();

        IJobMessageRowSet OpenMessageRowset(RowSetType type);

        IRowEnumerator OpenMessageEnumerator();

        //Child jobs

        IClusterJob CreateChildJob(params StoreProperty[] jobProps);

        // Task Nice Id methods to be depricated.
    
        IClusterTask OpenTaskByNiceId(Int32 taskNiceId);

        IClusterTask OpenTaskByNiceId(string taskNiceId);

        IAsyncResult BeginCancelTaskNiceId(Int32 taskNiceId, AsyncCallback callback, object param, string message);

        IAsyncResult BeginCancelTaskNiceId(Int32 taskNiceId, CancelRequest request, AsyncCallback callback, object param, string message);

        //Scheduling admin job
        void ScheduleOnPhantomResources(int[] nodeIds, StoreProperty[] phantomResourceProps);
        
        // ExcludedNodes

        void AddExcludedNodes(string[] nodeNames);

        void RemoveExcludedNodes(string[] nodeNames);

        void ClearExcludedNodes();
        
        // EnvVars

        void SetEnvironmentVariable(string name, string value);

        Dictionary<string, string> GetEnvironmentVariables();
        
        event SchedulerTaskEventDelegate TaskEvent;

        //customVars
        void GetAllTaskCustomProperties(out PropertyRow[] resultRow);

        // Task graph
        void CreateTaskGroupsAndDependencies(List<string> newGroups, List<KeyValuePair<int, int>> newDependencies, int groupIdBase, out List<int> newGroupIds);

        IList<BalanceRequest> GetBalanceRequest();
    }
}
