//------------------------------------------------------------------------------
// <copyright file="ServiceContext.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Service Context Class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Provides a property and an event that a service host uses to write traces for SOA code and handle requests to exit.</para>
    /// </summary>
    public static class ServiceContext
    {
        /// <summary>
        /// if current process is a service host
        /// </summary>
        static bool inService;

        /// <summary>
        /// the lock object to prevent register cancel event twice
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// the trace object
        /// </summary>
        private static TraceSource logger = new HpcTraceSource();

        /// <summary>
        /// the cancel events chain
        /// </summary>
        private static event EventHandler<EventArgs> exitingEvents;

        /// <summary>
        /// a flag indicating if dataServerInfo has been initialized
        /// </summary>
        private static bool bDataServerInfoInitialized;

        /// <summary>
        /// lock object for bDataServerInfoInitialized
        /// </summary>
        private static object lockDataServerInfoInitialized = new object();

        /// <summary>
        ///   <para>An event that is raised when the service host is asked to exit.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The code for the SOA service can register an event handler for this event. For information about the delegate used with this event, see 
        /// <see cref="System.EventHandler{T}" />.</para>
        /// </remarks>
        /// <seealso cref="System.EventHandler{T}" />
        /// <seealso cref="System.EventArgs" />
        public static event EventHandler<EventArgs> OnExiting
        {
            add
            {
                if (!inService)
                    throw new InvalidOperationException(SR.ServiceContextIsNotAvailable);

                exitingEvents += value;
            }
            remove
            {
                if (!inService)
                    throw new InvalidOperationException(SR.ServiceContextIsNotAvailable);

                exitingEvents -= value;
            }
        }

        /// <summary>
        /// Used to manually fire Exiting event. Called within session API and from svchost when shrinking service instances
        /// </summary>
        /// <param name="sender"></param>
        private static void FireExitingEvent(object sender)
        {
            FireExitingEvent(sender, new EventArgs());
        }

        /// <summary>
        /// Used to manually fire Exiting event. Called within session API and from svchost when shrinking service instances
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args">event args</param>
        private static void FireExitingEvent(object sender, EventArgs args)
        {
            ServiceContext.Logger.TraceEvent(
                                TraceEventType.Warning,
                                0,
                                "ServiceHost get canceled by scheduler");

            if (exitingEvents != null)
            {
                try
                {
                    exitingEvents(sender, args);
                }

                catch (Exception e)
                {
                    ServiceContext.Logger.TraceEvent(
                                TraceEventType.Warning,
                                0,
                                "[HpcServiceHost]: Exception thrown when firing OnExiting event - {0}", e);
                }
            }
        }

        /// <summary>
        ///   <para>Gets a 
        /// <see cref="System.Diagnostics.TraceSource" /> object that SOA applications can use to trace the running of SOA code and associate SOA trace messages with their source.</para> 
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Diagnostics.TraceSource" /> object that SOA applications can use to trace the running of SOA code and associate SOA trace messages with their source.</para> 
        /// </value>
        public static TraceSource Logger
        {
            get
            {
                if (!inService)
                    throw new InvalidOperationException(SR.ServiceContextIsNotAvailable);

                return logger;
            }
        }


        /// <summary>
        /// Check if the current process is service host
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Doing so would introduce dependency between fields or even less performant code when initializing inService")]
        static ServiceContext()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            if (processName == "CcpServiceHost" || processName == "CcpServiceHost32")
            {
                inService = true;
                // TODO: logger in service context
            }
        }
    }
}