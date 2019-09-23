// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper.Trace
{

    public enum TracingType
    {
        TraceEvent,
        TraceInformation,
        TraceData,
        TraceTransfer
    }

    public enum TracingTestActionId
    {
        Default,
        TraceWithDelay,
        TraceLargeAmount,
        TraceBigSize,
        TraceFaultException,
        TraceProcessExit,
        TraceRequestProcessing,
        NoUserTrace,
        BrokerNodeOffline,
        TestTraceResponseTwice,
        RetryOperationError
    }
}
