namespace Microsoft.Hpc.ServiceBroker
{
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.ServiceBroker.BackEnd;

    internal class GracefulPreemptionResult
    {
        public GracefulPreemptionResult(List<DispatcherInfo> toShutdown, bool shouldResumeRemaining) 
        {
            this.DispatchersToShutdown = toShutdown;
            this.ResumeRemaining = shouldResumeRemaining;
        }

        public IEnumerable<int> TaskIdsInInterest { get; set; }

        public bool ResumeRemaining { get; set; }

        public List<DispatcherInfo> DispatchersToShutdown { get; set; }
    }
}
