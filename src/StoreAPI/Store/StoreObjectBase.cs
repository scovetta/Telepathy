using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    abstract class StoreObjectBase : IClusterStoreObject, ICustomTypeDescriptor
    {
        protected SchedulerStoreSvc _helper;
        ObjectType _obType;

        public StoreObjectBase(SchedulerStoreSvc helper, ObjectType obType)
        {
            _obType = obType;
            _helper = helper;
        }

        /// <summary>
        /// Get the store service
        /// </summary>
        protected SchedulerStoreSvc Helper
        {
            get { return _helper; }
        }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return this.GetProperties();
        }

        public PropertyDescriptorCollection GetProperties()
        {
            PropertyRow row = this.GetAllProps();
            int length = row.Props.Length;
            PropertyId[] ids = new PropertyId[length];
            for (int i = 0; i < length; i++)
            {
                ids[i] = row.Props[i].Id;
            }

            return new PropertyDescriptorCollection(this.Helper.GetPropertyDescriptors(_obType, ids));
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }
        
        //custom variables
        public void GetCustomProperties(out StoreProperty[] props)
        {
            if (_obType != ObjectType.Job && _obType != ObjectType.Task)
            {
                props = null;
                return;
            }

            _helper.GetCustomProperties(_obType, Id, out props);
        }
        
        protected virtual PropertyId[] GetExportV3Pids() { return null; }
        
        protected StoreProperty[] GetPropsToExport(bool isV2Server)
        {
            StoreProperty[] propsToExport = null;
            //if the server is v2, get all properties including custom properties and write it out
            // if the server is of a higher version, get only the properties specifed in _exportV3Xml
            if (isV2Server)
            {
                PropertyRow props = GetAllProps();
                propsToExport = props.Props;
            }
            else
            {
                PropertyRow props = GetProps(GetExportV3Pids());

                StoreProperty[] customProps = null;
                GetCustomProperties(out customProps);

                if (customProps != null && customProps.Length > 0)
                {
                    List<StoreProperty> propList = new List<StoreProperty>();
                    propList.AddRange(props.Props);
                    propList.AddRange(customProps);

                    propsToExport = propList.ToArray();
                }
                else
                {
                    propsToExport = props.Props;
                }

            }
            return propsToExport;
        }


        #endregion

        #region IStoreObject Members

        public abstract int Id {get;}

        public abstract PropertyRow GetProps(params PropertyId[] propertyIds);
        public abstract PropertyRow GetPropsByName(params string[] propertyNames);
        public abstract PropertyRow GetAllProps();
        public abstract void SetProps(params StoreProperty[] properties);
        public abstract void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags);
        public abstract void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions flags);
        

        #endregion
    }
}
