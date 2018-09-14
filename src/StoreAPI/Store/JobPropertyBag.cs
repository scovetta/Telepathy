using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

using Microsoft.Hpc.Scheduler.Properties;
using System.Diagnostics;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class PropertyBag
    {
        protected Dictionary<PropertyId, StoreProperty>   _props = new Dictionary<PropertyId,StoreProperty>();
        protected Dictionary<string, string>              _customProps = new Dictionary<string,string>();

        internal List<XMLEnabledPropertyConverter> _v2PropConverters = new List<XMLEnabledPropertyConverter>();

        internal IEnumerable<StoreProperty> GetItemProperties(ISchedulerStore store, ObjectType obType)
        {
            List<StoreProperty> result = new List<StoreProperty>();
            
            result.AddRange(_props.Values);
            
            if (_customProps.Count > 0)
            {
                result.AddRange(ResolveCustomProperties(_customProps, store, obType));
            }
            
            return result;
        }

        internal static List<StoreProperty> ResolveCustomProperties(Dictionary<string, string> map, ISchedulerStore store, ObjectType obType)
        {
            List<StoreProperty> result = new List<StoreProperty>();
            List<string> names = new List<string>();
            
            foreach (KeyValuePair<string, string> mapitem in map)
            {
                result.Add(new StoreProperty(mapitem.Key, mapitem.Value));
                names.Add(mapitem.Key);
            }
            
            StorePropertyDescriptor[] pids = (StorePropertyDescriptor[])store.GetPropertyDescriptors(names.ToArray(), obType);
            
            int i;
            
            for (i = 0; i < result.Count; i++)
            {
                if (pids[i].PropId != StorePropertyIds.Error)
                {
                    result[i].Id = pids[i].PropId;
                }
                else
                {
                    result[i].Id = store.CreatePropertyId(obType, StorePropertyType.String, names[i], names[i]);
                }
            }
            
            return result;
        }

        internal void ReadCustomProps(XmlReader reader)
        {
            ReadExtendedTerms(reader, XmlNames.CustomProps);            
        }


        // Add in obsolete propeties for compatibility with previous versions
        internal void PreWriteProps_Convert(XmlExportOptions flags)
        {
            // Always try to add the older properties, so that the XML can be read by previous versions of the Store API            
            foreach (XMLEnabledPropertyConverter converter in _v2PropConverters)
            {
                PropertyId originalId = converter.GetPropId();                

                if (_props.ContainsKey(originalId))
                {
                    StoreProperty[] replacements = converter.ConvertXMLWriteProp(_props[originalId]);
                    if (replacements != null)
                    {
                        foreach (StoreProperty prop in replacements)
                        {
                            if (!_props.ContainsKey(prop.Id))
                            {
                                _props.Add(prop.Id, prop);
                            }
                        }
                    }
                }
            }
        }

        // Convert any deprecated properties that may appear in the XML back to the latest Store properties
        internal void PostReadProps_Deconvert()
        {
            List<PropertyId> propIdsToRemove = new List<PropertyId>();

            foreach (XMLEnabledPropertyConverter converter in _v2PropConverters)
            {               
                PropertyId originalId = converter.GetPropId();
                PropertyId[] replacementIds = converter.GetXMLReplacementPropIds();

                // See if we have all the replacements in the bag, and iff so, invoke the de-converter to obtain
                // the original property.
                bool haveOriginal = _props.ContainsKey(originalId);
                if (!haveOriginal)
                {
                    StoreProperty[] replacements = PropertyLookup.LookupAllPropIds<StoreProperty>(_props, replacementIds);                
                    if (replacements != null)
                    {
                        _props.Add(originalId, converter.DeconvertXMLReadProps(replacements));
                        haveOriginal = true;
                    }
                }

                // Now, see if we need to remove the replacement IDs from the bag.  Don't do this yet,
                // since the replacement properties may still be needed by other converters.
                if (haveOriginal && converter.NeedsToRemoveReplacementPropsAfterXMLDeconvert())
                {
                    propIdsToRemove.AddRange(replacementIds);
                }
            }

            // Remove all the replacement properties slated for removal
            foreach (PropertyId id in propIdsToRemove)
            {
                _props.Remove(id);
            }
        }

        
        internal enum ReadingNameOrValue { None = 0, ReadingName = 1, ReadingValue };

        //Method used to read a name value pair from the xmlreader. 
        //The endElementName parameter specifies the element to which this name value pair belongs
        protected internal void ReadNameValuePair(XmlReader reader, string endElementName, out string name, out string value)
        {
            bool quitloop = false;
            ReadingNameOrValue readState = ReadingNameOrValue.None;
            name = null;
            value = null;

            //Read the next token
            while (reader.Read())
            {                
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == endElementName)
                        {/**
                          * *We are done reading this name value pair
                          */                            
                            quitloop = true;
                        }
                        break;
                    case XmlNodeType.Element:
                        // If the token is an element, it could be either the start of a name or value tag
                        if (reader.LocalName == XmlNames.Name)
                        {
                            readState = ReadingNameOrValue.ReadingName;
                        }
                        if (reader.LocalName ==  XmlNames.Value)
                        {
                            readState = ReadingNameOrValue.ReadingValue;
                        }
                        break;
                    case XmlNodeType.Text:
                        //If we began reading a name tag this text is the name.                        
                        if (readState == ReadingNameOrValue.ReadingName)
                        {
                            name = reader.Value.Trim();
                        }
                        //If we began reading a value tag this text is the value
                        if (readState == ReadingNameOrValue.ReadingValue)
                        {
                            value = reader.Value.Trim();
                        }
                        readState = ReadingNameOrValue.None;
                        break;
                    default:
                        //whitespace
                        break;
                }
                if (quitloop)
                {
                    break;
                }
            }
        }

        internal void ReadExtendedTerms(XmlReader reader)
        {        
            ReadExtendedTerms(reader,XmlNames.ExtendedTerms );
        }

        private void ReadExtendedTerms(XmlReader reader, string endElement)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == endElement)
                {
                    break;
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Term)
                {
                    string name;
                    string value;

                    ReadNameValuePair(reader, XmlNames.Term, out name, out value);

                    if (name != null && value != null)
                    {
                        _customProps[name] = value;
                    }
                }
            }
        }
        
        public void SetProperties(IEnumerable<StoreProperty> props)
        {
            _props.Clear();
            _customProps.Clear();

            if (props != null)
            {
                foreach (StoreProperty prop in props)
                {
                    _props[prop.Id] = prop;
                }
            }
        }
        
        protected void WritePropertyXml(XmlWriter writer, XmlExportOptions flags)
        {
            List<PropertyId> exportedProps = new List<PropertyId>();
            
            Dictionary<string, string> customProps = new Dictionary<string,string>();
            
            writer.WriteAttributeString(XmlNames.Version, XmlNames.VersionValue);

            foreach (StoreProperty prop in _props.Values)
            {
                bool fExport = false;
                
                if (prop.Value is byte[] || prop.Value is Dictionary<string, string>)
                {
                    // Don't export this one.
                }
                else if ((prop.Id.Flags & PropFlags.Custom) != 0)
                {
                    customProps[prop.Id.Name] = prop.Value.ToString();
                }
                else if ((flags & XmlExportOptions.AllProperties) != 0)
                {
                    fExport = true;
                }
                else if ((prop.Id.Flags & PropFlags.Calculated) == 0)
                {
                    fExport = true;
                }

                if (exportedProps.Contains(prop.Id))
                {
                    fExport = false;
                }
                
                if (fExport)
                {                   
                    if (prop.Id.Type == StorePropertyType.AllocationList)
                    {
                        //if a property is an allocation list
                        // the list has to be traversed to generate valid allocation lists
                        StringBuilder bldr = new StringBuilder();
                        ICollection<KeyValuePair<string, int>> allocationList = prop.Value as ICollection<KeyValuePair<string, int>>;
                        int listLen = allocationList.Count;
                        int count=0;
                        
                        foreach (KeyValuePair<string, int> allocation in allocationList)
                        {
                            bldr.Append(allocation.Key);

                            if (count != listLen - 1)
                            {
                                bldr.Append(",");
                            }
                            count++;
                        }
                        writer.WriteAttributeString(prop.Id.Name, bldr.ToString());
                    }
                    else if (prop.Id == JobPropertyIds.ExpandedPriority)
                    {
                        // This is a special case: instead of the interger, we print the human-readable string
                        // The property will also be named "Priority" instead of "ExpandedPriority"
                        writer.WriteAttributeString(PriorityXmlPropertyHandler.XmlPropName, ExpandedPriority.ToString((int)prop.Value));
                    }                    
                    else
                    {
                        writer.WriteAttributeString(prop.Id.Name, ToXmlValue(prop.Value));
                    }
                    exportedProps.Add(prop.Id);
                }
            }
            
            // Export any custom properties

            if (customProps.Count > 0)
            {
                writer.WriteStartElement(XmlNames.ExtendedTerms);
                
                foreach (KeyValuePair<string, string> item in customProps)
                {
                    writer.WriteStartElement(XmlNames.Term);

                    writer.WriteElementString(XmlNames.Name, item.Key);
                    writer.WriteElementString(XmlNames.Value, item.Value);
                    
                    writer.WriteEndElement();
                }
                
                writer.WriteEndElement(); // Custom properties
            }
        }

        protected void PruneProperties()
        {
            //create a list of properties that we do not need to write out
            List<PropertyId> propsToRemoveList = new List<PropertyId>(5);

            //if some properties do not exist we should remove the error property caused by those
            propsToRemoveList.Add(StorePropertyIds.Error);

            // if the version of the server is V2, "Priority" and "ExpandedPriority" are both passed back to client side, remove "Priority"
            // or else, two "Prioirty" properties will be written into the xml file resulting in an xml writting exception
            if (_props.ContainsKey(JobPropertyIds.Priority) && _props.ContainsKey(JobPropertyIds.ExpandedPriority))
            {
                propsToRemoveList.Add(JobPropertyIds.Priority);
            }

            StoreProperty unitTypeRow;
            JobUnitType unitType = JobUnitType.Core;
            if (_props.TryGetValue(StorePropertyIds.UnitType, out unitTypeRow))
            {
                unitType = (JobUnitType)unitTypeRow.Value;
            }    

            switch (unitType)
            {
                case JobUnitType.Core:
                    {
                        PropertyId[] propsToRemove = { StorePropertyIds.MinSockets, StorePropertyIds.MaxSockets, StorePropertyIds.MinNodes, StorePropertyIds.MaxNodes };
                        propsToRemoveList.AddRange(propsToRemove);
                        break;
                    }
                case JobUnitType.Socket:
                    {
                        PropertyId[] propsToRemove = { StorePropertyIds.MinCores, StorePropertyIds.MaxCores, StorePropertyIds.MinNodes, StorePropertyIds.MaxNodes };
                        propsToRemoveList.AddRange(propsToRemove); 
                        break;
                    }
                case JobUnitType.Node:
                    {
                        PropertyId[] propsToRemove = { StorePropertyIds.MinCores, StorePropertyIds.MaxCores, StorePropertyIds.MinSockets, StorePropertyIds.MaxSockets };
                        propsToRemoveList.AddRange(propsToRemove);
                        break;
                    }
            }

            RemoveFromBag(propsToRemoveList);
        }

        private void RemoveFromBag(IEnumerable<PropertyId> listToRemove)
        {
            foreach(PropertyId pid in listToRemove)
            {
                _props.Remove(pid);
            }
        }


        static string ToXmlValue(object value)
        {
            if (value is bool)
            {
                return value.ToString().ToLower();
            }            
            return value.ToString();
        }
    }


    public class JobPropertyBag : PropertyBag
    {
        List<TaskPropertyBag>                   _tasks = new List<TaskPropertyBag>();

        List<KeyValuePair<int, int>>            _taskDependencies = new List<KeyValuePair<int, int>>();

        public JobPropertyBag() : base()
        {
        }

        internal void CommitToJob(SchedulerStoreSvc store, IClusterJob job)
        {
           
            List<StoreProperty> props = new List<StoreProperty>(_props.Values);

            if (_customProps.Count > 0)
            {
                props.AddRange(ResolveCustomProperties(_customProps, store, ObjectType.Job));
            }            
            
            job.SetProps(props.ToArray());

            foreach (KeyValuePair<string, string> item in _envs)
            {
                job.SetEnvironmentVariable(item.Key, item.Value);
            }

            Dictionary<int, int> taskGroupIdMapping = new Dictionary<int,int>();

            IClusterTaskGroup rootGrp = job.GetRootTaskGroup();

            int rootGrpId = rootGrp.Id;

            if (store.ServerVersion.Version < VersionControl.V3SP4)
            {
                // This is for backward compatibility

                Dictionary<int, IClusterTaskGroup> taskGroupIdMappingOld = new Dictionary<int, IClusterTaskGroup>();
                ReconstructTaskGroups(rootGrp, _taskDependencies, taskGroupIdMappingOld);

                foreach (KeyValuePair<int, IClusterTaskGroup> item in taskGroupIdMappingOld)
                {
                    taskGroupIdMapping[item.Key] = item.Value.Id;
                }
            }
            else
            {
                ReconstructTaskGroupsEx(job, rootGrp, _taskDependencies, taskGroupIdMapping);
            }

            // Add the custom properties

            List<StoreProperty[]> tasksPropsList = new List<StoreProperty[]>(_tasks.Count);

            foreach (TaskPropertyBag task in _tasks)
            {                
                StoreProperty[] taskProps = task.GeneratePropsForTask(store, taskGroupIdMapping, rootGrpId);
                tasksPropsList.Add(taskProps);
            }

            List<IClusterTask> clusterTasks = job.CreateTasks(tasksPropsList);

           

            int taskIdx=0;
            foreach (TaskPropertyBag task in _tasks)
            {
                task.SetEnvVarsOnClusterTask(clusterTasks[taskIdx]);
                taskIdx++;
            }
           
        }

        static XmlPropertyHandlerCollection _map = null;
        
        void _InitMap()
        {
            if (_map == null)
            {
                PropertyId[] pids = 
                {
                    JobPropertyIds.RequestedNodes,
                    JobPropertyIds.IsExclusive,
                    JobPropertyIds.Name,                    
                    JobPropertyIds.ExpandedPriority,
                    JobPropertyIds.Project,
                    JobPropertyIds.RuntimeSeconds,
                    JobPropertyIds.RunUntilCanceled,
                    JobPropertyIds.SoftwareLicense,
                    JobPropertyIds.UnitType,
                    JobPropertyIds.FailOnTaskFailure,
                    JobPropertyIds.NodeGroups,
                    JobPropertyIds.OrderBy,
                    JobPropertyIds.MinCoresPerNode,
                    JobPropertyIds.MaxCoresPerNode,
                    JobPropertyIds.MinMemory,
                    JobPropertyIds.MaxMemory,
                    JobPropertyIds.AutoCalculateMax,
                    JobPropertyIds.AutoCalculateMin,
                    JobPropertyIds.NotifyOnStart,
                    JobPropertyIds.NotifyOnCompletion,
                    JobPropertyIds.EmailAddress,
                    JobPropertyIds.HoldUntil,
                    JobPropertyIds.NodeGroupOp,
                    JobPropertyIds.SingleNode,
                    JobPropertyIds.JobValidExitCodes,
                    JobPropertyIds.ParentJobIds,
                    JobPropertyIds.EstimatedProcessMemory,
                    JobPropertyIds.TaskExecutionFailureRetryLimit,
                };

                _map = new XmlPropertyHandlerCollection();
                
                _map.SetDefaultHandler(new DefaultXmlPropertyHandler(pids, _v2PropConverters));

                _map.AddHandler(
                    new MinAllocationPropertyHandler (
                        JobPropertyIds.MinCores,
                        JobPropertyIds.MinNodes,
                        JobPropertyIds.MinSockets));

                _map.AddHandler(
                    new MaxAllocationPropertyHandler(
                        JobPropertyIds.MaxCores,
                        JobPropertyIds.MaxNodes,
                        JobPropertyIds.MaxSockets));

                _map.AddHandler(new MinAllocationXmlHandler(JobUnitType.Core, "MinimumNumberOfProcessors", JobPropertyIds.MinCores));
                _map.AddHandler(new MaxAllocationXmlHandler(JobUnitType.Core, "MaximumNumberOfProcessors", JobPropertyIds.MaxCores));
                                
                _map.AddHandler(new RuntimeXmlPropertyHandler());
                
                _map.AddHandler(new RenameXmlPropertyHandler(JobPropertyIds.JobTemplate, "JobTemplate"));

                _map.AddHandler(new PriorityXmlPropertyHandler());
            }
        }

        public IEnumerable<StoreProperty> FetchProperties(ISchedulerStore store)
        {
            return base.GetItemProperties(store, ObjectType.Job);
        }

        Dictionary<string, string> _envs = new Dictionary<string, string>();

        void ReadEnvVars(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Var)
                {

                    string name;
                    string value;

                    ReadNameValuePair(reader, XmlNames.Var, out name, out value);


                    if (name != null && value != null)
                    {
                        _envs[name] = value;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.EnvVars)
                {
                    break;
                }
            }
        }

        public IDictionary<string, string> FetchEnvironmentVariables()
        {
            return _envs;
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            if (value == null && _envs.ContainsKey(name))
            {
                _envs.Remove(name);
            }
            else
            {
                _envs[name] = value;
            }
        }

        public void SetEnvironmentVariables(Dictionary<string, string> vars)
        {
            _envs = vars;
        }

        internal void SetEnvVarsOnClusterJob(IClusterJob job)
        {
            foreach (KeyValuePair<string, string> item in _envs)
            {
                job.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        private void FixPropConflicts()
        {
            // This function detects the properties with overlap functionality,
            // and judge what would be the final result

            // If AutoMin/Max is not specified but Min/Max is specified,
            // we need to set the AutoMin/Max to false so that the Min/Max
            // would not be overriden by a true AutoMin/Max in the validator

            if (!_props.ContainsKey(JobPropertyIds.AutoCalculateMin))
            {
                if (_props.ContainsKey(JobPropertyIds.MinCores) ||
                _props.ContainsKey(JobPropertyIds.MinSockets) ||
                _props.ContainsKey(JobPropertyIds.MinNodes))
                {
                    _props[JobPropertyIds.AutoCalculateMin] =
                        new StoreProperty(JobPropertyIds.AutoCalculateMin, false);
                }
                else
                {
                    _props[JobPropertyIds.AutoCalculateMin] =
                        new StoreProperty(JobPropertyIds.AutoCalculateMin, true);
                }
            }

            if (!_props.ContainsKey(JobPropertyIds.AutoCalculateMax))
            {
                if (_props.ContainsKey(JobPropertyIds.MaxCores) ||
                _props.ContainsKey(JobPropertyIds.MaxSockets) ||
                _props.ContainsKey(JobPropertyIds.MaxNodes))
                {
                    _props[JobPropertyIds.AutoCalculateMax] =
                        new StoreProperty(JobPropertyIds.AutoCalculateMax, false);
                }
                else
                {
                    _props[JobPropertyIds.AutoCalculateMax] =
                        new StoreProperty(JobPropertyIds.AutoCalculateMax, true);
                }
            }

        }

        public void ReadXML(XmlReader reader, XmlImportOptions options)
        {
            _InitMap();
            
            while (reader.Read())
            {              
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Job)
                {
                    // Get the properties (attributes) for the job
                    
                    reader.MoveToFirstAttribute();
                    
                    _map.ParseToPropertyDictionary(reader, _props);

                    PostReadProps_Deconvert();

                    FixPropConflicts();

                    // Now read ahead for the next big thing....
                    
                    while (reader.Read())
                    {
                        
                        if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.Job)
                        {
                            // Done with the Job
                            break;
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.EnvVars && reader.IsEmptyElement == false)
                        {
                            ReadEnvVars(reader);
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.CustomProps)
                        {
                            ReadCustomProps(reader);
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.ExtendedTerms)
                        {
                            ReadExtendedTerms(reader);
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Dependencies)
                        {
                            // Read in the dependencies

                            if ((options & XmlImportOptions.NoTaskGroups) == 0)
                            {
                                ReadDependencies(reader, _taskDependencies);
                            }
                        }
                        else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Tasks)
                        {
                            if (reader.IsEmptyElement)
                            {
                                continue;
                            }

                            if ((options & XmlImportOptions.NoTasks) == 0)
                            {
                                // Read in the tasks!

                                while (reader.Read())
                                {
                                    if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.Tasks)
                                    {
                                        break;
                                    }

                                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Task)
                                    {
                                        TaskPropertyBag task = new TaskPropertyBag();
                                        
                                        task.ReadXML(reader, true);
                                        
                                        _tasks.Add(task);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<TaskPropertyBag> GetTasks()
        {
            return _tasks;
        }

        public void AddTask(TaskPropertyBag taskBag)
        {
            _tasks.Add(taskBag);
        }

        public void WriteXml(XmlWriter writer, XmlExportOptions flags)
        {
            if ((flags & XmlExportOptions.NoJobElement) == 0)
            {
                writer.WriteStartElement(XmlNames.Job);
            }

            PruneProperties();
            

            PreWriteProps_Convert(flags);
            
            WritePropertyXml(writer, flags);

            if (_envs != null && _envs.Count > 0)
            {
                writer.WriteStartElement(XmlNames.EnvVars);

                foreach (KeyValuePair<string, string> var in _envs)
                {
                    writer.WriteStartElement(XmlNames.Var);

                    writer.WriteElementString(XmlNames.Name, var.Key);
                    writer.WriteElementString(XmlNames.Value, var.Value);

                    writer.WriteEndElement(); //Variable 
                }

                writer.WriteEndElement();
            }

            // Task groups
            
            // Tasks
            
            if (_tasks != null && _tasks.Count > 0)
            {
                writer.WriteStartElement(XmlNames.Tasks);
                
                foreach (TaskPropertyBag task in _tasks)
                {
                    task.WriteXml(writer, flags);
                }
                
                writer.WriteEndElement();  // tasks
            }
            
            if ((flags & XmlExportOptions.NoJobElement) == 0)
            {
                writer.WriteEndElement();  // Job
                writer.Flush();
            }
        }
        
        /*
         * <Dependencies>
         *     <Group Name=Default Default=true>
         *         <Child Name=GroupA/>
         *         <Child Name=GroupB/>
         *     </Group>
         *     <Group Name=GroupA>
         *         <Child Name=GroupC/>
         *     </Group>
         *     <Group Name=GroupB/>
         *     <Group Name=GroupC/>
         * <Dependencies/>
         * 
         */

        /*
         * Schema:
         * <Dependencies>
         *      <Parent GroupId = 8>
         *          <Child GroupId = 9/>
         *          <Child GroupId = 10/>
         *      </Parent>
         *      <Parent GroupId = 9>
         *          <Child GroupId = 10/>
         *          <Child GroupId = 11/>
         *          ...
         *      </Parent>
         *      ...
         * </Dependencies>
         * 
         * To:
         * 
         * <8, 9>
         * <8, 10>
         * <9, 10>
         * <9, 11> 
         * ...
         */
        private void ReadDependencies(XmlReader reader, List<KeyValuePair<int, int>> taskDependencies)
        {
            if (reader.IsEmptyElement)
                return;

            while (reader.Read())
            {
                int parent = 0;
                int child = 0;

                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.Dependencies)
                {
                    // Done with the dependencies
                    break;
                }

                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Parent)
                {
                    if (!reader.MoveToAttribute(XmlNames.GroupId))
                    {
                        continue;
                    }

                    if (!int.TryParse(reader.Value, out parent))
                    {
                        continue;
                    }

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.Parent)
                        {
                            // Done with a single parent
                            break;
                        }

                        if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Child)
                        {
                            if (!reader.MoveToAttribute(XmlNames.GroupId))
                            {
                                continue;
                            }

                            if (!int.TryParse(reader.Value, out child))
                            {
                                continue;
                            }

                            taskDependencies.Add(new KeyValuePair<int, int>(parent, child));
                        }
                    }
                }
            }
        }

        public List<KeyValuePair<int, int>> GetTaskDependencies()
        {
            return _taskDependencies;
        }

        public static void ReconstructTaskGroups(
            IClusterTaskGroup newRootGrp,
            List<KeyValuePair<int, int>> taskDependencies,
            Dictionary<int, IClusterTaskGroup> taskGroupIdMapping)
        {
            foreach (KeyValuePair<int, int> oldDep in taskDependencies)
            {
                if (taskGroupIdMapping.Count == 0)
                {
                    taskGroupIdMapping.Add(oldDep.Key, newRootGrp);
                }

                // The parent must have been added now

                if (taskGroupIdMapping.ContainsKey(oldDep.Value))
                {
                    taskGroupIdMapping[oldDep.Value].AddParent(taskGroupIdMapping[oldDep.Key]);
                }
                else
                {
                    taskGroupIdMapping.Add(
                        oldDep.Value,
                        taskGroupIdMapping[oldDep.Key].CreateChild(""));
                }
            }
        }

        const int FakeGroupIdBase = 1000000000;

        /// <summary>
        /// This is a method newly added in V3SP4 to do batch insertion on the 
        /// newly created task groups and the dependencies, so that the performance can be largely improved.
        /// </summary>
        /// <param name="job"></param>
        /// <param name="newRootGrp"></param>
        /// <param name="taskDependencies"></param>
        /// <param name="fakeTaskGroupIdMapping"></param>
        public static void ReconstructTaskGroupsEx(
            IClusterJob job,
            IClusterTaskGroup newRootGrp,
            List<KeyValuePair<int, int>> taskDependencies,
            Dictionary<int, int> taskGroupIdMapping)
        {
            // This is the dictionary used to save the mapping between the oldTaskGroupId to the faked newTaskGroupIds
            // We use faked ID here because we wouldn't be able to get the real task group ID before we do the batch insertion
            Dictionary<int, int> fakeTaskGroupIdMapping = new Dictionary<int, int>();

            // This dictionary is used to contain all the dependencies needed to be inserted
            List<KeyValuePair<int, int>> newDependencies = new List<KeyValuePair<int, int>>();

            // This list is used to contain all the new task groups needed to be inserted
            List<string> newGroups = new List<string>();

            // The current fake group Id allocation. It starts from a big number which we call FakeGroupIdBase.
            // All IDs bigger than or equals to this FakeGroupIdBase are fake IDs
            int curFakeGroupId = FakeGroupIdBase;

            foreach (KeyValuePair<int, int> oldDep in taskDependencies)
            {
                if (fakeTaskGroupIdMapping.Count == 0)
                {
                    fakeTaskGroupIdMapping.Add(oldDep.Key, newRootGrp.Id);
                }

                // The parent must have been added to the newGroups at this point

                int existingChildGrpId;
                if (fakeTaskGroupIdMapping.TryGetValue(oldDep.Value, out existingChildGrpId))
                {
                    // If the child group Id has already been added to the mapping, then we only need to add the dependency

                    int tmpParentGrpId;
                    if (fakeTaskGroupIdMapping.TryGetValue(oldDep.Key, out tmpParentGrpId))
                    {
                        newDependencies.Add(new KeyValuePair<int, int>(tmpParentGrpId, existingChildGrpId));
                    }
                    else
                    {
                        Debug.Assert(false);
                    }
                }
                else
                {
                    // If the child group hasn't been added, we need to create the new child group first, add it to the newGroups list,
                    // then add the dependency

                    int newGroupId = curFakeGroupId++;

                    int tmpParentGrpId;
                    if (fakeTaskGroupIdMapping.TryGetValue(oldDep.Key, out tmpParentGrpId))
                    {
                        newDependencies.Add(new KeyValuePair<int, int>(tmpParentGrpId, newGroupId));
                    }
                    else
                    {
                        Debug.Assert(false);
                    }

                    // should not use same group name for same job
                    string tmpGroupName = string.Concat("group", newGroups.Count + 1);
                    newGroups.Add(tmpGroupName);

                    fakeTaskGroupIdMapping.Add(oldDep.Value, newGroupId);
                }
            }

            // This is the list used to contain the real group Ids value. The list is ordered in the same way with the 
            // newGroups list to be sent to the store layer
            List<int> newGroupIds;

            // Start the batch insertion
            job.CreateTaskGroupsAndDependencies(newGroups, newDependencies, FakeGroupIdBase, out newGroupIds);

            // Adjust to the mapping with the new group Ids

            foreach (KeyValuePair<int, int> item in fakeTaskGroupIdMapping)
            {
                int oldId = item.Key;

                int newId = item.Value;
                if (newId >= FakeGroupIdBase)
                {
                    newId = newGroupIds[newId - FakeGroupIdBase];
                }
                taskGroupIdMapping.Add(oldId, newId);
            }
        }
    }

    //
    // TODO: Move the below classes to a new file
    // 

    internal class XmlNames
    {
        internal const string Job           = "Job";
        internal const string Task          = "Task";
        internal const string Tasks         = "Tasks";
        internal const string CustomProps   = "CustomProperties";
        internal const string Var           = "Variable";
        internal const string EnvVars       = "EnvironmentVariables";
        internal const string Name          = "Name";
        internal const string Value         = "Value";
        internal const string Dependencies  = "Dependencies";
        internal const string Parent        = "Parent";
        internal const string Child         = "Child";
        internal const string GroupId       = "GroupId";
        internal const string ExtendedTerms = "ExtendedTerms";
        internal const string Term          = "Term";
        internal const string JobProfile    = "JobTemplate";
        internal const string ProfileItem   = "TemplateItem";
        internal const string Version       = "Version";
        internal const string VersionValue  = "3.000";
        internal const string NameSpace     = "http://schemas.microsoft.com/HPCS2008R2/scheduler/";
        
        internal const string PropertyName      = "PropertyName";
        internal const string Default           = "Default";
        internal const string ReadOnly          = "ReadOnly";
        internal const string ValueRange        = "ValueRange";
        internal const string MinVal            = "MinVal";
        internal const string MaxVal            = "MaxVal";
        internal const string RequiredValues    = "RequiredValues";
    }

    internal abstract class XmlPropertyHandler
    {
        internal abstract StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader);
        
        internal abstract bool ValidName(string name);

        internal static object GetPropValueFromReader(PropertyId pid, XmlReader reader)
        {
            switch (pid.Type)
            {
                case StorePropertyType.TaskId:
                case StorePropertyType.Int32:
                    return Int32.Parse(reader.Value);

                case StorePropertyType.Int64:
                    return Int64.Parse(reader.Value);

                case StorePropertyType.Boolean:
                    bool result = false;
                    bool success = Boolean.TryParse(reader.Value, out result);

                    if (success == true)
                    {
                        return result;
                    }
                    else
                    {
                        int returnedValue = -1;
                        success = Int32.TryParse(reader.Value, out returnedValue);
                        if (success == true)
                        {
                            if(returnedValue == 0)
                            {
                                return false;
                            }
                            else if (returnedValue == 1)
                            {
                                return true;
                            }
                            else
                            {
                                throw new ArgumentException(SR.Wrong_Integer_For_Boolean); 
                            }
                        }

                        throw new ArgumentException(SR.Wrong_String_For_Boolean); 
                    }

                    
                case StorePropertyType.String:
                case StorePropertyType.StringList:
                    return reader.Value;

                case StorePropertyType.JobType:
                    return Enum.Parse(typeof(JobType), reader.Value);

                case StorePropertyType.JobUnitType:
                    return Enum.Parse(typeof(JobUnitType), reader.Value);

                case StorePropertyType.JobRuntimeType:
                    return Enum.Parse(typeof(JobRuntimeType), reader.Value);

                case StorePropertyType.TaskType:
                    return Enum.Parse(typeof(TaskType), reader.Value);

                case StorePropertyType.DateTime:
                    return DateTime.SpecifyKind(DateTime.Parse(reader.Value), DateTimeKind.Utc);

                case StorePropertyType.JobNodeGroupOp:
                    return Enum.Parse(typeof(JobNodeGroupOp), reader.Value);

                default:
                    return reader.Value;
            }
        }
    }
    
    internal class DefaultXmlPropertyHandler : XmlPropertyHandler
    {
        protected Dictionary<string, PropertyId> _map;

        internal DefaultXmlPropertyHandler(PropertyId[] pids) : this(pids, null)
        {            
        }

        internal DefaultXmlPropertyHandler(PropertyId[] pids, IEnumerable<XMLEnabledPropertyConverter> converters)
        {
            _map = new Dictionary<string,PropertyId>();
            
            foreach (PropertyId pid in pids)
            {
                _map[pid.Name.ToLowerInvariant()] = pid;
            }

            // Add all the properties that may be involved in property conversion to the default handler
            if (converters != null)
            {
                foreach (XMLEnabledPropertyConverter converter in converters)
                {                    
                    if (_map.ContainsKey(converter.GetPropId().Name.ToLowerInvariant()))
                    {
                        foreach (PropertyId pid in converter.GetXMLReplacementPropIds())
                        {
                            _map[pid.Name.ToLowerInvariant()] = pid;
                        }
                    }
                }
            }
        }
        
        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            PropertyId pid = _map[reader.LocalName.ToLowerInvariant()];
 	        return new StoreProperty[] { new StoreProperty(pid, GetPropValueFromReader(pid, reader)) };
        }

        internal override bool ValidName(string name)
        {
            return _map.ContainsKey(name.ToLowerInvariant());
        }
    }

    class PriorityXmlPropertyHandler : XmlPropertyHandler
    {
        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            int exPri;
            if (!ExpandedPriority.TryParse(reader.Value, out exPri))
            {
                throw new ArgumentException("The Priority value in the XML file is not recognizable.");
            }

            return new StoreProperty[]{
                new StoreProperty(JobPropertyIds.ExpandedPriority, exPri)
            };
        }

        internal override bool ValidName(string name)
        {
            return name.Equals(XmlPropName, StringComparison.InvariantCultureIgnoreCase);
        }

        internal static string XmlPropName
        {
            get { return "Priority"; }
        }
    }

    class MinAllocationPropertyHandler : DefaultXmlPropertyHandler
    {
        internal MinAllocationPropertyHandler(params PropertyId[] pids)
            : base(pids)
        { }

        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            PropertyId pid = _map[reader.LocalName.ToLowerInvariant()];
            return new StoreProperty[] { 
                                new StoreProperty(pid, GetPropValueFromReader(pid, reader)),
                                    };
        }
    }

    class MaxAllocationPropertyHandler : DefaultXmlPropertyHandler
    {
        internal MaxAllocationPropertyHandler(params PropertyId[] pids)
            : base(pids)
        { }

        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            PropertyId pid = _map[reader.LocalName.ToLowerInvariant()];
            return new StoreProperty[] { 
                                new StoreProperty(pid, GetPropValueFromReader(pid, reader)),
                                    };
        }
    }

    internal class RenameXmlPropertyHandler : XmlPropertyHandler
    {
        PropertyId _pid;
        string _rename;
        
        internal RenameXmlPropertyHandler(PropertyId pid, string rename)
        {
            _pid = pid;
            _rename = rename.ToLowerInvariant();
        }
    
        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            return new StoreProperty[] { new StoreProperty(_pid, GetPropValueFromReader(_pid, reader)) };
        }

        internal override bool ValidName(string name)
        {
            return name.ToLowerInvariant() == _rename.ToLowerInvariant();
        }
    }
    
    internal class RuntimeXmlPropertyHandler : XmlPropertyHandler
    {
        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            // Need to parse the runtime string and return
            // this as the number of seconds.
            
            //Regex regexpression = new Regex(@"^(((\d*:)?\d*:)?\d*)$|^infinite$");
            
            //if (!regexpression.IsMatch(reader.Value))
            //{
            //    throw new ArgumentException("The value of runtime isn't in the form of {[[DD:]HH:]MM|Infinite}");
            //}

            if (string.Compare(reader.Value, "infinite", true) != 0)
            {
                string[] parts = reader.Value.Split(':');
                
                int runtime = 0;
                
                switch (parts.Length)
                {
                    case 1:
                        runtime += int.Parse(parts[0]) * 60;
                        break;
                        
                    case 2: 
                        runtime += int.Parse(parts[0]) * 3600;
                        runtime += int.Parse(parts[1]) * 60;
                        break;
                        
                    case 3: 
                        runtime += int.Parse(parts[0]) * 86400;
                        runtime += int.Parse(parts[1]) * 3600;
                        runtime += int.Parse(parts[2]) * 60;
                        break;
                        
                    default:
                        throw new ArgumentException();
                }

                return new StoreProperty[] { new StoreProperty(StorePropertyIds.RuntimeSeconds, runtime) };
            }
            
            return new StoreProperty[] { new StoreProperty(StorePropertyIds.RuntimeSeconds, null) };
        }

        internal override bool ValidName(string name)
        {
            return name.ToLowerInvariant() == "runtime";
        }
    }

    internal class MinAllocationXmlHandler : XmlPropertyHandler
    {
        string _name;
        JobUnitType _type;
        PropertyId _pid;
        
        internal MinAllocationXmlHandler(JobUnitType type, string name, PropertyId pid)
        {
            _type = type;
            _name = name.ToLowerInvariant();
            _pid = pid;
        }
        
        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            return new StoreProperty[] 
                {
                    new StoreProperty(JobPropertyIds.UnitType, _type),
                    new StoreProperty(_pid, GetPropValueFromReader(_pid, reader)),
                    new StoreProperty(JobPropertyIds.AutoCalculateMin, false),
                };
        }

        internal override bool ValidName(string name)
        {
            return  name.ToLowerInvariant() == _name;
        }
    }

    internal class MaxAllocationXmlHandler : XmlPropertyHandler
    {
        string _name;
        JobUnitType _type;
        PropertyId _pid;

        internal MaxAllocationXmlHandler(JobUnitType type, string name, PropertyId pid)
        {
            _type = type;
            _name = name.ToLowerInvariant();
            _pid = pid;
        }

        internal override StoreProperty[] PropertiesFromCurrentAttribute(XmlReader reader)
        {
            return new StoreProperty[] 
                {
                    new StoreProperty(JobPropertyIds.UnitType, _type),
                    new StoreProperty(_pid, GetPropValueFromReader(_pid, reader)),
                    new StoreProperty(JobPropertyIds.AutoCalculateMax, false),
                };
        }

        internal override bool ValidName(string name)
        {
            return name.ToLowerInvariant() == _name;
        }
    }   

    internal class XmlPropertyHandlerCollection
    {
        List<XmlPropertyHandler> _handlers = new List<XmlPropertyHandler>();
        DefaultXmlPropertyHandler _defaultHandler = null;
        
        internal void SetDefaultHandler(DefaultXmlPropertyHandler handler)
        {
            _defaultHandler = handler;
        }
        
        internal void AddHandler(XmlPropertyHandler handler)
        {
            _handlers.Add(handler);
        }
        
        internal XmlPropertyHandler GetHandlerForAttribute(XmlReader reader)
        {
            foreach (XmlPropertyHandler handlerSearch in _handlers)
            {
                if (handlerSearch.ValidName(reader.LocalName))
                {
                    return handlerSearch;
                }
            }
            
            if (_defaultHandler.ValidName(reader.LocalName))
            {
                return _defaultHandler;
            }

            return null;
        }
    
    
        internal void ParseToPropertyDictionary(XmlReader reader, Dictionary<PropertyId, StoreProperty> props)
        {
            while (true)
            {
                XmlPropertyHandler handler = GetHandlerForAttribute(reader);
                
                if (handler != null)
                {
                    foreach (StoreProperty prop in handler.PropertiesFromCurrentAttribute(reader))
                    {
                        props[prop.Id] = prop;
                    }
                }
                
                if (reader.MoveToNextAttribute() == false)
                    break;
            }
        }
    
    }

}
