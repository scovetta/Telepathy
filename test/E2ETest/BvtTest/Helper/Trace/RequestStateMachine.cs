// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AITestLib.Helper.Trace
{
    class RequestStateMachine
    {
        private RequestState state = RequestState.BeforeFrontEndReceived;
        private Dictionary<RequestState, Dictionary<RequestAction, Action>> rule;

        private void InitRule()
        {
            rule = new Dictionary<RequestState, Dictionary<RequestAction, Action>>();

            foreach (var e in Enum.GetValues(typeof(RequestState)))
            {
                rule[(RequestState)e] = new Dictionary<RequestAction, Action>();
            }

            rule[RequestState.BeforeFrontEndReceived][RequestAction.FrontEndRequestReceived] = () => { state = RequestState.FrontEndReceived; };
            rule[RequestState.BeforeFrontEndReceived][RequestAction.FrontEndRequestRejected] = () => { state = RequestState.FrontEndRequestRejected; };

            rule[RequestState.FrontEndReceived][RequestAction.BackendRequestSent] = () => { state = RequestState.Dispatching; };

            rule[RequestState.Dispatching][RequestAction.BackendRequestSentFailed] = () => { state = RequestState.BackendDispatchFailedOrRetry; };
            rule[RequestState.Dispatching][RequestAction.BackendResponseReceived] = () => { state = RequestState.BackendRecived; };
            rule[RequestState.Dispatching][RequestAction.BackendResponseReceivedFailed] = () => { state = RequestState.BackendDispatchFailedOrRetry; };

            rule[RequestState.BackendRecived][RequestAction.FrontEndResponseSent] = () => { state = RequestState.RequestReturned; };
            rule[RequestState.BackendRecived][RequestAction.FrontEndResponseSentFailed] = () => { state = RequestState.RequestReturnedFailed; };

            rule[RequestState.BackendDispatchFailedOrRetry][RequestAction.BackendResponseReceivedFailedRetryLimitExceed] = () => { state = RequestState.BackendDispatchFailedNoRetry; };
            rule[RequestState.BackendDispatchFailedOrRetry][RequestAction.BackendRequestSent] = () => { state = RequestState.Dispatching; };

            rule[RequestState.BackendDispatchFailedNoRetry][RequestAction.BackendResponseGenerateFaultReply] = () => { state = RequestState.BackendGenerateFaultReply; };

            rule[RequestState.BackendGenerateFaultReply][RequestAction.FrontEndResponseSent] = () => { state = RequestState.RequestReturned; };
            rule[RequestState.BackendGenerateFaultReply][RequestAction.FrontEndResponseSentFailed] = () => { state = RequestState.RequestReturnedFailed; };
        }

        public RequestStateMachine()
        {
            InitRule();
        }

        public void Process(RequestAction action)
        {
            try
            {
                rule[state][action]();
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format("Invalid state transistion from {0} with action {1}", state, action));
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
