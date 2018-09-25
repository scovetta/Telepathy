using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Hpc.ServiceBroker.BrokerStorage
{
    public class ReemitToken
    {
        int state = 0;

        private ReemitToken() { }

        public static ReemitToken GetToken() { return new ReemitToken(); }

        public bool Finish()
        {
            return 1 == Interlocked.Increment(ref state);
        }

        public bool Available { get { return state == 0; } }
    }
}
