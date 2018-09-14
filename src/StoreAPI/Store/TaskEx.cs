using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    class TaskEx : StoreObjectBase, IClusterTask
    {
        private Int32                       _taskid;
        private Int32                       _parentjobid = 0;
        
        public TaskEx(Int32 parentJobId, Int32 taskId, SchedulerStoreSvc helper)
            : base(helper, ObjectType.Task)
        {
            _parentjobid = parentJobId;
            _taskid = taskId;
        }

        public override int Id
        {
            get { return _taskid; }
        }

        public int TaskId 
        {
            get { return _taskid; }
        }

        public override PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public override PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _helper.GetPropsFromServer(ObjectType.Task, _taskid, propertyIds);
        }

        public override PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Task.PropertyIdsFromNames(propertyNames));
        }

        public override void SetProps(params StoreProperty[] taskProperties)
        {
            _helper.SetPropsOnServer(ObjectType.Task, _taskid, taskProperties);
        }

        public IClusterTask Clone(StoreProperty[] taskProperties)
        {
            Int32 taskidNew = 0;
            
            _helper.ServerWrapper.Task_CloneTask(_taskid, ref taskidNew, taskProperties);
            
            return new TaskEx(_parentjobid, taskidNew, _helper);
        }


        private TaskState EndAsyncRequest(IAsyncResult result)
        {
            AsyncResult _result = (AsyncResult)result;

            // Wait for it to finish, note that this may
            // be already signalled.

            WaitHandle handle = _result.AsyncWaitHandle;
            if (handle != null)
            {
                handle.WaitOne();
            }

            TaskState state = (TaskState)_result.ResultState;

            _helper.CloseAsyncResult(_result);

            return state;
        }

        public IAsyncResult BeginSubmit(AsyncCallback callback, object param)
        {
            // Only if the job is currently queued or running 
            // will we submit the task.  Otherwise all tasks 
            // for a job will be moved to submitted automatically
            // by the store when the job is submitted.

            // This is kind of expensive to ask for the current
            // state of the job.

            JobState jobState;

            _helper.ServerWrapper.Job_GetJobState(_parentjobid, out jobState);
            
            if (jobState != JobState.Queued && jobState != JobState.Running)
            {
                AsyncResult now = new AsyncResult(ObjectType.Task, _taskid, 0, callback, param);


                //now.CompletedSynchronously = true;
                now.ResultState = (int)TaskState.Configuring;
                ((ManualResetEvent)now.AsyncWaitHandle).Set();
                now.Invoke();

                return now;
            }

            AsyncResult async = _helper.RegisterForTaskStateChange(
                    _taskid,
                    (int)(TaskState.Queued | TaskState.Failed | TaskState.Running | TaskState.Finished | TaskState.Canceled),
                    callback,
                    param
            );

            _helper.ServerWrapper.Task_SubmitTask(_parentjobid, _taskid);

            return async;
        }

        public TaskState EndSubmit(IAsyncResult result)
        {
            return EndAsyncRequest(result);
        }

        public void SubmitTask()
        {
            IAsyncResult async = BeginSubmit(null, null);
            
            TaskState state = EndSubmit(async);

            if (state == TaskState.Failed)
            {
                // Something went wrong.  Throw an error.

                PropertyRow props = GetProps(TaskPropertyIds.ErrorCode,
                    TaskPropertyIds.ErrorParams,TaskPropertyIds.PreviousState);

                int errorCode = (int)props[0].Value;
                string errorParams = props[1].Value as string;                
                TaskState prevState = PropertyUtil.GetValueFromProp(props[2],TaskPropertyIds.PreviousState,TaskState.Configuring);                     

                
                //if the task has not failed after it started running throw an exception
                if(prevState != TaskState.Running)
                {
                    throw new SchedulerException(errorCode, errorParams);
                }
            }
        }

        public void Configure()
        {
            _helper.ServerWrapper.Task_ConfigTask(_parentjobid, _taskid);
        }

        public void ServiceConclude(bool fCancelSubTasks)
        {
            _helper.ServerWrapper.Task_ConcludeServiceTask(_taskid, fCancelSubTasks);
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            _helper.ServerWrapper.Task_SetEnvironmentVariable(_taskid, name, value);
        }

        public Dictionary<string, string> GetEnvironmentVariables()
        {
            Dictionary<string, string> vars;
            
            _helper.ServerWrapper.Task_GetEnvironmentVariables(_taskid, out vars);
            
            return vars;
        }


        static internal PropertyId[] _exportV3XmlPids = 
        {
            TaskPropertyIds.Id,            
            TaskPropertyIds.ParentJobId,
            TaskPropertyIds.RequiredNodes,
            TaskPropertyIds.RequestedNodeGroup,
            TaskPropertyIds.RuntimeSeconds,
            TaskPropertyIds.State,
            TaskPropertyIds.UnitType,
            TaskPropertyIds.WorkDirectory,
            TaskPropertyIds.JobTaskId,
            TaskPropertyIds.CommandLine,
            TaskPropertyIds.DependsOn,
            TaskPropertyIds.IsRerunnable,
            TaskPropertyIds.StdOutFilePath,
            TaskPropertyIds.StdInFilePath,
            TaskPropertyIds.StdErrFilePath,            
            TaskPropertyIds.RequeueCount,            
            TaskPropertyIds.PendingReason,            
            TaskPropertyIds.StartValue,
            TaskPropertyIds.EndValue,
            TaskPropertyIds.IncrementValue,            
            TaskPropertyIds.GroupId,
            TaskPropertyIds.SubmitTime,
            TaskPropertyIds.StartTime,
            TaskPropertyIds.CreateTime,            
            TaskPropertyIds.Name,
            TaskPropertyIds.IsExclusive,            
            TaskPropertyIds.MinCores,
            TaskPropertyIds.MaxCores,
            TaskPropertyIds.MinSockets,
            TaskPropertyIds.MaxSockets,
            TaskPropertyIds.MinNodes,
            TaskPropertyIds.MaxNodes,
            TaskPropertyIds.AutoRequeueCount,
            TaskPropertyIds.Type,
            TaskPropertyIds.FailJobOnFailure,
            TaskPropertyIds.FailJobOnFailureCount,
            TaskPropertyIds.TaskValidExitCodes
        };

        protected override PropertyId[] GetExportV3Pids()
        {
            return _exportV3XmlPids;
        }

        void _ExportToXml(XmlWriter writer, XmlExportOptions flags)
        {
            bool isV2Server = _helper.ServerVersion.IsV2;
            StoreProperty[] propsToExport = null;

            propsToExport = GetPropsToExport(isV2Server);


            TaskPropertyBag taskBag = new TaskPropertyBag();
            
            taskBag.SetProperties(propsToExport);

            Dictionary<string, string> vars;
            _helper.ServerWrapper.Task_GetEnvironmentVariables(_taskid, out vars);
            taskBag.SetEnvironmentVariables(vars);
            
            taskBag.WriteXml(writer, flags);
        }

        internal static void _ExportEnvToXml(XmlWriter writer, SchedulerStoreSvc helper, int taskId)
        {
            Dictionary<string, string> vars;
            
            helper.ServerWrapper.Task_GetEnvironmentVariables(taskId, out vars);

            if (vars != null && vars.Count > 0)
            {
                writer.WriteStartElement("EnvironmentVariables");
                
                foreach (KeyValuePair<string, string> var in vars)
                {
                    writer.WriteStartElement(XmlNames.Var);

                    writer.WriteElementString(XmlNames.Name, var.Key);
                    writer.WriteElementString(XmlNames.Value, var.Value);

                    writer.WriteEndElement(); //Variable 
                }
                
                writer.WriteEndElement(); // EnvironmentVariables
            }
        }

        public override void PersistToXml(XmlWriter writer, XmlExportOptions flags)
        {
            _ExportToXml(writer, flags);
        }

        public override void RestoreFromXml(XmlReader reader, XmlImportOptions flags)
        {
            TaskPropertyBag taskbag = new TaskPropertyBag();
            
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Task)
                {
                    taskbag.ReadXML(reader);
                    break;
                }
            }
            
            taskbag.UpdateTask(_helper, this);
        }
    }
}
