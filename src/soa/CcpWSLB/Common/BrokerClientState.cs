//------------------------------------------------------------------------------
// <copyright file="BrokerClientState.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker client state
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    /// <summary>
    /// Broker client state
    /// </summary>
    internal enum BrokerClientState
    {
        /// <summary>
        /// Broker client not started
        /// </summary>
        NotStarted = 0,

        /// <summary>
        ///  Broker client connected
        /// </summary>
        ClientConnected = 1,

        /// <summary>
        /// End of message received
        /// </summary>
        EndRequests = 2,

        /// <summary>
        /// All request done
        /// </summary>
        AllRequestDone = 3,

        /// <summary>
        /// Get response
        /// </summary>
        GetResponse = 4,

        /// <summary>
        /// Client Disconnectted
        /// </summary>
        Disconnected = 5,
    }
}
