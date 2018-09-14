using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    class NodeEx : StoreObjectBase, IClusterNode
    {
        int                         _id;

        public NodeEx(int id, SchedulerStoreSvc helper)
            : base(helper, ObjectType.Node)
        {
            Debug.Assert(helper != null);

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
            return _helper.GetPropsFromServer(ObjectType.Node, _id, propertyIds);
        }

        public override PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Node.PropertyIdsFromNames(propertyNames));
        }

        public override void SetProps(params StoreProperty[] nodeProperties)
        {
            _helper.SetPropsOnServer(ObjectType.Node, _id, nodeProperties);
        }

        public void  TakeOffline()
        {
            _helper.ServerWrapper.Node_TakeNodeOffline(_id);
        }

        public void  PutOnline()
        {
            _helper.ServerWrapper.Node_PutNodeOnline(_id);
        }

        public void SetReachable()
        {
            _helper.RemoteServer.Node_SetNodeReachable(_helper.Token, _id);
        }

        public void SetUnreachable()
        {
            _helper.RemoteServer.Node_SetNodeUnreachable(_helper.Token, _id);
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
