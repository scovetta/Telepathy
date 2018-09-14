namespace Microsoft.Hpc.Scheduler.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;

    [Serializable]
    [DataContract]
    public class BalanceRequest
    {
        [DataMember]
        public int AllowedCoreCount { get; set; }

        [DataMember]
        public IList<int> TaskIds { get; set; }
    }
}
