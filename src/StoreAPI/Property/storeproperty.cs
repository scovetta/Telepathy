using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Serialization;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines a store property.</para>
    /// </summary>
    [Serializable]
    [KnownType(typeof(List<NetworkInfo>))]
    [KnownType(typeof(GpuInfo[]))]
    public class StoreProperty
    {
        PropertyId _pid;
        object _val;

        string _name = null;

        /// <summary>
        ///   <para>Constructors for initializing an empty instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class.</para>
        /// </summary>
        public StoreProperty()
        {
            _pid = StorePropertyIds.NA;
        }

        /// <summary>
        ///   <para>Constructors for initializing a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class using the specified property name and value.</para>
        /// </summary>
        /// <param name="propName">
        ///   <para>The name of the property.</para>
        /// </param>
        /// <param name="propValue">
        ///   <para>The value of the property.</para>
        /// </param>
        public StoreProperty(string propName, object propValue)
        {
            _pid = StorePropertyIds.NA;
            _name = propName;
            _val = propValue;
        }

        /// <summary>
        ///   <para>Retrieves the name of the property.</para>
        /// </summary>
        /// <value>
        ///   <para>The name of the property.</para>
        /// </value>
        public string PropName
        {
            get
            {
                if (_name != null)
                    return _name;


                if (_pid != null)
                    return _pid.Name;

                return null;
            }
        }

        /// <summary>
        ///   <para>Constructors for initializing a new instance of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> class using the specified property identifier and value.</para>
        /// </summary>
        /// <param name="propId">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property.</para>
        /// </param>
        /// <param name="propValue">
        ///   <para>The value of the property.</para>
        /// </param>
        /// <remarks>
        ///   <para>To specify a job property, set<paramref name=" propId" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" /> (for example, 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.State" />).</para>
        ///   <para>To specify a task property, set <paramref name="propId" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        ///   <para>To specify a node property, set <paramref name="propId" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public StoreProperty(PropertyId propId, object propValue)
        {
            _pid = propId;

            // Note that null is a valid value.

            _val = propValue;
        }

        /// <summary>
        ///   <para>A string that represents the property.</para>
        /// </summary>
        /// <returns>
        ///   <para>A string that represents the property.</para>
        /// </returns>
        public override string ToString()
        {
            if (_val == null)
            {
                return _pid.ToString() + ": null";
            }

            if (_pid == JobPropertyIds.Password)
            {
                return _pid.ToString() + ": ********";
            }

            return _pid.ToString() + ": " + _val.ToString();
        }

        /// <summary>
        ///   <para>Retrieves or sets the value of the property.</para>
        /// </summary>
        /// <value>
        ///   <para>The value of the property.</para>
        /// </value>
        /// <remarks>
        ///   <para>The property can be null if the property has not been set. Check that the property object is valid before accessing its value.</para>
        /// </remarks>
        /// <example />
        public object Value
        {
            get { return _val; }
            set { _val = value; }
        }

        /// <summary>
        ///   <para>Retrieves or sets the property identifier.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property.</para>
        /// </value>
        /// <remarks>
        ///   <para>To specify a job property, set <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty.Id" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds" /> (for example, 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.State" />).</para>
        ///   <para>To specify a task property, set <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty.Id" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.TaskPropertyIds" />.</para>
        ///   <para>To specify a node property, set <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty.Id" /> to a property of 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.NodePropertyIds" />.</para>
        /// </remarks>
        /// <example />
        public PropertyId Id
        {
            get { return _pid; }
            set { _pid = value; }
        }
    }
}
