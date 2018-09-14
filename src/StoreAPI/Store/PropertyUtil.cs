using System;
using System.Collections.Generic;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Security;
using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class PropertyUtil
    {
        // For validating single custom property (name=value) 
        public const string regexForCustomProperty = @"^\w+=[^=;]*$"; 

        // For validating all custom properties (name1=value1;name2=value2; ...)
        // value for the key can consist of any char including space except "=" and ";" 
        public const string regexForCustomProperties = @"^(\w+=[^=;]*)(;\w+=[^=;]*)*$";

        // Maximum length of custom property name and value  
        public const int MaxLengthOfCustomPropertyNameAndValue = 128;

        //The default value of the PlannedCoreCount property
        public const int DefaultPlannedCoreCount = -1;

        /// <summary>
        /// The default value of the TaskExecutionFailureRetryLimit property.
        /// </summary>
        public const int DefaultTaskExecutionFailureRetryLimit = 0;


        public static string GetAllocatedNodeList(ICollection<KeyValuePair<string, int>> allocatedNodes, char separator)
        {
            StringBuilder allocatedNodeList = new StringBuilder();
            if (allocatedNodes != null)
            {
                bool first = true;
                foreach (KeyValuePair<string, int> allocation in allocatedNodes)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        allocatedNodeList.Append(separator);
                    }

                    allocatedNodeList.Append(allocation.Key);
                }
            }
            return allocatedNodeList.ToString();
        }

        public static string GetMpiHostsString(ICollection<KeyValuePair<string, int>> allocationList)
        {
            StringBuilder allocationBuilder = new StringBuilder();
            if (allocationList != null && allocationList.Count > 0)
            {
                allocationBuilder.Append(allocationList.Count);
                foreach (KeyValuePair<string, int> allocation in allocationList)
                {
                    allocationBuilder.Append(" ").Append(allocation.Key).Append(" ").Append(allocation.Value);
                }
            }

            return allocationBuilder.ToString();
        }


        public static int CompareProperties(StoreProperty left, StoreProperty right)
        {
            if (left.Id.Type != right.Id.Type)
            {
                // This may be invalid
                return -1;
            }

            // Check to see if the prop values are null.

            if (left.Value == null)
            {
                if (right.Value == null)
                {
                    return 0;
                }

                return -1;
            }
            else if (right.Value == null)
            {
                return 1;
            }

            switch (left.Id.Type)
            {
                case StorePropertyType.Int32:
                case StorePropertyType.JobPriority:
                case StorePropertyType.JobState:
                case StorePropertyType.JobType:
                case StorePropertyType.JobUnitType:
                case StorePropertyType.TaskState:
                case StorePropertyType.TaskType:
                case StorePropertyType.ResourceState:
                case StorePropertyType.ResourceJobPhase:
                case StorePropertyType.NodeState:
                case StorePropertyType.NodeAvailability:
                case StorePropertyType.CancelRequest:
                case StorePropertyType.PendingReason:
                case StorePropertyType.JobMessageType:
                case StorePropertyType.FailureReason:
                case StorePropertyType.JobEvent:
                case StorePropertyType.JobSchedulingPolicy:
                case StorePropertyType.NodeEvent:
                case StorePropertyType.UInt32:
                case StorePropertyType.JobRuntimeType:
                    return ((int)left.Value).CompareTo((int)right.Value);

                case StorePropertyType.JobOrderby:
                    return ((JobOrderByList)left.Value).ToInt().CompareTo((JobOrderByList)right.Value);

                case StorePropertyType.Int64:
                    return ((long)left.Value).CompareTo((long)right.Value);

                case StorePropertyType.String:
                case StorePropertyType.StringList:
                    return string.Compare((string)left.Value, (string)right.Value, StringComparison.InvariantCultureIgnoreCase);

                case StorePropertyType.DateTime:
                    return ((DateTime)left.Value).CompareTo((DateTime)right.Value);

                case StorePropertyType.Boolean:
                    return ((bool)left.Value).CompareTo((bool)right.Value);

            }

            return -1;
        }


        public static bool TestPropAgainstFilter(FilterProperty filter, StoreProperty prop)
        {
            switch (filter.Operator)
            {
                case FilterOperator.Equal:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) == 0)
                        return true;
                    break;

                case FilterOperator.NotEqual:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) != 0)
                        return true;
                    break;

                case FilterOperator.GreaterThan:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) > 0)
                        return true;
                    break;

                case FilterOperator.GreaterThanOrEqual:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) >= 0)
                        return true;
                    break;

                case FilterOperator.LessThan:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) < 0)
                        return true;
                    break;

                case FilterOperator.LessThanOrEqual:
                    if (PropertyUtil.CompareProperties(prop, filter.Property) <= 0)
                        return true;
                    break;

                case FilterOperator.HasBitSet:
                    if (PropertyUtil.BitSet(filter.Property, prop))
                        return true;
                    break;

                case FilterOperator.HasNoBitSet:
                    if (!PropertyUtil.BitSet(filter.Property, prop))
                        return true;
                    break;
                case FilterOperator.IsNull:
                    if (prop.Value == null)
                        return true;
                    break;

                case FilterOperator.IsNotNull:
                    if (prop.Value != null)
                        return true;
                    break;

                case FilterOperator.In:
                    if (filter.Property.Value == null || !(filter.Property.Value is object[]))
                        return false;
                    object[] validValues = filter.Property.Value as object[];
                    StoreProperty tmpProp = new StoreProperty(filter.Property.Id, null);
                    foreach (object obj in validValues)
                    {
                        tmpProp.Value = obj;
                        if (CompareProperties(prop, tmpProp) == 0)
                            return true;
                    }
                    return false;

                case FilterOperator.StartWith:
                    if (filter.Property.Value == null || !(filter.Property.Value is string))
                        return false;
                    if (prop.Value is string &&
                        ((string)prop.Value).StartsWith((string)filter.Property.Value, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                    break;
            }

            return false;
        }

        public static bool BitSet(StoreProperty filterBits, StoreProperty item)
        {
            if (filterBits.Value == null)
            {
                throw new InvalidOperationException("Must supply a filter property value");
            }

            if (item.Value == null)
            {
                return false;
            }

            int bitMask = (int)filterBits.Value;
            int itemBits = (int)item.Value;

            if ((bitMask & itemBits) != 0)
            {
                return true;
            }

            return false;
        }


        // Convert a propery index to a property ID
        // This needs to be publically accessible, since the ProperyLookup class is not.
        public static PropertyId PropertyIdFromPropIndex(int propIdx)
        {
            return PropertyLookup.PropertyIdFromPropIndex(propIdx);
        }


        #region Job min and max

        static PropertyId[] _extraIdsForFindingMinMax =
            {
                JobPropertyIds.UnitType,
                JobPropertyIds.AutoCalculateMin,
                JobPropertyIds.AutoCalculateMax,
                JobPropertyIds.CanGrow,
                JobPropertyIds.CanShrink,
                ProtectedJobPropertyIds.ProfileMinResources,
                ProtectedJobPropertyIds.ProfileMaxResources,
            };

        //-------------------------------------------------------------------------------------------------
        //
        // NOTE: If you ever change this array, you must also change the GetExtremumPropId method below
        //
        static PropertyId[] _jobIdsForMinMaxByUnitType = 
            {
                JobPropertyIds.MinCores,
                JobPropertyIds.MinSockets,
                JobPropertyIds.MinNodes,
                JobPropertyIds.ComputedMinCores,
                JobPropertyIds.ComputedMinSockets,
                JobPropertyIds.ComputedMinNodes,
                JobPropertyIds.MaxCores,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.MaxNodes,
                JobPropertyIds.ComputedMaxCores,
                JobPropertyIds.ComputedMaxSockets,
                JobPropertyIds.ComputedMaxNodes,
            };

        static PropertyId GetExtremumPropId(JobUnitType unitType, bool computed, bool wantMax)
        {
            const int numUnitTypes = 3;
            int maxOffset = 2 * numUnitTypes * (wantMax ? 1 : 0);
            int computedOffset = numUnitTypes * (computed ? 1 : 0);
            return _jobIdsForMinMaxByUnitType[maxOffset + computedOffset + (int)unitType];
        }
        //--------------------------------------------------------------------------------------------------


        public static PropertyId[] GetJobPropIdsForFindingMinMax()
        {
            List<PropertyId> allPids = new List<PropertyId>(_jobIdsForMinMaxByUnitType);
            allPids.AddRange(_extraIdsForFindingMinMax);
            return allPids.ToArray();
        }

        public static PropertyId GetJobMinPropId(JobUnitType unitType, bool computed)
        {
            return GetExtremumPropId(unitType, computed, false);
        }

        public static PropertyId GetJobMaxPropId(JobUnitType unitType, bool computed)
        {
            return GetExtremumPropId(unitType, computed, true);
        }

        public static int ComputeJobMin(int userMin, int computedMin, bool isAutoCalc, bool canShrink, int profileMin)
        {
            if (canShrink)
            {
                if (isAutoCalc)
                {
                    if (profileMin != 0)
                    {
                        return Math.Max(computedMin, profileMin);
                    }
                    else
                    {
                        return computedMin;
                    }
                }
                return Math.Max(computedMin, userMin);
            }
            return userMin;
        }

        public static int ComputeJobMax(int userMax, int computedMax, bool isAutoCalc, bool canGrow, int profileMax)
        {
            if (canGrow)
            {
                if (isAutoCalc)
                {
                    if (profileMax != 0)
                    {
                        return Math.Min(computedMax, profileMax);
                    }
                    else
                    {
                        return computedMax;
                    }
                }
                return Math.Min(computedMax, userMax);
            }
            return userMax;
        }

        public static int GetJobMinFromRow(PropertyRow row)
        {
            return GetJobMinFromRow(row, false);
        }

        public static int GetJobMinFromRow(PropertyRow row, bool userOnly)
        {
            return GetJobExtremumFromRow(row, false, userOnly, JobPropertyIds.AutoCalculateMin, JobPropertyIds.CanShrink);
        }

        public static int GetJobMaxFromRow(PropertyRow row)
        {
            return GetJobMaxFromRow(row, false);
        }

        public static int GetJobMaxFromRow(PropertyRow row, bool userOnly)
        {
            return GetJobExtremumFromRow(row, true, userOnly, JobPropertyIds.AutoCalculateMax, JobPropertyIds.CanGrow);
        }

        // Compute either min or max of a job from a property row.  Loads all requisite properties from the row,
        // and calls the appropriate function.
        static int GetJobExtremumFromRow(PropertyRow row, bool wantMax, bool userOnly, PropertyId autoCalcPid, PropertyId growOrShrinkPid)
        {
            JobUnitType unitType = GetRequiredValueFromPropRow<JobUnitType>(row, JobPropertyIds.UnitType);
            int userExtremum = GetValueFromPropRow<int>(row, GetExtremumPropId(unitType, false, wantMax), 1);
            if (userOnly)
            {
                return userExtremum;
            }
            int computedExtremum = GetValueFromPropRow<int>(row, GetExtremumPropId(unitType, true, wantMax), 1);
            bool isAutoCalc = GetValueFromPropRow<bool>(row, autoCalcPid, false);
            bool canGrowOrShrink = GetValueFromPropRow<bool>(row, growOrShrinkPid, true);

            PropertyId profileExtremumPropId = wantMax ? ProtectedJobPropertyIds.ProfileMaxResources : ProtectedJobPropertyIds.ProfileMinResources;
            int profileExtremum = GetValueFromPropRow<int>(row, profileExtremumPropId, 0);

            return wantMax ? ComputeJobMax(userExtremum, computedExtremum, isAutoCalc, canGrowOrShrink, profileExtremum) :
                             ComputeJobMin(userExtremum, computedExtremum, isAutoCalc, canGrowOrShrink, profileExtremum);
        }

        #endregion


        #region Property retrieval helper methods

        public static T GetRequiredValueFromPropRow<T>(PropertyRow row, PropertyId pid)
        {
            return GetRequiredValueFromProp<T>(row[pid], pid);
        }

        public static T GetValueFromPropRow<T>(PropertyRow row, PropertyId pid, T defaultValue)
        {
            return GetValueFromProp<T>(row[pid], pid, defaultValue);
        }

        // Note: null does not constitute a valid value
        public static T GetRequiredValueFromProp<T>(StoreProperty prop, PropertyId pid)
        {
            if (prop == null || prop.Id != pid || prop.Value == null)
            {
                throw new InvalidOperationException(string.Format("The property {0} does not have a valid value", pid));
            }
            return (T)prop.Value;
        }

        public static T GetValueFromProp<T>(StoreProperty prop, PropertyId pid, T defaultValue)
        {
            if (prop == null || prop.Id != pid || prop.Value == null)
            {
                return defaultValue;
            }
            return (T)prop.Value;
        }

        #endregion


        #region  Error code helper methods

        public static bool IsDatabaseError(int errorCode)
        {
            switch (errorCode)
            {
                case ErrorCode.Operation_ServerIsBusy:
                case ErrorCode.Operation_ObjectInUse:
                case ErrorCode.Operation_DatabaseException:
                    return true;
            }
            return false;
        }

        #endregion


        private static string[] _certificateChooserString = null;

        private static string[] CertificateChooserString
        {
            get
            {
                if (null == _certificateChooserString)
                {
                    _certificateChooserString =
                        new string[]
                        {
                            SR.CertificateChooser_Title,
                            SR.CertificateChooser_Text,
                        };
                }
                return _certificateChooserString;
            }
        }


        /// <summary>
        /// Method to get a certificate with the specified thumbprint and the template name from the local cert store
        /// A random password is generated and used to encrypt the certificate which is returned as a pfxBlob
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="templateName"></param>
        /// <param name="pfxPassword"></param>
        /// <returns></returns>
        public static byte[] GetCertFromStore(string thumbprint, string templateName, out SecureString pfxPassword)
        {
            return CertificateHelper.GetCertFromStore(thumbprint, templateName, SchedulerStore._fConsole, SchedulerStore._hWnd, out pfxPassword, CertificateChooserString);
        }

        /// <summary>
        /// JobPropertyIds.JobValidExitCodes, TaskPropertyIds.TaskValidExitCodes should follow a format requirement 
        /// Valid exit codes syntax:
        /// 1.	-1,-4..12,400..500. Exit codes can be both negative and positive numbers
        /// 2.	min..1120,3456,4567..8000,9000..max.  "min..1120" equals Int32.MinValue..1120 and "9000..max" equals 9000..Int32.MaxValue
        /// 3.	min..max
        /// 4.  "min" and "max" are case insensitive
        /// </summary>
        /// <param name="input">Parameter value</param>
        /// <returns>Valid or not</returns>
        public static bool CheckValidExitCodesArgumentFormat(string input)
        {
            // It is allowed for user to set empty string for ValidExitCodes property
            // since users may want to erase the original setting 
            if (IsNullOrEmptyString(input))
            {
                return true;
            }

            string[] parts = input.Trim().Split(',');

            // Check each element in parts to see whether if it is a valid integer or range
            foreach (string str in parts)
            {
                if (CheckExitCodeValidity(str) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check whether each part of exit codes separated by comma is a valid integer or integer range
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool CheckExitCodeValidity(string str)
        {
            // There should be an integer or integer range between two commas
            // empty string or " " is not a valid exit code
            if (IsNullOrEmptyString(str))
            {
                return false;
            }

            // User input should be case insensitive 
            string trimmedStr = str.Trim().ToLowerInvariant();

            // If str represents an integer
            int code;
            bool result = Int32.TryParse(trimmedStr, out code);

            if (result == true)
            {
                return true;
            }

            // If str does not represent an integer, further check whether str reprensents a range like 8..20
            // "min..-20" equals Int32.MinValue..-20 and "20..max" equals 20..Int32.MaxValue
            // we do not allow "..", it must be "min..max" or "max..min"
            // but it supports "3..7" and "7..3"
            string[] range = trimmedStr.Split(new string[] { ".." }, StringSplitOptions.None);

            if (range.Length != 2)
            {
                return false;
            }

            // Parse the first and second part
            if ((String.Compare(range[0].Trim(), "min") == 0 || String.Compare(range[0].Trim(), "max") == 0 || Int32.TryParse(range[0].Trim(), out code)) &&
                (String.Compare(range[1].Trim(), "min") == 0 || String.Compare(range[1].Trim(), "max") == 0 || Int32.TryParse(range[1].Trim(), out code)) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 1. Value of JobPropertyIds.ParentJobIds is a string of Job ids separated by commas
        /// 2. 0 is not a valid parentjob id
        /// The syntax:
        /// 1. 112
        /// 2. 112,113,145
        /// Notes: 0 is not a valid job id
        /// </summary>
        /// <param name="input">Parameter value</param>
        /// <returns>Valid or not</returns>
        public static bool CheckParentJobIdsArgumentFormat(string input)
        {
            // If the string is empty, it means user wants to remove the value
            if (IsNullOrEmptyString(input))
            {
                return true;
            }

            string[] ids = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < ids.Length; i++)
            {
                int id = -1;

                if (!int.TryParse(ids[i], out id) || id == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Some inputs from users such as parentjobids in the form of "1,2,3" should not contain duplicate items 
        /// so remove duplication before it is stored in database
        /// </summary>
        /// <param name="input">a string consisting of digit characters separated by commas, and having the format of "1,2,3....."</param>
        /// <returns></returns>
        public static string RemoveDuplicateItems(string input)
        {
            if (IsNullOrEmptyString(input))
            {
                return String.Empty;
            }

            string[] ids = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, bool> dic = new Dictionary<string, bool>();
            StringBuilder builder = new StringBuilder();
            bool first = true;

            for (int i = 0; i < ids.Length; i++)
            {
                string id = ids[i].Trim();

                if (!dic.ContainsKey(id))
                {
                    dic.Add(id, true);

                    if (first)
                    {
                        builder.Append(id);
                        first = false;
                    }
                    else
                    {
                        builder.Append(",");
                        builder.Append(id);
                    }
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Value of CustomProperties is a string of (name, value) pairs separated by semicolon, which is for the consistency with
        /// the format of environment variables
        /// The syntax:
        /// 1. customProperty1=value1
        /// 2. customProperty1=value1;customProperty2=value2;...  
        /// </summary>
        /// <param name="input">Parameter value</param>
        public static void CheckCustomPropertiesArgumentFormat(string input)
        {
            string str = input.Trim();

            Regex pattern = new Regex(regexForCustomProperties);

            if (!pattern.IsMatch(str))
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidCustomProperties, regexForCustomProperties);
            }
        }

        public static bool IsNullOrEmptyString(string input)
        {
            if (String.IsNullOrEmpty(input) || input.Trim() == String.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Add up the values of the integer type properties in the provided row
        /// and return the sum.
        /// Can throw an invalidcastexception if a non-integer property type is provided.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="pids"></param>
        /// <returns></returns>
        public static int AddUpPropertyValuesFromRow(PropertyRow row, IEnumerable<PropertyId> pids)
        {
            int runningTasksCount = 0;

            foreach (PropertyId id in pids)
            {
                runningTasksCount += GetValueFromPropRow<int>(row, id, 0);
            }

            return runningTasksCount;
        }

    }
}
