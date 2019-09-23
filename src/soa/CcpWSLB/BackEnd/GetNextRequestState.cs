// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System.Diagnostics;

    /// <summary>
    /// Delegate of the GetNextRequest method.
    /// </summary>
    internal delegate void GetNextRequestDelegate(int serviceClientIndex);

    /// <summary>
    /// This class is used to trigger the GetNextRequest method.
    /// </summary>
    internal class GetNextRequestState
    {
        /// <summary>
        /// It stores the delegate of GetNextRequest method.
        /// </summary>
        private GetNextRequestDelegate getNextRequest;

        /// <summary>
        /// It stores the parameter of GetNextRequest method.
        /// </summary>
        private int clientIndex;

        /// <summary>
        /// Create an instance of the GetNextRequestState.
        /// </summary>
        public GetNextRequestState(GetNextRequestDelegate getNextRequest, int clientIndex)
        {
            Debug.Assert(getNextRequest != null);
            Debug.Assert(clientIndex >= 0);

            this.getNextRequest = getNextRequest;
            this.clientIndex = clientIndex;
        }

        public void Invoke()
        {
            BrokerTracing.TraceVerbose("[GetNextRequestState]. Invoke: Call GetNextRequest method, clientIndex is {0}", this.clientIndex);
            this.getNextRequest(this.clientIndex);
        }
    }
}
