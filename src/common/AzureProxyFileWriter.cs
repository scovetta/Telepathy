//------------------------------------------------------------------------------
// <copyright file="AzureProxyFileWriter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     An instance of this class will run a timer that checks instances of proxy role
//     and keeps instance addresses in a file.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// An instance of this class will run a timer that checks instances of proxy role
    /// and keeps instance addresses in a file.
    /// Only caller holding admin previlege will be able to instantiate this class.
    /// </summary>
    internal class AzureProxyFileWriter : IDisposable
    {
        /// <summary>
        /// Interval between 2 successive updates of Azure proxy file: 5 minutes.
        /// </summary>
        private static TimeSpan proxyFileUpdateInterval = TimeSpan.FromSeconds(300);

        /// <summary>
        /// Azure proxy file path
        /// </summary>
        private string filePath;

        /// <summary>
        /// Addresses of all proxy role instances in current deployment
        /// </summary>        
        private List<string> proxyInstances = new List<string>();

        /// <summary>
        /// Timer to refresh or update Azure proxy file
        /// </summary>
        private Timer timer;

        /// <summary>
        /// Initializes a new instance of the AzureProxyFileWriter class
        /// </summary>
        public AzureProxyFileWriter()
        {
            this.filePath = AzureNaming.AzureProxyFileName;
            this.timer = new Timer(this.Update, null, TimeSpan.Zero, proxyFileUpdateInterval);
        }

        /// <summary>
        /// Implement IDisposable interface
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            this.timer.Dispose();
            this.timer = null;
        }

        /// <summary>
        /// Read proxy role instances information from RoleEnvironment and update it into Azure proxy file
        /// </summary>
        /// <param name="state">callback state</param>
        private void Update(object state)
        {
            try
            {                
                SortedSet<string> instances = new SortedSet<string>();
                if (RoleEnvironment.Roles.ContainsKey(RoleNames.Proxy))
                {
                    foreach (RoleInstance roleInstance in RoleEnvironment.Roles[RoleNames.Proxy].Instances)
                    {
                        // DataProxy lives in every proxy role, so use DataProxyEndpoint here
                        instances.Add(roleInstance.InstanceEndpoints[DataProxyEndpointNames.DataProxyEndpoint].IPEndpoint.Address.ToString());
                    }
                }

                // check if proxy instances are changed since last update
                bool changedFlag = false;
                if(this.proxyInstances.Count != instances.Count)
                {
                    changedFlag = true;
                }
                else
                {
                    foreach(string proxyInstance in this.proxyInstances)
                    {
                        if(!instances.Contains(proxyInstance))
                        {
                            changedFlag = true;
                            break;
                        }
                    }
                }

                // proxy instances changed, update file
                if(changedFlag)
                {
                    using (StreamWriter file = File.CreateText(this.filePath))
                    {
                        foreach (string instance in instances)
                        {
                            file.WriteLine(instance);
                        }

                        file.WriteLine(AzureProxyFileReader.EndOfFileFlag);
                    }
                }
            }
            catch (Exception)
            {
                // swallow exceptions
            }
        }
    }
}
