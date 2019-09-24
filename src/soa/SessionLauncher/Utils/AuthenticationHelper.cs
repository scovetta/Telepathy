// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Utils
{
    using System.Security.Principal;

    /// <summary>
    /// The delegate of CheckBrokerByName.
    /// </summary>
    /// <param name="name">node name</param>
    /// <returns>is broker node or not</returns>
    internal delegate bool CheckBrokerByName(string name);

    /// <summary>
    /// Helper class to authenticate incoming user for both session launcher service
    /// and scheduler delegation service
    /// </summary>
    internal static class AuthenticationHelper
    {
        /// <summary>
        /// Check the caller if it is a broker node.
        /// </summary>
        /// <param name="identity">
        /// indicating caller's identity
        /// </param>
        /// <param name="checkBrokerByName">
        /// it is a delegate to check broker node by name
        /// </param>
        public static bool IsBrokerNode(WindowsIdentity identity, CheckBrokerByName checkBrokerByName)
        {
            if (identity != null && identity.IsAuthenticated)
            {
#if DEBUG
                using (WindowsIdentity current = WindowsIdentity.GetCurrent())
                {
                    // enable run under command line
                    if (identity.User == current.User)
                    {
                        return true;
                    }
                }
#endif
                // check if this is a valid broker node name or is a system
                if (identity.IsSystem)
                {
                    return true;
                }

                string nodeName = ExtractMachineName(identity.Name);

                if (!string.IsNullOrEmpty(nodeName) &&
                    checkBrokerByName != null &&
                    checkBrokerByName(nodeName))
                {
                    return true;
                }
            }

            return false;
        }

#if HPCPACK
        /// <summary>
        /// Check the caller if it is a node in the cluster.
        /// </summary>
        /// <param name="identity">indicating caller's identity</param>
        /// <param name="nodes">collection of cluster node</param>
        public static bool IsClusterNode(WindowsIdentity identity, ISchedulerCollection nodes)
        {
            if (identity.IsSystem)
            {
                return true;
            }

            string nodeName = ExtractMachineName(identity.Name);

            foreach (ISchedulerNode node in nodes)
            {
                if (string.Equals(nodeName, node.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        /// <summary>
        /// Extract machine name from machine account
        /// </summary>
        /// <param name="machineAccount">indicating the machine account</param>
        /// <returns>returns the machine name</returns>
        private static string ExtractMachineName(string machineAccount)
        {
            int index = machineAccount.IndexOf('\\');
            if (index == -1)
            {
                return string.Empty;
            }
            else if (machineAccount[machineAccount.Length - 1] == '$')
            {
                // Machine account should always ends with "$"
                return machineAccount.Substring(index + 1, machineAccount.Length - index - 2);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
