//-------------------------------------------------------------------------------------------------
// <copyright file="HPCIdentity.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//    Simple class to manage the identity of an HPC cluster. For V2, this is either
//    the process identity in a single HN deployment or the identity of the HA SQL
//    network name resources in a failover cluster.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Security.Principal;

    using Microsoft.ComputeCluster.Management.Win32Helpers;

    public sealed class HPCIdentity : IDisposable
    {
        /// <summary>
        /// The virtual identity for a headnode service to use when connecting to the compute
        /// nodes or accessing a resource hosted by another headnode. Null if not running in
        /// a failover cluster.
        /// </summary>
        internal static WindowsIdentity hpcIdentity;

        internal static bool isSystem = WindowsIdentity.GetCurrent().IsSystem;

        internal WindowsImpersonationContext context;

        private static string clusterName = null;

        private bool _disposed = false;

        private object lockObj = new object();

        static HPCIdentity()
        {
            if (isSystem)
            {
                HPCIdentity.InitializeIdentity();
            }
        }

        /// <summary>
        /// Creates a new HPCIdentity instance
        /// </summary>
        public HPCIdentity()
        {
        }

        public void Dispose()
        {
            lock (lockObj)
            {
                if (!_disposed)
                {
                    if (context != null)
                    {
                        try
                        {
                            // Undo can throw but should never do that (ha!). Don't let the process explode due to an unhandled exception
                            context.Undo();
                        }
                        catch { }
                        context.Dispose();
                        context = null;
                    }

                    _disposed = true;
                }
            }
        }

        public void Impersonate()
        {
            // only considering impersonating if the process is LocalSystem and we're on
            // a failover cluster.
            if (isSystem && hpcIdentity != null)
            {
                context = hpcIdentity.Impersonate();
            }
        }

        [Obsolete("Please use context.FabricContext.Registry.GetClusterNameAsync")]
        public static string ClusterName
        {
            get
            {
                if (clusterName == null)
                {
                    using (Win32.RegistryKey key = Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\HPC"))
                    {
                        // this code is meant to be shared by all services but for now it is only used by the scheduler.
                        // Throwing a scheduler exception might make sense for now but doesn't for future use. The chance
                        // of this value not being in place seems very low.
                        clusterName = (string)key.GetValue("ClusterName");
                    }
                }

                return clusterName;
            }

            set
            {
                clusterName = value;
            }
        }

         /// <summary>
         /// Sets the impersonation identity to the failover cluster identity. This is only 
         /// called from the services to avoid loading HPCUtils from the client.
         /// </summary>
        public static void InitializeIdentity()
        {
            HPCIdentity.hpcIdentity = HAUtils.GetHPCIdentity();
        }

    }
}
