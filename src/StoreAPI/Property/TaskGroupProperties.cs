using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the identifiers that uniquely identify the properties of a task group.</para>
    /// </summary>
    public class TaskGroupPropertyIds
    {
        /// <summary>
        ///   <para>An identifier that uniquely identifies the task group in the store.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Id
        {
            get { return StorePropertyIds.Id; }
        }

        /// <summary>
        ///   <para>The name of the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Name
        {
            get { return StorePropertyIds.Name; }
        }

        /// <summary>
        ///   <para>The last time that the server changed one of the property values of the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId ChangeTime
        {
            get { return StorePropertyIds.ChangeTime; }
        }

        /// <summary>
        ///   <para>The level (depth) of the task group node in the graph.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Level
        {
            get { return _Level; }
        }

        /// <summary>
        ///   <para>All tasks in the group have run (finished, failed, or was canceled).</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Complete
        {
            get { return _Complete; }
        }

        /// <summary>
        ///   <para>Indicates whether this group is the default task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId Default
        {
            get { return _Default; }
        }

        /// <summary>
        ///   <para>An identifier that uniquely identifies the job that contains the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId JobId
        {
            get { return _JobId; }
        }

        /// <summary>
        ///   <para>The maximum value of all the minimum core values specified for the tasks in the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId MaxMinCores
        {
            get { return _MaxMinCores; }
        }

        /// <summary>
        ///   <para>The sum of the maximum core values specified in all the tasks of the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId SumMaxCores
        {
            get { return _SumMaxCores; }
        }

        /// <summary>
        ///   <para>The maximum value of all the minimum socket values specified for the tasks in the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId MaxMinSockets
        {
            get { return _MaxMinSockets; }
        }

        /// <summary>
        ///   <para>The sum of the maximum socket values specified in all the tasks of the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId SumMaxSockets
        {
            get { return _SumMaxSockets; }
        }

        /// <summary>
        ///   <para>The maximum value of all the minimum node values specified for the tasks in the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId MaxMinNodes
        {
            get { return _MaxMinNodes; }
        }

        /// <summary>
        ///   <para>The sum of the maximum node values specified in all the tasks of the task group.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId SumMaxNodes
        {
            get { return _SumMaxNodes; }
        }

        /// <summary>
        ///   <para>The number of failed tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId FailedTaskCount
        {
            get { return StorePropertyIds.FailedTaskCount; }
        }

        /// <summary>
        ///   <para>The number of canceled tasks.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies this property.</para>
        /// </value>
        static public PropertyId CanceledTaskCount
        {
            get { return StorePropertyIds.CanceledTaskCount; }
        }

        static PropertyId _Level = new PropertyId(StorePropertyType.Int32, "Level", PropertyIdConstants.TaskGroupPropertyIdStart + 1);
        static PropertyId _Complete = new PropertyId(StorePropertyType.Boolean, "Complete", PropertyIdConstants.TaskGroupPropertyIdStart + 2);
        static PropertyId _Default = new PropertyId(StorePropertyType.Boolean, "Default", PropertyIdConstants.TaskGroupPropertyIdStart + 3);
        static PropertyId _JobId = new PropertyId(StorePropertyType.Int32, "JobId", PropertyIdConstants.TaskGroupPropertyIdStart + 4);
        static PropertyId _MaxMinCores = new PropertyId(StorePropertyType.Int32, "MaxMinCores", PropertyIdConstants.TaskGroupPropertyIdStart + 5, PropFlags.Calculated);
        static PropertyId _SumMaxCores = new PropertyId(StorePropertyType.Int32, "SumMaxCores", PropertyIdConstants.TaskGroupPropertyIdStart + 6, PropFlags.Calculated);
        static PropertyId _MaxMinSockets = new PropertyId(StorePropertyType.Int32, "MaxMinSockets", PropertyIdConstants.TaskGroupPropertyIdStart + 7, PropFlags.Calculated);
        static PropertyId _SumMaxSockets = new PropertyId(StorePropertyType.Int32, "SumMaxSockets", PropertyIdConstants.TaskGroupPropertyIdStart + 8, PropFlags.Calculated);
        static PropertyId _MaxMinNodes = new PropertyId(StorePropertyType.Int32, "MaxMinNodes", PropertyIdConstants.TaskGroupPropertyIdStart + 9, PropFlags.Calculated);
        static PropertyId _SumMaxNodes = new PropertyId(StorePropertyType.Int32, "SumMaxNodes", PropertyIdConstants.TaskGroupPropertyIdStart + 10, PropFlags.Calculated);
    }
}
