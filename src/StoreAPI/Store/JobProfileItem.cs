namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;

    [Serializable]
    public class ClusterJobProfileItem 
    {
        public const int DefaultProfileId = 1;

        private static List<PropertyId> _defaultProfileProperties = new List<PropertyId>(
            new PropertyId[] 
            {
                JobPropertyIds.MinCores,
                JobPropertyIds.MaxCores,
                JobPropertyIds.MinSockets,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.MinNodes,
                JobPropertyIds.MaxNodes,
                JobPropertyIds.UnitType,
                JobPropertyIds.IsExclusive,
                JobPropertyIds.RunUntilCanceled,
                JobPropertyIds.ExpandedPriority,
                JobPropertyIds.AutoCalculateMax,
                JobPropertyIds.AutoCalculateMin,
                JobPropertyIds.FailOnTaskFailure,
                JobPropertyIds.Preemptable,
                JobPropertyIds.EstimatedProcessMemory,
                JobPropertyIds.TaskExecutionFailureRetryLimit,
            });

        public static List<PropertyId> DefaultProfileProperties
        {
            get
            {
                return _defaultProfileProperties;
            }
        }

        const bool _readOnlyDefault = false;

        static Dictionary<StorePropertyType, Type> _PropertyTypeMapping;

        private static Dictionary<PropertyId, ClusterJobProfileItem> JobProfileItemFactoryDefaultTable
            = new Dictionary<PropertyId,ClusterJobProfileItem>();

        private static Dictionary<StorePropertyType, ClusterJobProfileItem> JobProfileItemFactoryDefaultPrototypes
            = new Dictionary<StorePropertyType, ClusterJobProfileItem>();

        static ClusterJobProfileItem()
        {
            _PropertyTypeMapping = new Dictionary<StorePropertyType, Type>();
            
            _PropertyTypeMapping.Add(StorePropertyType.JobUnitType, typeof(JobUnitType));
            _PropertyTypeMapping.Add(StorePropertyType.JobType, typeof(JobType));
            _PropertyTypeMapping.Add(StorePropertyType.JobNodeGroupOp, typeof(JobNodeGroupOp));

            // Prototypes for factory settings for all types
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.Int32, ClusterJobProfileItem.CreateIntProfileItem(JobPropertyIds.MinCores, false, 1, 1, int.MaxValue));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.Int64, JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Int32]);
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.Boolean, ClusterJobProfileItem.CreateBooleanProfileItem(JobPropertyIds.IsExclusive, false, false, null));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.String, ClusterJobProfileItem.CreateStringProfileItem(JobPropertyIds.Project, false, null, null));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.StringList, ClusterJobProfileItem.CreateStringListProfileItem(JobPropertyIds.RequestedNodes, false, null, null, null));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.JobOrderby, ClusterJobProfileItem.CreateJobOrderbyProfileItem(JobPropertyIds.OrderBy, false, new JobOrderByList()));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.JobType, ClusterJobProfileItem.CreateEnumProfileItem(JobPropertyIds.JobType, false, (int)JobType.Batch, null));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.JobUnitType, ClusterJobProfileItem.CreateEnumProfileItem(JobPropertyIds.UnitType, false, (int)JobUnitType.Core, null));
            JobProfileItemFactoryDefaultPrototypes.Add(StorePropertyType.JobNodeGroupOp, ClusterJobProfileItem.CreateEnumProfileItem(JobPropertyIds.NodeGroupOp, false, (int)JobNodeGroupOp.Intersect, null));

            // These are special factory settings other than the prototypes

            ClusterJobProfileItem booleanWithDefaultTrue = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Boolean]);
            booleanWithDefaultTrue.BoolDefaultValue = true;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.AutoCalculateMax] = booleanWithDefaultTrue;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.AutoCalculateMin] = booleanWithDefaultTrue;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.Preemptable] = booleanWithDefaultTrue;

            JobProfileItemFactoryDefaultTable[JobPropertyIds.RuntimeSeconds] = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Int32]);
            JobProfileItemFactoryDefaultTable[JobPropertyIds.RuntimeSeconds].IntDefaultValue = 86400;// 1 day is the default runtime period
            JobProfileItemFactoryDefaultTable[JobPropertyIds.RuntimeSeconds].IntMinValue = 0;

            JobProfileItemFactoryDefaultTable[JobPropertyIds.ExpandedPriority] = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Int32]);
            JobProfileItemFactoryDefaultTable[JobPropertyIds.ExpandedPriority].IntDefaultValue = ExpandedPriority.Normal;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.ExpandedPriority].IntMaxValue = ExpandedPriority.Highest;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.ExpandedPriority].IntMinValue = ExpandedPriority.Lowest;

            JobProfileItemFactoryDefaultTable[JobPropertyIds.Pool] = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.String]);
            JobProfileItemFactoryDefaultTable[JobPropertyIds.Pool].StringDefaultValue = "Default";

            JobProfileItemFactoryDefaultTable[JobPropertyIds.EstimatedProcessMemory] = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Int32], JobPropertyIds.EstimatedProcessMemory);
            JobProfileItemFactoryDefaultTable[JobPropertyIds.EstimatedProcessMemory].IntMinValue = 0;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.EstimatedProcessMemory].IntDefaultValue = 0;

            JobProfileItemFactoryDefaultTable[JobPropertyIds.TaskExecutionFailureRetryLimit] = new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.Int32], JobPropertyIds.TaskExecutionFailureRetryLimit);
            JobProfileItemFactoryDefaultTable[JobPropertyIds.TaskExecutionFailureRetryLimit].IntMinValue = 0;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.TaskExecutionFailureRetryLimit].IntDefaultValue = 0;
            JobProfileItemFactoryDefaultTable[JobPropertyIds.TaskExecutionFailureRetryLimit].IntMaxValue = int.MaxValue;
        }


        private PropertyId m_pid;

        private bool m_readonly;

        private bool m_defaultBool;
        private bool[] m_boolValues;

        private Int32 m_maxVal;
        private Int32 m_minVal;
        private Int32 m_defaultInt;

        private string m_defaultString;
        private string[] m_stringValues;

        private int m_defaultEnum;
        private int[] m_enumValues;

        private string[] m_requiredStrings;

        void _JobProfileItem()
        {
            m_pid                = JobPropertyIds.NA;

            m_readonly           = _readOnlyDefault;

            m_minVal             = 0;
            m_maxVal             = Int32.MaxValue;
            m_defaultInt         = 0;
            
            m_defaultBool        = false;
            m_boolValues         = null;
            
            m_defaultString      = "";
            m_stringValues       = null;
            m_requiredStrings    = null;
            
            m_defaultEnum        = 0;
            m_enumValues         = null;
        }

        public ClusterJobProfileItem()
        {
            _JobProfileItem();
        }

        public ClusterJobProfileItem(PropertyId pid, bool readOnly)
        {
            _JobProfileItem();
        
            this.PropId = pid;

            m_readonly = readOnly;
        }

        // Clone the local object
        private ClusterJobProfileItem(ClusterJobProfileItem item)
        {
            CopyDataFrom(item);
        }

        private ClusterJobProfileItem(ClusterJobProfileItem item, PropertyId pid)
        {
            CopyDataFrom(item);
            this.PropId = pid;
        }

        private void CopyDataFrom(ClusterJobProfileItem item)
        {
            this.PropId         = item.PropId;
            m_readonly          = item.m_readonly;
            m_minVal            = item.m_minVal;
            m_maxVal            = item.m_maxVal;
            m_defaultInt        = item.m_defaultInt;
            m_defaultBool       = item.m_defaultBool;
            m_boolValues        = item.m_boolValues;
            m_defaultString     = item.m_defaultString;
            m_stringValues      = item.m_stringValues;
            m_defaultEnum       = item.m_defaultEnum;
            m_enumValues        = item.m_enumValues;
            m_requiredStrings   = item.m_requiredStrings;
        }

        public static ClusterJobProfileItem CreateDefaultItem(PropertyId pid)
        {
            ClusterJobProfileItem item;
            
            if (JobProfileItemFactoryDefaultTable.TryGetValue(pid, out item) ||
                JobProfileItemFactoryDefaultPrototypes.TryGetValue(pid.Type, out item))
            {
                return new ClusterJobProfileItem(item, pid);
            }

            // by default give a string item -- which can not be reached theorectically
            Debug.Assert(true, "The required pid type doesn't exist in the profile item prototypes");
            
            return new ClusterJobProfileItem(JobProfileItemFactoryDefaultPrototypes[StorePropertyType.String], pid);
        }

        public PropertyId PropId
        {
            get { return m_pid; }
            set { m_pid = value; }
        }

        public string PropName
        {
            get
            {
                if (m_pid == JobPropertyIds.ExpandedPriority)
                {
                    return JobPropertyIds.Priority.Name + " or " + JobPropertyIds.ExpandedPriority.Name;
                }
                return m_pid.Name;
            }
        }

        public bool ReadOnly
        {
            get { return m_readonly; }
            set { m_readonly = value; }
        }

        // JobPriority is no longer used here, keep it for V2 back comp
        public JobPriority PriorityDefaultValue
        {
            get { return (JobPriority)m_defaultEnum; }
            set { m_defaultEnum = (int)value; }
        }

        public JobUnitType UnitTypeDefaultValue
        {
            get { return (JobUnitType)m_defaultEnum; }
            set { m_defaultEnum = (int)value; }
        }

        public JobType TypeDefaultValue
        {
            get { return (JobType)m_defaultEnum; }
            set { m_defaultEnum = (int)value; }
        }

        public JobNodeGroupOp JobNodeGroupOpDefaultValue
        {
            get { return (JobNodeGroupOp)m_defaultEnum; }
            set { m_defaultEnum = (int)value; }
        }

        public int EnumDefaultValue
        {
            get { return m_defaultEnum; }
            set { m_defaultEnum = value; }
        }

        public JobOrderByList OrderbyDefaultValue
        {
            get { return JobOrderByList.Parse(m_defaultString); }
            set { m_defaultString = value.ToString(); }
        }

        public Int32 IntDefaultValue
        {
            get { return m_defaultInt; }
            set { m_defaultInt = value; }
        }

        public Int32 IntMaxValue
        {
            get { return m_maxVal; }
            set { m_maxVal = value; }
        }

        public Int32 IntMinValue
        {
            get { return m_minVal; }
            set { m_minVal = value; }
        }

        public bool BoolDefaultValue
        {
            get { return m_defaultBool; }
            set { m_defaultBool = value; }
        }

        public string StringDefaultValue
        {
            get { return m_defaultString; }
            set 
            {                 
                if (value != null && value.IndexOf(',') != -1)
                {
                    throw new SchedulerException(SR.StringCantContainComma);
                }
                m_defaultString = value;
            }
        }

        public string[] StringListDefaultValue
        {
            get
            {
                if (m_defaultString == null)
                {
                    return null;
                }

                return m_defaultString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
            
            set
            {
                if (value != null)
                {
                    m_defaultString = Enumerable2String(value);
                }
                else
                {
                    m_defaultString = null;
                }               
            }
        }

        public string[] StringValues 
        {
            get { return m_stringValues; }
            set 
            {                 
                if (value != null)
                {
                    foreach (string testString in value)
                    {
                        if (testString != null && testString.IndexOf(',') != -1)
                        {
                            throw new SchedulerException(SR.StringCantContainComma);
                        }
                    }
                }
                m_stringValues = value;
            }
        }

        public string[] RequiredStrings
        {
            get { return m_requiredStrings; }
            set { m_requiredStrings = value; }
        }

        public bool[] BoolValues
        {
            get { return m_boolValues; }
            set { m_boolValues = value; }
        }

        // JobPriority is no longer used here, keep it for V2 back comp
        public JobPriority[] PriorityValues
        {
            get 
            {
                if (m_enumValues == null)
                {
                    return null;
                }

                List<JobPriority> results = new List<JobPriority>();
            
                foreach (int intValue in m_enumValues)
                {
                    results.Add((JobPriority)intValue);
                }
            
                return results.ToArray(); 
            }
            set 
            {
                
                List<int> results = new List<int>();
                
                foreach (JobPriority enumValue in value)
                {
                    results.Add((int)enumValue);
                }
                
                m_enumValues = results.ToArray(); 
            }
        }

        public JobType[] TypeValues
        {
            get
            {
                if (m_enumValues == null)
                {
                    return null;
                }

                List<JobType> results = new List<JobType>();
                
                foreach (int intValue in m_enumValues)
                {
                    results.Add((JobType)intValue);
                }
                
                return results.ToArray();
            }
            set
            {
                List<int> results = new List<int>();
                
                foreach (JobType enumValue in value)
                {
                    results.Add((int)enumValue);
                }
                
                m_enumValues = results.ToArray();
            }
        }

        public JobUnitType[] UnitTypeValues
        {
            get
            {
                if (m_enumValues == null)
                {
                    return null;
                }

                List<JobUnitType> results = new List<JobUnitType>();
                
                foreach (int intValue in m_enumValues)
                {
                    results.Add((JobUnitType)intValue);
                }
                
                return results.ToArray();
            }
            set
            {
                List<int> results = new List<int>();
                
                foreach (JobUnitType enumValue in value)
                {
                    results.Add((int)enumValue);
                }
                
                m_enumValues = results.ToArray();
            }
        }

        public JobNodeGroupOp[] JobNodeGroupOpValues
        {
            get
            {
                if (m_enumValues == null)
                {
                    return null;
                }

                List<JobNodeGroupOp> results = new List<JobNodeGroupOp>();

                foreach (int intValue in m_enumValues)
                {
                    results.Add((JobNodeGroupOp)intValue);
                }

                return results.ToArray();
            }
            set
            {
                List<int> results = new List<int>();

                foreach (JobNodeGroupOp enumValue in value)
                {
                    results.Add((int)enumValue);
                }

                m_enumValues = results.ToArray();
            }
        }


        public int[] EnumValues
        {
            get { return m_enumValues; }
            set { m_enumValues = value; }
        }

        internal void ValidateJobPropertyWithThrow(StoreProperty jobProperty)
        {
            bool found = false;
            string valStr = null;

            switch (m_pid.Type)
            {
                case StorePropertyType.Boolean:
                    
                    bool valBool = (bool)jobProperty.Value;

                    if (this.m_boolValues == null ||
                        this.m_boolValues.Length == 0)
                    {
                        // Do not validate if the valid value is null or empty
                        return;
                    }

                    foreach (bool validValue in this.m_boolValues)
                    {
                        if (validValue == valBool)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        throw new SchedulerException(ErrorCode.Operation_JobTemplateValueInvalid, this.PropName);
                    }

                    break;
                    
                    
                case StorePropertyType.Int32:
                case StorePropertyType.Int64:
                    
                    Int32 valInt = (Int32)jobProperty.Value;

                    if (valInt < IntMinValue)
                    {
                        throw new SchedulerException(ErrorCode.Operation_JobTemplateValueTooSmall, ErrorCode.MakeErrorParams(this.PropName, IntMinValue.ToString()));
                    }
                    else if (valInt > IntMaxValue)
                    {
                        throw new SchedulerException(ErrorCode.Operation_JobTemplateValueTooLarge, ErrorCode.MakeErrorParams(this.PropName, IntMaxValue.ToString()));
                    }
                    break;
                    
                    
                case StorePropertyType.String:
                    
                    valStr = jobProperty.Value as string;
                    if(string.IsNullOrEmpty(valStr))
                    {
                        return;
                    }

                    string[] valueStrings = valStr.Split(new char[1]{','}, StringSplitOptions.RemoveEmptyEntries);

                    if (m_stringValues == null ||
                        m_stringValues.Length == 0)
                    {
                        // Do not validate if the valid value is null or empty
                        return;
                    }

                    foreach (string tmpValue in valueStrings)
                    {
                        // Check every string in the job term
                        // All of them must be within the strings defined in the job profile

                        found = false;
                        foreach (string validValue in m_stringValues)
                        {
                            if (validValue.Equals(tmpValue, StringComparison.InvariantCultureIgnoreCase))
                            {
                                found = true;
                                break;
                            }
                        }
                        
                        if (!found)
                        {
                            throw new SchedulerException(ErrorCode.Operation_JobTemplateValueInvalid, this.PropName);
                        }
                    }

                    break;
                    
                case StorePropertyType.StringList:

                    valStr = jobProperty.Value as string;
                    if (string.IsNullOrEmpty(valStr))
                    {
                        return;
                    }

                    if (m_pid == JobPropertyIds.RequestedNodes ||
                        m_pid == JobPropertyIds.SoftwareLicense)
                    {
                        // The valid values must cover every input values

                        if (m_stringValues == null)
                        {
                            return;
                        }

                        if (!StringListContain(m_stringValues, valStr.Split(',')))
                        {
                            throw new SchedulerException(ErrorCode.Operation_JobTemplateValueInvalid, this.PropName);
                        }
                    }
                    else if (m_pid == JobPropertyIds.NodeGroups)
                    {                                                
                        if (m_requiredStrings != null && m_requiredStrings.Length >= 0)                       
                        {
                            // The input values must cover every required values
                            if (!StringListContain(valStr.Split(','), m_requiredStrings))
                            {
                                throw new SchedulerException(ErrorCode.Operation_JobTemplateRequiredValueMissing,
                                    ErrorCode.MakeErrorParams(this.PropName, m_requiredStrings[0]));
                            }
                        }

                        if (m_stringValues != null)
                        {
                            // The valid values must cover every input value
                            if (!StringListContain(m_stringValues, valStr.Split(',')))
                            {
                                throw new SchedulerException(ErrorCode.Operation_JobTemplateValueInvalid, this.PropName);
                            }
                        }

                        return;
                    }
                    break;
                    
                case StorePropertyType.JobType:
                case StorePropertyType.JobUnitType:
                case StorePropertyType.JobNodeGroupOp:
                    
                    int valEnum = (int)jobProperty.Value;

                    if (m_enumValues == null ||
                        m_enumValues.Length == 0)
                    {
                        // Do not validate if the valid value is null or empty
                        return;
                    }
                    
                    found = m_enumValues.Contains(valEnum);
                    // if valid JobUnitType is Gpu, but actual unit type is Socket, should pass the validation, as GPU job's unit type is Socket in V4SP5
                    if (!found && m_pid.Type == StorePropertyType.JobUnitType && ((JobUnitType)jobProperty.Value) == JobUnitType.Socket)
                    {
                        found = m_enumValues.Contains((int)JobUnitType.Gpu);
                    }

                    if (!found)
                    {
                        throw new SchedulerException(ErrorCode.Operation_JobTemplateValueInvalid, this.PropName);
                    }
                    
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Return true if string list A contains all the values in string list B. 
        /// Assume A and B are all not null
        /// </summary>
        /// <param name="listA"></param>
        /// <param name="listB"></param>
        /// <returns></returns>
        internal static bool StringListContain(string[] listA, string[] listB)
        {
            Debug.Assert(listA != null);
            Debug.Assert(listB != null);

            foreach (string bstr in listB)
            {
                if (string.IsNullOrEmpty(bstr))
                {
                    continue;
                }

                bool found = false;
                foreach (string astr in listA)
                {
                    if (string.IsNullOrEmpty(astr))
                    {
                        continue;
                    }

                    if (string.Compare(astr, bstr, StringComparison.InvariantCultureIgnoreCase) == 0)
                    {
                        found = true;
                    }
                }
                if (!found)
                    return false;
            }

            return true;
        }

        public StoreProperty GetDefaultProperty()
        {
            switch (PropId.Type)
            {
                case StorePropertyType.Int32:
                    return new StoreProperty(PropId, IntDefaultValue);

                case StorePropertyType.Boolean:
                    return new StoreProperty(PropId, BoolDefaultValue);

                case StorePropertyType.String:
                case StorePropertyType.StringList:
                    return new StoreProperty(PropId, StringDefaultValue);

                case StorePropertyType.JobUnitType:
                    return new StoreProperty(PropId, UnitTypeDefaultValue);

                case StorePropertyType.JobType:
                    return new StoreProperty(PropId, TypeDefaultValue);

                case StorePropertyType.JobOrderby:
                    try
                    {
                        return new StoreProperty(PropId, OrderbyDefaultValue);
                    }
                    catch { }
                    return null;
                case StorePropertyType.JobNodeGroupOp:
                    return new StoreProperty(PropId, JobNodeGroupOpDefaultValue);
            }
            return null;
        }

        public override string ToString()
        {
            if (m_pid == JobPropertyIds.NA)
            {
                return "no settings";
            }

            StringBuilder bldr = new StringBuilder(80);
            
            bldr.Append(m_pid.ToString());

            bldr.Append(" readonly:");
            bldr.Append(m_readonly);

            switch (m_pid.Type)
            {
                case StorePropertyType.Int32:
                    if (m_pid == JobPropertyIds.ExpandedPriority)
                    {
                        bldr.Append(" default:");
                        bldr.Append(ExpandedPriority.ToString(m_defaultInt));
                        bldr.Append(" min:");
                        bldr.Append(ExpandedPriority.ToString(m_minVal));
                        bldr.Append(" max:");
                        bldr.Append(ExpandedPriority.ToString(m_maxVal));
                    }
                    else
                    {
                        bldr.Append(" default:");
                        bldr.Append(m_defaultInt);
                        bldr.Append(" min:");
                        bldr.Append(m_minVal);
                        bldr.Append(" max:");
                        bldr.Append(m_maxVal);
                    }
                    break;
                    
                case StorePropertyType.Boolean:
                    bldr.Append(" default:");
                    bldr.Append(m_defaultBool);
                    break;
                    
                case StorePropertyType.String:
                case StorePropertyType.StringList:
                    bldr.Append(" default:");
                    bldr.Append(m_defaultString);
                    break;
                    
                case StorePropertyType.JobUnitType:
                    bldr.Append(" default:");
                    bldr.Append((JobUnitType)m_defaultEnum);
                    break;
                    
                case StorePropertyType.JobType:
                    bldr.Append(" default:");
                    bldr.Append((JobType)m_defaultEnum);
                    break;

                case StorePropertyType.JobOrderby:
                    bldr.Append(" default:");
                    bldr.Append(m_defaultString);
                    break;
            }
            
            return bldr.ToString();

        }

        public void PersistToXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(XmlNames.ProfileItem);

            writer.WriteAttributeString(XmlNames.PropertyName, m_pid.ToString());

            //writer.WriteAttributeString(XmlNames.ReadOnly, m_readonly.ToString());

            bool first = true;

            switch (m_pid.Type)
            {
                case StorePropertyType.Boolean:
                
                    writer.WriteAttributeString(XmlNames.Default, m_defaultBool.ToString());
                
                    StringBuilder boolValueRange = new StringBuilder();

                    if (m_boolValues != null)
                    {
                        foreach (bool valueBool in m_boolValues)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                boolValueRange.Append(",");
                            }
                            boolValueRange.Append(valueBool);
                        }
                    }
                
                    writer.WriteAttributeString(XmlNames.ValueRange, boolValueRange.ToString());
                
                    break;
                    
                    
                case StorePropertyType.Int32:
                case StorePropertyType.Int64:
                
                    writer.WriteAttributeString(XmlNames.Default, m_defaultInt.ToString());
                    writer.WriteAttributeString(XmlNames.MinVal, m_minVal.ToString());
                    writer.WriteAttributeString(XmlNames.MaxVal, m_maxVal.ToString());
                
                    break;
                    
                    
                case StorePropertyType.String:
                case StorePropertyType.StringList:
                    
                    writer.WriteAttributeString(XmlNames.Default, m_defaultString);
                    
                    StringBuilder strValueRange = new StringBuilder();

                    if (m_stringValues != null)
                    {
                        foreach (string item in m_stringValues)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                strValueRange.Append(",");
                            }

                            strValueRange.Append(item);
                        }
                    }
                    
                    writer.WriteAttributeString(XmlNames.ValueRange, strValueRange.ToString());

                    strValueRange.Remove(0, strValueRange.Length);
                    
                    if (m_requiredStrings != null)
                    {
                        strValueRange.Remove(0, strValueRange.Length);
                        
                        first = true;
                        
                        foreach (string val in m_requiredStrings)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                strValueRange.Append(",");
                            }
                            
                            strValueRange.Append(val);
                        }
                    }

                    writer.WriteAttributeString(XmlNames.RequiredValues, strValueRange.ToString());

                    break;
                    
                    
                case StorePropertyType.JobUnitType:
                case StorePropertyType.JobType:
                case StorePropertyType.JobNodeGroupOp:
                    
                    if (!_PropertyTypeMapping.ContainsKey(m_pid.Type))
                    {
                        throw new FormatException("Unknown profile item: " + m_pid.Type.ToString());
                    }
                    
                    writer.WriteAttributeString(XmlNames.Default, Enum.GetName(_PropertyTypeMapping[m_pid.Type], m_defaultEnum));

                    StringBuilder enumValueRange = new StringBuilder();

                    if (m_enumValues != null)
                    {
                        foreach (int valueEnum in m_enumValues)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                enumValueRange.Append(",");
                            }

                            enumValueRange.Append(Enum.GetName(_PropertyTypeMapping[m_pid.Type], valueEnum));
                        }
                    }
                    
                    writer.WriteAttributeString(XmlNames.ValueRange, enumValueRange.ToString());
                    
                    break;
                    
                    
                case StorePropertyType.JobOrderby:
                    
                    writer.WriteAttributeString(XmlNames.Default, m_defaultString);
                    break;
                    
                    
                default:
                    break;
            }


            writer.WriteEndElement();
        }


        public static ClusterJobProfileItem RestoreFromXml(System.Xml.XmlReader reader)
        {
            string propName = reader.GetAttribute(XmlNames.PropertyName);

            PropertyId pid = PropertyLookup.Job.PropertyIdFromPropName(propName);

            bool readOnly = false;

            if (pid == null)
            {
                throw new ArgumentException(string.Format("{0} is not a valid Job property", propName));
            }

            readOnly = (reader.GetAttribute(XmlNames.ReadOnly) == "True");

            ClusterJobProfileItem item = new ClusterJobProfileItem(pid, readOnly);

            try
            {
                switch (pid.Type)
                {
                    case StorePropertyType.Boolean:
                        item.BoolDefaultValue = bool.Parse(reader.GetAttribute(XmlNames.Default));

                        string boolValueRange = reader.GetAttribute(XmlNames.ValueRange);
                        if (string.IsNullOrEmpty(boolValueRange))
                        {
                            break;
                        }

                        string[] boolValueStrs = boolValueRange.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        List<bool> valueBools = new List<bool>();
                        foreach (string valueStr in boolValueStrs)
                        {
                            valueBools.Add(bool.Parse(valueStr));
                        }
                        item.BoolValues = valueBools.ToArray();
                        break;
                    
                    
                    case StorePropertyType.Int32:
                    case StorePropertyType.Int64:
                        item.IntDefaultValue = Int32.Parse(reader.GetAttribute(XmlNames.Default));
                        item.IntMaxValue = Int32.Parse(reader.GetAttribute(XmlNames.MaxVal));
                        item.IntMinValue = Int32.Parse(reader.GetAttribute(XmlNames.MinVal));

                        if (item.IntDefaultValue > item.IntMaxValue ||
                            item.IntDefaultValue < item.IntMinValue)
                        {
                            throw new ArgumentException("Integer template item's default value must between its min and max.");
                        }

                        if (pid == JobPropertyIds.ExpandedPriority)
                        {
                            if (item.IntMinValue < ExpandedPriority.Lowest)
                            {
                                throw new ArgumentException(string.Format("ExpandedPriority template item's min value cannot be less than {0}.", ExpandedPriority.Lowest));
                            }

                            if (item.IntMaxValue > ExpandedPriority.Highest)
                            {
                                throw new ArgumentException(string.Format("ExpandedPriority template item's max value cannot be greater than {0}.", ExpandedPriority.Highest));
                            }
                        }
                        break;


                    case StorePropertyType.String:
                        item.StringDefaultValue = reader.GetAttribute(XmlNames.Default);
                        {
                            string strValueRange = reader.GetAttribute(XmlNames.ValueRange);
                            if (!string.IsNullOrEmpty(strValueRange))
                            {
                                item.StringValues = strValueRange.Split(new char[] { ',' });
                            }

                            string strRequired = reader.GetAttribute(XmlNames.RequiredValues);
                            if (!string.IsNullOrEmpty(strRequired))
                            {
                                item.RequiredStrings = strRequired.Split(',');
                            }
                        }

                        break;
                    case StorePropertyType.StringList:
                        string strDefault = reader.GetAttribute(XmlNames.Default);
                        if (strDefault == null)
                        {
                            item.StringListDefaultValue = null;
                        }else{
                            item.StringListDefaultValue = strDefault.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        
                            string strValueRange = reader.GetAttribute(XmlNames.ValueRange);
                            if (!string.IsNullOrEmpty(strValueRange))
                            {
                                item.StringValues = strValueRange.Split(new char[] { ',' });
                            }

                            string strRequired = reader.GetAttribute(XmlNames.RequiredValues);
                            if (!string.IsNullOrEmpty(strRequired))
                            {
                                item.RequiredStrings = strRequired.Split(',');
                            }
                        }

                        // Below is for V3 bug 7852.
                        // We should ignore importing invalid software license formats from XML
                        // We cannot stop importing the whole template because in V2 we didn't
                        // check whether the format is correct, so there are some V2 template
                        // having invalid formats and those templates have to been imported to 
                        // V3 on upgrading.
                        if (pid == JobPropertyIds.SoftwareLicense)
                        {
                            if (item.StringListDefaultValue != null)
                            {
                                foreach (string str in item.StringListDefaultValue)
                                {
                                    if (!ValidateSoftwareLicense(str))
                                    {
                                        return null;
                                    }
                                }
                            }

                            if (item.StringValues != null)
                            {
                                foreach (string str in item.StringValues)
                                {
                                    if (!ValidateSoftwareLicense(str))
                                    {
                                        return null;
                                    }
                                }
                            }
                        }

                        break;
                    
                    
                    case StorePropertyType.JobUnitType:
                    case StorePropertyType.JobType:
                    case StorePropertyType.JobNodeGroupOp:
                        if (!_PropertyTypeMapping.ContainsKey(pid.Type))
                        {
                            throw new FormatException("Unknow profile item: " + pid.Type.ToString());
                        }

                        item.EnumDefaultValue = (int)Enum.Parse(_PropertyTypeMapping[pid.Type], reader.GetAttribute(XmlNames.Default));

                        string enumValueRange = reader.GetAttribute(XmlNames.ValueRange);
                        if (string.IsNullOrEmpty(enumValueRange))
                        {
                            break;
                        }

                        string[] enumValueStrs = enumValueRange.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        List<int> valueEnums = new List<int>();
                        foreach (string valueStr in enumValueStrs)
                        {
                            valueEnums.Add((int)Enum.Parse(_PropertyTypeMapping[pid.Type], valueStr));
                        }
                        item.EnumValues = valueEnums.ToArray();
                        break;

                    case StorePropertyType.JobPriority:

                        // Translate the Priority item in V2 template to ExpandedPriority item
                        
                        int defaultValue = (int)Enum.Parse(typeof(JobPriority), reader.GetAttribute(XmlNames.Default));
                        int exPriDefault = ExpandedPriority.JobPriorityToExpandedPriority(defaultValue);

                        int max = (int)JobPriority.Lowest;
                        int exPriMax = ExpandedPriority.Lowest;

                        string priValueRange = reader.GetAttribute(XmlNames.ValueRange);
                        if (!string.IsNullOrEmpty(priValueRange))
                        {
                            string[] priValueStrs = priValueRange.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string valueStr in priValueStrs)
                            {
                                int tmp = (int)Enum.Parse(typeof(JobPriority), valueStr);
                                if (tmp > max)
                                {
                                    max = tmp;
                                }
                            }
                            exPriMax = ExpandedPriority.JobPriorityToExpandedPriority(max);
                        }
                        else 
                        {
                            // Fall back to the factory setting
                            exPriMax = JobProfileItemFactoryDefaultTable[JobPropertyIds.ExpandedPriority].IntMaxValue;
                        }
                        
                        item = new ClusterJobProfileItem(JobPropertyIds.ExpandedPriority, readOnly);
                        
                        item.IntDefaultValue = exPriDefault;
                        item.IntMaxValue = exPriMax;
                        item.IntMinValue = ExpandedPriority.Lowest;
                        break;
                    
                    case StorePropertyType.JobOrderby:
                        item.StringDefaultValue = reader.GetAttribute(XmlNames.Default);
                        break;
                    
                    
                    default:
                        break;
                }
            }
            catch 
            {
                throw new SchedulerException(ErrorCode.Operation_InvalidJobTemplateItemXml, propName);
            }

            return item;
        }

        // Check if the format is "xxx:digits|*"
        private static bool ValidateSoftwareLicense(string licenseStr)
        {
            if (string.IsNullOrEmpty(licenseStr))
            {
                return true;
            }

            string[] strs = licenseStr.Split(new char[]{':'}, StringSplitOptions.RemoveEmptyEntries);

            if (strs.Length != 2)
            {
                return false;
            }

            int count;
            if (!int.TryParse(strs[1].Trim(), out count) && 
                string.Compare(strs[1].Trim(), "*") != 0)
            {
                // If it is neither a number or *
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create a boolean typed profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultValue"></param>
        /// <param name="validValues"> A null value means both True and False are all acceptable. </param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateBooleanProfileItem(
            PropertyId pid,
            bool readOnly,
            bool defaultValue,
            params bool[] validValues)
        {

            if (pid.Type != StorePropertyType.Boolean)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.BoolDefaultValue = defaultValue;
            resultItem.BoolValues = validValues;

            return resultItem;
        }

        /// <summary>
        /// Create an integer typed profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateIntProfileItem(
            PropertyId pid,
            bool readOnly,
            int defaultValue,
            int minValue,
            int maxValue)
        {
            if (pid.Type != StorePropertyType.Int32 &&
                pid.Type != StorePropertyType.Int64)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.IntDefaultValue = defaultValue;
            resultItem.IntMinValue = minValue;
            resultItem.IntMaxValue = maxValue;

            return resultItem;
        }

        /// <summary>
        /// Create a string typed profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultString"></param>
        /// <param name="validValues"> Null means all values are acceptable. </param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateStringProfileItem(
            PropertyId pid,
            bool readOnly,
            string defaultString,
            params string[] validValues)
        {
            if (pid.Type != StorePropertyType.String)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.StringDefaultValue = defaultString;
            resultItem.StringValues = validValues;

            return resultItem;
        }


        /// <summary>
        /// Create a string typed profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultString"></param>
        /// <param name="validValues"> Null means all values are acceptable. </param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateStringProfileItem(
            PropertyId pid,
            bool readOnly,
            string defaultString,
            string[] validValues,
            string[] requiredValues)
        {
            if (pid.Type != StorePropertyType.String)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.StringDefaultValue = defaultString;
            resultItem.StringValues = validValues;
            resultItem.RequiredStrings = requiredValues;

            return resultItem;
        }

        /// <summary>
        /// Create a string typed profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultString"></param>
        /// <param name="validValues"> Null means all values are acceptable. </param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateStringListProfileItem(
            PropertyId pid,
            bool readOnly,
            string[] defaultValues,
            string[] validValues,
            string[] requiredValues)
        {
            if (pid.Type != StorePropertyType.StringList)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);

            if (defaultValues == null)
                resultItem.StringListDefaultValue = null;
            else 
                resultItem.StringListDefaultValue = defaultValues;
            
            resultItem.StringValues = validValues;
            resultItem.RequiredStrings = requiredValues;

            return resultItem;
        }



        /// <summary>
        /// Create a JobOrderby profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultOrder"></param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateJobOrderbyProfileItem(
            PropertyId pid,
            bool readOnly,
            JobOrderByList defaultOrder)
        {
            if (pid.Type != StorePropertyType.JobOrderby)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.StringDefaultValue = defaultOrder.ToString();

            return resultItem;
        }

        /// <summary>
        /// Create an enumeration profile item.
        /// </summary>
        /// <param name="pid"></param>
        /// <param name="defaultEnum"></param>
        /// <param name="validValues"> Null means all values are acceptable. </param>
        /// <returns></returns>
        public static ClusterJobProfileItem CreateEnumProfileItem(
            PropertyId pid,
            bool readOnly,
            int defaultEnum,
            params int[] validValues)
        {
            if (pid.Type != StorePropertyType.JobType &&
                pid.Type != StorePropertyType.JobUnitType && 
                pid.Type != StorePropertyType.JobNodeGroupOp)
            {
                throw new SchedulerException(ErrorCode.Operation_ProfileItemTypeInconsistent, pid.Name);
            }

            ClusterJobProfileItem resultItem = new ClusterJobProfileItem(pid, readOnly);
            resultItem.EnumDefaultValue = defaultEnum;
            resultItem.EnumValues = validValues;

            return resultItem;
        }

        static internal string Enumerable2String(IEnumerable<string> enumer)
        {
            bool first = true;
            StringBuilder builder = new StringBuilder();
            foreach (string item in enumer)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(',');
                }
                if (item != null && item.IndexOf(',') != -1)
                {
                    throw new SchedulerException(SR.StringCantContainComma);
                }
                builder.Append(item);
            }

            return builder.ToString();
        }


    }

}
