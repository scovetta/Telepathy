using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the aggregate operation to perform on a column (property) in a rowset.</para>
    /// </summary>
    [Serializable]
    public enum AggregateOperation
    {
        /// <summary>
        ///   <para>Do not perform aggregation on this column (property).</para>
        /// </summary>
        None,
        /// <summary>
        ///   <para>For integer properties, sum the values for all objects (for example, sum the run-time for all tasks).</para>
        /// </summary>
        Sum,
        /// <summary>
        ///   <para>For integer properties, find the object with the minimum value (for example, the task with the shortest run-time).</para>
        /// </summary>
        Min,
        /// <summary>
        ///   <para>For integer properties, find the object with the maximum value (for example, the task with the longest run-time).</para>
        /// </summary>
        Max,
        /// <summary>
        ///   <para>Count all occurrences of a property.</para>
        /// </summary>
        Count,
        /// <summary>
        ///   <para>Count all distinct property values (for example, count the number of distinct names of users that submitted jobs).</para>
        /// </summary>
        CountDistinct,
        /// <summary>
        ///   <para>Get the distinct property values (for example, get the distinct names of users that submitted jobs).</para>
        /// </summary>
        Distinct,
    }

    /// <summary>
    ///   <para>Defines the property and the type of aggregation to perform on that property.</para>
    /// </summary>
    [Serializable]
    public class AggregateColumn
    {
        PropertyId _pid;
        AggregateOperation _op;

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.AggregateColumn" /> class.</para>
        /// </summary>
        /// <param name="pid">
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object. For example, to perform aggregation on the state property of a job, set this parameter to  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.State" />.</para>
        /// </param>
        /// <param name="op">
        ///   <para>The type of aggregation to perform. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.AggregateOperation" /> enumeration.</para>
        /// </param>
        public AggregateColumn(PropertyId pid, AggregateOperation op)
        {
            _pid = pid;
            _op = op;
        }

        /// <summary>
        ///   <para>Indentifies the property on which to perform aggregation.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property. Access the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId.Name" /> property to determine the name of the property.</para>
        /// </value>
        public PropertyId PropId
        {
            get { return _pid; }
        }

        /// <summary>
        ///   <para>The aggregate operation to perform.</para>
        /// </summary>
        /// <value>
        ///   <para>The aggregation to perform. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.AggregateOperation" /> enumeration.</para>
        /// </value>
        public AggregateOperation Operation
        {
            get { return _op; }
        }
    }

}
