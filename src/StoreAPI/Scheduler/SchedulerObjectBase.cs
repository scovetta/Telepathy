using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Hpc.Scheduler.Properties;
using Microsoft.Hpc.Scheduler.Store;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>The abstract class from which the HPC objects inherit.</para>
    /// </summary>
    public abstract class SchedulerObjectBase
    {
        /// <summary>
        ///   <para>All the properties that have been cached for the object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected Dictionary<PropertyId, StoreProperty> _props = new Dictionary<PropertyId, StoreProperty>();
        internal Dictionary<PropertyId, bool> _deferredPropsLoaded = new Dictionary<PropertyId, bool>();

        internal protected void InitFromObject(IClusterStoreObject obj, PropertyId[] propIds)
        {
            if (propIds == null)
            {
                propIds = GetPropertyIds();
            }

            _InitFromRow(obj.GetProps(propIds));
        }

        internal protected void _InitFromRow(PropertyRow row)
        {
            _props = new Dictionary<PropertyId, StoreProperty>();
            _deferredPropsLoaded = new Dictionary<PropertyId, bool>();

            LoadProps(row);
        }

        protected void LoadProps(IEnumerable<StoreProperty> props)
        {
            foreach (StoreProperty prop in props)
            {
                if (prop.Id != StorePropertyIds.Error)
                {
                    _props[prop.Id] = prop;
                }
            }
        }

        internal void LoadDeferredProp(IClusterStoreObject obj, PropertyId pid)
        {
            if (obj == null || _deferredPropsLoaded.ContainsKey(pid))
            {
                return;
            }

            PropertyRow row = obj.GetProps(pid);
            if (row != null && row.Props.Length != 0 && row[0] != null && row[0].Id == pid)
            {
                _props[pid] = row[0];
            }
            else
            {
                _props.Remove(pid);
            }

            this._deferredPropsLoaded[pid] = true;
        }

        internal void InitFromRowInternal(PropertyRow row)
        {
            _InitFromRow(row);
        }

        internal PropertyId[] GetInitPropertyIdsInternal()
        {
            return GetPropertyIds();
        }

        /// <summary>
        ///   <para>An array of properties to query from the store when refreshing the job object.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        internal protected virtual PropertyId[] GetPropertyIds()
        {
            return null;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected virtual PropertyId[] GetWriteOnlyPropertyIds()
        {
            return null;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        protected PropertyId[] GetCurrentPropsToReload()
        {
            //These are the props we do not want to reload
            PropertyId[] writeOnlyProps = GetWriteOnlyPropertyIds();
            List<PropertyId> propsToReload = new List<PropertyId>();

            if (writeOnlyProps == null || writeOnlyProps.Length == 0)
            {
                //there are no writeonly props.. we should reload all our current props
                foreach (PropertyId prop in GetPropertyIds())
                {
                    if (!_props.ContainsKey(prop))
                    {
                        propsToReload.Add(prop);
                    }
                }

                propsToReload.AddRange(_props.Keys);
            }
            else
            {
                //create a dictionary of write only props to quickly lookup if a current property is write-only
                Dictionary<PropertyId, bool> writeOnlyPropsLookup = new Dictionary<PropertyId, bool>();
                foreach (PropertyId writeOnly in writeOnlyProps)
                {
                    writeOnlyPropsLookup.Add(writeOnly, true);
                }

                foreach (PropertyId prop in GetPropertyIds())
                {
                    if (!_props.ContainsKey(prop))
                    {
                        if (!writeOnlyPropsLookup.ContainsKey(prop))
                        {
                            //it is not a write only property
                            propsToReload.Add(prop);
                        }
                    }
                }

                foreach (PropertyId prop in _props.Keys)
                {
                    if (!writeOnlyPropsLookup.ContainsKey(prop))
                    {
                        //it is not a write only property
                        propsToReload.Add(prop);
                    }
                }
            }

            return propsToReload.ToArray();
        }


        internal virtual void GetPropertyVersionCheck(PropertyId propId)
        {
            // Don't make this abstract, another assembly extends this class
            throw new NotImplementedException();
        }

        internal virtual void SetPropertyVersionCheck(PropertyId propId, object propValue)
        {
            // Don't make this abstract, another assembly extends this class
            throw new NotImplementedException();
        }

        static internal void GetPropertyVersionCheck(ISchedulerStore store, ObjectType obType, String className, PropertyId propId)
        {
            if (store == null)
            {
                return;
            }
            PropertyVersioning.Info info = PropertyVersioning.GetInfo(obType, propId);
            if (store.ServerVersion.Version < info.EarliestCompatibleVersion)
            {
                throw new SchedulerException(ErrorCode.Operation_PropertyNotSupportedOnServerVersion,
                       ErrorCode.MakeErrorParams(
                            String.Format("{0}.{1}", className, propId.Name),
                            store.ServerVersion.Version.ToString(),
                            info.EarliestCompatibleVersion.ToString()));
            }
        }


        static internal void SetPropertyVersionCheck(ISchedulerStore store, ObjectType obType, String className, PropertyId propId, object propValue)
        {
            if (store == null)
            {
                return;
            }

            // First, make sure that server is at least the earliest compatible version with the property
            GetPropertyVersionCheck(store, obType, className, propId);

            PropertyVersioning.Info info = PropertyVersioning.GetInfo(obType, propId);
            Version serverVersion = store.ServerVersion.Version;
            if (serverVersion < info.AddedVersion && info.Converter != null)
            {
                // Make sure that ConvertSetProp does not throw an exception.  No need to save the results
                // the conversion now, since actual conversion will be take care of later by the Store API.
                info.Converter.ConvertSetProp(new StoreProperty(propId, propValue));
            }
        }
    }

    /// <summary>
    ///   <para>Contains custom properties as name / value pairs.</para>
    /// </summary>
    public class CustomPropContainer
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="obType">
        ///   <para />
        /// </param>
        /// <remarks />
        public CustomPropContainer(ObjectType obType)
        {
            _obType = obType;
        }

        Dictionary<string, string> _customprops = new Dictionary<string, string>();
        Dictionary<string, string> _custompropsChanged = new Dictionary<string, string>();

        ObjectType _obType;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        public void SetCustomProperty(string name, string value)
        {
            _customprops[name] = value;
            _custompropsChanged[name] = value;
        }

        bool _fCustomPropsFetched = false;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="store">
        ///   <para />
        /// </param>
        /// <param name="obj">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para>Returns <see cref="T:Microsoft.Hpc.Scheduler.NameValueCollection" />.</para>
        /// </returns>
        /// <remarks />
        public NameValueCollection GetCustomProperties(ISchedulerStore store, IClusterStoreObject obj)
        {
            if (obj != null && _fCustomPropsFetched == false)
            {
                if (_custompropsChanged.Count > 0)
                {
                    Commit(store, obj); // which includes a FetchCustomProps()
                }
                else
                {
                    Refresh(store, obj);
                }

                _fCustomPropsFetched = true;
            }

            return new NameValueCollection(_customprops);
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="store">
        ///   <para />
        /// </param>
        /// <param name="obj">
        ///   <para />
        /// </param>
        /// <remarks />
        public void Commit(ISchedulerStore store, IClusterStoreObject obj)
        {
            if (store != null && _custompropsChanged.Count > 0)
            {
                StoreProperty[] props = GetProps(store);

                obj.SetProps(props);

                _custompropsChanged.Clear();

                Refresh(store, obj);
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="store">
        ///   <para />
        /// </param>
        /// <param name="propsList">
        ///   <para />
        /// </param>
        /// <remarks />
        public void AddPropsToList(ISchedulerStore store, List<StoreProperty> propsList)
        {
            StoreProperty[] props = GetProps(store);
            propsList.AddRange(props);

            _custompropsChanged.Clear();
        }

        private StoreProperty[] GetProps(ISchedulerStore store)
        {
            List<string> names = new List<string>(_custompropsChanged.Keys);
            List<string> values = new List<string>(_custompropsChanged.Values);

            StorePropertyDescriptor[] descs = (StorePropertyDescriptor[])store.GetPropertyDescriptors(names.ToArray(), _obType);

            StoreProperty[] props = new StoreProperty[names.Count];

            for (int i = 0; i < names.Count; i++)
            {
                if (descs[i].PropId == StorePropertyIds.Error)
                {
                    PropertyId pid = store.CreatePropertyId(_obType, StorePropertyType.String, names[i], "");

                    props[i] = new StoreProperty(pid, values[i]);
                }
                else
                {
                    props[i] = new StoreProperty(descs[i].PropId, values[i]);
                }
            }
            return props;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="store">
        ///   <para />
        /// </param>
        /// <param name="obj">
        ///   <para />
        /// </param>
        /// <remarks />
        public void Refresh(ISchedulerStore store, IClusterStoreObject obj)
        {
            _customprops.Clear();
            _custompropsChanged.Clear();

            StoreProperty[] props;

            store.GetCustomProperties(_obType, obj.Id, out props);

            foreach (StoreProperty prop in props)
            {
                _customprops[prop.Id.Name] = prop.Value.ToString();
            }
        }

        internal int ChangeCount
        {
            get { return _custompropsChanged.Count; }
        }
    }

}
