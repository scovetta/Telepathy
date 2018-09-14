using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    /// <summary>
    ///  This class provides facilities to convert a property on the client into one or more properties that can
    ///  be written to a server, and to translate properties retrieved from the server back into the client-side
    ///  properties.  It also provides facilities to convert properties when reading them or writing them to XML.
    /// </summary>
    public abstract class PropertyConverter
    {
        /// <summary>
        /// Get the client-side Id of the property that is associated with this converter.
        /// </summary>
        /// <returns>The property Id</returns>
        public abstract PropertyId GetPropId();

        /// <summary>
        /// Get the property ids on the server corresponding to the converter's client-side property ids
        /// </summary>
        /// <returns>Array of server-side ids</returns>
        public abstract PropertyId[] GetAllServerReplacementPropIds();

        /// <summary>
        /// Get the only those server-side ids required to successfully de-convert to the client-side property
        /// </summary>
        /// <returns>Array of server-side ids</returns>
        public virtual PropertyId[] GetRequiredServerReplacementPropIds()
        {
            return GetAllServerReplacementPropIds();
        }

        /// <summary>
        /// Convert the properties retrieved from the server back to the client-side property
        /// </summary>
        /// <param name="props">Array of propeties retrieved from the server.  Should be in the same order,
        /// and with the same IDs as the array returned by GetServerReplacementPropIds()</param>
        /// <returns>The converter's client-side property, derived from the props parameter</returns>
        public abstract StoreProperty DeconvertGetProps(StoreProperty[] props);
        
        /// <summary>
        /// Convert the original client-side side property into one or more of server-side properties.        
        /// </summary>
        /// <param name="prop">The client-side property.  The Id must be the same as returned by GetPropId()</param>
        /// <returns>The array of server-side propeties.  If null, the property will not be set on the server.</returns>
        public abstract StoreProperty[] ConvertSetProp(StoreProperty prop);

        /// <summary>
        /// Convert a filter based on the client-side property into one or more filters that can be applied server-side.
        /// </summary>
        /// <param name="filter">The filter.  The filter property Id must be the same as that return by GetPropId()</param>
        /// <returns>The array of server-side filters.  If null, the filter will not be applied server-side.</returns>
        public virtual FilterProperty[] ConvertFilter(FilterProperty filter)
        {
            throw new InvalidOperationException(string.Format("You may not filter on the {0} property on your version of Windows HPC Server", GetPropId()));
        }

        /// <summary>
        /// Convert a client-side sort property into one that can be applied on the server side.
        /// </summary>
        /// <param name="sort"></param>
        /// <returns></returns>
        public virtual SortProperty[] ConvertSort(SortProperty sort)
        {
            throw new InvalidOperationException(string.Format("You may not sort on the {0} property on your version of Windows HPC Server", GetPropId()));
        }
    }

    /// <summary>
    /// This class provides the functionality of a property converter, and in addition provides a set of methods 
    /// for converting properties when reading and writing property XML.
    /// </summary>
    public abstract class XMLEnabledPropertyConverter : PropertyConverter
    {
        /// <summary>
        /// Retrieve the property ids that correspond to the converter's associated property when reading properties from XML
        /// </summary>
        /// <returns>Array of XML property ids</returns>
        public virtual PropertyId[] GetXMLReplacementPropIds()
        {
            return GetAllServerReplacementPropIds();
        }

        /// <summary>
        /// This method lets the XML parser know whether the XML replacement propeties should removed from the property bag
        /// after they are read from XML and de-converted.  Every sub-class must implement this method.
        /// </summary>
        public abstract bool NeedsToRemoveReplacementPropsAfterXMLDeconvert();

        /// <summary>
        /// Converts properties read from XML back to the converter's associated client-side property.
        /// </summary>
        /// <param name="props">The array of properties read from XML.</param>
        /// <returns>The converter's associated property, derived from the props parameter</returns>
        public virtual StoreProperty DeconvertXMLReadProps(StoreProperty[] props)
        {
            return DeconvertGetProps(props);
        }

        /// <summary>
        /// Converts the converter
        /// </summary>
        /// <param name="prop">The converter's associated property.  Id must be the same as returned by GetPropId()</param>
        /// <returns>The array of converted propeties to be written to XML</returns>
        public virtual StoreProperty[] ConvertXMLWriteProp(StoreProperty prop)
        {
            return ConvertSetProp(prop);
        }
    }

    public abstract class ReadOnlyPropertyConverter : PropertyConverter
    {
        public override StoreProperty[] ConvertSetProp(StoreProperty prop)
        {
            throw new SchedulerException(ErrorCode.Operation_PropertyIsReadOnly, prop.PropName);
        }
    }


    /// <summary>
    /// Converter used to skip sending properties to older servers.    
    /// </summary>
    internal class VersionMismatchConverter : PropertyConverter
    {
        PropertyId _clientPropertyId;
        Version _serverVersion;
        PropertyVersioning.Info _info;

        internal VersionMismatchConverter(PropertyId clientPropertyId, PropertyVersioning.Info info)
        {
            _clientPropertyId = clientPropertyId;
            _serverVersion = VersionControl.V2;
            _info = info;
        }

        internal void SetServerVersion(Version serverVersion)
        {
            _serverVersion = serverVersion;
        }

        public override PropertyId GetPropId()
        {
            return _clientPropertyId;
        }

        public override PropertyId[] GetAllServerReplacementPropIds()
        {
            return null;
        }

        public override StoreProperty DeconvertGetProps(StoreProperty[] props)
        {
            return new StoreProperty(_clientPropertyId, null);
        }

        public override StoreProperty[] ConvertSetProp(StoreProperty prop)
        {
            throw new SchedulerException(ErrorCode.Operation_PropertyNotSupportedOnServerVersion,
                     ErrorCode.MakeErrorParams(
                          _clientPropertyId.Name,
                          _serverVersion.ToString(),
                          _info.EarliestCompatibleVersion.ToString()));
         
        }

    }


    internal class ErrorMessageConverter : ReadOnlyPropertyConverter
    {
        public override PropertyId GetPropId()
        {
            return StorePropertyIds.ErrorMessage;
        }

        public override PropertyId[] GetAllServerReplacementPropIds()
        {
            return new PropertyId[] { StorePropertyIds.ErrorCode, StorePropertyIds.ErrorParams };
        }

        public override PropertyId[] GetRequiredServerReplacementPropIds()
        {
            return new PropertyId[] { StorePropertyIds.ErrorCode };
        }

        public override StoreProperty DeconvertGetProps(StoreProperty[] props)
        {
            Debug.Assert(props != null && props.Length == 2);
            Debug.Assert(props[0].Id == StorePropertyIds.ErrorCode);
            Debug.Assert(props[1].Id == StorePropertyIds.ErrorParams);

            int errorCode = (int)(props[0].Value ?? ErrorCode.Success);
            string errorParams = (string)(props[1].Value ?? String.Empty);
            return new StoreProperty(StorePropertyIds.ErrorMessage, ErrorCode.ToString(errorCode, errorParams));
        }
    }


    #region Convert SID to user name for V2 compatibility

    internal class OwnerSidConverter : PropertyConverter
    {
        public override PropertyId GetPropId()
        {
            return JobPropertyIds.OwnerSID;
        }

        public override PropertyId[] GetAllServerReplacementPropIds()
        {
            return new PropertyId[] { JobPropertyIds.Owner };
        }

        public override StoreProperty DeconvertGetProps(StoreProperty[] props)
        {
            Debug.Assert(props != null && props.Length == 1 && props[0].Id == JobPropertyIds.Owner);
            return new StoreProperty(JobPropertyIds.OwnerSID, GetSidFromName(props[0].Value as string));
        }

        public override StoreProperty[] ConvertSetProp(StoreProperty prop)
        {
            Debug.Assert(prop.Id == JobPropertyIds.OwnerSID);
            return new StoreProperty[] { new StoreProperty(JobPropertyIds.Owner, GetNameFromSid(prop.Value as string)) };
        }

        public override FilterProperty[] ConvertFilter(FilterProperty filter)
        {
            Debug.Assert(filter.Property.Id == JobPropertyIds.OwnerSID);

            if (!(filter.Operator == FilterOperator.Equal || filter.Operator == FilterOperator.NotEqual
                  || filter.Operator == FilterOperator.IsNull || filter.Operator == FilterOperator.IsNotNull))
            {
                throw new InvalidOperationException(
                    string.Format("Cannot filter on the 'OwnerSID' job property with a '{0}' on your version of Windows HPC server", filter.Operator));
            }

            return new FilterProperty[] {
                new FilterProperty(filter.Operator, JobPropertyIds.Owner, GetNameFromSid(filter.Property.Value as string))
            };
        }

        static Dictionary<string, string> _name2sid = new Dictionary<string, string>();
        static Dictionary<string, string> _sid2name = new Dictionary<string, string>();
        static object _mapLock = new object();

        static string GetSidFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            
            string sid = string.Empty;
            lock (_mapLock)
            {                
                if (_name2sid.TryGetValue(name.ToLowerInvariant(), out sid))
                {
                    return sid;
                }
            }

            try
            {
                sid = ((new NTAccount(name).Translate(typeof(SecurityIdentifier))) as SecurityIdentifier).Value;
            }
            catch 
            {
                sid = string.Empty;
            }

            lock(_mapLock)
            {
                _name2sid[name.ToLowerInvariant()] = sid;
                if (!string.IsNullOrEmpty(sid))
                {
                    _sid2name[sid.ToLowerInvariant()] = name;
                }
                return sid;
            }
        }

        static string GetNameFromSid(string sid)
        {
            if (string.IsNullOrEmpty(sid))
            {
                return string.Empty;
            }
            
            string name = string.Empty;
            lock (_mapLock)
            {                
                if (_sid2name.TryGetValue(sid.ToLowerInvariant(), out name))
                {
                    return name;
                }
            }

            try
            {
                name = ((new SecurityIdentifier(sid).Translate(typeof(NTAccount))) as NTAccount).Value;                
            }
            catch
            {
                name = string.Empty;
            }

            lock (_mapLock)
            {
                _sid2name[sid.ToLowerInvariant()] = name;
                if (!string.IsNullOrEmpty(name))
                {
                    _name2sid[name.ToLowerInvariant()] = sid;
                }
                return name;
            }            
        }
    }

    #endregion


    #region Convert ExpandedPriority to Priority for V2 compatibility

    internal class ExpandedPriorityConverter : PropertyConverter
    {
        public override PropertyId GetPropId()
        {
            return JobPropertyIds.ExpandedPriority;
        }

        public override PropertyId[] GetAllServerReplacementPropIds()
        {
            return new PropertyId[] { JobPropertyIds.Priority };
        }

        public override StoreProperty DeconvertGetProps(StoreProperty[] props)
        {
            Debug.Assert(props != null && props.Length == 1 && props[0].Id == JobPropertyIds.Priority);            
            JobPriority priValue = (JobPriority)(props[0].Value ?? JobPriority.Normal);
            return new StoreProperty(JobPropertyIds.ExpandedPriority, ExpandedPriority.JobPriorityToExpandedPriority((int)priValue));
        }

        public override StoreProperty[] ConvertSetProp(StoreProperty prop)
        {
            return new StoreProperty[] { PropExPri2Pri(prop, true) }; 
        }

        StoreProperty PropExPri2Pri(StoreProperty prop, bool predefinedOnly)
        {
            Debug.Assert(prop != null && prop.Id == JobPropertyIds.ExpandedPriority);
            int exPri = (int)(prop.Value ?? ExpandedPriority.Normal);
            
            if (exPri < ExpandedPriority.Lowest || exPri > ExpandedPriority.Highest)
            {
                throw new ArgumentOutOfRangeException("The expanded priority is out of range");
            }
            
            if (predefinedOnly && ((exPri % ExpandedPriority.LevelsPerPriorityBucket) != 0))
            {
                int closestLegalLow  = (exPri / ExpandedPriority.LevelsPerPriorityBucket) * ExpandedPriority.LevelsPerPriorityBucket;
                int closestLegalHigh = closestLegalLow + ExpandedPriority.LevelsPerPriorityBucket;
                throw new SchedulerException(ErrorCode.Operation_ExpandedPriorityNotValidOnServer,
                    ErrorCode.MakeErrorParams(ExpandedPriority.ToString(exPri), 
                    ExpandedPriority.ToString(closestLegalLow), 
                    ExpandedPriority.ToString(closestLegalHigh)));
            }

            return new StoreProperty(JobPropertyIds.Priority, ExpandedPriority.ExpandedPriorityToJobPriority(exPri));
        }

        public override FilterProperty[] ConvertFilter(FilterProperty filter)
        {
            Debug.Assert(filter != null && filter.Property.Id == JobPropertyIds.ExpandedPriority);

            if (filter.Operator == FilterOperator.In)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot filter on the 'ExpandedPriority' job property with '{0}' on your version of Windows HPC server", filter.Operator));
            }

            StoreProperty propPriority = PropExPri2Pri(filter.Property, true);
            return new FilterProperty[] { new FilterProperty(filter.Operator, propPriority) };
        }

        public override SortProperty[] ConvertSort(SortProperty sort)
        {
            Debug.Assert(sort != null && sort.Id == JobPropertyIds.ExpandedPriority);

            return new SortProperty[] { new SortProperty(sort.Order, JobPropertyIds.Priority) };
        }
    }

    #endregion



    #region Helper class that represent a conversion map associated with a set of properties

    /// <summary>
    /// Helper class that contains mapping information about property conversions
    /// </summary>
    internal class PropertyConversionMap
    {
        Dictionary<PropertyId, int> _propIndexMap;
        List<PropertyConverter> _converterList;

        internal PropertyConversionMap(IEnumerable<PropertyConverter> converters)
        {
            _propIndexMap = new Dictionary<PropertyId, int>();
            _converterList = new List<PropertyConverter>(converters);
#if DEBUG
            // Debug-only sanity checking code: make sure no original id is used more than once            
            Dictionary<PropertyId, object> converterIds = new Dictionary<PropertyId, object>();
#endif
            foreach (PropertyConverter converter in converters)
            {
                PropertyId coverterId = converter.GetPropId();
                PropertyId[] replacementIds = converter.GetAllServerReplacementPropIds();

#if DEBUG
                // Sanity-check to make sure that no two converters are responsible for the same prop id
                Debug.Assert(!converterIds.ContainsKey(coverterId));
                converterIds.Add(coverterId, null);
#endif
                
                _propIndexMap[coverterId] = -1;
                if (replacementIds != null)
                {
                    foreach (PropertyId id in replacementIds)
                    {
                        _propIndexMap[id] = -1;
#if DEBUG
                        // Sanity-check to make sure that no replacement Id is an original Id belonging to another converter
                        Debug.Assert(!converterIds.ContainsKey(id));
#endif
                    }
                }
            }            
        }


        internal void MapPropertyId(PropertyId id, int index)
        {
            int oldIndex;
            if (_propIndexMap.TryGetValue(id, out oldIndex))
            {
                if (oldIndex == -1)
                {
                    _propIndexMap[id] = index;
                }
            }
        }

        internal bool IsMapped(PropertyId id)
        {
            return _propIndexMap.ContainsKey(id);
        }

        internal int GetMappedIndex(PropertyId id)
        {
            int index;
            if (_propIndexMap.TryGetValue(id, out index))
            {
                return index;
            }
            return -1;
        }

        internal List<PropertyConverter> GetMappedConverters()
        { 
            List<PropertyConverter> result = new List<PropertyConverter>();
            foreach (PropertyConverter converter in _converterList)
            {
                if (GetMappedIndex(converter.GetPropId()) != -1)
                {
                    result.Add(converter);
                }
            }
            return result;
        }

        internal int[] GetMappedIndexes(PropertyId[] ids)
        {
            return PropertyLookup.LookupAllPropIds<int>(_propIndexMap, ids);
        }
    }

    #endregion
}
