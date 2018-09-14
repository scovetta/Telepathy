namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Security;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Properties;

    public enum RowSetType
    {
        Snapshot,
        SnapshotWithCustomProps,
        Dynamic
    }
    
    public enum SchedulerEventType
    {
        Nothing             = 0,
        ConnectionLost      = 1,
        ReConnected         = 2,
        Refresh             = 3,
    }

    // This is duplicating the SchedulerAPI's event code and args (ConnectionEvent.cs)
    // Should keep it the same with SchedulerAPI

    public enum SchedulerConnectionEventCode
    {
        None = 0,
        Connect = 1,
        StoreDisconnect = 2,
        StoreReconnect = 3,
        Exception = 4,
        EventDisconnect = 5,
        EventReconnect = 6,
    }

    public class SchedulerConnectionEventArgs
    {
        Exception _e = null;
        SchedulerConnectionEventCode _code = SchedulerConnectionEventCode.None;
        
        public Exception Exception
        {
            get { return _e; }
        }
        
        public SchedulerConnectionEventCode Code
        {
            get { return _code; }
        }

        internal SchedulerConnectionEventArgs(SchedulerConnectionEventCode code)
        {
            _code = code;
            _e = null;
        }        
        
        internal SchedulerConnectionEventArgs(SchedulerConnectionEventCode code, Exception e)
        {
            _code = code;
            _e = e;
        }        
    }

    public delegate void SchedulerConnectionHandler(object sender, SchedulerConnectionEventArgs args);
 
    public delegate void SchedulerObjectEventDelegate(Int32 id, EventType eventType, StoreProperty[] props);

    public delegate void SchedulerJobEventDelegate(Int32 jobId, EventType eventType, StoreProperty[] props);

    public delegate void SchedulerTaskEventDelegate(Int32 jobId, Int32 taskSystemId, TaskId taskId, EventType eventType, StoreProperty[] props);
#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)
    public delegate bool JobModifyFilter(int jobId, int profileId, UserPrivilege privilege, JobState state, IEnumerable<StoreProperty> props, out string errorMessage);
#pragma warning restore 618
    public delegate void SchedulerDeploymentDelegate(string depId, EventType eventType, StoreProperty [] props);

    public delegate void NodeQueryCacheInvalidNotification();    

    public interface ISchedulerStore : IDisposable
    {
        string Name { get; }

        // Management related methods
        
        IStoreManager OpenStoreManager();

        PropertyRow GetProps(params PropertyId[] properties);

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)

        // Get user privilege  
        UserPrivilege GetUserPrivilege();

