using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Represents a name/value pair.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, enumerate the items of an <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> collection.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidINameValue)]
    public interface INameValue
    {
        /// <summary>
        ///   <para>Retrieves the name of the name/value pair.</para>
        /// </summary>
        /// <value>
        ///   <para>The name.</para>
        /// </value>
        string Name { get; }
        /// <summary>
        ///   <para>Retrieves the value of the name/value pair.</para>
        /// </summary>
        /// <value>
        ///   <para>The value.</para>
        /// </value>
        string Value { get; }
    }

    /// <summary>
    ///   <para>Represents a name/value pair.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNameValueClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class NameValue : INameValue
    {
        string _name;
        string _value;

        internal NameValue(string name, string value)
        {
            _name = name;
            _value = value;
        }

        /// <summary>
        ///   <para>Retrieves the name of the name/value pair.</para>
        /// </summary>
        /// <value>
        ///   <para>The name.</para>
        /// </value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        ///   <para>Retrieves the value of the name/value pair.</para>
        /// </summary>
        /// <value>
        ///   <para>The value.</para>
        /// </value>
        public string Value
        {
            get { return _value; }
        }

        internal string SetName
        {
            get { return _name; }
            set { _name = value; }
        }

        internal string SetValue
        {
            get { return _value; }
            set { _value = value; }
        }
    }

    /// <summary>
    ///   <para>Represents a collection of name/value pairs.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To create this interface, call the <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateNameValueCollection" /> method. </para>
    ///   <para>The following properties return this collection:</para>
    ///   <list type="bullet">
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ICommandInfo.EnvironmentVariables" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.ClusterParameters" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.IScheduler.EnvironmentVariables" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.GetCustomProperties" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.EnvironmentVariables" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///     <item>
    ///       <description>
    ///         <para>
    ///           <see cref="Microsoft.Hpc.Scheduler.ISchedulerTask.GetCustomProperties" />
    ///         </para>
    ///       </description>
    ///     </item>
    ///   </list>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidINameValueCollection)]
    public interface INameValueCollection : ISchedulerCollection, ICollection<NameValue>
    {
        /// <summary>
        ///   <para>Adds a name/value pair to the collection.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value.</para>
        /// </param>
        void AddNameValue(string name, string value);

        /// <summary>
        ///   <para>Retrieves the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items.</para>
        /// </value>
        new int Count { get; }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the name/value pairs in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the name/value pairs in the collection.</para>
        /// </returns>
        /// <remarks>
        ///   <para>The collection contains an <see cref="Microsoft.Hpc.Scheduler.INameValue" /> interface for each name/value pair in the collection.</para>
        /// </remarks>
        /// <example />
        new IEnumerator GetEnumerator();
    }

    /// <summary>
    ///   <para>Represents a collection of name/value pairs.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead, use the <see cref="Microsoft.Hpc.Scheduler.INameValueCollection" /> interface.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [GuidAttribute(ComGuids.GuidNameValueCollectionClass)]
    [ClassInterface(ClassInterfaceType.None)]
    public class NameValueCollection : INameValueCollection
    {
        Dictionary<string, NameValue> table = new Dictionary<string, NameValue>();
        bool _readOnly = false;

        /// <summary>
        ///   <para>Initializes a new instance of this class.</para>
        /// </summary>
        public NameValueCollection()
        {
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the specified collection of name/value pairs.</para>
        /// </summary>
        /// <param name="values">
        ///   <para>A dictionary that contains a collection of name/value pairs.</para>
        /// </param>
        public NameValueCollection(Dictionary<string, string> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            foreach (KeyValuePair<string, string> item in values)
            {
                table[item.Key] = new NameValue(item.Key, item.Value);
            }
        }

        internal NameValueCollection(Dictionary<string, string> values, bool readOnly) :
            this(values)
        {
            _readOnly = readOnly;
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the name/value pairs in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the name/value pairs in the collection.</para>
        /// </returns>
        IEnumerator ISchedulerCollection.GetEnumerator()
        {
            return table.Values.GetEnumerator();
        }

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the name/value pairs in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the name/value pairs in the collection.</para>
        /// </returns>
        IEnumerator INameValueCollection.GetEnumerator()
        {
            return table.Values.GetEnumerator();
        }

        /// <summary>
        ///   <para>Retrieves the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items.</para>
        /// </value>
        public int Count
        {
            get { return table.Count; }
        }

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index to the item in the collection to retrieve.</para>
        /// </param>
        /// <returns>
        ///   <para>The  specified item from the collection.</para>
        /// </returns>
        public object this[int index]
        {
            get { throw new NotSupportedException(); }
        }

        /// <summary>
        ///   <para>Adds the specified name and value to the collection.</para>
        /// </summary>
        /// <param name="name">
        ///   <para>The name.</para>
        /// </param>
        /// <param name="value">
        ///   <para>The value.</para>
        /// </param>
        public void AddNameValue(string name, string value)
        {
            CheckIfReadOnly();
            Util.CheckArgumentNull(name, "name");
            Util.CheckArgumentNull(value, "value");

            table[name] = new NameValue(name, value);
        }

        /// <summary>
        ///   <para>Adds the specified object to the collection. </para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to add to the collection.</para>
        /// </param>
        void ISchedulerCollection.Add(object item)
        {
            NameValue nameValue = item as NameValue;
            if (nameValue == null)
            {
                throw new ArgumentException("item has to be a NameValue object", "item");
            }
            CheckIfReadOnly();

            ((NameValueCollection)this).Add(nameValue);
        }

        /// <summary>
        ///   <para>Removes the specified object from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The object to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the object is found in the collection and removed; otherwise, is false if the item is not found.</para>
        /// </returns>
        bool ISchedulerCollection.Remove(object item)
        {
            NameValue nameValue = item as NameValue;
            if (nameValue == null)
            {
                throw new ArgumentException("item has to be a NameValue object", "item");
            }
            CheckIfReadOnly();
            return ((NameValueCollection)this).Remove(nameValue);
        }

        /// <summary>
        ///   <para>Determines whether the specified object is in the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The object to find in the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the object is found in the collection; otherwise, false.</para>
        /// </returns>
        bool ISchedulerCollection.Contains(object item)
        {
            NameValue nameValue = item as NameValue;
            if (nameValue == null)
            {
                throw new ArgumentException("item has to be a NameValue object", "item");
            }

            return ((NameValueCollection)this).Contains(nameValue);
        }

        /// <summary>
        ///   <para>Removes all name/value pairs from the collection.</para>
        /// </summary>
        public void Clear()
        {
            CheckIfReadOnly();
            table.Clear();
        }

        private void CheckIfReadOnly()
        {
            if (_readOnly)
            {
                throw new InvalidOperationException("The collection is read-only");
            }
        }

        #region IEnumerable<NameValue> Members

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the name/value pairs in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.Generic.IEnumerator{T}" /> interface that you can use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return table.Values.GetEnumerator();
        }

        IEnumerator<NameValue> IEnumerable<NameValue>.GetEnumerator()
        {
            return table.Values.GetEnumerator();
        }

        #endregion

        #region ICollection<NameValue> Members

        /// <summary>
        ///   <para>Adds a name/value pair to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.NameValue" /> object that contains the name/value pair to add to the collection.</para>
        /// </param>
        public void Add(NameValue item)
        {
            CheckIfReadOnly();
            table.Add(item.Name, item);
        }

        /// <summary>
        ///   <para>Determines whether the collection contains the name/value pair.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The name/value pair to find.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the collection contains the name/value pair; otherwise, false.</para>
        /// </returns>
        public bool Contains(NameValue item)
        {
            NameValue savedItem;
            if (table.TryGetValue(item.Name, out savedItem))
            {
                return savedItem == item;
            }

            return false;
        }

        /// <summary>
        ///   <para>Copies items from the collection to a <see cref="Microsoft.Hpc.Scheduler.NameValue" /> array at the specified index.</para>
        /// </summary>
        /// <param name="array">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.NameValue" /> array to copy the contents of the collection to.</para>
        /// </param>
        /// <param name="arrayIndex">
        ///   <para>The zero-based index in <paramref name="array" /> at which copying begins.</para>
        /// </param>
        public void CopyTo(NameValue[] array, int arrayIndex)
        {
            table.Values.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///   <para>Determines whether the collection is read-only.</para>
        /// </summary>
        /// <value>
        ///   <para>Is true if the collection is read-only; otherwise, false.</para>
        /// </value>
        public bool IsReadOnly
        {
            get { return _readOnly; }
        }

        /// <summary>
        ///   <para>Removes the first occurrence of the specified item from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The name/value pair to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        public bool Remove(NameValue item)
        {
            CheckIfReadOnly();
            return table.Remove(item.Name);
        }

        #endregion

        #region ICollection Members

        /// <summary>
        ///   <para>Copies items from the collection to an  array at the specified index.</para>
        /// </summary>
        /// <param name="array">
        ///   <para>An array to copy the contents of the collection to.</para>
        /// </param>
        /// <param name="index">
        ///   <para>The zero-based index in <paramref name="array" /> at which copying begins.</para>
        /// </param>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)table.Values).CopyTo(array, index);
        }

        /// <summary>
        ///   <para>Determines whether access to the collection is synchronized.</para>
        /// </summary>
        /// <value>
        ///   <para>Is true if the access to the collection is synchronized; otherwise, false.</para>
        /// </value>
        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        ///   <para>Retrieves an object that can be used to synchronize access to the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The object used to synchronize access to the collection. </para>
        /// </value>
        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
