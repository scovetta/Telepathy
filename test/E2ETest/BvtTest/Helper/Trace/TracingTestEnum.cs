// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace AITestLib.Helper.Trace
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