#pragma warning restore 618

        // Get users roles
        UserRoles GetUserRoles();

        // GetServerVersion
        
        Version GetServerVersion();
        
        // Get server properties
        Dictionary<string, object> GetServerProperties();

        // Transactions
        
        IClusterStoreTransaction BeginTransaction();
        
        // Job related methods
        
        IClusterJob OpenJob(Int32 jobId);

        IClusterJob CreateJob(params StoreProperty[] jobProperties);

        IClusterJob CreateJobFromXml(XmlReader reader);

        IClusterJob CreateJobFromXml(XmlReader reader,StoreProperty [] existingProps);

        int DeleteJob(Int32 jobId);

        IJobRowSet OpenJobRowSet();

        IJobRowSet OpenJobRowSet(RowSetType type);
        
        IRowEnumerator OpenJobEnumerator();

        void GetJobShrinkRequests(int jobid, out Dictionary<string, Dictionary<int, ShrinkRequest>> shrinkRequestsByNode);

        void AddJobShrinkRequest(int jobid, int resourceId, int nodeId, ShrinkRequest request);
        
        // Resource related methods
        
        IClusterResource OpenResource(Int32 resourceId);
        
        IResourceRowSet OpenResourceRowSet();

        IResourceRowSet OpenResourceRowSet(RowSetType type);

        IRowEnumerator OpenResourceRowEnumerator();
        
        
        // Profile related methods
        
        IClusterJobProfile OpenProfile(Int32 profileId);
        
        IClusterJobProfile OpenProfile(string profileName);
        
        IJobProfileRowSet OpenProfileRowSet();

        IJobProfileRowSet OpenProfileRowSet(RowSetType type);
        
        IRowEnumerator OpenProfileEnumerator();

        // Allocation history related methods

        /// <summary>
        /// This is only for Reporting service in V3 and above. 
        /// Please pay extreme attention on the performance impact when using this.
        /// </summary>
        /// <returns></returns>
        IRowEnumerator OpenAllocationEnumerator();

        // Node related methods

        INodeRowSet OpenNodeRowSet();

        INodeRowSet OpenNodeRowSet(RowSetType type);

        IRowEnumerator OpenNodeEnumerator();

        IClusterNode OpenNode(Guid nodeId);
        
        IClusterNode OpenNode(int id);
        
        IClusterNode OpenNode(string name);
        
        IClusterNode OpenNode(System.Security.Principal.SecurityIdentifier sid);

        void InvalidNodeQueryCache();

        //Other methods
        
        void SetClusterEnvironmentVariable(string name, string value);
        
        Dictionary<string, string> GetClusterEnvironmentVariables();

        IEnumerable<NodeGroup> GetNodeGroups();

        string[] GetNodesFromGroup(string nodeGroupName);

        void CancelAsyncWait(IAsyncResult result);
        
        PropertyDescriptor[] GetPropertyDescriptors(ObjectType typeMask, PropertyId[] propIds);
        
        PropertyDescriptor[] GetPropertyDescriptors(ObjectType typeMask, PropFlags flagMask);
        
        PropertyDescriptor[] GetPropertyDescriptors(string[] names, ObjectType typeMask);
        
        PropertyId CreatePropertyId(ObjectType type, StorePropertyType propertyType, string propertyName, string propertyDescription);

        void GetCustomProperties(ObjectType obType, int objId, out StoreProperty[] props);

        void PurgeCredentials(string username);

        // Allocation objects
        
        IClusterAllocation OpenAllocationObject(int allocationId);
        
        IClusterAllocation OpenAllocationObject(int nodeId, int taskId);

        IClusterAllocation OpenAllocationObject(int nodeId, int jobId, int taskNiceId);        

        void UpdateTaskNodeStats(int nodeId, int jobId, int taskId, StoreProperty[] props);

        byte[] EncryptCredential(string username, string password);

        void SetCachedCredentials(string userName, string password);

        void SetCachedCredentials(string userName, string password, string ownerName);

        void SaveUserCertificate(string userName, SecureString pfxPassword, bool? reusable, byte[] certificate);

        void SaveUserExtendedData(string userName, string extendedData);

        UserCredential[] GetCredentialList(string ownerName, bool all);

        string EnrollCertificate(string templateName);

        void GetCertificateInfo(out SchedulerCertInfo certInfo);

        int ExpandParametricSweepTasksInBatch(int taskId, int maxExpand, TaskState expansionState);

        //Pool

        IClusterPool OpenPool(string poolName);

        IClusterPool AddPool(string poolName);

        IClusterPool AddPool(string poolName,int poolWeight);

        void DeletePool(string poolName);

        void DeletePool(string poolName,bool force);

        IPoolRowSet OpenPoolRowset();

        IPoolRowSet OpenPoolRowSet(RowSetType rowSetType);

        // Scheduler On Azure

        void AddAzureUser(string username, string password, bool isAdmin);

        void RemoveAzureUser(string username);

        bool ValidateAzureUser(string username, string password);

        // Events
        
        event SchedulerJobEventDelegate JobEvent;

        event SchedulerTaskEventDelegate TaskEvent;

        event SchedulerObjectEventDelegate NodeEvent;
        
        event SchedulerObjectEventDelegate ResourceEvent;
        
        event SchedulerObjectEventDelegate ProfileEvent;

        event SchedulerConnectionHandler ConnectionEvent;
        

        void SetJobModifyFilter(JobModifyFilter filter);

        void SetNodeQueryCacheInvalidNotification(NodeQueryCacheInvalidNotification handler);

        void SetUserNamePassword(string username, byte[] encryptedPassword);

        VersionControl ServerVersion { get; }
        VersionControl ClientVersion { get; }

        void AddPropertyConverter(PropertyConverter converter);

        void GetConfigSettingsValues(IEnumerable<string> configSettings, out List<string> configValues);

        string GetActiveHeadNodeName();

        bool OverHttp { get; }

        string Owner { get; }

        string OwnerSid { get; }

        void CreateDeployment(string DeploymentId, StoreProperty[] props);

        void DeleteDeployment(string DeploymentId);

        bool PingScheduler();

        int GetServerLinuxHttpsValue();

        Task<string> GetSchedulerNodeNameAsync();

        Task<string> PeekTaskOutputAsync(int jobId, int taskId);
    }
}
 