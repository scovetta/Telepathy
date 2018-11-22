namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Internal;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    public class SchedulerJobInternal : SchedulerJob
    {
        bool _fShrinkEnabled = true;
        bool _fGrowEnabled = true;

        public SchedulerJobInternal(ISchedulerStore store, bool shrinkEnabled, bool growEnabled) : base(store)
        {
            this._fShrinkEnabled = shrinkEnabled;
            this._fGrowEnabled = growEnabled;
        }

        public IEnumerable<StoreProperty> PropertyList
        {
            get { return this._props.Values; }
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

        public IClusterJob Job
        {
            get
            {
                if (this._job == null)
                {
                    StoreProperty prop = this._props[JobPropertyIds.JobObject];
                    
                    if (prop != null)
                    {
                        this._job = (IClusterJob) prop.Value;
                    }
                    else
                    {
                        this._job = this._store.OpenJob(this.Id);
                    }
                }
                
                return this._job;
            }
        }

        public override JobUnitType UnitType
        {
            get
            {
                JobUnitType unitType = base.UnitType;
                if (unitType == JobUnitType.Gpu)
                {
                    unitType = JobUnitType.Socket;
                }

                return unitType;
            }
            set
            {
                base.UnitType = value;
            }
        }

        static PropertyId[] _pidsLocal = null;

        protected override PropertyId[] GetPropertyIds()
        {
            if (_pidsLocal == null)
            {
                PropertyId[] additionalPids = 
                {
                    JobPropertyIds.OrderBy,
                    JobPropertyIds.CurrentCoreCount,
                    JobPropertyIds.CurrentNodeCount,
                    JobPropertyIds.CurrentSocketCount,
                    JobPropertyIds.ComputedMaxCores,
                    JobPropertyIds.ComputedMinCores,
                    JobPropertyIds.ComputedMaxNodes,
                    JobPropertyIds.ComputedMinNodes,
                    JobPropertyIds.ComputedMaxSockets,
                    JobPropertyIds.ComputedMinSockets,                    
                    JobPropertyIds.RequestCancel,
                    JobPropertyIds.ErrorCode,
                    JobPropertyIds.JobType,
                    JobPropertyIds.TargetResourceCount,
                    JobPropertyIds.HoldUntil,
                    ProtectedJobPropertyIds.ProfileMinResources,
                    ProtectedJobPropertyIds.ProfileMaxResources
                };
                
                PropertyId[] basePids = base.GetPropertyIds();

                List<PropertyId> pids = new List<PropertyId>(basePids.Length + additionalPids.Length);
                
                pids.AddRange(basePids);
                pids.AddRange(additionalPids);
                
                _pidsLocal = pids.ToArray();
            }
        
            return _pidsLocal;
        }

        public void InternalRestoreFromXml(Stream xmlStream, XmlImportOptions options)
        {
            this.ProtectedRestoreFromXml(xmlStream, options);
        }

        void _Init()
        {
            StoreProperty prop;
            
            if (this._props.TryGetValue(JobPropertyIds.RuntimeSeconds, out prop))
            {
                this._schedprops.Add(prop);
            }
            
            if (this._props.TryGetValue(JobPropertyIds.RunUntilCanceled, out prop))
            {
                this._schedprops.Add(prop);
            }

            if (this._props.TryGetValue(ProtectedJobPropertyIds.HasNodePrepTask, out prop))
            {
                this._schedprops.Add(prop);
            }

            if (this._props.TryGetValue(JobPropertyIds.State, out prop))
            {
                this._schedprops.Add(prop);
            }

            if (this._props.TryGetValue(JobPropertyIds.StartTime, out prop))
            {
                this._schedprops.Add(prop);
            }
        }

        public void InitFromJob(IClusterJob job)
        {
            this._job = job;
            
            this.InitFromObject(job, null);
            
            this._Init();
        }

        public void InitFromJob(IClusterJob job, PropertyId[] pids)
        {
            this._job = job;

            this.InitFromObject(job, null);

            this._Init();
        }

        public void InitFromRow(PropertyRow row)
        {
            this._InitFromRow(row);
            
            if (this._job == null)
            {
                StoreProperty prop = this._props[JobPropertyIds.JobObject];
                
                if (prop != null)
                {
                    this._job = (IClusterJob) prop.Value;
                }
            }

            this._Init();
        }

        public void LoadProps(params PropertyId[] propIds)
        {
            this.LoadProps(this._job.GetProps(propIds));
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


        public int[] GetTaskIdsByType(TaskType taskType, int maxNumIds)
        {
            List<int> typeTaskIds = new List<int>();

            // Determine if there are non-canceled tasks of this type associated with the job
            using (ITaskRowSet tasks = this.Job.OpenTaskRowSet(RowSetType.Snapshot, TaskRowSetOptions.NoParametricExpansion))
            {
                tasks.SetFilter(
                    new FilterProperty(FilterOperator.Equal, TaskPropertyIds.Type, taskType),
                    new FilterProperty(FilterOperator.NotEqual, TaskPropertyIds.State, TaskState.Canceled)                    
                );
                tasks.SetColumns(
                    TaskPropertyIds.Id
                );
                tasks.SetTop(maxNumIds);

                foreach (PropertyRow taskRow in tasks)
                {
                    StoreProperty prop = taskRow[0];
                    if (prop.Id == TaskPropertyIds.Id)
                    {
                        typeTaskIds.Add((int)prop.Value);
                    }
                }
            }

            return typeTaskIds.ToArray();
        }


        int _min = 1;
        int _max = 1;
        bool _fMinMaxInited = false;

        public void InvalidateMinMax ()
        {
            this._fMinMaxInited = false;
        }

        void InitMinMax()
        {
            if (this._fMinMaxInited)
            {
                return;
            }
            
            this._fMinMaxInited = true;

            this._min = PropertyUtil.ComputeJobMin(this.UserMin, this.ComputedMin, this.AutoCalculateMin, this.CanShrink,this.ProfileMin);

            if (this.HoldUntil > DateTime.UtcNow)
            {
                this._max = this._min;
                return;
            }

            this._max = PropertyUtil.ComputeJobMax(this.UserMax, this.ComputedMax, this.AutoCalculateMax, this.CanGrow,this.ProfileMax);

            if (this._max < this._min)
            {
                this._max = this._min;
            }
        }

        public int Min
        {
            get 
            {
                this.InitMinMax();
                
                return this._min;
            }
        }
        
        public int Max
        {
            get 
            {
                this.InitMinMax();
                
                return this._max;
            }
        }

        enum UnitPropIdx
        {
            UserMin = 0,
            UserMax = 1,
            ComputedMin = 2,
            ComputedMax = 3,
            CurrentAllocation = 4,
            ProfileMin=5,
            ProfileMax=6,
        }

        static PropertyId[] _CorePropIds = 
            {
                JobPropertyIds.MinCores,
                JobPropertyIds.MaxCores,
                JobPropertyIds.ComputedMinCores,
                JobPropertyIds.ComputedMaxCores,
                JobPropertyIds.CurrentCoreCount,                
            };

        static PropertyId[] _NodePropIds = 
            {
                JobPropertyIds.MinNodes,        
                JobPropertyIds.MaxNodes,        
                JobPropertyIds.ComputedMinNodes,
                JobPropertyIds.ComputedMaxNodes,
                JobPropertyIds.CurrentNodeCount,
            };

        static PropertyId[] _SocketPropIds = 
            {
                JobPropertyIds.MinSockets,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.ComputedMinSockets,
                JobPropertyIds.ComputedMaxSockets,
                JobPropertyIds.CurrentSocketCount,
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

        private int GetIntProperty(PropertyId pid, int defaultValue)
        {
            return this.GetProperty<int>(pid, defaultValue);
        }

        private T GetProperty<T>(PropertyId pid, T defaultValue)
        {
            StoreProperty prop;
            if (this._props.TryGetValue(pid, out prop))
            {
                return (T)prop.Value;
            }

            return defaultValue;
        }

        public int UserMin
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.UserMin], 1);
            }
        }
        
        public int UserMax
        {
            get 
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.UserMax], 1);
            }
        }
            
        public int ComputedMin
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.ComputedMin], 1);
            }
        }
        
        public int ComputedMax
        {
            get 
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.ComputedMax], 0);
            }
        }
            
        public int CurrentAllocation
        {
            get
            {
                return this.GetIntProperty(this.PSetByType()[(int)UnitPropIdx.CurrentAllocation], 0);
            }
        }

        internal int ProfileMin
        {
            get
            {
                return this.GetIntProperty(ProtectedJobPropertyIds.ProfileMinResources, 0);
            }
        }

        internal int ProfileMax
        {
            get
            {
                return this.GetIntProperty(ProtectedJobPropertyIds.ProfileMaxResources, 0);
            }
        }


        public DateTime TaskLevelUpdateTime
        {
            get 
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.TaskLevelUpdateTime, out prop))
                {
                    return (DateTime)prop.Value;
                }

                return DateTime.MinValue;
            }
        }

        public DateTime MinMaxUpdateTime
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.MinMaxUpdateTime, out prop))
                {
                    return (DateTime)prop.Value;
                }

                return DateTime.MinValue;
            }
        }

        public int ErrorCode
        {
            get
            {
                return this.GetIntProperty(JobPropertyIds.ErrorCode, 0);
            }
        }

        public bool HasNodePrepTask
        {
            get 
            { 
                return this.GetProperty<bool>(ProtectedJobPropertyIds.HasNodePrepTask, false); 
            }
        }

        public bool HasNodeReleaseTask
        {
            get 
            { 
                return this.GetProperty<bool>(ProtectedJobPropertyIds.HasNodeReleaseTask, false); 
            }
        }

        public bool HasServiceTask
        {
            get
            {
                return this.GetProperty<bool>(ProtectedJobPropertyIds.HasServiceTask, false);
            }
        }

        public JobOrderByList OrderByInternal
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.OrderBy, out prop))
                {
                    return (JobOrderByList)prop.Value;
                }
                
                return null;
            }
        }

        public CancelRequest RequestCancel
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.RequestCancel, out prop))
                {
                    return (CancelRequest)prop.Value;
                }

                return CancelRequest.None;
            }
        }

        public JobType Type
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.JobType, out prop))
                {
                    return (JobType)prop.Value;
                }
                
                return JobType.Batch;
            }
        }
        
       

        public IStringCollection RequiredNodes
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.RequiredNodes, out prop))
                {
                    string val = (string) prop.Value;
                    
                    if (string.IsNullOrEmpty(val) == false)
                    {
                        return new StringCollection (PropertyUtils2.String2Array(val));
                    }
                }
                
                return new StringCollection();
            }
        }

        public ICollection<string> ComputedNodesList
        {
            get
            {
                StoreProperty prop;
                if (this._props.TryGetValue(JobPropertyIds.ComputedNodeList, out prop))
                {
                    string val = (string)prop.Value;

                    // If the value is null, we want to return null instead of empty array. 
                    // The empty array means no node to use.
                    // Null means you can use any node.
                    if (val == null)
                    {
                        return null;
                    }

                    return PropertyUtils2.String2Array(val);
                }

                return null;
            }
        }
        
        public DateTime LimitTime
        {
            get
            {
                StoreProperty prop;
                
                if (this._props.TryGetValue(JobPropertyIds.RuntimeSeconds, out prop))
                {
                    int runtime = (int)prop.Value;

                    if (this.State == JobState.Running)
                    {
                        return this.StartTime.AddSeconds(runtime);
                    }
                    else
                    {
                        return DateTime.UtcNow.AddSeconds(runtime);
                    }    
                }
                
                return DateTime.MaxValue;
            }
        }


        List<StoreProperty> _schedprops = new List<StoreProperty>(4);

        public StoreProperty[] ScheduleProps
        {
            get { return this._schedprops.ToArray(); }
        }

        public void SetRunning()
        {
            this.SetRunning(null);
        }

        public void SetRunning(IEnumerable<StoreProperty> additionalProps)
        {
            if (this.State == JobState.Queued)
            {
                List<StoreProperty> propsToSet = new List<StoreProperty>();
                propsToSet.Add(new StoreProperty(JobPropertyIds.State, JobState.Running));
                var now = DateTime.UtcNow;
                propsToSet.Add(new StoreProperty(JobPropertyIds.StartTime, now < this.SubmitTime ? this.SubmitTime : now));
                propsToSet.Add(new StoreProperty(JobPropertyIds.PendingReason, PendingReason.ReasonCode.None));

                if (additionalProps != null)
                {
                    foreach (StoreProperty additionalProp in additionalProps)
                    {
                        propsToSet.Add(additionalProp);
                    }
                }
                
                this.SetProps(propsToSet.ToArray());

                this.Commit();
            }
        }
    }
}
