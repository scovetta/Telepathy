//-------------------------------------------------------------------------------------------------
// <copyright file="GpuPerfCounterConsts.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//    The constants of GPU related performance counter
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Common
{
    using System.Diagnostics;

    public class GpuPerfCounterConsts
    {
        public const string CategoryName = "GPU";

        public const string CategoryHelp = "GPU performance counters";

        public const string GpuFanSpeedAlias = "HPCGpuFanSpeed";

        public const string GpuFanSpeed = "GPU Fan Speed (%)";

        public const string GpuFanSpeedHelp = "The intended operating speed of the device's fan, as a percent. Not applicable for passively-cooled GPUs";

        public const string GpuTimeAlias = "HPCGpuTime";

        public const string GpuTime = "GPU Time (%)";

        public const string GpuTimeHelp = "Percent of time over the past sample period during which one or more kernels was executing on the GPU";

        public const string ClusterGpuTimeAlias = "HPCClusterGpuTime";

        public const string ClusterGpuTime = "Cluster GPU Time (%)";

        public const string ClusterGpuTimeHelp = "Percent of time over the past sample period during which one or more kernels was executing on the GPU of HPC cluster";

        public const string GpuMemoryUsageAlias = "HPCGpuMemoryUsage";

        public const string GpuMemoryUsage = "GPU Memory Usage (%)";

        public const string GpuMemoryUsageHelp = "Percent of used GPU memory";

        public const string GpuMemoryUsedAlias = "HPCGpuMemoryUsed";

        public const string GpuMemoryUsed = "GPU Memory Used (MB)";

        public const string GpuMemoryUsedHelp = "Allocated memory (in MBs). Note that the driver/GPU always sets aside a small amount of memory for bookkeeping";

        public const string GpuPowerAlias = "HPCGpuPowerUsage";

        public const string GpuPower = "GPU Power Usage (Watts)";

        public const string GpuPowerHelp = "Power usage for this GPU in Watts and its associated circuitry (e.g. memory)";

        public const string GpuSMClockAlias = "HPCGpuSMClock";

        public const string GpuSMClock = "GPU SM Clock (MHz)";

        public const string GpuSMClockHelp = "The current SM clock speed for the device, in MHz";

        public const string GpuTemperatureAlias = "HPCGpuTemperature";

        public const string GpuTemperature = "GPU Temperature (degrees C)";

        public const string GpuTemperatureHelp = "The current temperature readings for the device, in degrees C";

        public static CounterCreationDataCollection CounterCollection = 
            new CounterCreationDataCollection
            {
                    new CounterCreationData(GpuFanSpeed, GpuFanSpeedHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuTime, GpuTimeHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuMemoryUsage, GpuMemoryUsageHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuMemoryUsed, GpuMemoryUsedHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuPower, GpuPowerHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuSMClock, GpuSMClockHelp, PerformanceCounterType.NumberOfItems32),

                    new CounterCreationData(GpuTemperature, GpuTemperatureHelp, PerformanceCounterType.NumberOfItems32),
           };
    }
}
