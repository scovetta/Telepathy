using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Security.Principal;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public delegate void TaskStateChangeDelegate(int jobId, JobState jobState, int taskId, TaskState prevState, TaskState newState);

    public delegate void JobStateChangeDelegate(int jobId, JobState prevState, JobState newState);

    // This delegate extends the JobStateChangeDelegate by allowing the event advisor to specify a set of known properties for a
    // job that underwent a state change.
    public delegate void JobStateChangeDelegateEx(int jobId, JobState prevState, JobState newState, StoreProperty[] contextualProps);

    public delegate void ResourceStateChangeDelegate(int resourceId, ResourceState prevState, ResourceState newState);

    public delegate void NodeStateChangeDelegate(int nodeId, NodeState prevState, NodeState newState);

    // Invoked whenever a cluster configuration parameter changes
    public delegate void ClusterConfigChangeDelegate(string paramName, string oldValue, string newValue);


    public interface IStoreManager
    {
        // Job related methods
        
        IRowEnumerator OpenJobHistoryEnumerator();

        // Node related functions
        
        IClusterNode AddNode(StoreProperty[] props);

        void RemoveNode(Guid nodeId);
        
        void TakeNodeOffline(Guid nodeId);

        void TakeNodeOffline(Guid nodeId, bool force);

        void TakeNodesOffline(Guid[] nodeIds, bool force);
        
        void PutNodeOnline(Guid nodeId);

        void PutNodesOnline(Guid[] nodeIds);

        void SetDrainingNodesOffline();

        int AddPhantomResourceToNode(int nodeId, JobType type);

        void RemovePhantomResource(int resourceId);
        
        IRowEnumerator OpenNodeHistoryEnumerator();

        // Profile related methods

        IClusterJobProfile CreateProfile(XmlReader reader, string profileName);

        IClusterJobProfile CreateProfile(string profileName);
        
        void DeleteProfile(Int32 profileId);

        //task related methods

        IClusterTask OpenGlobalTask(int taskId);

        IRowEnumerator OpenGlobalTaskEnumerator(TaskRowSetOptions option);

        ITaskRowSet OpenGlobalTaskRowSet(RowSetType type, TaskRowSetOptions option);

        // Cluster config settings
        
        Dictionary<string, string> GetConfigurationSettings();

        Dictionary<string, string> GetConfigurationDefaults();

        Dictionary<string, string[]> GetConfigurationLimits();

        void SetConfigurationSetting(string name, string value);

        void SetEmailCredential(string username, string password);

        string GetEmailCredentialUser();

        // State handlers
        
        void RegisterTaskStateHandler(TaskStateChangeDelegate handler);
        
        void UnRegisterTaskStateHandler(TaskStateChangeDelegate handler);

        void RegisterJobStateHandler(JobStateChangeDelegate handler);
        void RegisterJobStateHandlerEx(JobStateChangeDelegateEx handler);

        void UnRegisterJobStateHandler(JobStateChangeDelegate handler);
        void UnRegisterJobStateHandlerEx(JobStateChangeDelegateEx handler);

        void RegisterResourceStateHandler(ResourceStateChangeDelegate handler);

        void UnRegisterResourceStateHandler(ResourceStateChangeDelegate handler);

        void RegisterNodeStateHandler(NodeStateChangeDelegate handler);

        void UnRegisterNodeStateHandler(NodeStateChangeDelegate handler);


        void RegisterConfigChangeHandler(ClusterConfigChangeDelegate handler);

        void UnRegisterConfigChangeHandler(ClusterConfigChangeDelegate handler);
    }
}
