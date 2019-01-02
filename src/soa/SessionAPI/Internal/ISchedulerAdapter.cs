namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISchedulerAdapter
    {
        Task<bool> UpdateBrokerInfoAsync(int sessionId, Dictionary<string, object> properties);

        bool IsDiagTraceEnabled(int sessionId);


    }
}
