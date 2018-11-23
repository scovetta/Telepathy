namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Hpc.Scheduler;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    /// <summary>
    /// A wrapper for resource object
    /// </summary>
    public partial class SchedulerResourceInternal : SchedulerObjectBase
    {
        /// <summary>
        /// properties changed and has been submitted
        /// </summary>
        Dictionary<PropertyId, StoreProperty> changedProps = new Dictionary<PropertyId, StoreProperty>();
        /// <summary>
        /// the resource object which could be used to update the record
        /// </summary>
        IClusterResource resource = null;
        
        /// <summary>
        /// In memory variable used to store the result from the shrinkrequest db table
        /// </summary>
        bool _shouldShrink = false;

        SchedulerResourceInternal(SchedulerResourceInternal that)
        {
            this._props = new Dictionary<PropertyId, StoreProperty>(that._props);
            this.changedProps = new Dictionary<PropertyId, StoreProperty>(that.changedProps);
            this.resource = that.resource;
        }

        public SchedulerResourceInternal(PropertyRow row)
        {
            this._InitFromRow(row);
            
            StoreProperty prop = row[ResourcePropertyIds.ResourceObject];
            
            if (prop != null)
            {
                this.resource = (IClusterResource) prop.Value;
            }
        }

        public SchedulerResourceInternal(IClusterResource resource, params PropertyId[] propIds)
        {
            this.resource = resource;
            this.InitFromObject(resource, propIds);
        }
        
        #region virtual methods implementation
        protected override PropertyId[] GetPropertyIds()
        {
            return base.GetPropertyIds();
        }
        #endregion

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
                this.changedProps[pid] = prop;
            }
        }

        /// <summary>
        /// update the object in memory directly without put the change to the pending for DB update list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void Update(PropertyId id, object value)
        {
            this._props[id] = new StoreProperty(id, value);
        }

        public ResourceState State
        {
            get { return (ResourceState)this[ResourcePropertyIds.State]; }
        }

        public ResourceJobPhase JobPhase
        {
            get { return (ResourceJobPhase)this[ResourcePropertyIds.JobPhase]; }
        }

        public int Id
        {
            get { return (int)this[ResourcePropertyIds.Id]; }
        }

        public int NodeId
        {
            get
            {
                return (int)this[ResourcePropertyIds.NodeId];
            }
        }

        public long SocketId
        {
            get
            {
                return (long)this[ResourcePropertyIds.SocketId];
            }
        }

        public string NodeName
        {
            get
            {
                return (string)this[ResourcePropertyIds.NodeName];
            }
        }

        public void Commit()
        {
            //at commit time if we have saved the state or phase for use in restore later
            //we should throw away the information if the resource gets committed without getting restored
            this._savedStateAndPhase = false;
            this._stateAtSave = ResourceState.NA;
            this._phaseAtSave = ResourceJobPhase.NA;

            if (this.changedProps.Keys.Count > 0)
            {
                this.resource.SetProps(new List<StoreProperty>(this.changedProps.Values).ToArray());
                
                this.changedProps.Clear();
            }
        }

        public SchedulerResourceInternal Clone()
        {
            return new SchedulerResourceInternal(this);
        }

        public bool ShouldShrink
        {
            get { return this._shouldShrink; }
            set { this._shouldShrink = value; }
        }

        bool _savedStateAndPhase = false;
        ResourceState _stateAtSave = ResourceState.NA;
        ResourceJobPhase _phaseAtSave = ResourceJobPhase.NA;

        /// <summary>
        /// Save the current state and phase of the resource.
        /// So, that these variables can be restored to these values later on if necessary
        /// Used for task scheduling
        /// </summary>
        public void SaveStateAndPhase()
        {
            this._stateAtSave = this.State;
            this._phaseAtSave = this.JobPhase;
            this._savedStateAndPhase = true;
        }

        /// <summary>
        /// Restore the state and phase values of the resource to the saved values 
        /// This is used when a task scheduling decision is abandoned midway if a task 
        /// cannot get all the resources it needs
        /// </summary>
        public void RestoreStateAndPhase()
        {
            if (this._savedStateAndPhase)
            {
                this[ResourcePropertyIds.State] = this._stateAtSave;
                this[ResourcePropertyIds.JobPhase] = this._phaseAtSave;
                this._savedStateAndPhase = false;
            }
        }
    }
}
