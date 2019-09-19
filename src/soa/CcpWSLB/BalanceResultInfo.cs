// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    using System.Collections.Generic;

    internal class BalanceResultInfo
    {
        public BalanceResultInfo(bool fastBalance)
        {
            this.UseFastBalance = fastBalance;
            this.GracefulPreemptionResults = new List<GracefulPreemptionResult>();
            this.ShouldCancelJob = false;
        }

        public bool UseFastBalance { get; private set; }

        public List<GracefulPreemptionResult> GracefulPreemptionResults { get; private set; }

        public bool ShouldCancelJob { get; set; }
    }
}
