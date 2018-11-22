namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Store;

    public class SchedulerInternal : Scheduler
    {
        SchedulerInternal(ISchedulerStore store)
            : base(store)
        {
        }

        public static Scheduler Convert(ISchedulerStore store)
        {
            return new SchedulerInternal(store);
        }
    }
}
