//-------------------------------------------------------------------------------------------------
// <copyright file="GpuPerfCounterManagement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//    GPU performance counter collect and update
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceProcess;
    using System.Text;
    using System.Timers;
    using Microsoft.Hpc.Common;

    internal class GpuPerfCounterManagement
    {
        private Timer counterTimer;

        private int timerInterval = 1000;

        private bool perfCounterCreated = false;

        private bool nvmlInit = false;

        private IList<string> gpuInfos = new List<string>();

        private DateTimeOffset lastRefreshGpuInfoTime = DateTimeOffset.MinValue;

        private IHpcContext context;

        private bool runInMonitoringService;

        public GpuPerfCounterManagement(IHpcContext context, bool runInMonitoringService = false)
        {
            System.Diagnostics.Trace.TraceInformation("[GpuPerfCounterManagement] initialize GpuPerfCounterManagement");
            this.context = context;
            this.RefreshLocalInterval();
            this.counterTimer = new Timer(this.timerInterval);
            this.counterTimer.Elapsed += new ElapsedEventHandler(this.UpdateCounter);
            this.counterTimer.AutoReset = false;
            this.runInMonitoringService = runInMonitoringService;
        }

        public void Start()
        {
            System.Diagnostics.Trace.TraceInformation("[GpuPerfCounterManagement] start");
            this.Init();
            this.EnableTimer(this.counterTimer);
        }

        public void Stop()
        {
            NvmlHelper.Shutdown();
            this.nvmlInit = false;
            this.counterTimer?.Dispose();
            this.counterTimer = null;
        }

        private void EnableTimer(Timer t, double? interval = null)
        {
            if (t != null)
            {
                try
                {
                    if (interval.HasValue) t.Interval = interval.Value;
                    t.Enabled = true;
                }
                catch (ObjectDisposedException)
                {
                    System.Diagnostics.Trace.TraceWarning("[GpuPerfCounterManagement] ObjectDisposedException throw during enable timer");
                }
            }
        }

        private void RefreshLocalInterval()
        {
            int interval =
                context.Registry.GetValueAsync<int>(HpcConstants.HpcFullKeyName, HpcConstants.CollectionIntervalRegVal,
                    context.CancellationToken, 1).GetAwaiter().GetResult();
            int value = interval * 1000;
            if (value >= 1000 && value <= 30 * 1000)
            {
                this.timerInterval = value;
            }
            else
            {
                System.Diagnostics.Trace.TraceInformation("[GpuPerfCounterManagement] Invalid collection interval {0} second(s) from registry, value must be between 1 and 30 seconds, use current interval {1} second", value / 1000, this.timerInterval);
            }
        }

        private void RefreshGpuInfo()
        {
            if (DateTimeOffset.UtcNow - this.lastRefreshGpuInfoTime < TimeSpan.FromMinutes(5)) return;
            this.lastRefreshGpuInfoTime = DateTimeOffset.UtcNow;
            this.RefreshLocalInterval();
            this.gpuInfos.Clear();
            foreach (var gpu in this.EnumDevices())
            {
                this.gpuInfos.Add(gpu);
            }

            System.Diagnostics.Trace.TraceInformation("[GpuPerfCounterManagement] node has {0} GPUs", this.gpuInfos.Count);
        }

        private void UpdateCounter(object o, ElapsedEventArgs e)
        {
            try
            {
                this.RefreshGpuInfo();
                if (this.gpuInfos.Count > 0)
                {
                    this.AddGpuPerfCounter();
                    this.CollectGpuPerfCounterValue();
                }

                this.RemoveNotValidGpuPerfInstance();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("[GpuPerfCounterManagement] Exception occurs during update counter: {0}", ex);
            }
            finally
            {
                this.EnableTimer(this.counterTimer, this.timerInterval);
            }
        }

        private void AddGpuPerfCounter()
        {
            if (!this.perfCounterCreated)
            {
                if (!PerformanceCounterCategory.Exists(GpuPerfCounterConsts.CategoryName))
                {
                    System.Diagnostics.Trace.TraceInformation(
                        "[GpuPerfCounterManagement] Creat performance counters for category {0}",
                        GpuPerfCounterConsts.CategoryName);
                    PerformanceCounterCategory.Create(GpuPerfCounterConsts.CategoryName,
                        GpuPerfCounterConsts.CategoryHelp, PerformanceCounterCategoryType.MultiInstance,
                        GpuPerfCounterConsts.CounterCollection);

                    // need restart HpcMonitoring service to take effect
                    if (this.runInMonitoringService)
                    {
                        KillHpcMonitoringService();
                    }
                    else
                    {
                        RestartMonitoringClientService();
                    }
                }
                else
                {
                    bool newAdded =
                        GpuPerfCounterConsts.CounterCollection.Cast<CounterCreationData>()
                            .Any(
                                c =>
                                    !PerformanceCounterCategory.CounterExists(c.CounterName,
                                        GpuPerfCounterConsts.CategoryName));
                    if (newAdded)
                    {
                        System.Diagnostics.Trace.TraceInformation(
                            "[GpuPerfCounterManagement] Performance counters under category {0} are changed, will remove the category first then re-create with new counter definitions",
                            GpuPerfCounterConsts.CategoryName);
                        PerformanceCounterCategory.Delete(GpuPerfCounterConsts.CategoryName);
                        System.Diagnostics.Trace.TraceInformation(
                            "[GpuPerfCounterManagement] Creat performance counters for category {0}",
                            GpuPerfCounterConsts.CategoryName);
                        PerformanceCounterCategory.Create(GpuPerfCounterConsts.CategoryName,
                            GpuPerfCounterConsts.CategoryHelp, PerformanceCounterCategoryType.MultiInstance,
                            GpuPerfCounterConsts.CounterCollection);
                    }
                }

                this.perfCounterCreated = true;
            }
        }

        private void CollectGpuPerfCounterValue()
        {
            if (this.gpuInfos.Count == 0) return;
            NvmlHelper.nvmlReturn err;
            if (!this.nvmlInit)
            {
                this.Init();
            }

            IntPtr device;
            NvmlHelper.nvmlUtilization utilization;
            NvmlHelper.nvmlMemory memory;
            uint index = 0;
            foreach (var gpu in this.gpuInfos)
            {
                err = this.nvmlDeviceGetHandleByIndex(index, out device);
                if (err == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                {
                    PerformanceCounter counter;
                    string instance = $"{gpu}({index})";
                    if (NvmlHelper.nvmlDeviceGetUtilizationRates(device, out utilization) == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                    {
                        counter = new PerformanceCounter(GpuPerfCounterConsts.CategoryName, GpuPerfCounterConsts.GpuTime, instance, false);
                        counter.RawValue = utilization.gpu;
                        counter = new PerformanceCounter(GpuPerfCounterConsts.CategoryName, GpuPerfCounterConsts.GpuMemoryUsage, instance, false);
                        counter.RawValue = utilization.memory;
                    }

                    if (NvmlHelper.nvmlDeviceGetMemoryInfo(device, out memory) == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                    {
                        counter = new PerformanceCounter(GpuPerfCounterConsts.CategoryName, GpuPerfCounterConsts.GpuMemoryUsed, instance, false);
                        counter.RawValue = (long)(memory.used / (1024 * 1024));
                    }

                    UpdateGpuPerfCounter(GpuPerfCounterConsts.GpuFanSpeed, device, instance);
                    UpdateGpuPerfCounter(GpuPerfCounterConsts.GpuPower, device, instance);
                    UpdateGpuPerfCounter(GpuPerfCounterConsts.GpuTemperature, device, instance);
                    UpdateGpuPerfCounter(GpuPerfCounterConsts.GpuSMClock, device, instance);
                }

                index++;
            }
        }

        private NvmlHelper.nvmlReturn Init()
        {
            NvmlHelper.nvmlReturn err = NvmlHelper.Init();
            if (err == NvmlHelper.nvmlReturn.NVML_SUCCESS)
            {
                System.Diagnostics.Trace.TraceInformation("[GpuPerfCounterManagement] successful to run NvmlHelper.Init: {0}", err);
                this.nvmlInit = true;
            }

            return err;
        }

        public IEnumerable<string> EnumDevices()
        {
            NvmlHelper.nvmlReturn err = NvmlHelper.nvmlReturn.NVML_SUCCESS;
            if (!this.nvmlInit)
            {
                err = this.Init();
            }

            if (err != NvmlHelper.nvmlReturn.NVML_SUCCESS) yield break;

            int cdev;
            err = NvmlHelper.nvmlDeviceGetCount(out cdev);
            if (err != NvmlHelper.nvmlReturn.NVML_SUCCESS)
            {
                yield break;
            }

            IntPtr device;
            for (int i = 0; i < cdev; i++)
            {
                err = NvmlHelper.nvmlDeviceGetHandleByIndex((uint)i, out device);
                if (err == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                {
                    var name = new StringBuilder(NvmlHelper.MaxNameLen);
                    if (NvmlHelper.nvmlDeviceGetName(device, name, (uint)name.Capacity) == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                    {
                        yield return name.ToString();
                    }
                }
            }
        }

        private NvmlHelper.nvmlReturn nvmlDeviceGetHandleByIndex(uint index, out IntPtr device)
        {
            NvmlHelper.nvmlReturn err;
            err = NvmlHelper.nvmlDeviceGetHandleByIndex(index, out device);
            if (err == NvmlHelper.nvmlReturn.NVML_ERROR_UNINITIALIZED)
            {
                err = this.Init();
                if (err == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                {
                    err = NvmlHelper.nvmlDeviceGetHandleByIndex(index, out device);
                }
            }

            return err;
        }

        private void UpdateGpuPerfCounter(string counterName, IntPtr device, string instance)
        {
            uint val = 0;
            NvmlHelper.nvmlReturn rtn = NvmlHelper.nvmlReturn.NVML_ERROR_UNKNOWN;
            PerformanceCounter counter = new PerformanceCounter(GpuPerfCounterConsts.CategoryName, counterName, instance, false);
            switch (counterName)
            {
                case GpuPerfCounterConsts.GpuFanSpeed:
                    rtn = NvmlHelper.nvmlDeviceGetFanSpeed(device, out val);
                    break;
                case GpuPerfCounterConsts.GpuPower:
                    rtn = NvmlHelper.nvmlDeviceGetPowerUsage(device, out val);
                    if (rtn == NvmlHelper.nvmlReturn.NVML_SUCCESS)
                    {
                        val = val / 1000;
                    }
                    break;
                case GpuPerfCounterConsts.GpuTemperature:
                    rtn = NvmlHelper.nvmlDeviceGetTemperature(device, NvmlHelper.nvmlTemperatureSensors.NVML_TEMPERATURE_GPU, out val);
                    break;
                case GpuPerfCounterConsts.GpuSMClock:
                    rtn = NvmlHelper.nvmlDeviceGetClockInfo(device, NvmlHelper.nvmlClockType.NVML_CLOCK_SM, out val);
                    break;
            }

            if (rtn == NvmlHelper.nvmlReturn.NVML_SUCCESS)
            {
                counter.RawValue = val;
            }
            else
            {
                counter.RawValue = 0;
            }
        }

        private void RemoveNotValidGpuPerfInstance()
        {
            foreach (PerformanceCounterCategory category in PerformanceCounterCategory.GetCategories())
            {
                if (category.CategoryName.Equals(GpuPerfCounterConsts.CategoryName))
                {
                    List<string> validInstanceNames = new List<string>();
                    uint index = 0;
                    foreach (var gpu in this.gpuInfos)
                    {
                        validInstanceNames.Add($"{gpu.ToLowerInvariant()}({index})");
                        index++;
                    }

                    PerformanceCounter counter = null;
                    foreach (string instanceName in category.GetInstanceNames().Where(inst => !validInstanceNames.Contains(inst.ToLowerInvariant())))
                    {
                        System.Diagnostics.Trace.TraceWarning("[GpuPerfCounterManagement] GPU Instance {0} does not exist, will be removed", instanceName);
                        if (counter == null)
                        {
                            //give counter name as any valid counter name, as we only want to remove instance
                            counter = new PerformanceCounter(GpuPerfCounterConsts.CategoryName, GpuPerfCounterConsts.GpuFanSpeed, instanceName, false);
                        }
                        else
                        {
                            counter.InstanceName = instanceName;
                        }

                        counter.RemoveInstance(); // it will remove instance for all GPU counters
                    }

                    break;
                }
            }
        }

        private static void RestartMonitoringClientService()
        {
            try
            {
                ServiceController sc = new ServiceController(HpcConstants.HpcMonitoringClient);
                try
                {
                    if (sc.Status != ServiceControllerStatus.Stopped)
                    {
                        System.Diagnostics.Trace.TraceError("[GpuPerfCounterManagement] Stopping {0} service", HpcConstants.HpcMonitoringClient);
                        sc.Stop();
                        sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(2));
                    }

                    System.Diagnostics.Trace.TraceError("[GpuPerfCounterManagement] Starting {0} service", HpcConstants.HpcMonitoringClient);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(2));
                }
                finally
                {
                    sc.Dispose();
                }
            }
            catch (Exception e) when (e is Win32Exception || e is InvalidOperationException || e is System.TimeoutException)
            {
                System.Diagnostics.Trace.TraceError("[GpuPerfCounterManagement] Failed to restart HpcMonitoringClient service: {0}", e);
            }
        }

        private static void KillHpcMonitoringService()
        {
            System.Diagnostics.Trace.TraceWarning("[GpuPerfCounterManagement] Make HpcMonitoring service crash to restart, so the new added GPU perfcounter category can take effect");
            Environment.Exit(0);
        }
    }
}
