// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Telepathy.RuntimeTrace;

    /// <summary>
    /// Maintains broker's identity when calling user services. BN computer account for non-
    /// failover clusters. Resource group's network name computer account failover clusters
    /// </summary>
    public class BrokerIdentity : DisposableObjectSlim
    {
        static WindowsIdentity brokerIdentity;
        static bool isSystem = WindowsIdentity.GetCurrent().IsSystem;

        private WindowsImpersonationContext context;

        /// <summary>
        /// Get identity once per process
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Easier to initialize this way")]
        static BrokerIdentity()
        {
            if (isSystem)
            {
                brokerIdentity = GetBrokerIdentity();
            }
        }

        public static bool IsHAMode
        {
            get
            {
                return brokerIdentity != null;
            }
        }

        public BrokerIdentity()
        {

        }

        /// <summary>
        /// Impersonate using broker's identity
        /// </summary>
        public void Impersonate()
        {
            // only considering impersonating if the process is LocalSystem and we're on
            // a failover cluster.
            if (isSystem && brokerIdentity != null)
            {
                this.context = brokerIdentity.Impersonate();
                WindowsPrincipal principal = new WindowsPrincipal(brokerIdentity);
                Thread.CurrentPrincipal = principal;

                TraceHelper.TraceEvent(
                    TraceEventType.Verbose,
                    "[BrokerIdentity].Impersonate: Impersonate to account {0}",
                    principal.Identity.Name);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Release the resource. Base object DisposableObjectSlim is already
        /// thread safe when invokes this method.
        /// </summary>
        protected override void DisposeInternal()
        {
            if (this.context != null)
            {
                try
                {
                    using (this.context)
                    {
                        this.context.Undo();
                    }
                }
                catch (Exception e)
                {
                    TraceHelper.TraceEvent(
                        TraceEventType.Error,
                        "[BrokerIdentity].DisposeInternal: Error happens when undo ImpersonationContext and dispose it, {0}", e);
                }
                finally
                {
                    this.context = null;
                }
            }

            base.DisposeInternal();
        }

        #endregion

        /// <summary>
        /// Get the current broker node name.
        /// </summary>
        /// <returns>broker node name</returns>
        public static string GetBrokerName()
        {
            WindowsIdentity brokerIdentity = GetBrokerIdentity();

            if (brokerIdentity != null)
            {
                // For HA broker node, return resource group network name.
                return brokerIdentity.Name;
            }
            else
            {
                // This code runs on the broker node, so return current machine name.
                return Environment.MachineName;
            }
        }

        /// <summary>
        /// Get broker's identity. If on a failover cluster, get resoruce group network name
        /// computer account login token
        /// Notice: This method returns null if it is not failover cluster or it is Azure cluster.
        /// </summary>
        /// <returns></returns>
        private static WindowsIdentity GetBrokerIdentity()
        {
            if (SoaHelper.IsOnAzure())
            {
                // Skip following logic if it is on Azure.
                return null;
            }

            int clusterState = (int)ClusterState.ClusterStateNotInstalled;

            uint ret = Win32API.GetNodeClusterState(null, out clusterState);
            if (ret != 0)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "Cannot access local failover cluster state. Error = {0}", ret);
                return null;
            }

            if (clusterState == (int)ClusterState.ClusterStateNotConfigured || clusterState == (int)ClusterState.ClusterStateNotInstalled)
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "Cannot access local failover cluster state. Error = {0}", ret);
                return null;
            }

            IntPtr hCluster = IntPtr.Zero;
            IntPtr hResource = IntPtr.Zero;

            try
            {
                // Get handle to local failover cluster
                hCluster = Win32API.OpenCluster(null);
                if (hCluster == IntPtr.Zero)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "Cannot connect to local failover cluster. Error = {0}", Marshal.GetLastWin32Error());
                    return null;
                }

                // Get handle to this broker's network name resource
                hResource = Win32API.OpenClusterResource(hCluster, Environment.MachineName);
                if (hResource == IntPtr.Zero)
                {
                    TraceHelper.TraceEvent(TraceEventType.Error, "Cannot connect to local failover cluster. Error = {0}", Marshal.GetLastWin32Error());
                    return null;
                }

                // Get the login token of the network name for the broker's resource group's network name. This
                // is a computer account login token and is use to impersonate all calls to the services. The services
                // then ensure calls are only from broker nodes or resource groups.
                IntPtr loginToken = IntPtr.Zero;

                try
                {
                    loginToken = GetNetworkNameLoginToken(hCluster, hResource);

                    if (loginToken != IntPtr.Zero)
                    {
                        // Put login token in a WindowsIdentity object 
                        // WindowsIdentity calls DuplicateToken and it is fine to close the handle later
                        return new WindowsIdentity(loginToken);
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (loginToken != IntPtr.Zero)
                        Win32API.CloseHandle(loginToken);
                }
            }

            finally
            {
                if (hResource != IntPtr.Zero)
                    Win32API.CloseClusterResource(hResource);

                if (hCluster != IntPtr.Zero)
                    Win32API.CloseCluster(hCluster);
            }
        }

        /// <summary>
        /// Returns the login token for the specified network name resource (its compute account)
        /// </summary>
        /// <param name="hCluster"></param>
        /// <param name="hResource"></param>
        /// <returns></returns>
        private static IntPtr GetNetworkNameLoginToken(IntPtr hCluster, IntPtr hResource)
        {
            int length = Win32API.MAX_COMPUTERNAME_LENGTH + 1;
            StringBuilder computerName = new StringBuilder(length);

            // Get the NETBIOS name of the local physical node
            if (!Win32API.GetComputerNameEx((int)COMPUTER_NAME_FORMAT.ComputerNamePhysicalNetBIOS, computerName, ref length))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot get node's physical computer name");

            // Get a handle to the local node
            IntPtr hClusterNode = Win32API.OpenClusterNode(hCluster, computerName.ToString());
            if (hClusterNode == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Cannot access local failover node");

            try
            {
                // Get login token 

                CLUS_NETNAME_VS_TOKEN_INFO tokenInfo = new CLUS_NETNAME_VS_TOKEN_INFO();
                tokenInfo.ProcessID = (uint)Process.GetCurrentProcess().Id;

                int bytesReturned = 0;
                IntPtr loginToken = IntPtr.Zero;
                int ret = Win32API.ClusterResourceControl_NetNameToken(hResource, hClusterNode,
                    (int)CLUSCTL_RESOURCE_CODES.CLUSCTL_RESOURCE_NETNAME_GET_VIRTUAL_SERVER_TOKEN,
                    ref tokenInfo, CLUS_NETNAME_VS_TOKEN_INFO.GetSize(), out loginToken, IntPtr.Size, out bytesReturned);
                if (ret != 0 || bytesReturned != IntPtr.Size)
                    throw new Win32Exception(ret, "Cannot retrieve network name login token");

                return loginToken;
            }

            finally
            {
                if (hClusterNode != IntPtr.Zero)
                    Win32API.CloseClusterNode(hClusterNode);
            }
        }
    }
}
