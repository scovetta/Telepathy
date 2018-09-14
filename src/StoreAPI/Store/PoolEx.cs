using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    class PoolEx : StoreObjectBase, IClusterPool
    {
        int _id=-1;
        public PoolEx(SchedulerStoreSvc helper,int id)
            :base(helper, ObjectType.Pool)
        {
            _id = id;
        }

        public override int Id
        {
            get { return _id; }
        }

        public override PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public override PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _helper.GetPropsFromServer(ObjectType.Pool, _id, propertyIds);
        }

        public override PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Pool.PropertyIdsFromNames(propertyNames));
        }

        public override void SetProps(params StoreProperty[] poolProperties)
        {
            _helper.SetPropsOnServer(ObjectType.Pool, _id, poolProperties);
        }

        public override void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}