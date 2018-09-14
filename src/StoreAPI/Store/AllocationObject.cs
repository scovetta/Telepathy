using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class AllocationObject : IClusterAllocation
    {
        SchedulerStoreSvc _owner;
        int _id;
        
        internal AllocationObject(SchedulerStoreSvc owner, int id)
        {
            _owner = owner;
            _id = id;
        }
            
        
        public int Id
        {
            get { return _id; }
        }

        public PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _owner.GetPropsFromServer(ObjectType.Allocation, _id, propertyIds);
        }

        public PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Allocation.PropertyIdsFromNames(propertyNames));
        }

        public PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public void SetProps(params StoreProperty[] properties)
        {
            _owner.SetPropsOnServer(ObjectType.Allocation, _id, properties);
        }

        public void PersistToXml(XmlWriter writer, XmlExportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RestoreFromXml(XmlReader reader, XmlImportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
