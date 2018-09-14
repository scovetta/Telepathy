using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    [Serializable]
    public class StorePropertyDescriptor : PropertyDescriptor
    {
        PropertyId  _pid;
        string      _systemname;
        string      _localname;
        string      _description;
        ObjectType  _objecttype;

        public StorePropertyDescriptor(
                PropertyId pid,
                string     systemName,
                ObjectType obType,
                string     description
        ) : base(systemName, GetPropertyAttributes(pid))
        {
            _pid = pid;
            _systemname = systemName;
            _objecttype = obType;
            _localname = systemName;
            _description = description;
        }
        

        internal StorePropertyDescriptor(
                PropertyId  propId,
                string      systemName,
                ObjectType  objectType
        ) : base(systemName, GetPropertyAttributes(propId))
        {
            _pid            = propId;
            _systemname     = systemName;
            _objecttype     = objectType;
        }


        static Attribute[] GetPropertyAttributes(PropertyId pid)
        {
            List<Attribute> attrList = new List<Attribute>();
            if ((pid.Flags & PropFlags.Obsolete) != 0)
            {
                attrList.Add(new ObsoleteAttribute());
            }
            return attrList.Count > 0 ? attrList.ToArray() : null;
        }


        /// <summary>
        /// PropertyId for the property that this descriptor is describing
        /// </summary>

        public PropertyId PropId
        {
            get { return _pid; }
        }
        
        /// <summary>
        /// Name of the property within the system as the API would interpret it.
        /// </summary>

        public override string Name
        {
            get { return _systemname; }
        }

        /// <summary>
        /// Localized name of the property.  For use when displaying to a user.
        /// </summary>
        
        public override string DisplayName
        {
            get
            {
                if (_localname == null)
                {
                    try
                    {
                        _localname = SR.ResourceManager.GetString(
                            _objecttype.ToString() + "_" +  _pid.Name + "_DisplayName");
                    }
                    catch
                    {
                        _localname = null;
                    }
                    
                    
                    if (_localname == null)
                        _localname = _pid.Name;
                }

                return _localname;
            }
        }
        
        /// <summary>
        /// Description of the property.
        /// </summary>

        public override string Description
        {
            get
            {
                if (_description == null)
                {
                    try
                    {
                        _description = SR.ResourceManager.GetString(
                            _objecttype.ToString() + "_" + _pid.Name + "_Description");
                    }
                    catch
                    {
                        _description = null;
                    }
                    
                    if (_description == null)
                        _description = _pid.Name;
                }

                return _description;
            }
        }

        /// <summary>
        /// Bitfield that describes what object the property can be used on.
        /// </summary>
        public ObjectType ObjectType 
        {
            get { return _objecttype; }
        }

        /// <summary>
        /// If this property can be used to sort
        /// </summary>
        public bool IsSortable
        {
            get
            {
                return (_pid.Flags & PropFlags.Indexed) != 0;
            }
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get 
            {
                return typeof(IClusterStoreObject);
            }
        }

        public override object GetValue(object component)
        {
            IClusterStoreObject obj = (IClusterStoreObject)component;
            return obj.GetProps(_pid).Props[0].Value;
        }

        public override bool IsReadOnly
        {
            get 
            {
                return (_pid.Flags & PropFlags.ReadOnly) != 0;
            }
        }

        public override Type PropertyType
        {
            get 
            {
                return GetTypeForProperty(_pid.Type);
            }
        }

        public static Type GetTypeForProperty(StorePropertyType PropType)
        {
            switch (PropType)
            {
                case StorePropertyType.Int32: return typeof(Int32);
                case StorePropertyType.UInt32: return typeof(UInt32);
                case StorePropertyType.Int64: return typeof(Int64);
                case StorePropertyType.String: return typeof(String);
                case StorePropertyType.StringList: return typeof(String);
                case StorePropertyType.DateTime: return typeof(DateTime);
                case StorePropertyType.Boolean: return typeof(bool);
                case StorePropertyType.Guid: return typeof(Guid);
                case StorePropertyType.Binary: return typeof(byte[]);
                case StorePropertyType.Object: return typeof(object);

                case StorePropertyType.JobPriority: return typeof(JobPriority);
                case StorePropertyType.JobState: return typeof(JobState);
                case StorePropertyType.JobUnitType: return typeof(JobUnitType);
                case StorePropertyType.FailureReason: return typeof(string);
                case StorePropertyType.JobType: return typeof(JobType);
                case StorePropertyType.JobOrderby: return typeof(JobOrderByList);
                case StorePropertyType.TaskType: return typeof(TaskType);
                case StorePropertyType.JobMessageType: return typeof(JobMessageType);
                case StorePropertyType.JobRuntimeType: return typeof(JobRuntimeType);

                case StorePropertyType.TaskState: return typeof(TaskState);
                case StorePropertyType.ResourceState: return typeof(ResourceState);
                case StorePropertyType.ResourceJobPhase: return typeof(ResourceJobPhase);
                case StorePropertyType.NodeState: return typeof(NodeState);
                case StorePropertyType.NodeAvailability: return typeof(NodeAvailability);

                case StorePropertyType.PendingReason: return typeof(PendingReason.ReasonCode);
                case StorePropertyType.AllocationList: return typeof(ICollection<KeyValuePair<string, int>>);
                case StorePropertyType.TaskId: return typeof(TaskId);
                case StorePropertyType.JobNodeGroupOp: return typeof(JobNodeGroupOp);

                case Microsoft.Hpc.Scheduler.Properties.StorePropertyType.CancelRequest:

                default:
                    return null;
            }
        }

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value)
        {
            IClusterStoreObject obj = (IClusterStoreObject)component;
            obj.SetProps(new StoreProperty(_pid, value));
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }
    }
}
