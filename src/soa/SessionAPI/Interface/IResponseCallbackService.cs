// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// The interface which is used as a callback which broker is send back the response through.
    /// </summary>
    [ServiceContract(Name = "ResponseServiceCallback", Namespace = "http://hpc.microsoft.com")]
    public interface IResponseServiceCallback
    {
        /// <summary>
        /// Send back the response to the client, Fetch the asyncstate from header
        /// </summary>
        /// <param name="m">If m has a special Action = "http://hpc.microsoft.com/EndOfGetResponse", there is no more responses.</param>
        [OperationContract(Action = "*", IsOneWay = true)]
        void SendResponse(Message m);

        /// <summary>
        /// Send back the response to the client
        /// </summary>
        /// <param name="m">message</param>
        /// <param name="clientData">client callback id</param>
        void SendResponse(Message m, string clientData);

        /// <summary>
        /// Sends a broker down signal
        /// </summary>
        void SendBrokerDownSignal(bool isBrokerNodeDown);

        /// <summary>
        /// Closes the callback object
        /// </summary>
        void Close();
    }
}