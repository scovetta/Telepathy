//------------------------------------------------------------------------------
// <copyright file="CallbackManager.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Manages broker's response message callback for a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Diagnostics;

    /// <summary>
    /// Manages broker's response message callback for a session
    /// </summary>
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class CallbackManager : IResponseServiceCallback
    {
        /// <summary>
        /// Maintains internal callbacks within a session
        /// </summary>
        private Dictionary<string, IResponseServiceCallback> responseCallbacks =
                        new Dictionary<string, IResponseServiceCallback>();

        private bool isSchedulerOnAzure;

        /// <summary>
        /// Initializes a new instance of the CallbackManager class
        /// </summary>
        public CallbackManager(bool isSchedulerOnAzure)
        {
            this.isSchedulerOnAzure = isSchedulerOnAzure;
        }

        #region IResponseServiceCallback Members

        /// <summary>
        /// Called when responses arrive from broker response service
        /// </summary>
        /// <param name="message">Response message </param>
        public void SendResponse(Message message)
        {
            if (message == null || message.Headers == null)
            {
                SessionBase.TraceSource.TraceData(TraceEventType.Error, 0, "Null or headerless message received in main callback");
                return;
            }

            if (this.isSchedulerOnAzure)
            {
                // Use heartbeat to avoid being idle 1 minute.
                if (message.Headers.Action == Constant.BrokerHeartbeatAction)
                {
                    return;
                }
            }

            int index = message.Headers.FindHeader(Constant.ResponseCallbackIdHeaderName, Constant.ResponseCallbackIdHeaderNS);

            if (index != -1)
            {
                string responseCallbackId = message.Headers.GetHeader<string>(index);
                IResponseServiceCallback responseServiceCallback = null;
                bool callbackExists = false;

                lock (this.responseCallbacks)
                {
                    callbackExists = this.responseCallbacks.TryGetValue(responseCallbackId, out responseServiceCallback);
                }

                if (callbackExists)
                {
                    responseServiceCallback.SendResponse(message);
                }
                else
                {
                    SessionBase.TraceSource.TraceInformation("call back {0} doesn't exist. callbacks count: {1}", responseCallbackId, this.responseCallbacks.Count);
                    // Enumerator or async listener exited early so ignore further responses
                }
            }
            else
            {
                SessionBase.TraceSource.TraceData(TraceEventType.Error, 0, "Unexpected message received in main callback - {0}", message.Headers.Action);
            }
        }

        // Sends broker down signal to all active callbacks
        public void SendBrokerDownSignal(bool isBrokerNodeDown)
        {
            // Lock in case a new callback is registered or unregistered while we enum. The wide lock is OK here because no further
            // responses will be accepted after the heartbeat signals anyhow
            lock (this.responseCallbacks)
            {
                foreach (IResponseServiceCallback responseCallback in this.responseCallbacks.Values)
                {
                    try
                    {
                        responseCallback.SendBrokerDownSignal(isBrokerNodeDown);
                    }

                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceData(TraceEventType.Error, 0, "Error signalling broker is down. {0}", e);
                    }
                }

                // after sending broker down signal to all cllbacks, cleanup the responseCallbacks dictionary
                this.responseCallbacks.Clear();
            }
        }

        // Close down all the enumerators
        public void Close()
        {
            lock (this.responseCallbacks)
            {
                // Loop through the callbacks and close them
                foreach (IResponseServiceCallback responseCallback in this.responseCallbacks.Values)
                {
                    try
                    {
                        responseCallback.Close();
                    }

                    catch (Exception e)
                    {
                        SessionBase.TraceSource.TraceInformation("Error clsosing callback channel. {0}", e);
                    }
                }

                // all response callbacks are closed. clean the responseCallbacks dictionary
                this.responseCallbacks.Clear();
            }
        }

        #endregion

        /// <summary>
        /// We need to multiplex callbacks for multiple enumerators and async listeners
        /// or else we will have to reconnect for each
        /// This handles registration of internal callback
        /// </summary>
        /// <param name="callback">Callback to register with the manager</param>
        /// <returns>Returns a registration ID</returns>
        public string Register(IResponseServiceCallback callback)
        {
            lock (this.responseCallbacks)
            {
                // BUG 5023 : In v3 only one response enumerator (via GetResponses or AddResponseHandler is allowed per BrokerClient)
                if (this.responseCallbacks.Count != 0)
                {
                    throw new NotSupportedException(SR.OneResponseEnumerationPerBrokerClient);
                }

                // Add the callback
                string id = Guid.NewGuid().ToString();
                this.responseCallbacks.Add(id, callback);
                return id;
            }
        }

        /// <summary>
        /// Unregisters internal callback
        /// </summary>
        /// <param name="id">Callback ID to unregister</param>
        public void Unregister(string id)
        {
            lock (this.responseCallbacks)
            {
                this.responseCallbacks.Remove(id);
            }
        }

        // not implemented
        public void SendResponse(Message m, string clientData)
        {
            throw new NotImplementedException();
        }
    }
}
