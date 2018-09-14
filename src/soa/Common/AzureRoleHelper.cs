//-----------------------------------------------------------------------
// <copyright file="AzureRoleHelper.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Utilities in Azure role environment</summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Hpc.Azure.Common;
    using Microsoft.WindowsAzure.ServiceRuntime;

    internal static class AzureRoleHelper
    {
        /// <summary>
        /// Get the IP address of local machine.
        /// Notice: Don't use method Dns.GetHostAddresses, which returns multi ip addresses.
        /// We can't differentiate which one is correct for the endpoint.
        /// </summary>
        /// <returns>broker node address</returns>
        internal static string GetLocalMachineAddress()
        {
            return GetRoleInstanceAddress(RoleEnvironment.CurrentRoleInstance);
        }

        /// <summary>
        /// Get the IP addresses of all the broker roles in current Azure deployment.
        /// </summary>
        /// <returns>all the broker node addresses</returns>
        internal static List<string> GetAllBrokerAddress()
        {
            List<string> brokerNodes = new List<string>();

            string brokerRoleConfigName = SchedulerConfigNames.BrokerRoles;

            string rolenames = RoleEnvironment.GetConfigurationSettingValue(brokerRoleConfigName);

            if (!string.IsNullOrEmpty(rolenames))
            {
                string[] roles = rolenames.ToLowerInvariant().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> list = new List<string>(roles);

                foreach (Role role in RoleEnvironment.Roles.Values)
                {
                    if (list.Contains(role.Name.ToLowerInvariant()))
                    {
                        foreach (RoleInstance instance in role.Instances)
                        {
                            string address = GetRoleInstanceAddress(instance);

                            if (!string.IsNullOrEmpty(address))
                            {
                                brokerNodes.Add(address);
                            }
                        }
                    }
                }
            }

            return brokerNodes;
        }

        /// <summary>
        /// Get IP address of specified role instance.
        /// </summary>
        /// <param name="instance">role instance</param>
        /// <returns>
        /// Return IP address if AllPorts endpoint is declared. Otherwise, return empty (not expected).
        /// </returns>
        private static string GetRoleInstanceAddress(RoleInstance instance)
        {
            RoleInstanceEndpoint endpoint;

            if (instance.InstanceEndpoints.TryGetValue(SchedulerEndpointNames.AllPorts, out endpoint))
            {
                return endpoint.IPEndpoint.Address.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
