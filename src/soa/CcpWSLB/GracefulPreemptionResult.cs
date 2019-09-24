// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker
{
    using System.Collections.Generic;

    using Microsoft.Telepathy.ServiceBroker.BackEnd;

    internal class GracefulPreemptionResult
    {
        public GracefulPreemptionResult(List<DispatcherInfo> toShutdown, bool shouldResumeRemaining) 
        {
            this.DispatchersToShutdown = toShutdown;
            this.ResumeRemaining = shouldResumeRemaining;
        }

        public IEnumerable<string> TaskIdsInInterest { get; set; }

        public bool ResumeRemaining { get; set; }

        public List<DispatcherInfo> DispatchersToShutdown { get; set; }
    }
}
