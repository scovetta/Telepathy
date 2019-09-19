// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Microsoft.Hpc.Scheduler.Session
{
    public class HpcTraceSource : TraceSource
    {
        /// <summary>
        /// the name for service trace source.
        /// </summary>
        const string TraceSourceName = "Microsoft.Hpc.HpcServiceHosting";

        /// <summary>
        /// Setting if we want to propagate activityId
        /// </summary>
        const string PropagateActivityValue = "propagateActivity";

        public HpcTraceSource() : base(TraceSourceName)
        {
            Trace.AutoFlush = true;
        }

        protected override string[] GetSupportedAttributes()
        {
            return new string[] { PropagateActivityValue };
        }
    }
}
