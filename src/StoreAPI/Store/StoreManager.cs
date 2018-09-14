namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Hpc.Scheduler.Properties;

    internal class StoreManager : IStoreManager
    {
        ConnectionToken token          = null;
        SchedulerStoreSvc helper        = null;

        public StoreManager(ConnectionToken token, SchedulerStoreSvc helper)
        {
            this.token = token;
            this.helper = helper;
        }
        
        public IClusterNode AddNode(StoreProperty[] props)
        {
            if (props == null || (props != null && props.Length == 1 && props[0] == null))
            {
                throw new IndexOutOfRangeException("Must provide properties");
            }

            props = PropertyLookup.ProcessSetProps(helper, ObjectType.Node, props);
    
            StoreProperty propGuid = null;
            
            foreach (StoreProperty prop in props)
            {
                if (prop.Id == NodePropertyIds.Guid)
                {
                    propGuid = prop;
                    break;
                }
            }

            if (propGuid == null)
            {
                throw new IndexOutOfRangeException("Must provide a Node Id");
            }

            int id = helper.ServerWrapper.Node_AddNode(token, props);

            return new NodeEx(id, helper);
        }

        public void RemoveNode(Guid nodeId)
        {
            this.helper.ServerWrapper.Node_RemoveNode(token, nodeId);
        }

        public void TakeNodeOffline(Guid nodeId)
        {
            helper.ServerWrapper.Node_TakeNodeOffline(nodeId);
        }

        public void TakeNodeOffline(Guid nodeId,bool force)
        {
            helper.ServerWrapper.Node_TakeNodeOffline(nodeId,force);
        }

        public void TakeNodesOffline(Guid[] nodeIds, bool force)
        {
            helper.ServerWrapper.Node_TakeNodesOffline(nodeIds, force);
        }

        public void PutNodeOnline(Guid nodeId)
        {
            helper.ServerWrapper.Node_PutNodeOnline(nodeId);
        }

        public void PutNodesOnline(Guid[] nodeIds)
        {
            helper.ServerWrapper.Node_PutNodesOnline(nodeIds);
        }

        public void SetDrainingNodesOffline()
        {
            helper.ServerWrapper.Node_SetDrainingNodesOffline();
        }

        public int AddPhantomResourceToNode(int nodeId, JobType type)
        {
            return helper.ServerWrapper.Node_AddPhantomResource(nodeId, type);
        }

        public void RemovePhantomResource(int resourceId)
        {
            helper.ServerWrapper.Node_RemovePhantomResource(resourceId);
        }

        public IRowEnumerator OpenNodeHistoryEnumerator()
        {
            return new LocalRowEnumerator(helper, ObjectType.NodeHistory, StorePropertyIds.NodeHistoryObject);
        }

        public IRowEnumerator OpenJobHistoryEnumerator()
        {
            return new LocalRowEnumerator(helper, ObjectType.JobHistory, StorePropertyIds.JobObject);
        }

        string ValidateNewProfileName(string name)
        {
            // Only trim off whitespace from the name.

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
            }
            else
            {
                // Return Empty string for either null or empty string
                //the XML must contain the name of the profile
                name = string.Empty;
            }
            
            return name;
        }

        public IClusterJobProfile CreateProfile(System.Xml.XmlReader reader, string profileName)
        {
            JobProfilePropertyBag bag = new JobProfilePropertyBag(helper.StoreInProc);
            
            bag.ReadXML(reader, XmlImportOptions.None);
            
            return bag.Create(helper, profileName);
        }

        public IClusterJobProfile CreateProfile(string profileName)
        {
            Int32 profileId;

            profileName = ValidateNewProfileName(profileName);
            
            if (string.IsNullOrEmpty(profileName))
            {
                throw new SchedulerException(ErrorCode.Operation_MustProvideProfileName, "");
            }

            helper.ServerWrapper.Profile_CreateProfile(profileName, out profileId);
            
            return new JobProfile(helper, token, profileId);
        }

        public void DeleteProfile(Int32 profileId)
        {
            helper.ServerWrapper.Profile_DeleteProfile(profileId);
        }

        public IClusterTask OpenGlobalTask(int taskId)
        {
            int parentJobId;

            helper.ServerWrapper.Task_ValidateTaskId(taskId, out parentJobId);
            
            return new TaskEx(parentJobId, taskId, helper);
        }

        public IRowEnumerator OpenGlobalTaskEnumerator(TaskRowSetOptions option)
        {
            return new LocalRowEnumerator(helper, ObjectType.Task, TaskPropertyIds.TaskObject, TaskPropertyIds.ParentJobId, option);
        }

        public ITaskRowSet OpenGlobalTaskRowSet(RowSetType type, TaskRowSetOptions option)
        {
            LocalTaskRowSet rowset = new LocalTaskRowSet(helper, type, 0, option);            
            return rowset;
        }

        public Dictionary<string, string> GetConfigurationSettings()
        {
            return helper.ServerWrapper.Config_GetSettings();
        }
        
        public Dictionary<string, string> GetConfigurationDefaults()
        {
            return helper.ServerWrapper.Config_GetDefaults();
        }

        public Dictionary<string, string[]> GetConfigurationLimits()
        {
            return helper.ServerWrapper.Config_GetLimits();
        }

        public void SetConfigurationSetting(string name, string value)
        {
            helper.ServerWrapper.Config_SetSetting(name, value);
        }

        public void SetEmailCredential(string username, string password)
        {
            helper.ServerWrapper.Config_SetEmailCredential(username, password);
        }

        public string GetEmailCredentialUser()
        {
            return helper.ServerWrapper.Config_GetEmailCredentialUser();
        }

        public void RegisterTaskStateHandler(TaskStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterTaskStateChange(token, handler);
            }
        }

        public void UnRegisterTaskStateHandler(TaskStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterTaskStateChange(token, handler);
            }
        }

        public void RegisterJobStateHandler(JobStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterJobStateHandler(token, handler);
            }
        }

        public void RegisterJobStateHandlerEx(JobStateChangeDelegateEx handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterJobStateHandlerEx(token, handler);
            }
        }

        public void UnRegisterJobStateHandler(JobStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterJobStateHandler(token, handler);
            }
        }

        public void UnRegisterJobStateHandlerEx(JobStateChangeDelegateEx handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterJobStateHandlerEx(token, handler);
            }
        }

        public void RegisterResourceStateHandler(ResourceStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterResourceStateHandler(token, handler);
            }
        }

        public void UnRegisterResourceStateHandler(ResourceStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterResourceStateHandler(token, handler);
            }
        }

        public void RegisterNodeStateHandler(NodeStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterNodeStateHandler(token, handler);
            }
        }

        public void UnRegisterNodeStateHandler(NodeStateChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterNodeStateHandler(token, handler);
            }
        }

        public void RegisterConfigChangeHandler(ClusterConfigChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.RegisterConfigChangeHandler(token, handler);
            }
        }

        public void UnRegisterConfigChangeHandler(ClusterConfigChangeDelegate handler)
        {
            if (helper.StoreInProc)
            {
                this.helper.ServerWrapper.UnRegisterConfigChangeHandler(token, handler);
            }
        }        
    }
}
