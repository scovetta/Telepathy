//------------------------------------------------------------------------------
// <copyright file="SessionUnitType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       enum for the resource unit type
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    ///   <para>Defines values to indicate the type of hardware resources used 
    /// to determine to nodes on which the service job for the session can run.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The least specific resource unit is the node and the most granular specific unit is the core. For example, a 
    /// node can have four cores on one socket and the node can contain multiple sockets. A job can specify that it needs a  
    /// minimum of four nodes to run, regardless of how many cores are on each node. The job could also specify that it needs 
    /// four cores to run, so it could run on one node that had four cores or on multiple nodes with one or two cores.</para> 
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const Core = 0
    /// const Socket = 1
    /// const Node = 2</code>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionResourceUnitType" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.UnitType" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobUnitType" />
    public enum SessionUnitType
    {
        /// <summary>
        ///   <para>Uses cores to schedule the service job. This enumeration member represents a value of 0.</para>
        /// </summary>
        Core = 0,
        /// <summary>
        ///   <para>Uses sockets to scheduler the service job. This enumeration member represents a value of 1.</para>
        /// </summary>
        Socket = 1,
        /// <summary>
        ///   <para>Uses nodes to schedule the service job. This enumeration member represents a value of 2.</para>
        /// </summary>
        Node = 2,
        /// <summary>
        ///   <para />
        /// </summary>
        Gpu = 3,
    }
}
