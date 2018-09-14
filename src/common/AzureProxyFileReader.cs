//------------------------------------------------------------------------------
// <copyright file="AzureProxyFileReader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     A helper class that facilitates reading addresses of proxy role instances
//     from the Azure proxy file.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    /// <summary>
    ///  A helper class that facilitates reading addresses of proxy role instances
    ///  from the Azure proxy file.
    /// </summary>
    internal class AzureProxyFileReader
    {
        /// <summary>
        /// End of file flag
        /// </summary>
        public const string EndOfFileFlag = "#end";

        /// <summary>
        /// Wait interval between retries: 200 ms
        /// </summary>
        private const int RetryInterval = 200;

        /// <summary>
        /// Azure proxy file path
        /// </summary>
        private string filePath;

        /// <summary>
        /// Addresses of proxy role instances in current deployment
        /// </summary>
        private List<string> proxyInstances;

        /// <summary>
        /// Lock object for proxyInstances
        /// </summary>
        private object lockProxyInstances = new object();

        /// <summary>
        /// Initializes a new instance of the AzureProxyFileReader class
        /// </summary>
        public AzureProxyFileReader()
        {
            this.filePath = AzureNaming.AzureProxyFileName;
        }

        /// <summary>
        /// Gets a list of addresses of all proxy role instances in current deployment
        /// </summary>
        /// <param name="refresh">a flag indicating if refresh from file</param>
        /// <returns>list of proxy role instance addresses</returns>
        public List<string> GetProxyInstances(bool refresh)
        {
            if (this.proxyInstances == null || this.proxyInstances.Count == 0 || refresh)
            {
                lock (this.lockProxyInstances)
                {
                    List<string> instances = new List<string>();
                    
                    int maxRetryCount = 3;
                    Exception exception = null;
                    while (maxRetryCount-- > 0)
                    {
                        bool hasEndOfFileFlag = false;

                        // read from file
                        try
                        {
                            using (StreamReader file = File.OpenText(this.filePath))
                            {
                                string line;
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (!string.Equals(line, EndOfFileFlag, StringComparison.OrdinalIgnoreCase))
                                    {
                                        instances.Add(line);
                                    }
                                    else
                                    {
                                        // last line found
                                        hasEndOfFileFlag = true;
                                        break;
                                    }
                                }
                            }

                            exception = null;
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }

                        if (hasEndOfFileFlag)
                        {
                            break;
                        }

                        // the Azure proxy file is being updated. wait for a while and retry
                        Thread.Sleep(RetryInterval);
                    }

                    if (exception != null)
                    {
                        throw exception;
                    }

                    this.proxyInstances = instances;
                }
            }

            return this.proxyInstances;
        }
    }
}
