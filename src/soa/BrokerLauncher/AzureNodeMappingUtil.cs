// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Azure.Common
{
    /// <summary>
    /// This is the list of modules in the Azure deployment.
    /// </summary>
    internal enum Module : int
    {
        // Start with 0, increment by one
        // This will be converted to an integer which represents the index of the port in the eprstring

        FullString = 0, // Will return the IP address of that node within Azure
        Ip = 1,
        Job = 2,  // Will return the endpoint address used by the scheduler
        FileStaging = 3, // Will return the endpoint address of the file staging

        // Insert other modules here and change the indexes below
        // ...
        
        ApplicationI = 4,  // Will return the endpoint address of the first set of Application endpoint, or it can be interpreted as SOAData endpoints
        ApplicationII = 12, // Here the gap between the ApplicationI and ApplicationII (or between SOAData and SOAControl) equals to the NumApplicationPorts setting in the AzureNaming.cs, which is currently 8
    }

    /// <summary>
    /// This is the interface for proxy modules to access the logical node name -> physial endpoint mapping.
    /// The user has to manually control the cache refreshing.
    /// This is not a static class, so every object has its own mapping cache.
    /// Only the scheduler should call the method UpdateNodeMapping. Other modules should only read.
    /// </summary>
    internal partial class NodeMapping
    {

        /// <summary>
        /// The protocol for each module is hard-coded
        /// </summary>
        internal static string[] ProtocolPrefix = new string[] { 
            "",             // Full string
            "",         // Ip
            "tcp://",   // Job
            "net.tcp://",   // File staging
            "net.tcp://",   // SOA
        };

        /// <summary>
        /// This is a static function to help user parsing out the endpoint address for specific module from the endpoint string
        /// This overload is for module other than SOA
        /// </summary>
        /// <param name="endpointString"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static string GetModuleAddress(string endpointString, Module module)
        {
            return GetModuleAddress(endpointString, module, 0);
        }

        /// <summary>
        /// This is a static function to help user parsing out the endpoint address for specific module from the endpoint string
        /// This overload is for Application ports or SOA ports
        /// </summary>
        /// <param name="endpointString"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static string GetModuleAddress(string endpointString, Module module, int coreId)
        {
            if (string.IsNullOrEmpty(endpointString))
            {
                return string.Empty;
            }

            if (module == Module.FullString)
            {
                return endpointString;
            }

            string[] items = endpointString.Split(':');

            if (module == Module.Ip)
            {
                return items[0];
            }

            string prefix;

            if ((int)module >= ProtocolPrefix.Length)
            {
                prefix = ProtocolPrefix[(int)Module.ApplicationI]; // All of them are app or soa ports
            }
            else
            {
                prefix = ProtocolPrefix[(int)module];
            }

            if ((int)module + coreId >= items.Length)
            {
                throw new ArgumentException("Invalid module for this Azure node.");
            }

            return prefix + items[0] + ":" + items[(int)module + coreId];
        }
    }

}
