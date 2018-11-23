namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    public class SchedulerNodeInternal : SchedulerNode
    {
        protected Dictionary<PropertyId, StoreProperty> changeProps = new Dictionary<PropertyId, StoreProperty>();

        public SchedulerNodeInternal(ISchedulerStore store, PropertyRow row)
            : base (store, row)
        {            
        }

        public SchedulerNodeInternal(ISchedulerStore store, IClusterNode node, PropertyId[] pids)    
            : base (store)
        {
            this.Init(node, pids);
        }

        public object this[PropertyId pid]
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(pid, out prop))
                {
                    return prop.Value;
                }
                return null;
            }

            set
            {
                StoreProperty prop = new StoreProperty(pid, value);

                this._props[pid] = prop;
                this.changeProps[pid] = prop;
            }
        }

        public IClusterNode NodeEx
        {
            get { return this._node; }
        }

        public void Commit()
        {
            if (this.changeProps.Keys.Count > 0)
            {
                this._node.SetProps(new List<StoreProperty>(this.changeProps.Values).ToArray());

                this.changeProps.Clear();
            }
        }

        public SecurityIdentifier Sid
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(NodePropertyIds.Sid, out prop))
                {
                    return new SecurityIdentifier (prop.Value as string);
                }

                return null;
            }
        }

        public DateTime LastPingTime
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(NodePropertyIds.LastPingTime, out prop))
                {
                    return (DateTime)prop.Value;
                }

                return DateTime.MinValue;
            }
        }

        public void SetUnreachable()
        {
            // Try to set the DB first. If it fails, the exception will be thrown and the in-memory state will not be set.
            this._node.SetUnreachable();
            this[NodePropertyIds.Reachable] = false;
        }

        
        public void SetReachable()
        {
            // Try to set the DB first. If it fails, the exception will be thrown and the in-memory state will not be set.            
            this._node.SetReachable();
            this[NodePropertyIds.Reachable] = true;
        }

        bool? _Affinity_Default = null;
        public bool? Affinity
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(NodePropertyIds.Affinity, out prop))
                {
                    return (bool?)prop.Value;
                }

                return this._Affinity_Default;
            }
        }

        public PropertyRow GetProps(params PropertyId[] pids)
        {
            if (pids == null)
            {
                return null;
            }

            PropertyRow result = new PropertyRow(pids.Length);

            StoreProperty propNotFound = new StoreProperty(StorePropertyIds.Error, PropertyError.NotFound);

            for (int i = 0; i < pids.Length; i++)
            {
                StoreProperty prop;

                if (this._props.TryGetValue(pids[i], out prop))
                {
                    result[i] = prop;
                }
                else
                {
                    result[i] = propNotFound;
                }
            }

            return result;
        }
    }
}
