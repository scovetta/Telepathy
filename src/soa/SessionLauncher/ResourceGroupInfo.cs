// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Text;

    using Microsoft.Telepathy.Session.Common;

    /// <summary>
    /// Retrieves info about a failover cluster resource group
    /// </summary>
    internal class ResourceGroupInfo
    {
        private readonly string networkName;
        private readonly bool available;
        private readonly string hostName;

        /// <summary>
        /// Creates instance
        /// </summary>
        /// <param name="networkName"></param>
        /// <param name="available"></param>
        /// <param name="loginToken"></param>
        public ResourceGroupInfo(string networkName, bool available, string hostName)
        {
            this.networkName = networkName;
            this.available = available;
            this.hostName = hostName;
        }

        /// <summary>
        /// Returns NetworkName
        /// </summary>
        public string NetworkName
        {
            get
            {
                return this.networkName;
            }
        }

        /// <summary>
        /// Whether resource group is available
        /// </summary>
        public bool Available
        {
            get
            {
                return this.available;
            }
        }

        /// <summary>
        /// Resource group's current host name
        /// </summary>
        public string HostName
        {
            get
            {
                return this.hostName;
            }
        }

        /// <summary>
        /// Extracts information about a brokerlauncher resource group
        /// </summary>
        /// <param name="hCluster">Failover cluster connection</param>
        /// <param name="resourceGroupName">Name of the response group</param>
        /// <returns>Resource group availability and network name</returns>
        static public ResourceGroupInfo Get(IntPtr hCluster, string resourceGroupName)
        {
            IntPtr hGroup = IntPtr.Zero;
            IntPtr hResourceGroupEnum = IntPtr.Zero;
            IntPtr hResource = IntPtr.Zero;
            ResourceGroupInfo resourceGroupInfo = null;
            int resourceGroupNetworkNameLen = Win32API.MAX_HOST_NAME_LEN;
            StringBuilder resourceGroupNetworkName = new StringBuilder(resourceGroupNetworkNameLen);
            bool resourceGroupAvailable = false;
            int nameLen = Win32API.MAX_HOST_NAME_LEN;
            StringBuilder name = new StringBuilder(nameLen);
            int hostNameLen = Win32API.MAX_HOST_NAME_LEN;
            StringBuilder hostName = new StringBuilder(hostNameLen);

            try
            {
                // Open the resource group
                hGroup = Win32API.OpenClusterGroup(hCluster, resourceGroupName);
                if (hGroup == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), string.Format(SR.ResourceGroupInfo_CannotOpenResourceGroup, name));

                // Get group's state and save whether its available
                CLUSTER_GROUP_STATE groupState = Win32API.GetClusterGroupState(hGroup, hostName, ref hostNameLen);
                resourceGroupAvailable = IsGroupAvailable(groupState);

                // If the resource group isnt available, move on to the next one
                if (!resourceGroupAvailable)
                    return null;

                // Enumerate resources within the resource group
                hResourceGroupEnum = Win32API.ClusterGroupOpenEnum(hGroup, (int)CLUSTER_GROUP_ENUM.CLUSTER_GROUP_ENUM_ALL);
                if (hResourceGroupEnum == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error(), SR.ResourceGroupInfo_CannotEnumerateResourceGroup);

                int index = 0;
                int type = 0;
                bool exit = false;
                int enumResult = (int)CLUSTER_ENUM_RESULT.ERROR_SUCCESS;
                IntPtr loginToken = IntPtr.Zero;

                nameLen = Win32API.MAX_HOST_NAME_LEN;

                // Enumerate through the resources to get network name
                while (!exit)
                {
                    name = new StringBuilder(nameLen);

                    // If previous iteration opened a resource, close it before opening another
                    if (hResource != IntPtr.Zero)
                    {
                        Win32API.CloseClusterResource(hResource);
                        hResource = IntPtr.Zero;
                    }

                    enumResult = Win32API.ClusterGroupEnum(hResourceGroupEnum, index, out type, name, ref nameLen);
                    if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_SUCCESS)
                    {
                        // Debug.Assert(type == (int)CLUSTER_GROUP_ENUM.CLUSTER_GROUP_ENUM_CONTAINS);

                        if (type == (int) CLUSTER_GROUP_ENUM.CLUSTER_GROUP_ENUM_CONTAINS)
                        {
                            hResource = Win32API.OpenClusterResource(hCluster, name.ToString());

                            if (hResourceGroupEnum == IntPtr.Zero)
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error(),
                                    String.Format(SR.ResourceGroupInfo_CannotOpenResource, name));
                            }

                            // Check if the resource type is a Generic Application. If not continue to the next
                            if (!Win32API.ResUtilResourceTypesEqual("Generic Application", hResource))
                            {
                                nameLen = Win32API.MAX_HOST_NAME_LEN;
                                index++;
                                continue;
                            }

                            // TODO: Consider checking generic application's command line to see if its brokerlauncher

                            if (
                                !Win32API.GetClusterResourceNetworkName(hResource, resourceGroupNetworkName,
                                    ref resourceGroupNetworkNameLen))
                            {
                                int errorCode = Marshal.GetLastWin32Error();

                                if (errorCode != (int) WIN32_ERRORS.ERROR_DEPENDENCY_NOT_FOUND)
                                {
                                    throw new Win32Exception(errorCode,
                                        string.Format(SR.ResourceGroupInfo_CannotGetNetworkName, resourceGroupName));
                                }
                            }

                            exit = true;
                        }
                        else
                        {
                            nameLen = Win32API.MAX_HOST_NAME_LEN;
                            index++;
                            continue;
                        }

                    }
                    else if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_NO_MORE_ITEMS)
                    {
                        exit = true;
                    }
                    else if (enumResult == (int)CLUSTER_ENUM_RESULT.ERROR_MORE_DATA)
                    {
                        // try same item again with returned nameLen
                        continue;
                    }
                    else
                    {
                        throw new Exception(String.Format(SR.UnexpectedReturnValue, enumResult));
                    }
                }

                if (resourceGroupNetworkName.Length != 0)
                {
                    resourceGroupInfo = new ResourceGroupInfo(resourceGroupNetworkName.ToString(), resourceGroupAvailable, hostName.ToString());
                }

                return resourceGroupInfo;
            }

            finally
            {
                if (hResource != IntPtr.Zero)
                    Win32API.CloseClusterResource(hResource);

                if (hResourceGroupEnum != IntPtr.Zero)
                    Win32API.ClusterResourceCloseEnum(hResourceGroupEnum);

                if (hGroup != IntPtr.Zero)
                    Win32API.CloseClusterGroup(hGroup);
            }
        }

        /// <summary>
        /// Returns whether specified resource group is available to broker clients
        /// </summary>
        /// <param name="resourceGroupState">The state of the resource group</param>
        /// <returns>Whether the resource group is available</returns>
        static bool IsGroupAvailable(CLUSTER_GROUP_STATE resourceGroupState)
        {
            return (resourceGroupState == CLUSTER_GROUP_STATE.CLUSTER_GROUP_ONLINE);
        }
    }
}
