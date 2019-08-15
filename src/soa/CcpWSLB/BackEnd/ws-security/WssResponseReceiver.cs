//-----------------------------------------------------------------------
// <copyright file="WssResponseReceiver.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     This is a class for receiving response from Java WSS4J service host.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    /// <summary>
    /// This is a class for receiving response from Java WSS4J service host.
    /// </summary>
    internal class WssResponseReceiver : OnPremiseResponseReceiver
    {
        /// <summary>
        /// Initializes a new instance of the WssResponseReceiver class.
        /// </summary>
        public WssResponseReceiver(IDispatcher dispatcher)
            : base(dispatcher)
        {
        }
    }
}
