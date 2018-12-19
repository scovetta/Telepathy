namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [Serializable]
    [DataContract]
    public class SoaBalanceRequest
    {
        [DataMember]
        public int AllowedCoreCount { get; set; }

        [DataMember]
        public IList<int> TaskIds { get; set; }
    }
}
