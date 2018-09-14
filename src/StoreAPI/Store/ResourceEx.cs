using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    using Microsoft.Hpc.Scheduler.Store;

    internal class ResourceEx : IClusterResource
    {
        ISchedulerStoreInternal _store;
        ConnectionToken _token;
        Int32 _id;
        SchedulerStoreSvc _helper;
        
        public ResourceEx(ISchedulerStoreInternal internalStore, ConnectionToken token, Int32 id, SchedulerStoreSvc helper)
        {
            _store = internalStore;
            _token = token;
            _id = id;
            
            _helper = helper;
        }

        public int Id
        {
            get { return _id; }
        }

        public PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _helper.GetPropsFromServer(ObjectType.Resource, _id, propertyIds);
        }

        public PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.Resource.PropertyIdsFromNames(propertyNames));
        }

        public void SetProps(params StoreProperty[] resourceProperties)
        {
            _helper.SetPropsOnServer(ObjectType.Resource, _id, resourceProperties);
        }

        public void ScheduleJob(int jobId, StoreProperty[] jobProperties)
        {
            StoreTransaction transaction = _helper.GetTransaction();
            
            if (transaction != null)
            {
                transaction.ScheduleJob(_id, jobId, jobProperties);
            }
            else
            {
                _store.ScheduleResource(_token, _id, jobId, jobProperties);
            }
        }

        public void ReserveForJob(int jobId, DateTime reserveLimitTime, StoreProperty[] jobProperties)
        {
            StoreProperty[] props = 
            {
                new StoreProperty(ResourcePropertyIds.ReserveJobId, jobId),
                new StoreProperty(ResourcePropertyIds.ReserveLimitTime, reserveLimitTime)
            };

            _helper.SetPropsOnServer(ObjectType.Resource, _id, props);
        }

        public void ClearJob()
        {
            throw new SchedulerException(string.Format(SR.MethodNotImplemented,"ClearJob"));
        }

        public void ClearReservedJob()
        {
            throw new SchedulerException(string.Format(SR.MethodNotImplemented, "ClearReservedJob"));
        }


        public void PersistToXml(System.Xml.XmlWriter writer, XmlExportOptions flags)
        {
            throw new Exception(string.Format(SR.MethodNotImplemented, "PersistToXml"));
        }

        public void RestoreFromXml(System.Xml.XmlReader reader, XmlImportOptions flags)
        {
            throw new Exception(string.Format(SR.MethodNotImplemented, "RestoreFromXml"));
        }

    }
}
