using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;


using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.AddInFilter.HpcServer;
using System.ComponentModel;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class JobProfile : StoreObjectBase, IClusterJobProfile, ProfileValidation.IProfileItemQuery
    {
        Int32 profileId;
        ConnectionToken token;
        private SchedulerStoreSvc owner;
        
        internal JobProfile(SchedulerStoreSvc owner, ConnectionToken token, Int32 profileId)
            :base(owner, ObjectType.JobTemplate)
        {
            this.owner = owner;
            this.profileId = profileId;
            this.token = token;
        }

        Dictionary<PropertyId, ClusterJobProfileItem> _cache = null;
        object _cacheLock = new object();

        private void InitLocalCache()
        {
            if (_cache == null)
            {
                lock (_cacheLock)
                {
                    if (_cache == null)
                    {
                        _cache = new Dictionary<PropertyId, ClusterJobProfileItem>();

                        ClusterJobProfileItem[] items;

                        this.owner.ServerWrapper.GetProfileItems(token, profileId, out items);

                        if (items != null)
                        {
                            foreach (ClusterJobProfileItem item in items)
                            {
                                _cache[item.PropId] = item;
                            }
                        }
                    }
                }
            }
        }


        public override int Id
        {
            get { return profileId; }
        }

        public IClusterJobProfile Clone(string profileNameNew)
        {
            int newProfileId = -1;
            _helper.ServerWrapper.Profile_CloneProfile(profileId, profileNameNew, out newProfileId);

            return new JobProfile(_helper, token, newProfileId);
        }

        public override PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public override PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _helper.GetPropsFromServer(ObjectType.JobTemplate, profileId, propertyIds);
        }

        public override PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Profile.PropertyIdsFromNames(propertyNames));
        }

        public override void SetProps(StoreProperty[] profileProperties)
        {
            _helper.SetPropsOnServer(ObjectType.JobTemplate, profileId, profileProperties);
        }

        bool _fLocalValidation = true;

        public void AddProfileItem(ClusterJobProfileItem item)
        {
            if (_fLocalValidation)
            {
                InitLocalCache();
                ValidateProfileItem(item);
            }
        
            _helper.ServerWrapper.Profile_ItemOperation(profileId, ProfileItemOperator.Add, null, item);
            
            _cache[item.PropId] = item;
        }

        public void ModifyProfileItem(ClusterJobProfileItem item)
        {
            if (_fLocalValidation)
            {
                InitLocalCache();
                ValidateProfileItem(item);
            }

            _helper.ServerWrapper.Profile_ItemOperation(profileId, ProfileItemOperator.Modify, null, item);

            _cache[item.PropId] = item;
        }

        public void DeleteProfileItem(PropertyId pid)
        {
            InitLocalCache();
        
            _helper.ServerWrapper.Profile_ItemOperation(profileId, ProfileItemOperator.Delete, pid, null);

            if (_cache.ContainsKey(pid))
            {
                _cache.Remove(pid);
            }
        }

        public void ReplaceProfileItems(IEnumerable<ClusterJobProfileItem> items)
        {
            ProfileValidation validation = new ProfileValidation(owner.StoreInProc);

            // Make sure that there is no conflicting or bad profile
            // items as part of the replacement.
            
            CallResult cr = validation.ValidateItems(null, items);
            
            if (cr.Code != ErrorCode.Success)
            {
                cr.Throw();
            }

            //if the client is of a newer version than the server, then there is a chance
            //that some of the profile items may not be supported by the server and
            //we should not send those properties to the server
            if (_helper.ClientVersion.Version > _helper.ServerVersion.Version)
            {
                List<ClusterJobProfileItem> filteredList = new List<ClusterJobProfileItem>();
                foreach (ClusterJobProfileItem item in items)
                {
                    if (validation.ValidateServerVersion(item.PropId, _helper.ServerVersion.Version)
                         == CallResult.Succeeded)
                    {
                        filteredList.Add(item);
                    }
                }
                items = filteredList;
            }
            
            _helper.ServerWrapper.Profile_UpdateItems(profileId, null, items, false);
        }

        public ClusterJobProfileItem GetProfileItemForPropId(PropertyId propertyId)
        {
            InitLocalCache();
            
            ClusterJobProfileItem item = null;
            
            if (_cache.TryGetValue(propertyId, out item) == true)
            {
                return item;
            }
            
            return null;
        }        

        public ClusterJobProfileItem[] GetProfileItems()
        {
            InitLocalCache();
                        
            return new List<ClusterJobProfileItem>(_cache.Values).ToArray();
        }

        public bool ValidateJobProperty(StoreProperty jobProperty)
        {
            bool result = true;
            
            try
            {
                ValidateJobPropertyWithThrow(jobProperty);
            }
            catch
            {
                result = false;
            }
            
            return result;
        }

        public void ValidateJobPropertyWithThrow(StoreProperty jobProperty)
        {
            ClusterJobProfileItem item = GetProfileItemForPropId(jobProperty.Id);

            if (item != null)
            {
                item.ValidateJobPropertyWithThrow(jobProperty);
            }
            else
            {
                // by default is "valid"
            }
        }

        public override void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags)
        {
            if (writer == null)
            {
                throw new SchedulerException(SR.MustProvideXmlWriter);
            }
            
            PropertyId[] ids = 
            {
                JobTemplatePropertyIds.Name,
                JobTemplatePropertyIds.Description,
                JobTemplatePropertyIds.CreateTime,
            };
            
            
            PropertyRow row = GetProps(ids);
            
            writer.WriteStartElement(XmlNames.JobProfile);

            foreach (StoreProperty prop in row.Props)
            {
                if (prop.Id != JobTemplatePropertyIds.Error)
                {
                    writer.WriteAttributeString(prop.Id.ToString(), prop.Value.ToString());
                }
            }

            ClusterJobProfileItem[] items = GetProfileItems();
            
            foreach (ClusterJobProfileItem item in items)
            {
                item.PersistToXml(writer);
            }
            
            writer.WriteEndElement();
        }
        
     
        public override void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions flags)
        {
            JobProfilePropertyBag bag = new JobProfilePropertyBag(owner.StoreInProc);
            
            bag.ReadXML(reader, XmlImportOptions.None);
            
            if ((flags & XmlImportOptions.UpdateJobTemplateItems) != 0)
            {
                bag.Update(this);
            }
            else
            {
                bag.Replace(this);
            }
        }

        static ProfileValidation _validator = null;

        CallResult ValidateProfileItem(ClusterJobProfileItem item)
        {
            if (_validator == null)
            {
                _validator = new ProfileValidation(owner.StoreInProc);
            }
            
            return _validator.ValidateItem(this, item);
        }
        
        CallResult ValidateProfileItems(IEnumerable<ClusterJobProfileItem> items)
        {
            CallResult cr = CallResult.Succeeded;

            foreach (ClusterJobProfileItem item in items)
            {
                cr = ValidateProfileItem(item);
                
                if (cr.Code != CallResult.Succeeded.Code)
                {
                    break;
                }
            }
            
            return cr;
        }

        public ClusterJobProfileItem GetItemForValidation(PropertyId pid)
        {
            return GetProfileItemForPropId(pid);
        }
    }

    public class ProfileValidation
    {
        // Some validation must not be run outside of the scheduler process.
        // One example of this is AddInFilter validation.
        private bool _isInSchedulerProc = false;

        const int PerFilterMaxStartupTime = 20000;

        private ProfileValidation()
        {
        }

        public ProfileValidation(bool isInSchedulerProc)
        {
            _isInSchedulerProc = isInSchedulerProc;
        }

        /// <summary>
        /// Only accepts job property ids
        /// </summary>
        /// <param name="?"></param>
        /// <returns></returns>
        internal static string GetDisplayName(PropertyId pid)
        {
            try
            {
                return PropertyLookup.GetPropertyDescriptors(ObjectType.Job, new PropertyId[] { pid })[0].DisplayName;
            }
            catch
            {
                return pid.Name;
            }
        }

        public interface IProfileItemQuery
        {
            ClusterJobProfileItem GetItemForValidation(PropertyId pid);
        }

        class ProfileItemCollection : ProfileValidation.IProfileItemQuery
        {
            Dictionary<PropertyId, ClusterJobProfileItem> _items = null;
            
            internal ProfileItemCollection(IEnumerable<ClusterJobProfileItem> items)
            {
                _items = new Dictionary<PropertyId,ClusterJobProfileItem>();
                
                foreach (ClusterJobProfileItem item in items)
                {
                    _items[item.PropId] = item;
                }
            }
        
            public ClusterJobProfileItem GetItemForValidation(PropertyId pid)
            {
                ClusterJobProfileItem item = null;
                
                if (_items.TryGetValue(pid, out item))
                {
                    return item;
                }
                
                return null;
            }
        }

        static Dictionary<PropertyId, ProfileItemValidator> _validProps = null;
        static Dictionary<PropertyId, Version> _minServerVersion = null;
        static object _validPropsLock = new object();

        void InitPropMap()
        {
            if (_validProps == null)
            {
                lock (_validPropsLock)
                {
                    if (_validProps == null)
                    {
                        // Set up the valid property/validator dictionary.
                        // This is only done once per instance of the scheduler
                        // and resused continually.

                        _validProps = new Dictionary<PropertyId, ProfileItemValidator>();

                        ProfileEnumValidator enumValidator = new ProfileEnumValidator();
                        ProfileStringValidator stringValidator = new ProfileStringValidator();
                        ProfileStringListValidator stringListValidator = new ProfileStringListValidator();
                        ProfileBoolValidator boolValidator = new ProfileBoolValidator();
                        ProfileIntegerValidator intValidator = new ProfileIntegerValidator();
                        ProfileNoOpValidator noOpValidator = new ProfileNoOpValidator();

                        ProfileMinMaxValidator coreValidator = new ProfileMinMaxValidator(JobPropertyIds.MinCores, JobPropertyIds.MaxCores);
                        ProfileMinMaxValidator nodeValidator = new ProfileMinMaxValidator(JobPropertyIds.MinNodes, JobPropertyIds.MaxNodes);
                        ProfileMinMaxValidator socketValidator = new ProfileMinMaxValidator(JobPropertyIds.MinSockets, JobPropertyIds.MaxSockets);
                        ProfileMinMaxValidator memoryValidator = new ProfileMinMaxValidator(JobPropertyIds.MinMemory, JobPropertyIds.MaxMemory);
                        ProfileMinMaxValidator CoresPerNodeValidator = new ProfileMinMaxValidator(JobPropertyIds.MinCoresPerNode, JobPropertyIds.MaxCoresPerNode);

                        // Filter validators
                        ProfileFilterListValidator activationFilterValidator = new ProfileFilterListValidator(true);
                        ProfileFilterListValidator submissionFilterValidator = new ProfileFilterListValidator(false);

                        _validProps[JobPropertyIds.MinCores] = coreValidator;
                        _validProps[JobPropertyIds.MaxCores] = coreValidator;
                        _validProps[JobPropertyIds.MinNodes] = nodeValidator;
                        _validProps[JobPropertyIds.MaxNodes] = nodeValidator;
                        _validProps[JobPropertyIds.MinSockets] = socketValidator;
                        _validProps[JobPropertyIds.MaxSockets] = socketValidator;
                        _validProps[JobPropertyIds.MinMemory] = memoryValidator;
                        _validProps[JobPropertyIds.MaxMemory] = memoryValidator;
                        _validProps[JobPropertyIds.MinCoresPerNode] = CoresPerNodeValidator;
                        _validProps[JobPropertyIds.MaxCoresPerNode] = CoresPerNodeValidator;

                        _validProps[JobPropertyIds.UnitType] = enumValidator;

                        _validProps[JobPropertyIds.Name] = stringValidator;
                        _validProps[JobPropertyIds.RequestedNodes] = stringListValidator;
                        _validProps[JobPropertyIds.ExpandedPriority] = intValidator;
                        _validProps[JobPropertyIds.Project] = stringValidator;
                        _validProps[JobPropertyIds.SoftwareLicense] = stringListValidator;
                        _validProps[JobPropertyIds.RuntimeSeconds] = intValidator;
                        _validProps[JobPropertyIds.IsExclusive] = boolValidator;
                        _validProps[JobPropertyIds.NodeGroups] = stringListValidator;
                        _validProps[JobPropertyIds.OrderBy] = noOpValidator;
                        _validProps[JobPropertyIds.RunUntilCanceled] = boolValidator;

                        _validProps[JobPropertyIds.AutoCalculateMax] = boolValidator;
                        _validProps[JobPropertyIds.AutoCalculateMin] = boolValidator;
                        _validProps[JobPropertyIds.Preemptable] = boolValidator;
                        _validProps[JobPropertyIds.FailOnTaskFailure] = boolValidator;

                        _validProps[JobPropertyIds.ServiceName] = stringValidator;
                        _validProps[JobPropertyIds.Pool] = stringValidator;

                        _validProps[ProtectedJobPropertyIds.ActivationFilters] = activationFilterValidator;
                        _validProps[ProtectedJobPropertyIds.SubmissionFilters] = submissionFilterValidator;
                        _validProps[JobPropertyIds.NodeGroupOp] = enumValidator;
                        _validProps[JobPropertyIds.EstimatedProcessMemory] = intValidator;

                        _validProps[JobPropertyIds.SingleNode] = boolValidator;

                        _validProps[JobPropertyIds.TaskExecutionFailureRetryLimit] = intValidator;

                        _validProps[JobPropertyIds.NodePrepareTask] = stringValidator;
                        _validProps[JobPropertyIds.NodeReleaseTask] = stringValidator;

                        //add any new template properties to this dictionary
                        _minServerVersion = new Dictionary<PropertyId, Version>();
                        _minServerVersion[JobPropertyIds.Pool] = VersionControl.V3SP2;
                        _minServerVersion[ProtectedJobPropertyIds.ActivationFilters] = VersionControl.V3SP2;
                        _minServerVersion[ProtectedJobPropertyIds.SubmissionFilters] = VersionControl.V3SP2;
                        _minServerVersion[JobPropertyIds.NodeGroupOp] = VersionControl.V4;
                        _minServerVersion[JobPropertyIds.EstimatedProcessMemory] = VersionControl.V4;
                        _minServerVersion[JobPropertyIds.SingleNode] = VersionControl.V4;
                        _minServerVersion[JobPropertyIds.TaskExecutionFailureRetryLimit] = VersionControl.V4SP3;
                    }
                }
            }
        }

        public CallResult ValidateItem(IProfileItemQuery query, ClusterJobProfileItem item)
        {
            InitPropMap();
            
            ProfileItemValidator validator;
            
            if (_validProps.TryGetValue(item.PropId, out validator) == true)
            {
                if (SkipInprocOnlyValidations(item.PropId))
                {
                    return CallResult.Succeeded;
                }
                else
                {
                    return validator.Validate(query, item);
                }
            }
            
            return new CallResult(ErrorCode.Operation_IllegalProfileProperty, ProfileValidation.GetDisplayName(item.PropId));
        }

        /// <summary>
        /// these property id's must not be validated only in the scheduler
        /// process.  All other processes get free "success"
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool SkipInprocOnlyValidations(PropertyId id)
        {
            if (!_isInSchedulerProc)
            {
                if ((ProtectedJobPropertyIds.ActivationFilters == id) ||
                    (ProtectedJobPropertyIds.SubmissionFilters == id))
                {
                    return true;
                }
            }

            return false;
        }

        public CallResult ValidateItems(IProfileItemQuery query, IEnumerable<ClusterJobProfileItem> items)
        {
            if (query == null)
            {
                query = new ProfileItemCollection(items);
            }
            
            foreach (ClusterJobProfileItem item in items)
            {
                CallResult cr = ValidateItem(query, item);
                
                if (cr.Code != ErrorCode.Success)
                {
                    return cr;
                }
            }
            
            return CallResult.Succeeded;
        }

        /// <summary>
        /// This method checks if a particular property is known to be supported by the supplied server version.
        /// This method assumes that the property itself has already been validated to be a valid template item
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="serverVersion"></param>
        /// <returns></returns>
        internal CallResult ValidateServerVersion(PropertyId pid, Version serverVersion)
        {
            //we check if there is an entry for this property in the _minServerVersion dictionary
            Version minVersion;
            if (_minServerVersion.TryGetValue(pid, out minVersion))
            {
                if (minVersion > serverVersion)
                {
                    return new CallResult(ErrorCode.Operation_PropertyNotSupportedOnServerVersion, string.Empty);
                }
                else
                {
                    return CallResult.Succeeded;
                }
            }
            else
            {
                //if the _minserverversion does not have an entry for this pid, it is assumed that 
                //the pid is supported from v2 on wards
                return CallResult.Succeeded;
            }

        }


        public abstract class ProfileItemValidator
        {
            internal abstract CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item);
        }

        internal class ProfileBoolValidator : ProfileItemValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                if (item.BoolValues != null)
                {
                    foreach (bool val in item.BoolValues)
                    {
                        if (item.BoolDefaultValue == val)
                        {
                            return CallResult.Succeeded;
                        }
                    }

                    return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueInvalid, ProfileValidation.GetDisplayName(item.PropId));
                }

                return CallResult.Succeeded;
            }
        }

        internal class ProfileNoOpValidator : ProfileIntegerValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                return CallResult.Succeeded;
            }
        }

        internal class ProfileEnumValidator : ProfileItemValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                if (item.EnumValues != null)
                {
                    foreach (int val in item.EnumValues)
                    {
                        if (item.EnumDefaultValue == val)
                        {
                            return CallResult.Succeeded;
                        }
                    }

                    return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueInvalid, ProfileValidation.GetDisplayName(item.PropId));
                }

                return CallResult.Succeeded;
            }
        }

        internal class ProfileStringValidator  : ProfileItemValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                if (item.StringDefaultValue == null)
                {
                    return new CallResult(ErrorCode.Operation_ProfileItemMustProvideDefaultStringValue, ProfileValidation.GetDisplayName(item.PropId));
                }
                
                if (item.StringValues != null)
                {
                    foreach (string str in item.StringValues)
                    {
                        if (str.CompareTo(item.StringDefaultValue) == 0)
                        {
                            return CallResult.Succeeded;
                        }
                    }

                    return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueInvalid, ProfileValidation.GetDisplayName(item.PropId));
                }
                
                return CallResult.Succeeded;
            }
        }

        internal class ProfileStringListValidator : ProfileItemValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                if (item.StringListDefaultValue == null)
                {
                    return new CallResult(ErrorCode.Operation_ProfileItemMustProvideDefaultStringValue, ProfileValidation.GetDisplayName(item.PropId));
                }

                if (item.StringValues != null)
                {
                    if (!ClusterJobProfileItem.StringListContain(item.StringValues, item.StringListDefaultValue))
                    {
                        return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueInvalid, ProfileValidation.GetDisplayName(item.PropId));
                    }
                }

                if (item.RequiredStrings != null)
                {
                    if (!ClusterJobProfileItem.StringListContain(item.StringListDefaultValue, item.RequiredStrings))
                    {
                        return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueNotIncludeRequiredValue, ProfileValidation.GetDisplayName(item.PropId));
                    }
                }

                return CallResult.Succeeded;
            }
        }

        internal class ProfileFilterListValidator : ProfileItemValidator
        {
            /// <summary>
            /// True for activation filter, false for submission filter
            /// </summary>
            bool _isActivationFilter = false;

            public ProfileFilterListValidator(bool activationFilter)
            {
                _isActivationFilter = activationFilter;
            }

            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                CallResult callResult;

                if ((item.StringValues == null) || (item.StringValues.Length == 0))
                {
                    // early exit if no filters
                    callResult = CallResult.Succeeded;
                }
                else
                {
                    string filterName = "Unspecified";
                    int iFilter = -1;  // index of current filter.  outside the loop so can be used in forensics/eventing
                    int errorCode = ErrorCode.Operation_FilterSystemError;

                    try
                    {
                        IAddInFilterFactory filterFactory = AddInFilterDemandLoad.GetAddInFilterFactory();

                        for (iFilter = 0; iFilter < item.StringValues.Length; iFilter++)
                        {
                            string origFilterName = item.StringValues[iFilter];
                            string curFilter = string.Empty;

                            if (!string.IsNullOrEmpty(origFilterName))
                            {
                                curFilter = origFilterName.Trim();
                            }

                            if (string.IsNullOrEmpty(curFilter))
                            {
                                // skip this entry, it is eqivilent to empty
                                continue;
                            }

                            filterName = filterFactory.MakeCanonicalFilterName(curFilter);

                            IAddInFilter filter = filterFactory.GetAddInFilter(filterName, PerFilterMaxStartupTime);

                            if (null == filter)
                            {
                                throw new ArgumentNullException("filter", "Null filter returned for : " + filterName);
                            }

                            Object desiredInterface;

                            if (_isActivationFilter)
                            {
                                desiredInterface = filter.GetActivationFilter();
                                errorCode = ErrorCode.Operation_FilterNotActivation;
                            }
                            else
                            {
                                desiredInterface = filter.GetSubmissionFilter();
                                errorCode = ErrorCode.Operation_FilterNotSubmission;
                            }

                            if (null == desiredInterface)
                            {
                                callResult = new CallResult(errorCode, filterName);
                                break;
                            }
                        }

                        callResult = CallResult.Succeeded;
                    }
                    catch (AddInFilterTimeBoundedCallException tbcEx)
                    {
                        Exception inner = tbcEx.InnerException;
                        errorCode = ErrorCode.Operation_FilterSystemError;

                        if ((null != inner) && (inner is FileNotFoundException))
                        {
                            errorCode = ErrorCode.Operation_FilterNotFound;
                        }

                        callResult = MakeGenericCallResult(errorCode, tbcEx, iFilter, filterName);
                    }
                    catch (Exception ex)
                    {
                        callResult = MakeGenericCallResult(ErrorCode.Operation_FilterSystemError, ex, iFilter, filterName);
                    }
                }

                if (CallResult.Succeeded != callResult)
                {
                    callResult.Throw();
                }

                return callResult;
            }

            private CallResult MakeGenericCallResult(int errorCode, Exception ex, int iFilter, string filterName)
            {
                return new CallResult(errorCode, "Filter #" + iFilter + ", Name:" + filterName + ", Exception: " + ex.ToString());
            }
        }

        internal class ProfileIntegerValidator : ProfileItemValidator
        {
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                if (item.IntDefaultValue < 0)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_ValueLessThanZero,
                        ProfileValidation.GetDisplayName(item.PropId) + ErrorCode.ArgumentSeparator + 
                        "default" + ErrorCode.ArgumentSeparator + 
                        item.IntDefaultValue);
                }
                
                if (item.IntMinValue < 0)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_ValueLessThanZero,
                        ProfileValidation.GetDisplayName(item.PropId) + ErrorCode.ArgumentSeparator + 
                        "minimum" + ErrorCode.ArgumentSeparator + 
                        item.IntMinValue);
                }

                if (item.IntMaxValue < 0)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_ValueLessThanZero,
                        ProfileValidation.GetDisplayName(item.PropId) + ErrorCode.ArgumentSeparator + 
                        "maximum" + ErrorCode.ArgumentSeparator + 
                        item.IntMaxValue);
                }
                
                if (item.IntMinValue > item.IntMaxValue)
                {
                    return new CallResult(ErrorCode.Operation_ProfileItemMinGreaterThanMax, ProfileValidation.GetDisplayName(item.PropId));
                }
                
                if (item.IntDefaultValue < item.IntMinValue || item.IntDefaultValue > item.IntMaxValue)
                {
                    return new CallResult(ErrorCode.Operation_ProfileItemDefaultValueInvalid, ProfileValidation.GetDisplayName(item.PropId));
                }
                
                return CallResult.Succeeded;
            }
        }

        internal class ProfileMinMaxValidator : ProfileIntegerValidator
        {
            PropertyId _pidLeft;
            PropertyId _pidRight;
            
            internal ProfileMinMaxValidator(PropertyId pidLeft, PropertyId pidRight)
            {
                _pidLeft = pidLeft;
                _pidRight = pidRight;
            }
            
            internal override CallResult Validate(IProfileItemQuery query, ClusterJobProfileItem item)
            {
                CallResult result = base.Validate(query, item);
                
                if (result.Code != ErrorCode.Success)
                {
                    return result;
                }
                
                ClusterJobProfileItem other = null;

                if (item.PropId == _pidLeft)
                {
                    other = query.GetItemForValidation(_pidRight);
                    
                    if (other != null)
                    {
                        result = InternalValidate(item, other);
                    }
                }
                else if (item.PropId == _pidRight)
                {
                    other = query.GetItemForValidation(_pidLeft);
                    
                    if (other != null)
                    {
                        result = InternalValidate(other, item);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
                    
                return result;
            }
            
            CallResult InternalValidate(ClusterJobProfileItem left, ClusterJobProfileItem right)
            {
                if (left.IntMinValue > right.IntMinValue)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_LeftMinGreaterThanRightMin,
                        ProfileValidation.GetDisplayName(left.PropId) + ErrorCode.ArgumentSeparator + 
                        left.IntMinValue + ErrorCode.ArgumentSeparator +
                        ProfileValidation.GetDisplayName(right.PropId) + ErrorCode.ArgumentSeparator + 
                        right.IntMinValue);
                }
                else if (left.IntMaxValue > right.IntMaxValue)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_LeftMaxGreaterThanRightMax,
                        ProfileValidation.GetDisplayName(left.PropId) + ErrorCode.ArgumentSeparator + 
                        left.IntMaxValue + ErrorCode.ArgumentSeparator +
                        ProfileValidation.GetDisplayName(right.PropId) + ErrorCode.ArgumentSeparator + 
                        right.IntMaxValue);
                }
                else if (left.IntDefaultValue > right.IntDefaultValue)
                {
                    return new CallResult(
                        ErrorCode.Operation_ProfileItemRangeInconsistent_LeftDefaultGreaterThanRightDefault,
                        ProfileValidation.GetDisplayName(left.PropId) + ErrorCode.ArgumentSeparator + 
                        left.IntDefaultValue + ErrorCode.ArgumentSeparator +
                        ProfileValidation.GetDisplayName(right.PropId) + ErrorCode.ArgumentSeparator +
                        right.IntDefaultValue);
                }
                
                return CallResult.Succeeded;
            }

        }
    }

}
