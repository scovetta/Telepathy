using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{

    public class VersionControl
    {
        Version _version = null;
 
        public VersionControl(Version version)
        {
            _version = version;
        }

        public Version Version
        {
            get { return _version; }
        }

        public bool IsV2
        {
            get
            {
                return _version.Major == 2;
            }
        }

        public bool IsV3
        {
            get
            {
                return _version.Major == 3;
            }
        }

        public bool IsV4
        {
            get
            {
                return _version.Major == 4;
            }
        }

        public bool IsV5
        {
            get
            {
                return _version.Major == 5;
            }
        }

        public bool IsNewerThanV5RTM
        {
            get
            {
                return _version > new Version(5, 0, 5826, 0);
            }
        }

        public bool IsOlderThanV4SP5QFE
        {
            get
            {
                return _version < new Version(4, 5, 5158, 0);
            }
        }

        /// <summary>
        /// Is the version older than v3 sp1 (not including v3 Sp1)
        /// </summary>
        public bool IsOlderThanV3SP1
        {
            get
            {
                if (_version.Major < 3)
                {
                    return true;
                }
                if (_version.Major == 3)
                {
                    if (_version.Minor < 1)
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
        }
        
        public static readonly Version V2 = new Version(2, 0);
        public static readonly Version V3 = new Version(3, 0);
        public static readonly Version V3SP1 = new Version(3, 1);
        public static readonly Version V3SP2 = new Version(3, 2);
        public static readonly Version V3SP3 = new Version(3, 3);
        public static readonly Version V3SP4 = new Version(3, 4);
        public static readonly Version V4 = new Version(4, 0);
        public static readonly Version V4SP1 = new Version(4, 1);
        public static readonly Version V4SP3 = new Version(4, 3);
        public static readonly Version V4SP4 = new Version(4, 4);
        public static readonly Version V4SP5 = new Version(4, 5);
        public static readonly Version V4SP6 = new Version(4, 6);
        public static readonly Version V5 = new Version(5, 0);
        public static readonly Version V5SP2 = new Version(5, 2);
    }


    /// <summary>
    /// Maps property IDs to the product version where the property was first introduced.  Currently,
    /// only the subset of properties exposed to the public scheduler API are mapped.
    /// </summary>
    public static class PropertyVersioning
    {
        public class Info
        {
            Version _addedVersion;   // The product version when the property was added
            Version _compatVersion;  // The earliest version with which the client is compatible

            // The converter used to enforce compatibility between a later client and an earlier server
            PropertyConverter _compatConverter;

            VersionMismatchConverter _defaultConverter = null;

            internal Info(Version addedVersion)
            {
                _addedVersion = addedVersion;
                _compatVersion = addedVersion;
                _compatConverter = null;
            }

            //To be used for converters of properties, that need to be run for all server versions
            //older than the added version
            internal Info(Version addedVersion, PropertyId _propId)
                : this(addedVersion)
            {                                
                _defaultConverter = new VersionMismatchConverter(_propId, this);
            }


            internal Info(Version addedVersion, Version compatVersion, PropertyConverter converter, PropertyId _propId)
            {
                _addedVersion = addedVersion;
                _compatVersion = compatVersion ?? addedVersion;
                _compatConverter = converter;
                _defaultConverter = new VersionMismatchConverter(_propId, this);
            }

            public Version AddedVersion  { get { return _addedVersion; } }
            public Version EarliestCompatibleVersion { get { return _compatVersion; } }
            public PropertyConverter Converter { get { return _compatConverter; } }

            internal VersionMismatchConverter DefaultConverter { get { return _defaultConverter; } }

        }

        static readonly Info DefaultInfo = new Info(VersionControl.V2);

        static Dictionary<ObjectType, Dictionary<PropertyId, Info>> _propertyVersionMap;

        static PropertyVersioning()
        {
            _propertyVersionMap = new Dictionary<ObjectType, Dictionary<PropertyId, Info>>();

            _propertyVersionMap.Add(ObjectType.Job, GetJobPropertyVersionMap());
            _propertyVersionMap.Add(ObjectType.Task, GetTaskPropertyVersionMap());
        }


        static void AddInfoToPropertyVersionMap(
            Dictionary<PropertyId, Info> objectPropertyVersionMap,
            Version addedVersion,
            PropertyId propId)
        {
            objectPropertyVersionMap.Add(propId, new Info(addedVersion, propId));
        }

        static void AddInfoToPropertyVersionMap(
            Dictionary<PropertyId, Info> objectPropertyVersionMap,
            Version addedVersion,
            Version lastCompatibleVersion,
            PropertyConverter converter,
            PropertyId propId)
        {
            objectPropertyVersionMap.Add(propId, new Info(addedVersion, lastCompatibleVersion, converter, propId));
        }

        static Dictionary<PropertyId, Info> GetJobPropertyVersionMap()
        {
            Dictionary<PropertyId, Info> jobPropertyVersionMap = new Dictionary<PropertyId, Info>();

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.Progress);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.ProgressMessage);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.TargetResourceCount);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.NotifyOnStart);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.NotifyOnCompletion);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.HoldUntil);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, JobPropertyIds.ExcludedNodes);

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, VersionControl.V2, new ExpandedPriorityConverter(), JobPropertyIds.ExpandedPriority);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3, VersionControl.V2, new OwnerSidConverter(), JobPropertyIds.OwnerSID);
            
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3SP1, JobPropertyIds.EmailAddress);

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V3SP3, JobPropertyIds.RuntimeType);            

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.SingleNode);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.NodeGroupOp);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.EstimatedProcessMemory);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.JobValidExitCodes);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.ParentJobIds);
            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4, JobPropertyIds.FailDependentTasks);

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4SP1, JobPropertyIds.PlannedCoreCount);

            AddInfoToPropertyVersionMap(jobPropertyVersionMap, VersionControl.V4SP3, JobPropertyIds.TaskExecutionFailureRetryLimit);

            return jobPropertyVersionMap;
        }

        static Dictionary<PropertyId, Info> GetTaskPropertyVersionMap()
        {
            Dictionary<PropertyId, Info> taskPropertyVersionMap = new Dictionary<PropertyId, Info>();

            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V3, VersionControl.V2, new TaskTypeHelper.Converter_TaskType_IsParametric(), TaskPropertyIds.Type);            

            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V3, TaskPropertyIds.AllocatedCoreIds);
            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V3, TaskPropertyIds.IsServiceConcluded);
            
            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V3SP1, TaskPropertyIds.FailJobOnFailure);
            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V3SP1, TaskPropertyIds.FailJobOnFailureCount);
                        
            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V4, TaskPropertyIds.TaskValidExitCodes);

            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V4SP1, TaskPropertyIds.ExitIfPossible);
            AddInfoToPropertyVersionMap(taskPropertyVersionMap, VersionControl.V4SP3, TaskPropertyIds.ExecutionFailureRetryCount);

            AddInfoToPropertyVersionMap(taskPropertyVersionMap, new Version(4, 5, 5127), TaskPropertyIds.RequestedNodeGroup); // this is added in QFE KB3189996 which is post build 5126


            return taskPropertyVersionMap;
        }

        /// <summary>
        /// Returns the version information associated with a given property.  If the information is not available, will return
        /// a default value corresponding to Version 2.0 of the product.
        /// </summary>
        /// <param name="obType"></param>
        /// <param name="propId"></param>
        /// <returns></returns>
        public static Info GetInfo(ObjectType obType, PropertyId propId)
        {
            Dictionary<PropertyId, Info> objectPropertyVersionMap;
            if (!_propertyVersionMap.TryGetValue(obType, out objectPropertyVersionMap))
            {
                return DefaultInfo;
            }

            Info info;
            if (!objectPropertyVersionMap.TryGetValue(propId, out info))
            {
                return DefaultInfo;
            }

            Debug.Assert(info != null);
            return info;
        }


        internal static IEnumerable<PropertyConverter> GetBackCompatPropConverters(Version serverVersion)
        {
            Dictionary<PropertyId, PropertyConverter> converters = new Dictionary<PropertyId, PropertyConverter>();

            foreach (KeyValuePair<ObjectType, Dictionary<PropertyId, Info>> objectPair in _propertyVersionMap)
            {
                foreach (KeyValuePair<PropertyId, Info> propIdPair in objectPair.Value)
                {
                    //the server version is older than when this property was added
                    if (serverVersion < propIdPair.Value.AddedVersion)
                    {
                        //if the server version is between the earliest compatible and added version
                        //and there is a converter we use that converter
                        if (serverVersion >= propIdPair.Value.EarliestCompatibleVersion)
                        {
                            PropertyConverter converter = propIdPair.Value.Converter;
                            if (converter != null)
                            {
                                Debug.Assert(!converters.ContainsKey(propIdPair.Key));
                                converters[propIdPair.Key] = converter;
                                continue;
                            }
                        }

                        //if the server version is older than the earliest compatible version or it does not have a converter,
                        // use the default converter

                        if (propIdPair.Value.DefaultConverter != null)
                        {
                            propIdPair.Value.DefaultConverter.SetServerVersion(serverVersion);
                            converters[propIdPair.Key] = propIdPair.Value.DefaultConverter;
                        }
                        
                    }
                }
            }

            return converters.Values;
        }
    }
}
