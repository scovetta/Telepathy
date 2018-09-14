using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class TaskPropertyBag : PropertyBag
    {
        public TaskPropertyBag() : base()
        {
            _v2PropConverters.Add(new TaskTypeHelper.Converter_TaskType_IsParametric());
        }

        internal void CreateForJob(
            ISchedulerStore store,
            IClusterJob job,
            Dictionary<int, int> groupIdMapping,
            int defaultGrpId)
        {
            StoreProperty[] props = GeneratePropsForTask(store, groupIdMapping, defaultGrpId);

            IClusterTask task = job.CreateTask(props);

            SetEnvVarsOnClusterTask(task);
        }

        internal void SetEnvVarsOnClusterTask(IClusterTask task)
        {
            foreach (KeyValuePair<string, string> item in _envs)
            {
                task.SetEnvironmentVariable(item.Key, item.Value);
            }
        }

        internal StoreProperty [] GeneratePropsForTask(ISchedulerStore store, Dictionary<int, int> groupIdMapping, int defaultGrpId)
        {
            if (_props.ContainsKey(TaskPropertyIds.GroupId))
            {
                TaskType type = TaskType.Basic;
                if (_props.ContainsKey(TaskPropertyIds.Type))
                {
                    type = (TaskType)(_props[TaskPropertyIds.Type].Value);
                }

                if (TaskTypeHelper.IsGroupable(type))
                {
                    StoreProperty prop = _props[TaskPropertyIds.GroupId];

                    int oldId = 0;
                    bool validValue = false;
                    if (prop.Value is int)
                    {
                        oldId = (int)prop.Value;
                        validValue = true;
                    }
                    if (prop.Value is string)
                    {
                        validValue = int.TryParse((string)prop.Value, out oldId);
                    }

                    if (validValue)
                    {
                        int newId = defaultGrpId;
                        if (groupIdMapping.ContainsKey(oldId))
                        {
                            newId = groupIdMapping[oldId];
                        }

                        prop.Value = newId;
                    }
                }
                else
                {
                    _props.Remove(TaskPropertyIds.GroupId);
                }
            }


            List<StoreProperty> props = new List<StoreProperty>(_props.Values);
            if (_customProps.Count > 0)
            {
                props.AddRange(ResolveCustomProperties(_customProps, store, ObjectType.Task));
            }
            return props.ToArray();
        }
        
        internal void UpdateTask(ISchedulerStore store, IClusterTask task)
        {
            List<StoreProperty> props = new List<StoreProperty>(_props.Values);

            if (_customProps.Count > 0)
            {
                props.AddRange(ResolveCustomProperties(_customProps, store, ObjectType.Task));
            }            

            task.SetProps(props.ToArray());
            
            foreach (KeyValuePair<string, string> item in _envs)
            {
                task.SetEnvironmentVariable(item.Key, item.Value);
            }
        }
        
        Dictionary<string, string>              _envs = new Dictionary<string,string>();

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

        public IEnumerable<StoreProperty> FetchProperties(ISchedulerStore store)
        {
            return base.GetItemProperties(store, ObjectType.Job);
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

        public void ReadXML(XmlReader reader)
        {
            ReadXML(reader, false);
        }

        internal void ReadXML(XmlReader reader, bool readWithJob)
        {
            _InitMap();

            // Check to see if the reader is on the task.  If not advance 
            // to the first task block (or the end).

            if (reader.LocalName != XmlNames.Task)
            {
                bool found = false;
            
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.Task)
                    {
                        found = true;
                        break;
                    }
                }
                
                if (!found)
                {
                    return;
                }
            }

            bool emptyElement = reader.IsEmptyElement;

            // First get the attributes (properties)
        
            reader.MoveToFirstAttribute();

            _map.ParseToPropertyDictionary(reader, _props);

            // Convert any properties after reading, if necessary.
            PostReadProps_Deconvert();

            if (!readWithJob)
            {
                // Remove the GroupId property
                if (_props.ContainsKey(TaskPropertyIds.GroupId))
                {
                    _props.Remove(TaskPropertyIds.GroupId);
                }
            }
            
            // If this is all we have for this task return.

            if (emptyElement)
            {
                return;
            }

            // Now look for env vars and custom properties
            
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == XmlNames.Task)
                {
                    break;
                }
                
                if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.EnvVars && reader.IsEmptyElement == false)
                {
                    ReadEnvVars(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.ExtendedTerms && reader.IsEmptyElement == false)
                {
                    ReadExtendedTerms(reader);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.LocalName == XmlNames.CustomProps && reader.IsEmptyElement == false)
                {
                    ReadCustomProps(reader);
                }
            }
        }

        public void WriteXml(XmlWriter writer, XmlExportOptions options)
        {
            writer.WriteStartElement(XmlNames.Task,XmlNames.NameSpace);

            PreWriteProps_Convert(options);
            PruneProperties();

            WritePropertyXml(writer, options);
            
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
            
            writer.WriteEndElement();  // Task
        }

        static XmlPropertyHandlerCollection _map = null;
        
        void _InitMap()
        {
            if (_map == null)
            {
                PropertyId[] pids = 
                {
                    TaskPropertyIds.Name,
                    TaskPropertyIds.MinCores,
                    TaskPropertyIds.MaxCores,
                    TaskPropertyIds.MinNodes,
                    TaskPropertyIds.MaxNodes,
                    TaskPropertyIds.MinSockets,
                    TaskPropertyIds.MaxSockets,
                    TaskPropertyIds.CommandLine,
                    TaskPropertyIds.RuntimeSeconds,
                    TaskPropertyIds.IsExclusive,
                    TaskPropertyIds.IsRerunnable,
                    TaskPropertyIds.StdErrFilePath,
                    TaskPropertyIds.StdInFilePath,
                    TaskPropertyIds.StdOutFilePath,
                    TaskPropertyIds.WorkDirectory,
                    TaskPropertyIds.RequiredNodes,
                    TaskPropertyIds.DependsOn,
                    TaskPropertyIds.Type,
                    TaskPropertyIds.StartValue,
                    TaskPropertyIds.EndValue,
                    TaskPropertyIds.IncrementValue,
                    TaskPropertyIds.GroupId,
                    TaskPropertyIds.TaskValidExitCodes,
                    TaskPropertyIds.FailJobOnFailure,
                    TaskPropertyIds.FailJobOnFailureCount,
                    TaskPropertyIds.RequestedNodeGroup

                };

                _map = new XmlPropertyHandlerCollection();
                
                _map.SetDefaultHandler(new DefaultXmlPropertyHandler(pids, _v2PropConverters));

                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.MinCores, "MinimumNumberOfProcessors"));
                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.MaxCores, "MaximumNumberOfProcessors"));
                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.DependsOn, "Depend"));

                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.StdErrFilePath, "Stderr"));
                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.StdOutFilePath, "Stdout"));
                _map.AddHandler(new RenameXmlPropertyHandler(TaskPropertyIds.StdInFilePath, "Stdin"));

                _map.AddHandler(new RuntimeXmlPropertyHandler());
            }
        }
    }
}
