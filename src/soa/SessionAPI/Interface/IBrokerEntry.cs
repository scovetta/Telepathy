// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System.Threading.Tasks;
    /// <summary>
    /// Interface for BrokerEntry
    /// </summary>
    public interface IBrokerEntry
    {
        /// <summary>
        /// Gets the <see cref="BrokerAuthorization"/>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        BrokerAuthorization Auth { get; }

        /// <summary>
        /// Gets the session ID
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        string SessionId { get; }

        /// <summary>
        /// Attach to the broker.
        /// </summary>
        void Attach();

        /// <summary>
        /// Broker finished event handler
        /// </summary>
        event EventHandler BrokerFinished;

        /// <summary>
        /// Close the broker
        /// </summary>
        /// <param name="cleanData">
        ///   <para />
        /// </param>
        Task Close(bool cleanData);

        /// <summary>
        /// Run the broker
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="brokerInfo">indicate the broker start info</param>
        /// <returns>initialization result</returns>
        BrokerInitializationResult Run(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo);

        /// <summary>
        /// Get the fronted for in-process broker
        /// </summary>
        /// <param name="callbackInstance">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        IBrokerFrontend GetFrontendForInprocessBroker(IResponseServiceCallback callbackInstance);
    }
}
