namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Internal;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    public class SchedulerTaskInternal : SchedulerTask
    {
        IClusterJob _job = null;
        JobUnitType _unitType = JobUnitType.Core;

        public SchedulerTaskInternal(IClusterJob job, JobUnitType unitType)
            : this (job, unitType, 0)
        {}

        public SchedulerTaskInternal(IClusterJob job, JobUnitType unitType, int taskId)
            : this (job, unitType, taskId, null)
        {}

        public SchedulerTaskInternal(IClusterJob job, JobUnitType unitType, int taskId, PropertyId[] propIds)
        {
            this._job = job;
            this._unitType = unitType;

            if (taskId > 0)
            {
                this.InitFromTask (job.OpenTask (taskId), propIds);
            }
        }

        public IClusterTask TaskEx
        {
            get { return this._task; }
        }

        public Int32 Id
        {
            get { return this._task.Id; }
        }

        public void InitFromRow(PropertyRow row)
        {
            this._InitFromRow(row);
            
            if (this._task == null)
            {
                StoreProperty prop;

                if (this._props.TryGetValue(TaskPropertyIds.TaskObject, out prop))
                {
                    if (prop != null)
                    {
                        this._task = (IClusterTask)prop.Value;
                    }
                }
            }
        }

        public void LoadProps(PropertyId[] propIds)
        {
            this.LoadProps (this._task.GetProps (propIds));
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
                this._changeProps[pid] = prop;
            }
        }

        public void SetProps(params StoreProperty[] props)
        {
            if (props != null)
            {
                foreach (StoreProperty prop in props)
                {
                    this._props[prop.Id] = prop;
                    this._changeProps[prop.Id] = prop;
                }
            }
        }

        /// <summary>
        /// Refresh property values in memory but not write it to DB(because they are already in DB)
        /// </summary>
        /// <param name="props"></param>
        public void Refresh(params StoreProperty[] props)
        {
            if (props != null)
            {
                foreach (StoreProperty prop in props)
                {
                    this._props[prop.Id] = prop;
                }
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

        public JobUnitType UnitType
        {
            get { return this._unitType; }
        }


        enum UnitPropIdx
        {
            Min = 0,
            Max = 1,
            Allocated = 2,
            Total = 3,
        }

        static PropertyId[] _CorePropIds = 
            {
                TaskPropertyIds.MinCores,
                TaskPropertyIds.MaxCores,
                TaskPropertyIds.AllocatedCores,
                TaskPropertyIds.TotalCoreCount,
            };

        static PropertyId[] _NodePropIds = 
            {
                TaskPropertyIds.MinNodes,        
                TaskPropertyIds.MaxNodes, 
                TaskPropertyIds.AllocatedNodes,
                TaskPropertyIds.TotalNodeCount,
            };

        static PropertyId[] _SocketPropIds = 
            {
                TaskPropertyIds.MinSockets,
                TaskPropertyIds.MaxSockets,
                TaskPropertyIds.AllocatedSockets,
                TaskPropertyIds.TotalSocketCount,
            };


        PropertyId[] PSetByType()
        {
            switch (this.UnitType)
            {
                case JobUnitType.Core:
                    return _CorePropIds;
                    
                case JobUnitType.Socket:
                    return _SocketPropIds;
                    
                case JobUnitType.Node:
                    return _NodePropIds;
            }
            
            return _CorePropIds;
        }

        private int? GetIntProperty(PropertyId pid, int? defaultValue)
        {
            StoreProperty prop;
            if (this._props.TryGetValue(pid, out prop))
            {
                return (int)prop.Value;
            }
            
            return defaultValue;
        }                

        public int? Min
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.Min], null);
            }
        }

        
        public int? Max
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.Max], null);
            }
        }
            
        public int? TotalUnits
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.Total], null);
            }
        }

        public ICollection<KeyValuePair<string, int>> AllocatedUnits
        {
            get
            {
                StoreProperty prop;
                
                if (this._props.TryGetValue(this.PSetByType()[(int)UnitPropIdx.Allocated], out prop))
                {
                    return (ICollection<KeyValuePair<string, int>>)prop.Value;
                }

                return new KeyValuePair<string, int>[] { };
            }
        }

        internal ICollection<string> RequiredNodesList
        {
            get { return PropertyUtils2.String2Array(this[TaskPropertyIds.RequiredNodes] as string); }
        }

        internal ICollection<string> DependentTasksList
        {
            get { return PropertyUtils2.String2Array(this[TaskPropertyIds.DependsOn] as string); }
        }

        public int RecordId
        {
            get
            {
                return (int)(this.GetIntProperty(ProtectedTaskPropertyIds.RecordId, 0));
            }
        }        
    }
}
