using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public static class TaskTypeHelper
    {
        #region Definitions for parametric tasks

        /// <summary>
        /// Describes the possible criteria which may be associated with parametric tasks
        /// </summary>
        public enum ParametricTaskCriteria
        {
            // Any parametric tasks will meet this criterion
            All = 0,

            // Parametric tasks with a dynamic number of instance meet this criterion
            Dynamic = 1,

            // Parametric tasks whose Start, End, and Increment values may be adjusted by the user will meet this criterion
            UserAdjustable = 2,
        }
        
        // The maximum number of instances in a parametric task
        public const int ParametricRecordLimit = 1000000;

        #endregion


        #region Estimate the number of instances of a parametric task

        public static int GetParametricCount(int start, int end, int inc)
        {
            if (inc <= 0 || end < start)
            {
                return 0;
            }

            return (end - start) / inc + 1;
        }

        public static int GetInstanceValueForId(int start, int inc, int instanceId)
        {
            return start + (instanceId - 1) * inc;
        }

        #endregion

        #region Parametric expansion helpers

        enum ParametricParseState
        {
            // The state machine starts here, and will remain in this state until it encounters a '^' or '*'.  
            // Whenever the machine encounters any character other than '^' or '*', or when it encounters a
            // '*' preceeded by '^', it will transition into this state.
            Default,

            // The state machine will transition into this state whenever it encounters a '*', unless the '*'
            // is preceeded by a '^'.  The state machine will remain in this state as long as the '*' character
            // is read.  While in this state, it will increment the length of the substitution string.
            Substitution,

            // The machine will enter this state when it encounters a '^'.  It will transition back into the
            // Default state after reading the next character.  '^' is treated literally unless it precedes 
            // another '^' or '*', in which case it is treated as an escape character.
            Escape,
        };
       
        public static string PerformParametricExpansion(string parametricString, string subValue)
        {
            const char SubChar = '*';
            const char EscapeChar = '^';
                
            if (string.IsNullOrEmpty(subValue))
            {
                throw new ArgumentNullException("subValue");
            }
            if (string.IsNullOrEmpty(parametricString))
            {
                return parametricString;
            }

            ParametricParseState parseState = ParametricParseState.Default;

            StringBuilder bldr = new StringBuilder();
            int subTokenLength = 0;
            foreach (char c in parametricString)
            {
                switch (parseState)
                {
                    case ParametricParseState.Default:
                        if (c == SubChar)
                        {
                            subTokenLength = 1;
                            parseState = ParametricParseState.Substitution;                            
                        }
                        else if (c == EscapeChar)
                        {
                            parseState = ParametricParseState.Escape;
                        }
                        else
                        {
                            bldr.Append(c);
                            parseState = ParametricParseState.Default;
                        }
                        break;

                    case ParametricParseState.Substitution:
                        if (c == SubChar)
                        {
                            subTokenLength++;
                            parseState = ParametricParseState.Substitution;
                        }
                        else if (c == EscapeChar)
                        {
                            AppendParametricSub(bldr, subTokenLength, subValue);
                            parseState = ParametricParseState.Escape;
                        }
                        else
                        {
                            AppendParametricSub(bldr, subTokenLength, subValue);
                            bldr.Append(c);
                            parseState = ParametricParseState.Default;
                        }
                        break;

                    // The sequence '^^' translates as '^', and '^*' translates as '*'.  All other sequences are treated literally.
                    case ParametricParseState.Escape:
                        if (c == SubChar)
                        {
                            bldr.Append(SubChar);
                            parseState = ParametricParseState.Default;
                        }
                        else if (c == EscapeChar)
                        {                            
                            bldr.Append(EscapeChar);
                            parseState = ParametricParseState.Default;
                        }
                        else
                        {
                            bldr.Append(EscapeChar);
                            bldr.Append(c);
                            parseState = ParametricParseState.Default;
                        }
                        break;

                    default:
                        throw new InvalidProgramException("Unknown parsing state: " + parseState);
                }
            }

            // If at the end, we end up in the non-default state, perform additional processing
            switch (parseState)
            {
                case ParametricParseState.Substitution:
                    AppendParametricSub(bldr, subTokenLength, subValue);
                    break;
            
                case ParametricParseState.Escape:
                    bldr.Append(EscapeChar);
                    break;
            }

            return bldr.ToString();
        }

        static void AppendParametricSub(StringBuilder bldr, int subTokenLength, string subValue)
        {
            for (int i = subValue.Length; i < subTokenLength; i++)
            {
                bldr.Append('0');
            }
            bldr.Append(subValue);
        }

        #endregion


        #region Methods to describing behaviors associated with the different types of tasks

        /// <summary>
        /// Check whether the type of a task corresponds to  a parametric tasks
        /// </summary>
        public static bool IsParametric(TaskType taskType)
        {
            return IsParametric(taskType, ParametricTaskCriteria.All);
        }

        /// <summary>
        /// Check whether the type of task corresponds to a parametric task that meets
        /// additional criteria.
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <param name="criteria">The criteria.  See the ParametricCretiria enumeration</param>
        /// <returns></returns>
        public static bool IsParametric(TaskType taskType, ParametricTaskCriteria criteria)
        {
            return GetDescriptor(taskType).IsParametric(criteria);
        }

        /// <summary>
        /// Returns true if the resource requirement for a given task type is fixed (i.e. not adjustable by the user).
        /// </summary>
        /// <param name="type">The task type</param>
        /// <returns>True if the task has a fixed resource requirement</returns>
        public static bool IsFixedResource(TaskType taskType)
        {
            return GetDescriptor(taskType).IsFixedResource();
        }
        
        /// <summary>
        /// Returns the internationalized display name for each type of task
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <returns>The localized task type name</returns>
        public static string GetDisplayName(TaskType taskType)
        {
            return GetDescriptor(taskType).GetDisplayName();
        }

        /// <summary>
        /// Get an enumeration of all unmodifiable properties for a given task type,
        /// as well as their fixed value.
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <returns>The enumeration of all fixed properties, or null if there no such properties.
        public static StoreProperty[] GetRestrictedProperties(TaskType taskType)
        {
            StoreProperty[] props = GetDescriptor(taskType).GetRestrictedProperties();
            if (props == null)
            {
                return new StoreProperty[] { };
            }
            
            // Make a copy of the fixed properties, so that the originals could not be overwritten
            StoreProperty[] propCopies = new StoreProperty[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                propCopies[i] = new StoreProperty(props[i].Id, props[i].Value);
            }
            return propCopies;
        }

        /// <summary>
        /// Returns true if tasks of may be requeued when they fail or are preempted.
        /// If this is false for the task's type, then the re-runnable flag on an individual
        /// task will be ignored.
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <returns>True iff tasks of this type may be requeued on task failure.</returns>
        public static bool MayRequeue(TaskType taskType)
        {
            return GetDescriptor(taskType).MayRequeue();
        }

        /// <summary>
        /// Returns true if tasks of the given type can be added to a job after it has been submitted
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <returns>True iff tasks of this type may be requeued on task failure.</returns>
        public static bool MayAddAfterJobSubmission(TaskType taskType)
        {
            return GetDescriptor(taskType).MayAddAfterJobSubmission();
        }

        /// <summary>
        /// Returns the maximum run time allowed by the server for a given task type.
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <param name="store">The scheduler store server</param>
        /// <returns>The timeout, or null, if timeout is unspecified</returns>
        public static int? GetRuntimeLimit(TaskType taskType, ISchedulerStore store)
        {
            // Don't retrieve the configuration settins if there is no config key for the task type's runtime limit
            if (GetDescriptor(taskType).GetRunTimeLimit_ConfigKey() == null)
            {
                return null;
            }
            return GetRuntimeLimit(taskType, store.OpenStoreManager().GetConfigurationSettings());
        }

        /// <summary>
        /// Returns the maximum run time allowed by the server for a given task type.
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <param name="configSettings">The configuration settings on the server</param>
        /// <returns>The timeout, or null, if timeout is unspecified</returns>
        public static int? GetRuntimeLimit(TaskType taskType, Dictionary<string, string> configSettings)
        {
            string configKey = GetDescriptor(taskType).GetRunTimeLimit_ConfigKey();
            if (configKey == null)
            {
                return null;
            }

            string timeoutStr;
            if (configSettings.TryGetValue(configKey, out timeoutStr))
            {
                return int.Parse(timeoutStr);
            }
            return null;
        }

        /// <summary>
        /// Check if tasks of this type can be put in a task group
        /// </summary>
        /// <param name="taskType">The task type</param>
        /// <returns>True if the task can be put in a task group</returns>
        public static bool IsGroupable(TaskType taskType)
        {
            return GetDescriptor(taskType).IsGroupable();
        }

        public static TaskType GetTypeAndVerifyProps(IEnumerable<StoreProperty> userProps)
        {
            if (userProps == null)
            {
                throw new ArgumentNullException("userProps");
            }
            Dictionary<PropertyId, StoreProperty> propMap = new Dictionary<PropertyId, StoreProperty>();
            foreach (StoreProperty prop in userProps)
            {
                propMap[prop.Id] = prop;
            }
            return GetTypeAndVerifyProps(propMap);
        }

        /// <summary>
        /// Verifies that a set of properties are applicable to a given task type
        /// Throws an exception if this is not the case.
        /// <returns>The task type specified in the property dictionary</returns>
        /// </summary>
        public static TaskType GetTypeAndVerifyProps(IDictionary<PropertyId, StoreProperty> userProps)
        {
            if (userProps == null)
            {
                throw new ArgumentNullException("userProps");
            }

            TaskType type = TaskType.Basic;

            StoreProperty propType = null;
            if (userProps.TryGetValue(TaskPropertyIds.Type, out propType))
            {
                type = (TaskType)propType.Value;
            }

            StoreProperty propIsParametric = null;
            if (userProps.TryGetValue(ParametricPropId, out propIsParametric))
            {
                bool isParametric = (bool)(propIsParametric.Value ?? false);
                TaskType typeFromParametric = (isParametric ? TaskType.ParametricSweep : TaskType.Basic);

                if (propType == null)
                {
                    type = typeFromParametric;
                }
                else if (type != typeFromParametric)
                {
                    throw new SchedulerException(ErrorCode.Operation_TaskTypeAndIsParametricIncompatible,
                        ErrorCode.MakeErrorParams(type.ToString(), isParametric.ToString()));
                }
            }

            foreach (StoreProperty fixedProp in TaskTypeHelper.GetRestrictedProperties(type))
            {
                StoreProperty userProp;
                if (userProps.TryGetValue(fixedProp.Id, out userProp))
                {                    
                    if (PropertyUtil.CompareProperties(userProp, fixedProp) != 0)
                    {
                        throw new SchedulerException(ErrorCode.Operation_InvalidPropForTaskType,
                            ErrorCode.MakeErrorParams(fixedProp.PropName, type.ToString()));
                    }
                }
            }

            return type;
        }

        #endregion


        // Disable warnings about obsolete IsParametric property
#pragma warning disable 0612,0618
        static PropertyId ParametricPropId = TaskPropertyIds.IsParametric;
#pragma warning restore 0612,0618

        #region  Internal descriptors defining the behavior of various task types

        static StoreProperty[] _basicRestrictedProps = new StoreProperty[]
        {
            new StoreProperty(TaskPropertyIds.StartValue, 0),
            new StoreProperty(TaskPropertyIds.EndValue, 0),
            new StoreProperty(TaskPropertyIds.IncrementValue, 1),
            new StoreProperty(TaskPropertyIds.FailJobOnFailureCount,1),
        };

        static StoreProperty[] _nonAdjustibleStartEnvIncValues = new StoreProperty[]
        {
            new StoreProperty(TaskPropertyIds.StartValue, 1),
            new StoreProperty(TaskPropertyIds.EndValue, 1),
            new StoreProperty(TaskPropertyIds.IncrementValue, 1),
        };


        static StoreProperty[] _fixedResourceProps = new StoreProperty[]
        {
            new StoreProperty(TaskPropertyIds.MinCores, 1),
            new StoreProperty(TaskPropertyIds.MaxCores, 1),
            new StoreProperty(TaskPropertyIds.MinSockets, 1),
            new StoreProperty(TaskPropertyIds.MaxSockets, 1),
            new StoreProperty(TaskPropertyIds.MinNodes, 1),
            new StoreProperty(TaskPropertyIds.MaxNodes, 1),
        };

        interface ITaskTypeDescriptor
        {
            TaskType GetTaskType();
            bool IsParametric(ParametricTaskCriteria criteria);
            bool IsFixedResource();
            string GetDisplayName();
            StoreProperty[] GetRestrictedProperties();
            bool MayRequeue();
            bool MayAddAfterJobSubmission();
            string GetRunTimeLimit_ConfigKey();
            bool IsGroupable();
        }


        abstract class BasicParametricCommonDescriptor : ITaskTypeDescriptor
        {
            public bool IsFixedResource() { return false; }            
            public bool MayRequeue() { return true; }
            public bool MayAddAfterJobSubmission() { return true; }
            public string GetRunTimeLimit_ConfigKey() { return null; }
            public bool IsGroupable() { return true; }

            public abstract TaskType GetTaskType();
            public abstract bool IsParametric(ParametricTaskCriteria criteria);
            public abstract string GetDisplayName();
            public abstract StoreProperty[] GetRestrictedProperties();
        }

        class BasicTaskTypeDescriptor : BasicParametricCommonDescriptor
        {
            public override TaskType GetTaskType() { return TaskType.Basic; }
            public override bool IsParametric(ParametricTaskCriteria criteria) { return false; }
            public override string GetDisplayName() { return SR.TaskTypeName_Basic; }
            public override StoreProperty[] GetRestrictedProperties() { return _basicRestrictedProps; }
        }

        class ParametricSweepTaskTypeDescriptor : BasicParametricCommonDescriptor
        {
            public override TaskType GetTaskType() { return TaskType.ParametricSweep; }
            public override bool IsParametric(ParametricTaskCriteria criteria) { return criteria != ParametricTaskCriteria.Dynamic; }
            public override string GetDisplayName() { return SR.TaskTypeName_ParametricSweep; }
            public override StoreProperty[] GetRestrictedProperties() { return null; }
        }

        abstract class PrepReleaseCommonDescriptor : ITaskTypeDescriptor
        {
            static protected StoreProperty[] _fixedPrepReleaseProps = new StoreProperty[]
            {
                new StoreProperty(TaskPropertyIds.GroupId, 0),
                new StoreProperty(TaskPropertyIds.DependsOn, null),

                new StoreProperty(TaskPropertyIds.IsRerunnable,  null),
                new StoreProperty(TaskPropertyIds.IsExclusive,   null),
                new StoreProperty(TaskPropertyIds.RequiredNodes, null),
            };

            protected PrepReleaseCommonDescriptor()
            {
                List<StoreProperty> fixedPropList = new List<StoreProperty>();
                fixedPropList.AddRange(_nonAdjustibleStartEnvIncValues);
                fixedPropList.AddRange(_fixedResourceProps);
                fixedPropList.AddRange(_fixedPrepReleaseProps);
                _fixedProps = fixedPropList.ToArray();
            }

            StoreProperty[] _fixedProps;

            public bool IsParametric(ParametricTaskCriteria criteria) { return criteria != ParametricTaskCriteria.UserAdjustable; }
            public bool IsFixedResource() { return true; }
            public StoreProperty[] GetRestrictedProperties() { return _fixedProps; }
            public bool MayRequeue()  { return false; }
            public bool MayAddAfterJobSubmission() { return false; }
            public bool IsGroupable() { return false; }

            public abstract TaskType GetTaskType();
            public abstract string GetDisplayName();
            public abstract string GetRunTimeLimit_ConfigKey();

        }

        class NodePrepTaskTypeDescriptor : PrepReleaseCommonDescriptor
        {
            public override TaskType GetTaskType() { return TaskType.NodePrep; }
            public override string GetDisplayName() { return SR.TaskTypeName_NodePrep; }
            public override string GetRunTimeLimit_ConfigKey() { return null; }
        }

        class NodeReleaseTaskTypeDescriptor : PrepReleaseCommonDescriptor
        {
            public override TaskType GetTaskType() { return TaskType.NodeRelease; }
            public override string GetDisplayName() { return SR.TaskTypeName_NodeRelease; }
            public override string GetRunTimeLimit_ConfigKey() { return "NodeReleaseTaskTimeout"; }
        }

        class ServiceTaskTypeDescriptor : ITaskTypeDescriptor
        {
            StoreProperty[] _fixedProps;

            internal ServiceTaskTypeDescriptor()
            {
                List<StoreProperty> fixedPropList = new List<StoreProperty>();                
                fixedPropList.AddRange(_nonAdjustibleStartEnvIncValues);
                fixedPropList.Add(new StoreProperty(TaskPropertyIds.DependsOn, null));
                fixedPropList.Add(new StoreProperty(TaskPropertyIds.IsRerunnable, null));
                _fixedProps = fixedPropList.ToArray();
            }

            public TaskType GetTaskType() { return TaskType.Service; }
            public bool IsParametric(ParametricTaskCriteria criteria) { return criteria != ParametricTaskCriteria.UserAdjustable; }
            public bool IsFixedResource() { return false; }
            public string GetDisplayName() { return SR.TaskTypeName_Service; }
            public StoreProperty[] GetRestrictedProperties() { return _fixedProps; }
            public bool MayRequeue() { return false; }
            public bool MayAddAfterJobSubmission() { return false; }
            public string GetRunTimeLimit_ConfigKey() { return null; }
            public bool IsGroupable() { return true; }
        }


        #endregion


        #region Static members and constructor

        static ITaskTypeDescriptor[] _allTypeDescriptors = new ITaskTypeDescriptor[]
        {
            new BasicTaskTypeDescriptor(),
            new ParametricSweepTaskTypeDescriptor(),            
            new NodePrepTaskTypeDescriptor(),
            new NodeReleaseTaskTypeDescriptor(),
            new ServiceTaskTypeDescriptor(),
        };

        static ITaskTypeDescriptor GetDescriptor(TaskType type)
        {
            int index = (int)type;
            if (index < 0 || index >= _allTypeDescriptors.Length)
            {
                throw new ArgumentException(String.Format("Error: Task type {0} maps to an invalid index {1}", type, index));
            }
            return _allTypeDescriptors[(int)type];
        }

        static TaskTypeHelper()
        {
            // Verify that we have type descriptors for all task types
            foreach (TaskType taskType in Enum.GetValues(typeof(TaskType)))
            {
                int index = (int)taskType;
                if (index < 0 || index >= _allTypeDescriptors.Length)
                {
                    throw new InvalidProgramException(String.Format("Store API Error: Descriptor for task type {0} not found", taskType));
                }
                if (_allTypeDescriptors[index].GetTaskType() != taskType)
                {
                    throw new InvalidProgramException(String.Format("Store API Error: Task type descriptor at index {0} does not match task type {1}, reports {2} instead",
                                                                    index, taskType, _allTypeDescriptors[index].GetTaskType()));
                }                
            }
        }

        #endregion


        #region Helper for converting between the IsParametric and TaskType properties

        /// <summary>
        /// Replace TaskType with the IsParametric property, for V3 to V2 compatibility.
        /// </summmary>
        internal class Converter_TaskType_IsParametric : XMLEnabledPropertyConverter
        {
            public override PropertyId GetPropId()
            {
                return TaskPropertyIds.Type;
            }

            public override PropertyId[] GetAllServerReplacementPropIds()
            {
                return new PropertyId[] { ParametricPropId };
            }

            public override StoreProperty DeconvertGetProps(StoreProperty[] props)
            {
                Debug.Assert(props != null && props.Length == 1);
                Debug.Assert(props[0].Id == ParametricPropId);
            
                bool isParametric = (bool)(props[0].Value ?? false);
                TaskType type = isParametric ? TaskType.ParametricSweep : TaskType.Basic;
                return new StoreProperty(TaskPropertyIds.Type, type);
            }

            public override StoreProperty[] ConvertSetProp(StoreProperty prop)
            {
                Debug.Assert(prop.Id == TaskPropertyIds.Type);
                TaskType type = (TaskType)(prop.Value ?? TaskType.Basic);
                if (type != TaskType.Basic && type != TaskType.ParametricSweep)
                {
                    // This method should only be called when a V3 client communicates with a V2 server.
                    // Only Basic and ParametricSweep task types are supported on V2 servers.
                    throw new SchedulerException(ErrorCode.Operation_TaskTypeNotSupportedOnServer, type.ToString());
                }
                return new StoreProperty[] { new StoreProperty(ParametricPropId, IsParametric(type)) };
            }

            public override StoreProperty[] ConvertXMLWriteProp(StoreProperty prop)
            {
                Debug.Assert(prop.Id == TaskPropertyIds.Type);
                TaskType type = (TaskType)(prop.Value ?? TaskType.Basic);
                if (type != TaskType.ParametricSweep)
                {
                    // Don't output the IsParametric property unless this is a parametric task.  When a V2 client
                    // attempts to read the task XML of any other task type, it should treat it as a basic task.
                    return null;
                }
                return new StoreProperty[] { new StoreProperty(ParametricPropId, true) };
            }

            public override bool NeedsToRemoveReplacementPropsAfterXMLDeconvert()
            {
                // Remove the IsParametric property after converting it to TaskType
                return true;
            }

            public override FilterProperty[] ConvertFilter(FilterProperty filter)
            {
                Debug.Assert(filter.Property.Id == TaskPropertyIds.Type);
                
                bool wantParametric = PropertyUtil.TestPropAgainstFilter(filter, new StoreProperty(TaskPropertyIds.Type, TaskType.ParametricSweep));
                bool wantBasic = PropertyUtil.TestPropAgainstFilter(filter, new StoreProperty(TaskPropertyIds.Type, TaskType.Basic));

                if (wantBasic && wantParametric)
                {
                    // Do not filter at all on the IsParametric property, since both parametric-and non-parametric tasks are included
                    return null;
                }

                FilterProperty newFilter = new FilterProperty(FilterOperator.Equal, ParametricPropId, wantParametric);
                return new FilterProperty[] { newFilter };
            }
        }

        #endregion
    }        
}
