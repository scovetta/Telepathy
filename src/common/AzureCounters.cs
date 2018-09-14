//-------------------------------------------------------------------------------------------------
// <copyright file="AzureCounters.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//     Defines the fixed set of counters for Azure
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc.Common.Azure
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    /// <summary>
    /// This struct defines an Azure counter.
    /// </summary>
    internal struct AzureCounter
    {
        public AzureCounterEnum index;
        public string category;
        public string counter;
        public string instance;

        public AzureCounter(AzureCounterEnum index, string category, string counter, string instance)
        {
            this.index = index;
            this.category = category;
            this.counter = counter;
            this.instance = instance;
        }
    }

    /// <summary>
    /// Enums for Azure counters.
    /// The ordering is IMPORTANT.
    /// </summary>
    internal enum AzureCounterEnum : int
    {
        AZURE_COUNTER_PROCESSOR_CPU = 0,
        AZURE_COUNTER_MEMORY_PAGES = 1,
        AZURE_COUNTER_SYSTEM_CONTEXT_SWITCHES = 2,
        AZURE_COUNTER_SYSTEM_CALLS = 3,
        AZURE_COUNTER_PHYSICALDISK_BYTES = 4,
        AZURE_COUNTER_LOGICALDISK_QUEUE = 5,
        AZURE_COUNTER_MEMORY_MBYTES = 6,
        AZURE_COUNTER_NODEMANAGER_CORES = 7,
        AZURE_COUNTER_NODEMANAGER_JOBS = 8,
        AZURE_COUNTER_NODEMANAGER_TASKS = 9,
        AZURE_COUNTER_MAX = 10,
        AZURE_COUNTER_UNKNOWN = -1
    }

    /// <summary>
    /// Helper class for Azure counters
    /// </summary>
    internal static class AzureCounterHelper
    {
        /// <summary>
        /// Version for Azure counter set
        /// </summary>
        public static int AzureCounterVersion = 1;

        /// <summary>
        /// List of Azure counters
        /// </summary>
        static List<AzureCounter> azureCounters;

        /// <summary>
        /// List of performance counters
        /// </summary>
        static List<PerformanceCounter> perfCounters;

        /// <summary>
        /// Query a set of Azure performance counter values
        /// </summary>
        public static double[] QueryCounters(bool worker = true)
        {
            if (AzureCounterHelper.perfCounters == null)
            {
                AzureCounterHelper.perfCounters = new List<PerformanceCounter>();

                //
                //  IMPORTANT - make sure ordering matches enums above
                //
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Processor", "% Processor Time", "_Total"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Memory", "Pages/sec"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("System", "Context switches/sec"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("System", "System Calls/sec"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("PhysicalDisk", "Disk Bytes/sec", "_Total"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("LogicalDisk", "Avg. Disk Queue Length", "_Total"));
                AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Memory", "Available MBytes"));

                if (worker)
                {
                    AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Node Manager", "Number of Cores in use"));
                    AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Node Manager", "Number of Running Jobs"));
                    AzureCounterHelper.perfCounters.Add(new PerformanceCounter("Node Manager", "Number of Running Tasks"));
                }
            }

            double[] counterValues = new double[AzureCounterHelper.perfCounters.Count];

            //
            //  Loop through and get counter values
            //
            for (int i = 0; i < counterValues.Length; i++)
            {
                try
                {
                    counterValues[i] = AzureCounterHelper.perfCounters[i].NextValue();
                }
                catch (Exception /* e */)
                {
                    //
                    //  Catch any missing counters (eg - if custom counters did not get installed)
                    //  Don't log error in this path - will flood the logs
                    //
                    //Trace.TraceError("Error querying counter at index {0}: {1}", i, e.Message);
                }
            }

            return counterValues;
        }

        /// <summary>
        /// Match a given counter instance to an Azure counter enum
        /// </summary>
        public static AzureCounterEnum MatchCounter(string category, string counter, string instance)
        {
            if (AzureCounterHelper.azureCounters == null)
            {
                AzureCounterHelper.azureCounters = new List<AzureCounter>();

                //
                //  IMPORTANT - make sure ordering matches enums above
                //
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_PROCESSOR_CPU, "Processor", "% Processor Time", "_Total"));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_MEMORY_PAGES, "Memory", "Pages/sec", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_SYSTEM_CONTEXT_SWITCHES, "System", "Context switches/sec", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_SYSTEM_CALLS, "System", "System Calls/sec", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_PHYSICALDISK_BYTES, "PhysicalDisk", "Disk Bytes/sec", "_Total"));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_LOGICALDISK_QUEUE, "LogicalDisk", "Avg. Disk Queue Length", "_Total"));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_MEMORY_MBYTES, "Memory", "Available MBytes", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_NODEMANAGER_CORES, "Node Manager", "Number of Cores in use", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_NODEMANAGER_JOBS, "Node Manager", "Number of Running Jobs", null));
                AzureCounterHelper.azureCounters.Add(new AzureCounter(AzureCounterEnum.AZURE_COUNTER_NODEMANAGER_TASKS, "Node Manager", "Number of Running Tasks", null));
            }

            foreach (AzureCounter item in AzureCounterHelper.azureCounters)
            {
                // The passed in instance parameter always has a value of "", which won't equal to either 
                // null or "_Total", we can't change InstanceFilter for counters such as CPU Usage to "_Total"
                // because this metric is also used by other nodes such as CN/BN/HN/WN etc which will only
                // discover their instances at run time. We might bring back instance comparision when we
                // refactor Azure counters in the future.
                if ((item.category == category) &&
                    (item.counter == counter) //&&
                    //(item.instance == instance)
                   )
                {
                    return item.index;
                }
            }

            return AzureCounterEnum.AZURE_COUNTER_UNKNOWN;
        }


        /// <summary>
        /// Reports the counters.
        /// </summary>
        /// <param name="account">The account.</param>
        /// <param name="perfCounterTableName">Name of the perf counter table.</param>
        /// <param name="pk">The pk.</param>
        /// <param name="rk">The rk.</param>
        /// <param name="counterValues">The counter values.</param>
        public static void ReportCounters(CloudStorageAccount account, string perfCounterTableName, string pk, string rk, double[] counterValues)
        {
            CloudTableClient tableClient = account.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(perfCounterTableName);
            TableOperation retrieveOperation = TableOperation.Retrieve<CountersEntity>(pk, rk);
            var foundCounter = (CountersEntity)(table.Execute(retrieveOperation).Result);
            if (foundCounter == null)
            {
                foundCounter = new CountersEntity
                {
                    PartitionKey = pk,
                    RowKey = rk
                };
            }
            
            UpdateCounterValue(foundCounter, counterValues);

            TableOperation insertOrReplaceOperation = TableOperation.InsertOrReplace(foundCounter);
            table.Execute(insertOrReplaceOperation);
        }

        /// <summary>
        /// Updates the counter value.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="counterValues">The counter values.</param>
        private static void UpdateCounterValue(CountersEntity row, double[] counterValues)
        {
            for (int i = 0; i < counterValues.Length; i++)
            {
                PropertyInfo propertyInfo = row.GetType().GetProperty(
                    string.Format(
                    CultureInfo.InvariantCulture,
                    CountersEntity.CounterValuePropertyNamingFormat,
                    i));

                propertyInfo.SetValue(row, counterValues[i], null);
            }
        }
    }
}
