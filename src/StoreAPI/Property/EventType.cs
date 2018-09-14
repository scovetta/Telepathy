using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the reason for the event notification.</para>
    /// </summary>
    [Serializable]
    public enum EventType
    {
        /// <summary>
        ///   <para>A new row was added to the rowset.</para>
        /// </summary>
        Create = 1,
        /// <summary>
        ///   <para>A row in the rowset was modified.</para>
        /// </summary>
        Modify = 2,
        /// <summary>
        ///   <para>A row was deleted from the rowset.</para>
        /// </summary>
        Delete = 3,
    }
}
