//--------------------------------------------------------------------------
// <copyright file="AzureVmConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common module for Azure Vm Configuration information
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using Microsoft.Win32;

    internal class AzureVmConfiguration
    {
        public static Dictionary<int, KeyValuePair<int, int>> NumCoresToCpuMemoryMapping = new Dictionary<int, KeyValuePair<int, int>>();

        static AzureVmConfiguration()
        {
            NumCoresToCpuMemoryMapping[1] = new KeyValuePair<int, int>(2100, 1750);
            NumCoresToCpuMemoryMapping[2] = new KeyValuePair<int, int>(2100, 3500);
            NumCoresToCpuMemoryMapping[4] = new KeyValuePair<int, int>(2100, 7000);
            NumCoresToCpuMemoryMapping[8] = new KeyValuePair<int, int>(2100, 14000);
        }

        public static void GetCpuSpeedAndMemoryFromCoreNum(int numCores, out int cpuSpeed, out int memory)
        {
            KeyValuePair<int, int> config;
            cpuSpeed = memory = 0;
            if (AzureVmConfiguration.NumCoresToCpuMemoryMapping.TryGetValue(numCores, out config))
            {
                cpuSpeed = config.Key;
                memory = config.Value;
            }
            else
            {
                throw new InvalidOperationException("The core number is not supported.");
            }
        }
    }
}
