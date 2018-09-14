namespace Microsoft.Hpc.Scheduler.Properties
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    [DataContract]
    public class GpuInfo
    {
        /// <summary>
        ///   <para>
        /// nvmlDeviceGetName
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        ///   <para>
        /// nvmlDeviceGetUUID
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string Uuid { get; set; }

        /// <summary>
        ///   <para>
        /// nvmlDeviceGetPciInfo 
        /// string.Format("{0:X2}:{1:X2}", pci.bus, pci.device)
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string PciBusDevice { get; set; }

        /// <summary>
        ///   <para>
        /// nvmlDeviceGetPciInfo, Pci.busId
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string PciBusId { get; set; }

        /// <summary>
        ///   <para>
        /// nvmlDeviceGetMemoryInfo, Unit is MB
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long TotalMemory { get; set; }

        /// <summary>
        ///   <para>
        /// nvmlDeviceGetMaxClockInfo
        /// </para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long MaxSMClock { get; set; }
    }
}
