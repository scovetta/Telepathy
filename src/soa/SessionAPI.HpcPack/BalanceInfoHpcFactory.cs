namespace Microsoft.Hpc.Scheduler.Session.HpcPack
{
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Data;

    public static class BalanceInfoHpcFactory
    {
        public static BalanceInfo FromBalanceRequests(IEnumerable<BalanceRequest> balanceRequests)
        {
            return new BalanceInfo(balanceRequests.Select(b => new SoaBalanceRequest() { AllowedCoreCount = b.AllowedCoreCount, TaskIds = b.TaskIds }).ToList());
        }
    }
}