// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper.Trace
{
    using System;
    using System.Collections.Generic;

    class RequestStateMachine
    {
        private RequestState state = RequestState.BeforeFrontEndReceived;
        private Dictionary<RequestState, Dictionary<RequestAction, Action>> rule;

        private void InitRule()
        {
            this.rule = new Dictionary<RequestState, Dictionary<RequestAction, Action>>();

            foreach (var e in Enum.GetValues(typeof(RequestState)))
            {
                this.rule[(RequestState)e] = new Dictionary<RequestAction, Action>();
            }

            this.rule[RequestState.BeforeFrontEndReceived][RequestAction.FrontEndRequestReceived] = () => { this.state = RequestState.FrontEndReceived; };
            this.rule[RequestState.BeforeFrontEndReceived][RequestAction.FrontEndRequestRejected] = () => { this.state = RequestState.FrontEndRequestRejected; };

            this.rule[RequestState.FrontEndReceived][RequestAction.BackendRequestSent] = () => { this.state = RequestState.Dispatching; };

            this.rule[RequestState.Dispatching][RequestAction.BackendRequestSentFailed] = () => { this.state = RequestState.BackendDispatchFailedOrRetry; };
            this.rule[RequestState.Dispatching][RequestAction.BackendResponseReceived] = () => { this.state = RequestState.BackendRecived; };
            this.rule[RequestState.Dispatching][RequestAction.BackendResponseReceivedFailed] = () => { this.state = RequestState.BackendDispatchFailedOrRetry; };

            this.rule[RequestState.BackendRecived][RequestAction.FrontEndResponseSent] = () => { this.state = RequestState.RequestReturned; };
            this.rule[RequestState.BackendRecived][RequestAction.FrontEndResponseSentFailed] = () => { this.state = RequestState.RequestReturnedFailed; };

            this.rule[RequestState.BackendDispatchFailedOrRetry][RequestAction.BackendResponseReceivedFailedRetryLimitExceed] = () => { this.state = RequestState.BackendDispatchFailedNoRetry; };
            this.rule[RequestState.BackendDispatchFailedOrRetry][RequestAction.BackendRequestSent] = () => { this.state = RequestState.Dispatching; };

            this.rule[RequestState.BackendDispatchFailedNoRetry][RequestAction.BackendResponseGenerateFaultReply] = () => { this.state = RequestState.BackendGenerateFaultReply; };

            this.rule[RequestState.BackendGenerateFaultReply][RequestAction.FrontEndResponseSent] = () => { this.state = RequestState.RequestReturned; };
            this.rule[RequestState.BackendGenerateFaultReply][RequestAction.FrontEndResponseSentFailed] = () => { this.state = RequestState.RequestReturnedFailed; };
        }

        public RequestStateMachine()
        {
            this.InitRule();
        }

        public void Process(RequestAction action)
        {
            try
            {
                this.rule[this.state][action]();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format("Invalid state transistion from {0} with action {1}", this.state, action));
            }
        }
    }

    enum RequestAction
    {
        FrontEndRequestReceived,
        FrontEndResponseSent,
        BackendRequestSent,
        BackendResponseReceived,
        BackendRequestSentFailed,
        BackendResponseReceivedFailed,
        BackendResponseReceivedFailedRetryLimitExceed,
        BackendResponseGenerateFaultReply,
        FrontEndResponseSentFailed,
        FrontEndRequestRejected
    }

    enum RequestState
    {
        BeforeFrontEndReceived,
        FrontEndReceived,
        FrontEndRequestRejected,
        Dispatching,
        BackendRecived,
        BackendDispatchFailedOrRetry,
        BackendDispatchFailedNoRetry,
        BackendGenerateFaultReply,
        FrontEndSendBack,
        FrontEndSendBackFailed,
        RequestReturned,
        RequestReturnedFailed
    }
}
