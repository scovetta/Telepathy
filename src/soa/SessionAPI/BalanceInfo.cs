namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Microsoft.Hpc.Scheduler.Properties;

    [DataContract]
    public class BalanceInfo
    {
        public BalanceInfo(IList<BalanceRequest> balanceRequests)
            : this(true, balanceRequests)
        {
        }

        public BalanceInfo(int plannedCoreCount)
            : this(false, new List<BalanceRequest>() { new BalanceRequest() { AllowedCoreCount = plannedCoreCount, TaskIds = null } })
        {
        }

        public BalanceInfo(bool useFastBalance, IList<BalanceRequest> balanceRequests)
        {
            this.UseFastBalance = useFastBalance;
            this.BalanceRequests = balanceRequests;
        }

        [DataMember]
        public bool UseFastBalance { get; private set; }

        [DataMember]
        public IList<BalanceRequest> BalanceRequests { get; private set; }

        public int AllowedCoreCount
        {
            get
            {
                return this.BalanceRequests.Sum(r => r.AllowedCoreCount);
            }
        }

        // TODO: is there a properer way to indicate on hold?
        public bool OnHold
        {
            get
            {
                return this.AllowedCoreCount == 0;
            }
        }
    }
}
