//------------------------------------------------------------------------------
// <copyright file="PersistantSessionAsyncResult.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       AsyncResult for async create persistant session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    /// AsyncResult for async create persistant session
    /// </summary>
    internal class DurableSessionAsyncResult : SessionAsyncResult
    {
        /// <summary>
        /// Initializes a new instance of the PersistantSessionAsyncResult class
        /// </summary>
        /// <param name="contract">the service contract</param>
        /// <param name="startInfo">the session start info</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="callback">the async callback</param>
        /// <param name="asyncState">the async state</param>
        public DurableSessionAsyncResult(
            SessionStartInfo startInfo,
            Binding binding,
            AsyncCallback callback,
            object asyncState) :
            base(startInfo, binding, callback, asyncState)
        {

        }

        /// <summary>
        /// Override BeginCreateSession to create persistant session
        /// </summary>
        /// <param name="uri">the uri to the broker launcher</param>
        protected override void BeginCreateSession(string uri)
        {
            BrokerLauncherClient brokerLauncher = new BrokerLauncherClient(new Uri(uri), this.StartInfo, this.binding);

            brokerLauncher.BeginCreateDurable(this.StartInfo.Data, this.SessionId, CreateCallback, brokerLauncher);
        }

        /// <summary>
        /// Override EndCreateSession to end the async operation
        /// </summary>
        /// <param name="ar">Async result</param>
        /// <returns>the session Info</returns>
        protected override BrokerInitializationResult EndCreateSession(IAsyncResult ar)
        {
            BrokerLauncherClient brokerLauncher = (BrokerLauncherClient)ar.AsyncState;

            try
            {
                return brokerLauncher.EndCreateDurable(ar);
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(brokerLauncher);
            }
        }
    }
}
