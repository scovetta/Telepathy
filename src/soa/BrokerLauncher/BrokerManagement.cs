//------------------------------------------------------------------------------
// <copyright file="BrokerManagement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker Management Service
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.ServiceModel;
    using System.Threading;

    /// <summary>
    /// BrokerManagement service allows management operations to be redirected
    /// from service instance of HpcBroker to FC resource group instance of HpcBroker. This
    /// service is only opened within the latter.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class BrokerManagement : IBrokerManagement
    {
        /// <summary>
        /// Only binding used for BrokerManagement service is named pipe binding which is used for local IPC
        /// </summary>
        internal static NetNamedPipeBinding Binding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.None);

        /// <summary>
        /// Address of BrokerManagement service
        /// </summary>
        internal static string Address = "net.pipe://localhost/BrokerManagement";

        /// <summary>
        /// Instance of HpcBroker
        /// </summary>
        private BrokerLauncher brokerLauncher;

        /// <summary>
        /// HpcBroker WCF service host
        /// </summary>
        private ServiceHost serviceHost;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="brokerLauncher">HpcBroker instance</param>
        public BrokerManagement(BrokerLauncher brokerLauncher)
        {
            this.brokerLauncher = brokerLauncher;
        }

        /// <summary>
        /// Opens BrokerManagement service
        /// </summary>
        public void Open()
        {
            ServiceHost serviceHost = new ServiceHost(this, new Uri(BrokerManagement.Address));
            serviceHost.AddServiceEndpoint(typeof(IBrokerManagement), BrokerManagement.Binding, String.Empty);
            serviceHost.Open();

            this.serviceHost = serviceHost;
        }

        /// <summary>
        /// Closes BrokerManagement service
        /// </summary>
        public void Close()
        {
            if (this.serviceHost != null)
            {
                this.serviceHost.Close();
                this.serviceHost = null;
            }
        }

        #region IBrokerManagement Members

        /// <summary>
        /// Offlines HpcBroker service
        /// </summary>
        public void Online()
        {
            this.brokerLauncher.Online();
        }

        /// <summary>
        /// Starts offlining HpcBroker service
        /// </summary>
        /// <param name="force">Forces any sessions to end</param>
        public void StartOffline(bool force)
        {
            this.brokerLauncher.StartOffline(force);
        }

        /// <summary>
        /// Whether service is offline
        /// </summary>
        /// <returns></returns>
        public bool IsOffline()
        {
            return !this.brokerLauncher.IsOnline;
        }

        #endregion
    }
}
