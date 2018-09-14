using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Defines the properties that are in a row of a rowset.</para>
    /// </summary>
    [Serializable]
    public class PropertyRow : IEnumerable<StoreProperty>
    {
        private StoreProperty[] _props;

        /// <summary>
        ///   <para>Initializes a new instance of this class with the specified number of empty columns.</para>
        /// </summary>
        /// <param name="columnCount">
        ///   <para>The number of columns to include in the row.</para>
        /// </param>
        public PropertyRow(int columnCount)
        {
            _props = new StoreProperty[columnCount];
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified properties.</para>
        /// </summary>
        /// <param name="props">
        ///   <para>The properties to include in the row.</para>
        /// </param>
        public PropertyRow(StoreProperty[] props)
        {
            _props = props;
        }

        /// <summary>
        ///   <para>Maps properties to index values.</para>
        /// </summary>
        /// <param name="map">
        ///   <para>A dictionary of property identifiers mapped to their respective index values.</para>
        /// </param>
        public void SetPropToIndexMap(Dictionary<PropertyId, int> map)
        {
            _id2index = map;
        }

        /// <summary>
        ///   <para>Retrieves the list of properties in the row.</para>
        /// </summary>
        /// <value>
        ///   <para>An array of <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> objects that contain the properties in the row.</para>
        /// </value>
        public StoreProperty[] Props
        {
            get
            {
                return _props;
            }
        }

        /// <summary>
        ///   <para>Retrieve a property from the list of properties in the row using an index as the indexer.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>A zero-based index of the property to retrieve.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> object that contains the property.</para>
        /// </returns>
        public StoreProperty this[int index]
        {
            get
            {
                return _props[index];
            }

            set
            {
                // null is valid
                _props[index] = value;
            }
        }

        Dictionary<PropertyId, int> _id2index = null;

        /// <summary>
        ///   <para>Retrieve a property from the list of properties in the row using a property identifier as the indexer.</para>
        /// </summary>
        /// <param name="propId">
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" /> object that identifies the property. For example, to specify the job state property, set the indexer to  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobPropertyIds.State" />.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> object that contains the property.</para>
        /// </returns>
        public StoreProperty this[PropertyId propId]
        {
            get
            {
                // See if we need to build a dictionary.

                // CONSIDER: It may be cool to build a 
                //           dictionary that could be 
                //           shared if we get more than 
                //           one row at a time.

                if (_id2index == null || _id2index.Count == 0)
                {
                    _id2index = new Dictionary<PropertyId, int>(_props.GetLength(0));

                    for (int i = 0; i < _props.GetLength(0); i++)
                    {
                        if (_props[i] != null && !_id2index.ContainsKey(_props[i].Id))
                        {
                            _id2index.Add(_props[i].Id, i);
                        }
                    }
                }

                // Use the dictionary to see if we
                // have the property that the caller
                // wants.  If not return null.

                // CONSIDER: Should we throw instead?

                int index = -1;

                if (_id2index.TryGetValue(propId, out index))
                {
                    if (_props[index] == null || _props[index].Id == StorePropertyIds.Error)
                    {
                        return null;
                    }

                    return _props[index];
                }

                return null;
            }
        }

        /// <summary>
        ///   <para>Retrieves a string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            StringBuilder bldr = new StringBuilder(255);

            foreach (StoreProperty prop in _props)
            {
                bldr.Append(prop.ToString());
                bldr.Append(" ");
            }

            return bldr.ToString();
        }


        /// <summary>
        ///   <para>Retrieves an enumerator that you use to enumerate the properties in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you can use to enumerate the properties in the collection.</para>
        /// </returns>
        public IEnumerator<StoreProperty> GetEnumerator()
        {
            return new PropertyRowEnumerator(this);
        }

        /// <summary>
        ///   <para>Implements the generic <see cref="System.Collections.IEnumerator" />.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" />.</para>
        /// </returns>
        /// <remarks>
        ///   <para>To enumerate the properties in the row, use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRow.GetEnumerator" /> method. </para>
        /// </remarks>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new PropertyRowEnumerator(this);
        }
    }

    /// <summary>
    ///   <para>Supports a simple iteration over a collection of the properties in a row of a rowset.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Implements the 
    /// <see cref="System.Collections.IEnumerator" /> interface. For complete details, see the Remarks section for each element of the interface.</para>
    ///   <para>You must dispose of this object when done.</para>
    /// </remarks>
    /// <example />
    public class PropertyRowEnumerator : IEnumerator<StoreProperty>
    {
        PropertyRow _row = null;
        int _index = -1;

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRowEnumerator" /> class.</para>
        /// </summary>
        /// <param name="row">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyRow" /> object that contains the properties in the row.</para>
        /// </param>
        public PropertyRowEnumerator(PropertyRow row)
        {
            _row = row;
        }

        /// <summary>
        ///   <para>Gets the current element in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.StoreProperty" /> object that contains the property.</para>
        /// </value>
        public StoreProperty Current
        {
            get { return _row.Props[_index]; }
        }

        /// <summary>
        ///   <para>Releases the resources used by the object.</para>
        /// </summary>
        public void Dispose()
        {
            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        object IEnumerator.Current
        {
            get { return _row.Props[_index]; }
        }

        /// <summary>
        ///   <para>Advances the enumerator to the next element of the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>Is True if the enumerator was successfully advanced to the 
        /// next element; otherwise, False if the enumerator has passed the end of the collection.</para>
        /// </returns>
        public bool MoveNext()
        {
            ++_index;

            if (_index < _row.Props.GetLength(0))
            {
                return true;
            }

            _index = 0;

            return false;
        }

        /// <summary>
        ///   <para>Sets the enumerator to its initial position, which is before the first element in the collection.</para>
        /// </summary>
        public void Reset()
        {
            _index = -1;
        }

    }
}
