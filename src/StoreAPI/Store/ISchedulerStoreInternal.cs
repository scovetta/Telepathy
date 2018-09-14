using System.Threading.Tasks;

namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using Microsoft.Hpc.Scheduler.Properties;

    public enum ConnectionRole
    {
        NormalClient,
        AdminClient,
        Service
    }

    public enum ProfileItemOperator
    {
        Add, Modify, Delete
    }

    [Serializable]
    public class RowSetResult
    {
        int _code;
        PropertyRow[] _rows;
        int _totalRowCount;
        int _rowsetId;

        public RowSetResult()
        {
            _code = 0;
            _rows = null;
            _totalRowCount = 0;
            _rowsetId = 0;
        }

        public int Code
        {
            get { return _code; }
            set { _code = value; }
        }

        public PropertyRow[] Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        public int TotalRowCount
        {
            get { return _totalRowCount; }
            set { _totalRowCount = value; }
        }

        public int RowSetId
        {
            get { return _rowsetId; }
            set { _rowsetId = value; }
        }
    }


    [Serializable]
    public class TaskGroupContainer
    {
        Dictionary<int, string> _taskGroups;
        List<KeyValuePair<int, int>> _links;

        internal TaskGroupContainer()
        {
            _taskGroups = null;
            _links = null;
        }

        public Dictionary<int, string> TaskGroups
        {
            get { return _taskGroups; }
            set { _taskGroups = value; }
        }

        public List<KeyValuePair<int, int>> Links
        {
            get { return _links; }
            set { _links = value; }
        }
    }

    [Serializable]
    public class SchedulerCertInfo
    {
        public string Thumbprint = null;
        public DateTime NotAfter = DateTime.MinValue;
        public DateTime NotBefore = DateTime.MaxValue;
    }

    [Serializable]
    public class UserCredential
    {
        public int Id;
        public string OwnerName;
        public string UserName;
        public bool Valid;
        public bool Softcard;
        public uint? ExtendedDataHash;
    }

    /// <summary>
    /// This interface is the remote entry point into the scheduler store
    /// running on the server.  
    /// It is primarily a functional interface with the exception being
    /// the enumerators which are returned as objects.
    /// 
    /// NOTE: Any new property type that is added for jobs,tasks or nodes and
    /// that needs to go over the wire to the HN from clients, we need to 
    /// add a ServiceKnownType here for the ISchedulerStoreInternal interface
    /// It is need for the store api over http case.
    ///  
    /// TODO: Need to add some sort of user token to the calls so that we
    ///       can determine what permissions the user has on the operation
    ///       that is requested.  For example a user should not be able to
    ///       set the state of a job from submitted to running.  That is 
    ///       only down by the scheduling service.  However a user needs 
    ///       to be able to change the state from 'preparing' to 'submitted'
    /// </summary>

    [ServiceContract]
    [ServiceKnownType(typeof(PropertyError))]
    [ServiceKnownType(typeof(PropFlags))]
    [ServiceKnownType(typeof(JobPriority))]
    [ServiceKnownType(typeof(JobState))]
    [ServiceKnownType(typeof(JobUnitType))]
    [ServiceKnownType(typeof(CancelRequest))]
    [ServiceKnownType(typeof(FailureReason))]
    [ServiceKnownType(typeof(JobType))]
    [ServiceKnownType(typeof(JobRuntimeType))]
    [ServiceKnownType(typeof(JobOrderByList))]
    [ServiceKnownType(typeof(JobOrderBy))]
    [ServiceKnownType(typeof(TaskState))]
    [ServiceKnownType(typeof(TaskType))]
    [ServiceKnownType(typeof(TaskId))]
    [ServiceKnownType(typeof(ObjectType))]
#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)
    [ServiceKnownType(typeof(UserPrivilege))]
