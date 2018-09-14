using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines a rowset object that you can use to retrieve the properties of one or more objects in the rowset.</para>
    /// </summary>
    [Serializable]
    public class PropertyRowSet
    {
        PropertyId[] _pids = null;
        PropertyRow[] _rows = null;

        /// <summary>
        ///   <para>Initializes a new instance of this class.</para>
        /// </summary>
        /// <param name="pids">
        ///   <para>An array of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> objects that identify the properties that the rowset will contain.</para>
        /// </param>
        /// <param name="rows">
        ///   <para>An array of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRow" /> objects that contain the rows of objects that the rowset will contain.</para>
        /// </param>
        public PropertyRowSet(PropertyId[] pids, PropertyRow[] rows)
        {
            _pids = pids;
            _rows = rows;
        }

        /// <summary>
        ///   <para>Retrieves the rows from the rowset.</para>
        /// </summary>
        /// <value>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRow" /> objects that contain the rows in the rowset.</para>
        /// </value>
        public PropertyRow[] Rows
        {
            get { return _rows; }
            set { _rows = value; }
        }

        /// <summary>
        ///   <para>Retrieves the specified row from the rowset.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>A zero-based index to the row in the rowset that you want to retrieve.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRow" /> object that contains the specified row from the rowset.</para>
        /// </returns>
        public PropertyRow this[int index]
        {
            get
            {
                if (_rows == null)
                {
                    return null;
                }

                return _rows[index];
            }
        }

        /// <summary>
        ///   <para>Retrieves the number of rows in the rowset.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of rows in the rowset.</para>
        /// </value>
        public int Length
        {
            get
            {
                if (_rows == null)
                {
                    return 0;
                }

                return _rows.Length;
            }
        }
    }
}
