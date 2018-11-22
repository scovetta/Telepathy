namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;

    public class SchedulerPoolInternal : SchedulerPool
    {
        int _currentAllocation=0;
        int _guarantee=0;
        int _minCoreAllocation = 0;
        public SchedulerPoolInternal(ISchedulerStore store):base (store)
        {
            this._currentAllocation = 0;
            this._guarantee = 0;
        }

        public void InitFromClusterPool(IClusterPool pool)
        {
            this._pool = pool;
            this.InitFromObject(pool,null);
        }


        public new int CurrentAllocation
        {
            get { return this._currentAllocation; }
            set { this._currentAllocation = value; }
        }


        public new int Guarantee
        {
            get { return this._guarantee; }
            set { this._guarantee = value; }
        }

        public int MinCoreAllocation
        {
            get { return this._minCoreAllocation; }
            set { this._minCoreAllocation = value; }
        }

        public bool AboveGuarantee
        {
            get { return this._currentAllocation > this._guarantee; }
        }

        public bool BelowGuarantee
        {
            get { return this._currentAllocation < this._guarantee; }
        }
        
    }
}