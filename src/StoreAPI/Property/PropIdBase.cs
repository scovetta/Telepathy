using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the types of errors that can occur when retrieving properties for a rowset.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code>const NotFound = 0
    /// const PermissionDenied = 1</code>
    /// </remarks>
    /// <example />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.Error" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds.Error" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds.Error" />
    public enum PropertyError
    {
        /// <summary>
        ///   <para>The property was not found. This enumeration member represents a value of 0.</para>
        /// </summary>
        NotFound = 0,
        /// <summary>
        ///   <para>The user does not have permission to access the property. For 
        /// example, trying to access the password property. This enumeration member represents a value of 1.</para>
        /// </summary>
        PermissionDenied = 1
    }

    /// <summary>
    ///   <para>Defines the attributes of the property.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const None = 0
    /// const ReadOnly = 1
    /// const Calculated = 2
    /// const Visible = 4
    /// const Indexed = 8
    /// const Custom = 16
    /// const Volatile = 32
    /// const Obsolete = 64
    /// </code>
    /// </remarks>
    /// <example />
    [Flags]
    public enum PropFlags
    {
        /// <summary>
        ///   <para>No flags are set. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0x0000,
        /// <summary>
        ///   <para>The property is read-only. This enumeration member represents a value of 1.</para>
        /// </summary>
        ReadOnly = 0x0001,
        /// <summary>
        ///   <para>The value of the property is calculated (for example, the number of 
        /// nodes on which a job is running). This enumeration member represents a value of 2.</para>
        /// </summary>
        Calculated = 0x0002,
        /// <summary>
        ///   <para>The property can be displayed in a user interface. This enumeration member represents a value of 4.</para>
        /// </summary>
        Visible = 0x0004,
        /// <summary>
        ///   <para>The property is indexed for faster retrieval. This enumeration member represents a value of 8.</para>
        /// </summary>
        Indexed = 0x0008,
        /// <summary>
        ///   <para>The property is a custom property. This enumeration member represents a value of 16.</para>
        /// </summary>
        Custom = 0x0010,
        /// <summary>
        ///   <para>The property is not persisted in the database. This enumeration member represents a value of 32.</para>
        /// </summary>
        Volatile = 0x0020,
        /// <summary>
        ///   <para>The property is obsolete and should not be used. This enumeration member represents a 
        /// value of 64. This member was introduced in Windows HPC Server 2008 R2 and is not supported in earlier versions.</para>
        /// </summary>
        Obsolete = 0x0040,
    }

    /// <summary>
    ///   <para>Defines a property.</para>
    /// </summary>
    /// <remarks>
    ///   <para>You need to know the context in which the property was returned so you know if the 
    /// property is for a job, task, node or resource. For example, the job and task objects both contain a Runtime property.</para>
    /// </remarks>
    /// <example />
    [Serializable]
    public class PropertyId
    {
        // Properties...

        /// <summary>
        ///   <para>Retrieves the data type of the property.</para>
        /// </summary>
        /// <value>
        ///   <para>The data type of the property. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType" /> fields.</para>
        /// </value>
        public StorePropertyType Type
        {
            get { return _PropType; }
        }

        /// <summary>
        ///   <para>Retrieves one or more flags that define the attributes of the property.</para>
        /// </summary>
        /// <value>
        ///   <para>One or more flags that define the attributes of the property. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropFlags" /> enumeration.</para>
        /// </value>
        public PropFlags Flags
        {
            get { return _PropFlags; }
        }

        /// <summary>
        ///   <para>Retrieves the name of the property.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the property.</para>
        /// </value>
        public string Name
        {
            get { return _PropName; }
        }

        /// <summary>
        ///   <para>Retrieves the identifier that uniquely identifies the property within the system.</para>
        /// </summary>
        /// <returns />
        public Int32 UniqueId
        {
            get { return _PropUniqueID; }
        }


        private StorePropertyType _PropType = StorePropertyType.None;
        private String _PropName = null;
        private Int32 _PropUniqueID = -1;
        private PropFlags _PropFlags = PropFlags.Visible;

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> class using the property type, name, and index.</para>
        /// </summary>
        /// <param name="propType">
        ///   <para>The property type. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType" /> enumeration.</para>
        /// </param>
        /// <param name="propName">
        ///   <para>The name of the property.</para>
        /// </param>
        /// <param name="propIndex">
        ///   <para />
        /// </param>
        public PropertyId(StorePropertyType propType, String propName, Int32 propIndex)
        {
            _PropType = propType;
            _PropName = propName;

            _PropUniqueID = propIndex;

            _PropFlags = PropFlags.Visible;
        }

        /// <summary>
        ///   <para>Initializes a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> class using the property type, name, index, and flags.</para>
        /// </summary>
        /// <param name="propType">
        ///   <para>The property type. For possible values, see the <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType" /> enumeration.</para>
        /// </param>
        /// <param name="propName">
        ///   <para>The name of the property.</para>
        /// </param>
        /// <param name="propIndex">
        ///   <para />
        /// </param>
        /// <param name="flags">
        ///   <para>One or more flags that define the attributes of the property. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropFlags" /> enumeration.</para>
        /// </param>
        public PropertyId(StorePropertyType propType, String propName, Int32 propIndex, PropFlags flags)
        {
            _PropType = propType;
            _PropName = propName;

            _PropUniqueID = propIndex;

            _PropFlags = flags;
        }

        /// <summary>
        ///   <para>Retrieves a string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            return _PropName;
        }

        /// <summary>
        ///   <para>Retrieves a unique hash code for the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A unique hash code for the object.</para>
        /// </returns>
        public override int GetHashCode()
        {
            return _PropUniqueID;
        }

        /// <summary>
        ///   <para>Determines whether the two property objects are equal.</para>
        /// </summary>
        /// <param name="obj">
        ///   <para>The property object to compare to this property object.</para>
        /// </param>
        /// <returns>
        ///   <para>Is True if the objects are equal; otherwise, False.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The objects are equal if their <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId.UniqueId" /> property values are the same.</para>
        /// </remarks>
        /// <example />
        public override bool Equals(object obj)
        {
            if (obj is PropertyId)
            {
                if (((PropertyId)obj)._PropUniqueID == this._PropUniqueID)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///   <para>Determines whether the two property objects are equal.</para>
        /// </summary>
        /// <param name="a">
        ///   <para>The property to be compared to the property in <paramref name="b" />.</para>
        /// </param>
        /// <param name="b">
        ///   <para>The property to be compared to the property in <paramref name="a" />.</para>
        /// </param>
        /// <returns>
        ///   <para>Is True if the objects are equal; otherwise, False.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The objects are equal if their <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId.UniqueId" /> property values are the same.</para>
        /// </remarks>
        /// <example />
        public static bool operator ==(PropertyId a, PropertyId b)
        {
            if ((object)a == null && (object)b == null)
            {
                return true;
            }

            if ((object)a == null || (object)b == null)
            {
                return false;
            }

            return (a._PropUniqueID == b._PropUniqueID);
        }

        /// <summary>
        ///   <para>Determines whether the two property objects are not equal.</para>
        /// </summary>
        /// <param name="a">
        ///   <para>The property to be compared to the property in <paramref name="b" />.</para>
        /// </param>
        /// <param name="b">
        ///   <para>The property to be compared to the property in <paramref name="a" />.</para>
        /// </param>
        /// <returns>
        ///   <para>Is True if the objects are not equal; otherwise, False.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The objects are not equal if their 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId.UniqueId" /> property values are different.</para>
        /// </remarks>
        /// <example />
        public static bool operator !=(PropertyId a, PropertyId b)
        {
            return !(a == b);
        }
    }
}
