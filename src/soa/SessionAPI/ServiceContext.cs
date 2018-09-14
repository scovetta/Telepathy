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

    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Data.DataContainer;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;
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
        /// the soa data server information
        /// </summary>
        private static DataServerInfo dataServerInfo;

        /// <summary>
        /// unrecoverable data exception that happens on data server info initiazliation
        /// </summary>
        private static DataException DataServerInfoConfigException;

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

        private static DataClient GetNonDomainDataClient(string dataClientId)
        {
            string jobIdEnvVar = Environment.GetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.JobIDEnvVar);
            if (!int.TryParse(jobIdEnvVar, out int jobId))
            {
                throw new InvalidOperationException($"jobIdEnvVar is invalid:{jobIdEnvVar}");
            }

            string jobSecretEnvVar = Environment.GetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.JobSecretEnvVar);
            return DataClient.Open(dataClientId, jobId, jobSecretEnvVar);
        }

        /// <summary>
        ///   <para>Gets the instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Data.DataClient" /> class that has the specified 
        /// string identifier.</para>
        /// </summary>
        /// <param name="dataClientId">
        ///   <para>A 
        /// <see cref="System.String" /> that specifies the identifier for the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Data.DataClient" /> object that you want to get.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Session.Data.DataClient" /> object that has the specified identifier.</para>
        /// </returns>
        public static DataClient GetDataClient(string dataClientId)
        {
            Microsoft.Hpc.Scheduler.Session.Data.Utility.ValidateDataClientId(dataClientId);

            if (!inService)
                throw new InvalidOperationException(SR.ServiceContextIsNotAvailable);

            using (NonHARegistry registry = new NonHARegistry())
            {
                if (registry.CheckIfNonDomain().GetAwaiter().GetResult())
                {
                    return GetNonDomainDataClient(dataClientId);
                }
            }

            if (SoaHelper.IsOnAzure())
            {
                // if it is running in Azure, just talk to DataProxy
                // Note: headnode name will not be used.
                return DataClient.Open("dataproxy", dataClientId);
            }

            lock (lockDataServerInfoInitialized)
            {
                if (!bDataServerInfoInitialized)
                {
                    string strDataServerInfo = Environment.GetEnvironmentVariable(Microsoft.Hpc.Scheduler.Session.Internal.Constant.SoaDataServerInfoEnvVar);
                    if (!string.IsNullOrEmpty(strDataServerInfo))
                    {
                        dataServerInfo = new DataServerInfo(strDataServerInfo);
                    }
                    else
                    {
                        logger.TraceEvent(TraceEventType.Error, 0, "[ServiceContext] .GetDataClient: no data server configured");
                        DataServerInfoConfigException = new DataException(DataErrorCode.NoDataServerConfigured, SR.NoDataServerConfigured);
                    }

                    bDataServerInfoInitialized = true;
                }
            }

            if (dataServerInfo != null)
            {
                string containerPath = DataContainerHelper.OpenDataContainer(dataServerInfo, dataClientId);
                return new DataClient(dataClientId, containerPath, /*readOnly = */true);
            }
            else
            {
                throw DataServerInfoConfigException;
            }
        }

        /// <summary>
        /// Check if the current process is service host
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Doing so would introduce dependency between fields or even less performant code when initializing inService")]
        static ServiceContext()
        {
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            if (processName == "HpcServiceHost" || processName == "HpcServiceHost32")
            {
                inService = true;
                Microsoft.Hpc.Scheduler.Session.Data.Internal.TraceHelper.TraceSource = Logger;
            }
        }
    }
}