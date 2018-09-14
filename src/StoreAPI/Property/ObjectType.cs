using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the internal object types.</para>
    /// </summary>
    [Serializable]
    [Flags]
    public enum ObjectType
    {
        /// <summary>
        ///   <para>Reserved.</para>
        /// </summary>
        None = 0x000,
        /// <summary>
        ///   <para>An internal job object.</para>
        /// </summary>
        Job = 0x001,
        /// <summary>
        ///   <para>An internal task object.</para>
        /// </summary>
        Task = 0x002,
        /// <summary>
        ///   <para>An internal resource object.</para>
        /// </summary>
        Resource = 0x004,
        /// <summary>
        ///   <para>An internal job template object.</para>
        /// </summary>
        JobTemplate = 0x008,
        /// <summary>
        ///   <para>An internal node object.</para>
        /// </summary>
        Node = 0x010,
        /// <summary>
        ///   <para>An internal store object.</para>
        /// </summary>
        Store = 0x020,
        /// <summary>
        ///   <para>An internal allocation object.</para>
        /// </summary>
        Allocation = 0x040,
        /// <summary>
        ///   <para>An internal task group object.</para>
        /// </summary>
        TaskGroup = 0x080,
        /// <summary>
        ///   <para>An internal node history object.</para>
        /// </summary>
        NodeHistory = 0x100,
        /// <summary>
        ///   <para>An internal job history object.</para>
        /// </summary>
        JobHistory = 0x200,
        /// <summary>
        ///   <para>An internal object that represents an email message that notifies a user 
        /// that a job started to run or completed. This value is supported only for Windows HPC Server 2008 R2.</para>
        /// </summary>
        JobMessage = 0x400,
        /// <summary>
        ///   <para>An internal pool object.</para>
        /// </summary>
        Pool = 0x800,
    }
}
