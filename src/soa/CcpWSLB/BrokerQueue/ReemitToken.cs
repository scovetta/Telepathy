// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System.Threading;

    public class ReemitToken
    {
        int state = 0;

        private ReemitToken() { }

        public static ReemitToken GetToken() { return new ReemitToken(); }

        public bool Finish()
        {
            return 1 == Interlocked.Increment(ref this.state);
        }

        public bool Available { get { return this.state == 0; } }
    }
}