#pragma warning restore 618
    [ServiceKnownType(typeof(UserRoles))]
    [ServiceKnownType(typeof(FilterOperator))]
    [ServiceKnownType(typeof(EventType))]
    [ServiceKnownType(typeof(FilterProperty))]
    [ServiceKnownType(typeof(JobEvent))]
    [ServiceKnownType(typeof(JobMessageType))]
    [ServiceKnownType(typeof(JobTemplateRights))]
    [ServiceKnownType(typeof(NodeGroup))]
    [ServiceKnownType(typeof(NodeEvent))]
    [ServiceKnownType(typeof(NodeState))]
    [ServiceKnownType(typeof(NodeAvailability))]
    [ServiceKnownType(typeof(NodeLocation))]
    [ServiceKnownType(typeof(PendingReason))]
    [ServiceKnownType(typeof(PendingReason.ReasonCode))]
    [ServiceKnownType(typeof(StorePropertyType))]
    [ServiceKnownType(typeof(ResourceState))]
    [ServiceKnownType(typeof(ResourceJobPhase))]
    [ServiceKnownType(typeof(SchedulerException))]
    [ServiceKnownType(typeof(SortProperty))]
    [ServiceKnownType(typeof(KeyValuePair<string, int>[]))]
    [ServiceKnownType(typeof(Dictionary<string, string>))]
    [ServiceKnownType(typeof(object[]))]
    [ServiceKnownType(typeof(JobNodeGroupOp))]
    public interface ISchedulerStoreInternal
    {

        string Name
        {
            [OperationContract]
            [FaultContract(typeof(ExceptionWrapper))]
            get;
        }

#pragma warning disable 618 // disable obsolete warnings (for UserPrivilege)

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RegisterV2(
            string clientSource,
            string userName,
            ConnectionRole role,
            out ConnectionToken token,
            out UserPrivilege privilege);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Register(
            string clientSource,
            string userName,
            ConnectionRole role,
            Version clientVersion,
            out ConnectionToken token,
            out UserPrivilege privilege,
            out Version serverVersion,
            out Dictionary<string, object> serverProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RegisterOverHttp(
            string clientSource,
            string userName,
            ConnectionRole role,
            Version clientVersion,
            out ConnectionToken token,
            out UserPrivilege privilege,
            out Version serverVersion,
            out Dictionary<string, object> serverProps,
            out int clientId,
            out int clientEventSleepPeriod);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void GetUserTokenAndPrivilege(out ConnectionToken token, out UserPrivilege privilege);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetUserPrivilege(ConnectionToken token, out UserPrivilege privilege);

#pragma warning restore 618 // obsolete warnings for UserPrivilege

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetUserRoles(ConnectionToken token, out UserRoles roles);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void Unregister(ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult EncryptCredential(ConnectionToken token, string userName, string password, out byte[] encryptedPassword);

        [FaultContract(typeof(ExceptionWrapper))]
        CallResult EncryptCredentialForSpecifiedOwner(ConnectionToken token, string userName, string password, string ownerName, out byte[] encryptedPassword);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult DisableCredentialReuse(ConnectionToken token, string userName);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SaveCertificate(ConnectionToken token, string username, string password, bool? reusable, byte[] certificate);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SaveExtendedData(ConnectionToken token, string username, string extendedData);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetCertificateInfo(ConnectionToken token, out SchedulerCertInfo certInfo);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetServerVersion(out Version version);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetServerLinuxHttpsValue(out int linuxHttps);

        [OperationContract]
        [FaultContract(typeof (ExceptionWrapper))]
        CallResult GetCredentialList(ConnectionToken token, string ownerName, bool all, out UserCredential[] credentials);

        // Transaction stuff
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RunTransaction(ref ConnectionToken token, StoreTransaction transaction);

        // Event stuff

        int RemoteEvent_GetEventPort();

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetEventDataOverHttp(ConnectionToken token, int connectionId, DateTime lastReadEvent, out List<byte[]> eventData);


        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RemoteEvent_RegisterForEvent(
            ref ConnectionToken token,
            Packets.EventObjectClass objectClass,
            Int32 objectId,
            Int32 parentObjectId,
            int connectionId,
            out int eventId
        );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RemoteEvent_UnRegisterForEvent(
            ref ConnectionToken token,
            int connectionId,
            int eventId
        );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RemoteEvent_TriggerTouch(
            ref ConnectionToken token,
            int connectionId
        );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RemoteEvent_CloseClient(
            ref ConnectionToken token,
            int connectonId
        );

        //
        // Base object related function calls
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Object_SetProps(ref ConnectionToken token, ObjectType obType, Int32 obId, StoreProperty[] props);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Object_GetProps(ref ConnectionToken token, ObjectType obType, Int32 obId, PropertyId[] ids, out StoreProperty[] props);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Object_EnumeratorPermissionCheck(ref ConnectionToken token, ObjectType obType, Int32 parentId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Object_GetCustomProperties(ref ConnectionToken token, ObjectType obType, Int32 obId, out StoreProperty[] props);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowSet_GetData(ref ConnectionToken token, int rowsetId, int firstRow, int lastRow);

        // Newly added in V3
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowSet_GetDataWithWindowBoundary(ref ConnectionToken token, int rowsetId, int firstRow, int lastRow);


        // V2 back compat version
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowSet_OpenRowSetV2(
                ref ConnectionToken token,
                ObjectType objectType,
                RowSetType rowsetType,
                int flags,
                PropertyId[] columns,
                FilterProperty[] filter,
                SortProperty[] sort,
                AggregateColumn[] aggragate,
                PropertyId[] orderby
                );

        // V3 version
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowSet_OpenRowSet(
                ref ConnectionToken token,
                ObjectType objectType,
                RowSetType rowsetType,
                int flags,
                PropertyId[] columns,
                FilterProperty[] filter,
                SortProperty[] sort,
                AggregateColumn[] aggragate,
                PropertyId[] orderby,
                PropertyId[] frozenIds,
                int top
                );


        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowSet_CloseRowSet(ref ConnectionToken token, int rowsetId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowSet_TouchRowSet(ref ConnectionToken token, int rowsetId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowSet_GetObjectIndex(ref ConnectionToken token, int rowsetId, Int32 objectId, out int index);


        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowSet_Freeze(ref ConnectionToken token, int rowsetId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowSet_Thaw(ref ConnectionToken token, int rowsetId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        RowSetResult RowEnum_Open(
                ref ConnectionToken token,
                ObjectType objectType,
                int options,
                PropertyId[] columns,
                FilterProperty[] filter,
                SortProperty[] sort
                );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowEnum_Close(
                ref ConnectionToken token,
                int id
                );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        PropertyRowSet RowEnum_GetRows(
                ref ConnectionToken token,
                int id,
                int numberOfRows
                );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowEnum_SetProps(
                ref ConnectionToken token,
                ObjectType objectType,
                StoreProperty[] props,
                FilterProperty[] filter
                );

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult RowEnum_Touch(
                ref ConnectionToken token,
                int rowenumId
                );

        //
        // Job related fuction calls
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_VerifyId(ConnectionToken token, Int32 jobId, out StoreProperty[] existingProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_AddJob(ConnectionToken token, ref Int32 jobId, StoreProperty[] jobProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_SubmitJob(ConnectionToken token, Int32 jobId, StoreProperty[] jobProps, out string userName);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_CancelJob(ConnectionToken token, Int32 jobId, CancelRequest request, StoreProperty[] cancelProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_CancelQueuedTasks(ConnectionToken token, int jobId, string message);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_FinishQueuedTasks(ConnectionToken token, int jobId, string message);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_ConfigJob(ConnectionToken token, Int32 jobId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_DeleteJob(ConnectionToken token, Int32 jobId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_Clone(ConnectionToken token, Int32 jobIdOld, ref Int32 jobIdNew);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_GetJobState(ConnectionToken token, Int32 jobId, out JobState state);

        CallResult Job_AddChildJob(ConnectionToken token, int parentJobId, ref Int32 childJobId, StoreProperty[] jobProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_SetEnvVar(ref ConnectionToken token, int jobId, string name, string value);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_GetEnvVars(ref ConnectionToken token, int jobId, out Dictionary<string, string> vars);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_GetShrinkRequests(ConnectionToken token, int jobId, out Dictionary<string, Dictionary<int, ShrinkRequest>> shrinkRequestsByNode);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_AddJobShrinkRequest(ConnectionToken token, int jobId, int resourceId, int nodeId, ShrinkRequest request);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_SetHoldUntil(ConnectionToken token, int jobId, DateTime holdUntil);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_GetAllTaskCustomProperties(ref ConnectionToken token, Int32 jobId, out PropertyRow[] resultsRow);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_AddExcludedNodes(ConnectionToken token, int jobId, string[] nodesToAdd);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_RemoveExcludedNodes(ConnectionToken token, int jobId, string[] nodesToRemove);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_ClearExcludedNodes(ConnectionToken token, int jobId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_GetBalanceRequest(ConnectionToken token, int jobId, out IList<BalanceRequest> request);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Job_RequeueJob(ConnectionToken token, int jobId);

        //
        // Task related function calls
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_ValidateTaskId(ConnectionToken token, Int32 taskId, out Int32 jobId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_AddTaskToJob(ref ConnectionToken token, Int32 jobId, ref Int32 taskId, StoreProperty[] taskProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_AddTasksToJob(ref ConnectionToken token, int jobid, ref List<int> taskidList, List<StoreProperty[]> rgpropsList);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_FindTaskByTaskId(ref ConnectionToken token, Int32 jobId, TaskId taskId, out Int32 taskSystemId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_FindTaskIdByFriendlyId(ref ConnectionToken token, Int32 jobId, Int32 jobTaskId, ref Int32 taskId);

        [OperationContract(Name = "Task_FindTaskIdByFriendlyIdByString")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_FindTaskIdByFriendlyId(ref ConnectionToken token, Int32 jobId, string niceId, ref Int32 taskId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_CloneTask(ref ConnectionToken token, Int32 taskId, ref Int32 taskIdNew, StoreProperty[] taskProps);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_SetEnvironmentVariable(ref ConnectionToken token, Int32 taskId, string name, string value);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_GetEnvironmentVariables(ref ConnectionToken token, Int32 taskId, out Dictionary<string, string> dict);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_FindJobTasksWithEnvVars(ref ConnectionToken token, Int32 jobId, out Int32[] taskIds);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_SubmitTask(ref ConnectionToken token, Int32 jobId, Int32 taskId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_SubmitTasks(ref ConnectionToken token, Int32 jobId, Int32[] taskIds);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_CancelTask(ref ConnectionToken token, Int32 jobId, Int32 taskId, CancelRequest request, Int32 errorCode, string message);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_DeleteTask(ref ConnectionToken token, Int32 jobId, Int32 taskId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_ConfigTask(ref ConnectionToken token, Int32 jobId, Int32 taskId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_ConcludeServiceTask(ref ConnectionToken token, Int32 taskId, bool fCancelSubTasks);

        //
        // Node/Resource related methods
        //
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_InvalidNodeQueryCache(ref ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_ValidateNodeId(ref ConnectionToken token, int id, out Guid nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_FindNodeIdByName(ref ConnectionToken token, string name, out int id, out Guid nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_FindNodeIdBySID(ref ConnectionToken token, string sid, out int id, out Guid nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_FindNodeIdByNodeId(ref ConnectionToken token, Guid nodeId, out int id);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        int Node_AddNode(ConnectionToken token, StoreProperty[] props);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void Node_RemoveNode(ConnectionToken token, Guid nodeId);

        [OperationContract(Name = "TakeNodeOfflineWithoutForce")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_TakeNodeOffline(ref ConnectionToken token, Guid nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_TakeNodeOffline(ref ConnectionToken token, Guid nodeId, bool force);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_PutNodeOnline(ref ConnectionToken toke, Guid nodeId);

        [OperationContract(Name = "TakeNodeOfflineByInt")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_TakeNodeOffline(ref ConnectionToken token, int nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_TakeNodesOffline(ref ConnectionToken token, Guid[] nodeIds, bool force);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_SetDrainingNodesOffline(ref ConnectionToken token);

        [OperationContract(Name = "PutNodeOnlineByInt")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_PutNodeOnline(ref ConnectionToken toke, int nodeId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_GetActiveHeadNodeName(ConnectionToken token, out string name);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_PutNodesOnline(ref ConnectionToken token, Guid[] nodeIds);

        void Node_SetNodeReachable(ConnectionToken token, Guid nodeid);

        void Node_SetNodeUnreachable(ConnectionToken toke, Guid nodeid);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void Node_SetNodeReachable(ConnectionToken token, int nodeid);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void Node_SetNodeUnreachable(ConnectionToken token, int nodeid);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_AddPhantomResource(ConnectionToken token, int nodeid, JobType type, out int resourceId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Node_RemovePhantomResource(ConnectionToken token, int resourceId);

        void UpdateNodePingTime(ConnectionToken token, Int32 nodeId, DateTime pingTime);

        void ScheduleResource(ConnectionToken token, Int32 resourceId, Int32 jobId, StoreProperty[] jobProps);

        void ReserveResourceForJob(ConnectionToken token, int resourceId, int jobId, DateTime limitTime, StoreProperty[] jobProperties);

        //
        // Profile related methods
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Profile_CreateProfile(ref ConnectionToken token, string profileName, out Int32 profileId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Profile_DeleteProfile(ref ConnectionToken token, Int32 profileId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Profile_CloneProfile(ref ConnectionToken token, int currentProfileId, string newProfileName, out Int32 newProfileId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        bool VerifyProfileId(ConnectionToken token, Int32 profileId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void GetProfileIdByName(ConnectionToken token, string profileName, out Int32 profileId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void GetProfileItems(ConnectionToken token, Int32 profileId, out ClusterJobProfileItem[] items);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Profile_ItemOp(ConnectionToken token, Int32 profileId, ProfileItemOperator op, PropertyId pid, ClusterJobProfileItem item);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Profile_UpdateItems(ref ConnectionToken token, Int32 profileId, IEnumerable<StoreProperty> props, IEnumerable<ClusterJobProfileItem> items, bool merge);

        //
        // Misc methods
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SetClusterEnvironmentVariable(ConnectionToken token, string name, string value);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Dictionary<string, string> GetClusterEnvironmentVariables(ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetNodeGroups(ConnectionToken token, out List<NodeGroup> groups);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetNodesFromGroup(ConnectionToken token, string nodeGroupName, out string[] nodes);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Prop_CreatePropertyId(ref ConnectionToken token, ObjectType type, StorePropertyType propertyType, string propertyName, string propertyDescription, out PropertyId pid);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Prop_GetPropertyId(ref ConnectionToken token, ObjectType obType, StorePropertyType propType, string propertyName, out PropertyId pid);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Prop_GetDescriptors(ref ConnectionToken token, ObjectType obType, string[] names, out ServerPropertyDescriptor[] result);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Dictionary<string, string> GetConfigurationSettings(ref ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Dictionary<string, string> GetConfigurationSettingDefaults(ref ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Dictionary<string, string[]> GetConfigurationSettingLimits(ref ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SetConfigurationSetting(ref ConnectionToken token, string name, string value);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SetEmailCredential(ref ConnectionToken token, string username, string password);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        string GetEmailCredentialUser(ref ConnectionToken token);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult GetTemplateCommonName(ConnectionToken token, string friendlyTemplateName, out string templateCommonName);

        //
        // Hook methods
        // Note that these can only be used when the client is within the 
        // process space that the store itself is running.
        //

        void RegisterTaskStateChange(ConnectionToken token, TaskStateChangeDelegate handler);

        void UnRegisterTaskStateChange(ConnectionToken token, TaskStateChangeDelegate handler);

        void RegisterJobStateHandler(ConnectionToken token, JobStateChangeDelegate handler);
        void RegisterJobStateHandlerEx(ConnectionToken token, JobStateChangeDelegateEx handler);

        void UnRegisterJobStateHandler(ConnectionToken token, JobStateChangeDelegate handler);
        void UnRegisterJobStateHandlerEx(ConnectionToken token, JobStateChangeDelegateEx handler);

        void RegisterResourceStateHandler(ConnectionToken token, ResourceStateChangeDelegate handler);

        void UnRegisterResourceStateHandler(ConnectionToken token, ResourceStateChangeDelegate handler);

        void RegisterNodeStateHandler(ConnectionToken token, NodeStateChangeDelegate handler);

        void UnRegisterNodeStateHandler(ConnectionToken token, NodeStateChangeDelegate handler);

        void RegisterEventPacketHandler(ConnectionToken token, ProcessEventPacketDelegate handler);

        void UnRegisterEventPacketHandler(ConnectionToken token, ProcessEventPacketDelegate handler);

        void RegisterConfigChangeHandler(ConnectionToken token, ClusterConfigChangeDelegate handler);

        void UnRegisterConfigChangeHandler(ConnectionToken token, ClusterConfigChangeDelegate handler);

        //
        // TaskGroup methods
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_CreateChild(ConnectionToken token, Int32 jobId, Int32 parentId, StoreProperty[] props, out Int32 childId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_AddParent(ConnectionToken token, Int32 jobId, Int32 groupId, Int32 parentId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_FetchStatistics(ConnectionToken token, Int32 jobId, out List<KeyValuePair<int, KeyValuePair<int, int>>> groups);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_FetchGroups(ConnectionToken token, Int32 jobId, out List<KeyValuePair<int, int>> tree);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_UpdateGroupMaxMin(ConnectionToken token, Int32 jobId, Int32 groupId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void TaskGroup_UpdateGroupsMaxMin(ConnectionToken token, Int32 jobId, IList<int> groupIds);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult TaskGroup_DeleteGroup(ref ConnectionToken token, Int32 jobId, Int32 groupId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult TaskGroup_CreateTaskGroupsAndDependencies(
            ref ConnectionToken token,
            Int32 jobId,
            List<string> newGroups,
            List<KeyValuePair<int, int>> newDependencies,
            Int32 groupIdBase,
            out List<int> newGroupIds);

        //
        // Allocation objects
        //
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        bool Allocation_VerifyId(ConnectionToken token, Int32 allocationId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        int Allocation_FindIdByNodeAndTask(ConnectionToken toke, Int32 nodeId, Int32 taskId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        int Allocation_FindIdByNodeJobAndTask(ConnectionToken toke, Int32 nodeId, Int32 jobId, Int32 taskNiceId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Allocation_UpdateTaskNodeStats(ref ConnectionToken token, Int32 nodeId, Int32 jobId, Int32 taskId, StoreProperty[] props);

        //
        // Pool objects
        //

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Pool_FindPoolIdByName(ConnectionToken token, string poolName, out int id);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Pool_AddPool(ConnectionToken token, string poolName, out int id);

        [OperationContract(Name = "Pool_AddPoolWithWeight")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Pool_AddPool(ConnectionToken token, string poolName, int poolWeight, out int id);

        [OperationContract(Name = "Pool_DeltePoolWithoutForce")]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Pool_DeletePool(ConnectionToken token, string poolName);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Pool_DeletePool(ConnectionToken token, string poolName, bool force);

        //
        // Scheduler On Azure Account Management

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SchedulerOnAzure_AddUser(ConnectionToken token, string username, string password, bool isAdmin);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SchedulerOnAzure_RemoveUser(ConnectionToken token, string username);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SchedulerOnAzure_ValidateUser(ConnectionToken token, string username, string password);

        //
        // Server side filter. only makes sense on server side
        //
        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void SetJobModifyFilter(JobModifyFilter filter);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        void SetNodeQueryCacheInvalidNotification(NodeQueryCacheInvalidNotification handler);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SetUserNamePassword(ConnectionToken token, string userName, byte[] password);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult Task_ExpandParametricSweepTasksInBatch(ref ConnectionToken _token, int taskId, int maxExpand, TaskState expansionState);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Guid Ping();

        //Operations for azure burst deployment that will be called from the management service

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SchedulerAzureBurst_CreateDeployment(ConnectionToken token, string deploymentId, StoreProperty[] props);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        CallResult SchedulerAzureBurst_DeleteDeployment(ConnectionToken token, string deploymentId);

        [OperationContract]
        [FaultContract(typeof(ExceptionWrapper))]
        Task<string> PeekTaskOutput(ConnectionToken cToken, int jobId, int taskId);
    }

    public delegate void ProcessEventPacketDelegate(Packets.EventPacket packet);
    public delegate string ActiveHeadNodeRequestHandler();


    public enum PreemptionBalancedMode
    {
        Immediate = 0,
        Graceful = 1
    }
}
