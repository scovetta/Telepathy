using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    static class PropertyLookup
    {
        internal class PropertyNameMap
        {
            enum PropertyAccessPermissions
            {
                Public,
                Protected                
            };

            internal PropertyNameMap(Type publicType, Type protectedType, PropertyNameMap fallback, ObjectType obtype)
            {
                _publicPropertyIdType = publicType;
                _protectedPropertyIdType = protectedType;
                _fallback = fallback;
                _obtype = obtype;
            }

            Type _publicPropertyIdType;
            Type _protectedPropertyIdType;
            PropertyNameMap _fallback;
            ObjectType _obtype;
            Object _lock = new Object();

            Dictionary<string, PropertyId> _string2propid = null;
            Dictionary<int, PropertyId> _index2propid = null;
            Dictionary<PropertyId, string> _propid2string = null;
            Dictionary<PropertyId, PropertyAccessPermissions> _propIdPermission = null;

            internal StorePropertyDescriptor[] GetPropertyDescriptors(PropertyId[] propIds)
            {
                BuildPropertyMap();

                List<StorePropertyDescriptor> result = new List<StorePropertyDescriptor>();

                foreach (PropertyId pid in propIds)
                {
                    string sysname;

                    if (!_propid2string.TryGetValue(pid, out sysname))
                    {
                        continue;
                    }

                    result.Add(new StorePropertyDescriptor(
                            pid,
                            sysname,
                            _obtype)
                            );

                }

                return result.ToArray();
            }

            internal StorePropertyDescriptor[] GetPropertyDescriptors(PropFlags flagMask)
            {
                BuildPropertyMap();

                List<StorePropertyDescriptor> result = new List<StorePropertyDescriptor>();

                foreach (KeyValuePair<PropertyId, string> item in _propid2string)
                {
                    if ((item.Key.Flags & flagMask) != 0)
                    {
                        string localename = item.Value;
                        string description = item.Value;

                        result.Add(new StorePropertyDescriptor(
                                item.Key,
                                item.Value,
                                _obtype)
                                );
                    }
                }


                return result.ToArray();
            }

            internal void GetPropertyDescriptors(string[] names, List<StorePropertyDescriptor> result)
            {
                BuildPropertyMap();

                for (int i = 0; i < names.Length; i++)
                {
                    PropertyId pid;

                    if (_string2propid.TryGetValue(names[i].ToLower(), out pid))
                    {
                        string sysname = _propid2string[pid];

                        result[i] = new StorePropertyDescriptor(
                                pid,
                                sysname,
                                _obtype
                                );
                    }
                }
            }

            void BuildPropertyMap()
            {
                lock (_lock)
                {
                    if (_string2propid == null)
                    {
                        _string2propid = new Dictionary<string, PropertyId>(70);
                        _index2propid = new Dictionary<int, PropertyId>(70);
                        _propid2string = new Dictionary<PropertyId, string>(70);
                        _propIdPermission = new Dictionary<PropertyId, PropertyAccessPermissions>(70);

                        BuildPropertyMapForType(_publicPropertyIdType, PropertyAccessPermissions.Public);
                        BuildPropertyMapForType(_protectedPropertyIdType, PropertyAccessPermissions.Protected);
                    }
                }
            }

            void BuildPropertyMapForType(Type propertyIdType, PropertyAccessPermissions permissions)
            {
                foreach (PropertyInfo info in propertyIdType.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    PropertyId pid = propertyIdType.InvokeMember(info.Name, BindingFlags.GetProperty, null, null, null) as PropertyId;

                    // All names must be unique
                    _string2propid.Add(info.Name.ToLower(), pid);

                    // Some property Ids are alias of others                        
                    if (!_index2propid.ContainsKey(pid.UniqueId))
                    {
                        _index2propid.Add(pid.UniqueId, pid);
                    }

                    // Only add a pid once.
                    if (!_propid2string.ContainsKey(pid))
                    {
                        _propid2string.Add(pid, info.Name);
                    }

                    _propIdPermission[pid] = permissions;
                }
            }

            internal void ValidatePropertyNames(StoreProperty[] props)
            {
                if (props != null)
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        if (props[i].Id == StorePropertyIds.NA)
                        {
                            if (props[i].PropName != null)
                            {
                                props[i].Id = PropertyIdFromPropName(props[i].PropName);

                                if (props[i].Id == null)
                                {
                                    throw new SchedulerException(string.Format(SR.UnknownProperty, props[i].PropName));
                                }
                            }
                        }
                    }
                }
            }

            internal PropertyId PropertyIdFromPropName(string name)
            {
                BuildPropertyMap();

                PropertyId pidFound = null;

                if (_string2propid.TryGetValue(name.ToLower(), out pidFound))
                {
                    return pidFound;
                }

                if (_fallback != null)
                {
                    return _fallback.PropertyIdFromPropName(name);
                }

                return null;
            }

            internal PropertyId PropertyIdFromPropIndex(int index)
            {
                BuildPropertyMap();

                PropertyId pidFound = null;

                if (_index2propid.TryGetValue(index, out pidFound))
                {
                    return pidFound;
                }

                if (_fallback != null)
                {
                    return _fallback.PropertyIdFromPropIndex(index);
                }

                return null;
            }

            internal string PropertyNameFromPropertyId(PropertyId pid)
            {
                BuildPropertyMap();

                string result = null;

                if (_propid2string.TryGetValue(pid, out result))
                {
                    return result;
                }

                if (_fallback != null)
                {
                    return _fallback.PropertyNameFromPropertyId(pid);
                }

                return null;
            }

            internal PropertyId[] PropertyIdsFromNames(string[] names)
            {
                if (names == null || names.Length == 0)
                {
                    return null;
                }

                PropertyId[] pids = new PropertyId[names.Length];

                for (int i = 0; i < names.Length; i++)
                {
                    pids[i] = PropertyIdFromPropName(names[i]);

                    if (pids[i] == null)
                    {
                        pids[i] = StorePropertyIds.Error;
                    }
                }

                return pids;
            }

            internal ObjectType ObType
            {
                get { return _obtype; }
            }
        }

        internal static PropertyNameMap Store = new PropertyNameMap(typeof(StorePropertyIds), typeof(ProtectedStorePropertyIds), null, ObjectType.Store);
        internal static PropertyNameMap Job = new PropertyNameMap(typeof(JobPropertyIds), typeof(ProtectedJobPropertyIds), null, ObjectType.Job);
        internal static PropertyNameMap Task = new PropertyNameMap(typeof(TaskPropertyIds), typeof(ProtectedTaskPropertyIds), null, ObjectType.Task);
        internal static PropertyNameMap Resource = new PropertyNameMap(typeof(ResourcePropertyIds), typeof(ProtectedResourcePropertyIds), null, ObjectType.Resource);
        internal static PropertyNameMap Profile = new PropertyNameMap(typeof(JobTemplatePropertyIds), typeof(ProtectedJobTemplatePropertyIds), null, ObjectType.JobTemplate);
        internal static PropertyNameMap Node = new PropertyNameMap(typeof(NodePropertyIds), typeof(ProtectedNodePropertyIds), null, ObjectType.Node);
        internal static PropertyNameMap Allocation = new PropertyNameMap(typeof(AllocationProperties), typeof(ProtectedAllocationPropertyIds), null, ObjectType.Allocation);
        internal static PropertyNameMap JobMessage = new PropertyNameMap(typeof(JobMessagePropertyIds), typeof(ProtectedJobMessagePropertyIds), null, ObjectType.JobMessage);
        internal static PropertyNameMap Pool = new PropertyNameMap(typeof(PoolPropertyIds), typeof(ProtectedPoolPropertyIds), null, ObjectType.Pool);

        // List of all property name maps for every object type
        static List<PropertyNameMap> _allMaps = new List<PropertyNameMap>(new PropertyNameMap[]
        {
            Store,
            Job,
            Task,
            Resource,
            Profile,
            Node,
            Allocation,
            JobMessage,
            Pool,
        });

        internal static StorePropertyDescriptor[] GetPropertyDescriptors(ObjectType typeMask, PropertyId[] propIds)
        {
            // Search all known maps.
            // Note that there is no fallback in this search.  It is assumed
            // that for any given property it exists in only one map.

            List<StorePropertyDescriptor> result = new List<StorePropertyDescriptor>();
            foreach (PropertyNameMap map in _allMaps)
            {
                if ((typeMask & map.ObType) != 0)
                {
                    result.AddRange(map.GetPropertyDescriptors(propIds));
                }
            }

            // TODO: Need to add code in the future to support
            //       properties that have been added to the store
            //       at runtime.

            return result.ToArray();
        }

        internal static StorePropertyDescriptor[] GetPropertyDescriptors(SchedulerStoreSvc owner, ObjectType typeMask, PropFlags flagMask)
        {
            // Search specific maps and match based on propflags

            List<StorePropertyDescriptor> result = new List<StorePropertyDescriptor>();
            foreach (PropertyNameMap map in _allMaps)
            {
                if ((typeMask & map.ObType) != 0)
                {
                    result.AddRange(map.GetPropertyDescriptors(flagMask));
                }
            }

            if ((flagMask & PropFlags.Custom) != 0)
            {
                ServerPropertyDescriptor[] items = owner.ServerWrapper.Prop_GetDescriptors(typeMask, null);

                if (items != null)
                {
                    foreach (ServerPropertyDescriptor item in items)
                    {
                        result.Add(new StorePropertyDescriptor(item.Id, item.Name, item.Type, item.Description));
                    }
                }
            }

            return result.ToArray();
        }

        // This is a cache for storing custom property name and property id mapping indexed by cluster name
        static Dictionary<string, Dictionary<ObjectType, Dictionary<string, StorePropertyDescriptor>>> _clusterNamePropCache = new Dictionary<string, Dictionary<ObjectType, Dictionary<string, StorePropertyDescriptor>>>(StringComparer.InvariantCultureIgnoreCase);

        internal static StorePropertyDescriptor[] GetPropertyDescriptors(SchedulerStoreSvc owner, string[] names, ObjectType typeMask)
        {
            // Search specific maps for specific names

            List<StorePropertyDescriptor> result = new List<StorePropertyDescriptor>(names.Length);

            // first fill the list with errors...

            StorePropertyDescriptor psError = new StorePropertyDescriptor(StorePropertyIds.Error, "Error", ObjectType.None);

            int i;

            for (i = 0; i < names.Length; i++)
            {
                result.Add(psError);
            }

            if ((typeMask & ObjectType.Job) != 0)
            {
                Job.GetPropertyDescriptors(names, result);
            }

            if ((typeMask & ObjectType.Task) != 0)
            {
                Task.GetPropertyDescriptors(names, result);
            }

            if ((typeMask & ObjectType.Resource) != 0)
            {
                Resource.GetPropertyDescriptors(names, result);
            }

            if ((typeMask & ObjectType.JobTemplate) != 0)
            {
                Profile.GetPropertyDescriptors(names, result);
            }

            if ((typeMask & ObjectType.Node) != 0)
            {
                Node.GetPropertyDescriptors(names, result);
            }


            //find the name to property cache for this cluster
            Dictionary<ObjectType, Dictionary<string, StorePropertyDescriptor>> namePropCache = null;
            _clusterNamePropCache.TryGetValue(owner.Name, out namePropCache);

            List<string> remoteNames = new List<string>();

            // Now go to the server to any names that we
            // could not resolve locally.

            for (i = 0; i < result.Count; i++)
            {
                if (result[i].PropId == StorePropertyIds.Error)
                {                    
                    if (namePropCache != null)
                    {
                        StorePropertyDescriptor desc = null;
                        // Look up from the cache first
                        Dictionary<string, StorePropertyDescriptor> table;
                        if (namePropCache.TryGetValue(typeMask, out table))
                        {
                            if (table.TryGetValue(names[i], out desc))
                            {
                                result[i] = desc;
                                continue;
                            }
                        }
                    }
                    remoteNames.Add(names[i]);
                }
            }

            if (remoteNames.Count > 0)
            {
                ServerPropertyDescriptor[] descs = owner.ServerWrapper.Prop_GetDescriptors(typeMask, remoteNames.ToArray());

                int j = 0;

                if (namePropCache == null)
                {
                    namePropCache = new Dictionary<ObjectType, Dictionary<string, StorePropertyDescriptor>>();
                    _clusterNamePropCache[owner.Name] = namePropCache;
                }

                for (i = 0; i < result.Count; i++)
                {
                    if (result[i].PropId == StorePropertyIds.Error)
                    {
                        if (descs[j] != null)
                        {
                            result[i] = new StorePropertyDescriptor(descs[j].Id, descs[j].Name, descs[j].Type, descs[j].Description);

                            if (descs[j].Id != StorePropertyIds.Error)
                            {
                                // Update the cache
                                Dictionary<string, StorePropertyDescriptor> table;
                                if (!namePropCache.TryGetValue(typeMask, out table))
                                {
                                    table = new Dictionary<string, StorePropertyDescriptor>();
                                    namePropCache[typeMask] = table;
                                }
                                table[names[i]] = result[i];
                            }
                        }

                        ++j;
                    }
                }
            }

            return result.ToArray();
        }

        static void ValidateInt32Prop(StoreProperty prop)
        {
            if (prop.Value is Int32)
            {
                return;
            }
            else if (prop.Value is string)
            {
                prop.Value = Int32.Parse((string)prop.Value);
            }
            else if (prop.Value is TimeSpan)
            {
                TimeSpan ts = (TimeSpan)prop.Value;

                prop.Value = ts.Seconds;
            }
            else
            {
                throw new SchedulerException(
                        string.Format(SR.BadTypeProperty,
                                prop.Id.Name,
                                prop.Value.GetType().Name
                                )
                        );
            }
        }

        static void ValidateBoolProp(StoreProperty prop)
        {
            if (prop.Value is bool)
            {
                return;
            }
            else if (prop.Value is string)
            {
                char c = ((string)prop.Value).Substring(0, 1).ToCharArray()[0];

                if (c == '1' || c == 't' || c == 'T')
                {
                    prop.Value = true;
                }
                else
                {
                    prop.Value = false;
                }
            }
            else if (prop.Value is Int32)
            {
                if ((int)prop.Value == 0)
                {
                    prop.Value = false;
                }
                else
                {
                    prop.Value = true;
                }
            }
            else
            {
                throw new SchedulerException(
                        string.Format(SR.BadTypeProperty,
                                prop.Id.Name,
                                prop.Value.GetType().Name
                                )
                        );

            }
        }

        static void ValidateStringProp(StoreProperty prop)
        {
            if (prop.Value is string)
            {
                return;
            }
            else
            {
                prop.Value = prop.Value.ToString();
            }
        }

        static void ValidateEnumProp<T>(StoreProperty prop)
        {
            if (prop.Value is T)
            {
                return;
            }

            //if the prop value is an int, check if it's a defined value
            try
            {
                Type intType = Enum.GetUnderlyingType(typeof(T));
                if (prop.Value.GetType() == intType)
                {
                    prop.Value = (T)prop.Value;
                    return;
                }
                else if (prop.Value is string)
                {
                    prop.Value = (T)Enum.Parse(typeof(T), (string)prop.Value, true);
                    return;
                }
            }
            catch (Exception)
            {
            }

            //if we reach here, the property has an invalid type
            throw new SchedulerException(
                        string.Format(SR.BadTypeProperty,
                                prop.Id.Name,
                                prop.Value.GetType().Name
                                )
                        );
        }

        static internal void ValidatJobOrderByType(StoreProperty prop)
        {
            if (prop.Value is IJobOrderByList)
            {
                return;
            }

            string text = prop.Value as string;
            if (text != null)
            {
                prop.Value = JobOrderByList.Parse(text);
            }
            else
            {
                throw new SchedulerException(
                    string.Format(SR.BadTypeProperty,
                        prop.Id.Name,
                        prop.Value.GetType().Name));
            }
        }

        static internal void ValidatePropertyType(StoreProperty prop)
        {
            if (prop.Value != null)
            {
                switch (prop.Id.Type)
                {
                    case StorePropertyType.Int32:
                        ValidateInt32Prop(prop);
                        break;

                    case StorePropertyType.Boolean:
                        ValidateBoolProp(prop);
                        break;

                    case StorePropertyType.String:
                    case StorePropertyType.StringList:
                        ValidateStringProp(prop);
                        break;

                    case StorePropertyType.JobPriority:
                        ValidateEnumProp<JobPriority>(prop);
                        break;

                    case StorePropertyType.JobState:
                        ValidateEnumProp<JobState>(prop);
                        break;

                    case StorePropertyType.JobUnitType:
                        ValidateEnumProp<JobUnitType>(prop);
                        break;

                    case StorePropertyType.JobOrderby:
                        ValidatJobOrderByType(prop);
                        break;

                    case StorePropertyType.JobRuntimeType:
                        ValidateEnumProp<JobRuntimeType>(prop);
                        break;

                    default:
                        break;
                }
            }
        }

        static internal void ValidateValueArrayType(StoreProperty arrayProp)
        {
            if (arrayProp != null && arrayProp.Value != null && arrayProp.Value is object[])
            {
                object[] valueArray = arrayProp.Value as object[];
                for (int i = 0; i < valueArray.Length; i++)
                {
                    StoreProperty tempProp = new StoreProperty(arrayProp.Id, valueArray[i]);
                    ValidatePropertyType(tempProp);
                    valueArray[i] = tempProp.Value;
                }
            }
            else
            {
                throw new SchedulerException(
                        string.Format(SR.BadTypeProperty,
                                arrayProp.Id.Name,
                                arrayProp.Value.GetType().Name
                                )
                        );
            }
        }

        static internal void ValidatePropertyTypes(StoreProperty[] props)
        {
            if (props != null)
            {
                foreach (StoreProperty prop in props)
                {
                    ValidatePropertyType(prop);
                }
            }
        }

        static internal void ValidateFilterTypes(FilterProperty[] filter)
        {
            if (filter != null)
            {
                foreach (FilterProperty item in filter)
                {
                    if (item.Operator == FilterOperator.In)
                    {
                        ValidateValueArrayType(item.Property);
                    }
                    else
                    {
                        ValidatePropertyType(item.Property);
                    }
                }
            }
        }

        static internal void ValidatePropertyNames(ObjectType obType, StoreProperty[] props)
        {
            PropertyNameMap objectMap = null;
            foreach (PropertyNameMap map in _allMaps)
            {
                if (map.ObType == obType)
                {
                    objectMap = map;
                    break;
                }
            }

            if (objectMap != null)
            {
                objectMap.ValidatePropertyNames(props);
            }
        }
        

        static internal PropertyId PropertyIdFromPropName(string propName)
        {
            foreach (PropertyNameMap map in _allMaps)
            {
                PropertyId propId = map.PropertyIdFromPropName(propName);
                if (propId != null)
                {
                    return propId;
                }
            }
            return null;
        }

        static internal PropertyId PropertyIdFromPropIndex(int propIndex)
        {
            foreach (PropertyNameMap map in _allMaps)
            {
                PropertyId propId = map.PropertyIdFromPropIndex(propIndex);
                if (propId != null)
                {
                    return propId;
                }
            }
            return null;
        }

        internal static bool IsPrivateProperty(PropertyId pid)
        {
            return pid.UniqueId >= PropertyIdConstants.PrivateJobPropertyIdStart;
        }

        /// <summary>
        /// This functions is called before a getprops call is dispatched to the server (or the local rowset call the remote rowset).
        /// It will convert any propeties on the client to their equivalent properties on the server.
        /// </summary>
        /// <param name="converters">A set of property converters to use</param>
        /// <param name="ids">The propety ids requested by the client</param>
        /// <param name="newIds">The new array of property ids.  Will be the same as ids if no properties need to be converted.</param>
        /// <param name="map">The map containing conversion property conversion information, if any properties need to be converted</param>        
        /// <returns>True if any conversions took place, false otherwise</returns>
        internal static bool PreGetProps_Convert(ICollection<PropertyConverter> converters, PropertyId[] ids, out PropertyId[] newIds, out PropertyConversionMap map)
        {
            if (converters == null || converters.Count == 0 || ids == null)
            {
                newIds = ids;
                map = null;
                return false;
            }
            
            map = new PropertyConversionMap(converters);
           
            // Go through all the property ids, record the indices of those that need to be converted
            for (int i=0; i<ids.Length; i++)
            {
                map.MapPropertyId(ids[i], i);
            }

            bool havePropsToConvert = false;
            List<PropertyId> newIdList = null;
            foreach (PropertyConverter converter in converters)
            {
                PropertyId clientPropId = converter.GetPropId(); 
                Debug.Assert(map.IsMapped(clientPropId));

                if (map.GetMappedIndex(clientPropId) == -1)
                {
                    // The property id for this converter is not in the list
                    continue;
                }

                havePropsToConvert = true;

                // If any of the replacement ids are not on the list of ids to retrieve from the server, add them now
                PropertyId[] serverPropIds = converter.GetAllServerReplacementPropIds();
                if (serverPropIds != null)
                {
                    foreach (PropertyId serverId in serverPropIds)
                    {                        
                        Debug.Assert(map.IsMapped(serverId));
                        if (map.GetMappedIndex(serverId) == -1)
                        {
                            if (newIdList == null)
                            {
                                newIdList = new List<PropertyId>(ids);
                            }
                            map.MapPropertyId(serverId, newIdList.Count);
                            newIdList.Add(serverId);
                        }
                    }
                }                
            }

            // If we have nothing to properties to convert, exit now.
            if (!havePropsToConvert)
            {
                map = null;
                newIds = ids;
                return false;
            }

            newIds = (newIdList == null ? ids : newIdList.ToArray());
            return true;
        }

        /// <summary>
        /// This function should be called after the GetProps (or Remote Rowset) call to server returns.  This will map the
        /// propeties retrieved from the server back into those requested by the client.
        /// </summary>
        /// <param name="map">The conversion map for the properties, returned by a previous call to PreGetProps_Convert.</param>
        /// <param name="props">The array of properties read from the server.  The order of the propeties of the array must obey the mapping in the conversion map.</param>
        /// <returns>True if any conversions took place, false otherwise</returns>
        static internal bool PostGetProps_Deconvert(PropertyConversionMap map, StoreProperty[] props)
        {
            if (map == null || props == null)
            {
                return false;
            }

            bool converted = false;     
            foreach (PropertyConverter converter in map.GetMappedConverters())
            {
                PropertyId clientId = converter.GetPropId();
                int clientPropIndex = map.GetMappedIndex(clientId);

                Debug.Assert(clientPropIndex >= 0 && clientPropIndex < props.Length, 
                             string.Format("Invalid clientPropIndex={0}, clientId={1}#{2}, props.Length={3}", 
                             clientPropIndex, clientId.Name, clientId.UniqueId, props.Length ));

                Debug.Assert(props[clientPropIndex].Id == clientId || props[clientPropIndex].Id == StorePropertyIds.Error,
                             string.Format("Invalid property at index={0}: expected={1}#{2}, found={3}#{4}",
                             clientPropIndex, clientId.Name, clientId.UniqueId, props[clientPropIndex].Id.Name, props[clientPropIndex].Id.UniqueId));

                PropertyId[] serverIds = converter.GetAllServerReplacementPropIds();
                if (serverIds != null)
                {
                    int[] serverPropIndexes = map.GetMappedIndexes(serverIds);
                    Debug.Assert(serverPropIndexes != null);

                    StoreProperty[] serverProps = new StoreProperty[serverPropIndexes.Length];
                    for (int i = 0; i < serverPropIndexes.Length; i++)
                    {
                        int index = serverPropIndexes[i];
                        Debug.Assert(index >= -1 && index < props.Length);

                        if (index != -1 && props[index].Id == serverIds[i])
                        {
                            serverProps[i] = props[index];
                        }
                        else
                        {
                            // The property is not in the array retrieved from the server
                            serverProps[i] = new StoreProperty(serverIds[i], null);
                        }
                    }

                    props[clientPropIndex] = converter.DeconvertGetProps(serverProps);
                    converted = true;
                }
            }
            return converted;
        }

        /// <summary>
        /// This function should be called if the property conversion map could not be computed prior to making the call to GetProps (e.g., because
        /// the array of ids is null), or after properties have been read from an XML file.  It will compute the conversion map on-the-fly, and return
        /// a new array of Store Properties, including the converted properties.
        /// 
        /// NOTE: Whenever possible, call the other variant of PostGetProps_ConvertBack instead.
        /// </summary>
        /// <param name="converters">The set of property converters to use.</param>
        /// <param name="props">The array of properties read from the server.</param>
        /// <param name="newProps">The array of converted properties.  Will be the same as props if no conversions took place.</param>
        /// <param name="map">The conversion map conmputed from props</param>
        /// <returns></returns>
        static internal bool PostGetProps_Deconvert(ICollection<PropertyConverter> converters, StoreProperty[] props, out StoreProperty[] newProps, out PropertyConversionMap map)
        {
            if (converters == null || converters.Count == 0 || props == null)
            {
                newProps = props;
                map = null;
                return false;
            }

            map = new PropertyConversionMap(converters);

            for (int i = 0; i < props.Length; i++)
            {
                map.MapPropertyId(props[i].Id, i);
            }

            bool havePropsToDeconvert = false;
            List<StoreProperty> newPropList = null;
            foreach (PropertyConverter converter in converters)
            {     
                PropertyId clientPropId = converter.GetPropId();
                PropertyId[] requiredServerIds = converter.GetRequiredServerReplacementPropIds();

                if (requiredServerIds == null)
                {
                    continue;
                }

                int[] serverPropIndexes = map.GetMappedIndexes(requiredServerIds);
                if (serverPropIndexes == null || Array.IndexOf(serverPropIndexes, -1) >= 0)
                {
                    // Not all required replacement properties for this converter are in the list.
                    continue;
                }

                havePropsToDeconvert = true;

                // If the original id is not in the list of props retrieved from server, add it now
                Debug.Assert(map.IsMapped(clientPropId));
                if (map.GetMappedIndex(clientPropId) == -1)
                {
                    if (newPropList == null)
                    {
                        newPropList = new List<StoreProperty>(props);
                    }
                    map.MapPropertyId(clientPropId, newPropList.Count);
                    newPropList.Add(new StoreProperty(clientPropId, null));
                }
            }

            if (!havePropsToDeconvert)
            {
                map = null;
                newProps = props;
                return false;
            }

            newProps = (newPropList == null ? props : newPropList.ToArray());
            return PostGetProps_Deconvert(map, newProps);
        }

        /// <summary>
        /// This function is called before a SetProps call is made on the server (or a similar call that sets properties on scheduler
        /// objects).  It will replace client-side properties with those compatible with the server.  Since the propeties are sent to
        /// the server one-way, there is no need for an additional method to run post remote-call (unlike PostGetProps_ConvertBack)
        /// </summary>
        /// <param name="converters">The list of property converters to use</param>
        /// <param name="props">The array of propeties that the client want to set</param>
        /// <param name="newProps">The array of converted store properites. Will be the same as props if no conversions took place</param>
        /// <returns>True if any conversions took place, false otherwise</returns>
        static internal bool SetProps_Convert(ICollection<PropertyConverter> converters, StoreProperty[] props, out StoreProperty[] newProps)
        {
            if (converters == null || converters.Count == 0 || props == null)
            {
                newProps = props;
                return false;
            }

            bool converted = false;
            List<StoreProperty> newPropList = new List<StoreProperty>();            
            foreach (StoreProperty prop in props)
            {
                PropertyConverter converter = FindConverterForPropId(converters, prop.Id);                
                if (converter != null)
                {
                    StoreProperty[] convertedProps = converter.ConvertSetProp(prop);
                    if (convertedProps != null)
                    {
                        newPropList.AddRange(convertedProps);
                    }
                    converted = true;
                }
                else
                {
                    newPropList.Add(prop);
                }
            }

            newProps = (converted ? newPropList.ToArray() : props);
            return converted;
        }

        /// <summary>
        /// This function is called before a remote rowset or row enumerator is opened.  It will replace client-side filters
        /// with those compatible with the server.  Since the filters are sent to the server one-way, there is no need for an 
        /// additional method to run after the remote call.
        /// </summary>
        /// <param name="converters">The list of property converters to use</param>
        /// <param name="filters">The array of filters specified by the client</param>
        /// <param name="newFilters">The array of converted filters. Will be the same as filters if no conversions took place</param>
        /// <returns>True if any conversions took place, false otherwise</returns>
        static internal bool FilterProps_Convert(ICollection<PropertyConverter> converters, FilterProperty[] filters, out FilterProperty[] newFilters)
        {
            if (converters == null || converters.Count == 0 || filters == null)
            {
                newFilters = filters;
                return false;
            }

            bool converted = false;
            List<FilterProperty> newFilterList = new List<FilterProperty>();
            foreach (FilterProperty filter in filters)
            {
                PropertyConverter converter = FindConverterForPropId(converters, filter.Property.Id);
                if (converter != null)
                {
                    FilterProperty[] convertedFilters = converter.ConvertFilter(filter);
                    if (convertedFilters != null)
                    {
                        newFilterList.AddRange(convertedFilters);
                    }
                    converted = true;
                }
                else
                {
                    newFilterList.Add(filter);
                }
            }

            newFilters = (converted ? newFilterList.ToArray() : filters);
            return converted;
        }

        /// <summary>
        /// This function is called before a remote rowset or row enumerator is opened.  It will replace client-side sort properties
        /// with those compatible with the server.  Since the sort properties are sent to the server one-way, there is no need for an 
        /// additional method to run after the remote call.
        /// </summary>
        /// <param name="converters">The list of property converters to use</param>
        /// <param name="sortProps">The array of sort properties specified by the client</param>
        /// <param name="newSortProps">The array of converted sort properties. Will be the same as sortProps if no conversions took place</param>
        /// <returns>True if any conversions took place, false otherwise</returns>
        static internal bool SortProps_Convert(ICollection<PropertyConverter> converters, SortProperty[] sortProps, out SortProperty[] newSortProps)
        {
            if (converters == null || converters.Count == 0 || sortProps == null)
            {
                newSortProps = sortProps;
                return false;
            }

            bool converted = false;
            List<SortProperty> newSortPropList = new List<SortProperty>();
            foreach (SortProperty sort in sortProps)
            {
                PropertyConverter converter = FindConverterForPropId(converters, sort.Id);
                if (converter != null)
                {
                    SortProperty[] convertedSort = converter.ConvertSort(sort);
                    if (convertedSort != null)
                    {
                        newSortPropList.AddRange(convertedSort);
                    }
                    converted = true;
                }
                else
                {
                    newSortPropList.Add(sort);
                }
            }

            newSortProps = (converted ? newSortPropList.ToArray() : sortProps);
            return converted;
        }




        private static PropertyConverter FindConverterForPropId(ICollection<PropertyConverter> converters, PropertyId targetId)
        {
            foreach (PropertyConverter converter in converters)
            {
                if (converter.GetPropId() == targetId)
                {
                    return converter;
                }
            }
            return null;
        }

        #region utility functions

        // Returns an array of values in a dictionary corresponding to the given set of keys.  If at least one key is not found, returns null.
        internal static T[] LookupAllPropIds<T>(Dictionary<PropertyId, T> dict, PropertyId[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return null;
            }

            T[] values = new T[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                if (!dict.ContainsKey(keys[i]))
                {
                    return null;
                }
                values[i] = dict[keys[i]];
            }
            return values;
        }

        #endregion

        /// <summary>
        /// Applies a set of validations and transformations to the array of properties before they are set on the server.
        /// </summary>
        static internal StoreProperty[] ProcessSetProps(SchedulerStoreSvc helper, ObjectType obType, StoreProperty[] props)
        {
            if (props == null)
            {
                return null;
            }

            ValidatePropertyNames(obType, props);
            ValidatePropertyTypes(props);

            StoreProperty[] newProps = props;
            if (helper.RequiresPropertyConversion)
            {                
                SetProps_Convert(helper.GetPropertyConverters(), props, out newProps);
            }
            return newProps;
        }
    }
}
