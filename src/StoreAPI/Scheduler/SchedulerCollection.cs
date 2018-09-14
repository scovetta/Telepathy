using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace Microsoft.Hpc.Scheduler
{
    /// <summary>
    ///   <para>Defines a generic collection of objects that the scheduler returns.</para>
    /// </summary>
    /// <typeparam name="T">
    ///   <para>The type of objects in the collection.</para>
    /// </typeparam>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.ISchedulerCollection" /> interface.</para>
    /// </remarks>
    public class SchedulerCollection<T> : ICollection<T>, ISchedulerCollection
    {
        List<T> list;
        bool readOnly = false;

        /// <summary>
        ///   <para>Initializes a new, empty instance of this class.</para>
        /// </summary>
        public SchedulerCollection() : this(false)
        {
        }

        /// <summary>
        ///   <para>Initializes a new instance of this class using the contents of an existing collection to populate the collection.</para>
        /// </summary>
        /// <param name="list">
        ///   <para>An existing collection used to populate this collection.</para>
        /// </param>
        public SchedulerCollection(IEnumerable<T> list) : this(list, false)
        {
        }

        internal SchedulerCollection(bool readOnly)
        {
            this.list = new List<T>();
            this.readOnly = readOnly;
        }

        internal SchedulerCollection(IEnumerable<T> list, bool readOnly)
        {
            this.list = new List<T>(list);
            this.readOnly = readOnly;
        }

        #region ISchedulerCollection Members

        /// <summary>
        ///   <para>Retrieves a count of the number of items in the collection.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items in the collection.</para>
        /// </value>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator ISchedulerCollection.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        object ISchedulerCollection.this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        ///   <para>Retrieves the specified item from the collection.</para>
        /// </summary>
        /// <param name="index">
        ///   <para>The zero-based index of the item to retrieve from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public T this[int index]
        {
            get { return list[index]; }
        }

        /// <summary>
        ///   <para>Adds an item to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to add to the collection. The type for all items in the collection must be the same.</para>
        /// </param>
        void ISchedulerCollection.Add(object item)
        {
            CheckIfReadOnly();
            Util.CheckArgumentNull(item, "item");
            if (item is T)
            {
                Add((T)item);
            }
            else
            {
                throw new ArgumentException("item has to be type of " + typeof(T).FullName, "item");
            }
        }

        /// <summary>
        ///   <para>Removes the first occurrence of the specified item from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        bool ISchedulerCollection.Remove(object item)
        {
            CheckIfReadOnly();
            Util.CheckArgumentNull(item, "item");
            if (item is T)
            {
                return Remove((T)item);
            }
            else
            {
                throw new ArgumentException("item has to be type of " + typeof(T).FullName, "item");
            }
        }

        /// <summary>
        ///   <para>Determines whether the collection contains the specified item.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to find.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found in the collection; otherwise, false.</para>
        /// </returns>
        bool ISchedulerCollection.Contains(object item)
        {
            Util.CheckArgumentNull(item, "item");
            if (item is T)
            {
                return Contains((T)item);
            }
            else
            {
                throw new ArgumentException("item has to be type of " + typeof(T).FullName, "item");
            }
        }

        void CheckIfReadOnly()
        {
            if (this.readOnly)
            {
                throw new InvalidOperationException("The collection is read-only");
            }
        }

        /// <summary>
        ///   <para>Removes all items from the collection.</para>
        /// </summary>
        public void Clear()
        {
            CheckIfReadOnly();
            list.Clear();
        }
        #endregion

        /// <summary>
        ///   <para>Adds an item to the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to add to the collection. The type for all items in the collection must be the same.</para>
        /// </param>
        public void Add(T item)
        {
            CheckIfReadOnly();
            Util.CheckArgumentNull(item, "item");
            list.Add(item);
        }

        /// <summary>
        ///   <para>Removes the first occurrence of the specified item from the collection.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to remove from the collection.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found and removed from the collection; otherwise, false.</para>
        /// </returns>
        public bool Remove(T item)
        {
            CheckIfReadOnly();
            Util.CheckArgumentNull(item, "item");
            return list.Remove(item);
        }

        /// <summary>
        ///   <para>Determines whether the collection contains the specified item.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>The item to find.</para>
        /// </param>
        /// <returns>
        ///   <para> Is true if the item is found in the collection; otherwise, false.</para>
        /// </returns>
        public bool Contains(T item)
        {
            Util.CheckArgumentNull(item, "item");
            return list.Contains(item);
        }

        internal List<T> List
        {
            get { return list; }
        }

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///   <para>Gets an enumerator that you can use to enumerate the items in the collection.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you use to enumerate the items in the collection.</para>
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ICollection<T> Members


        /// <summary>
        ///   <para>Copies items from the collection to an array at the specified index.</para>
        /// </summary>
        /// <param name="array">
        ///   <para>An array to which you want to copy the contents of the collection.</para>
        /// </param>
        /// <param name="arrayIndex">
        ///   <para>The zero-based index in <paramref name="array" /> at which copying begins.</para>
        /// </param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///   <para>Determines whether the collection is read-only.</para>
        /// </summary>
        /// <value>
        ///   <para>Is true if the collection is read-only; otherwise, false.</para>
        /// </value>
        public bool IsReadOnly
        {
            get { return this.readOnly; }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        ///   <para>Copies items from the collection to an array at the specified index.</para>
        /// </summary>
        /// <param name="array">
        ///   <para>An array to which you want to copy the contents of the collection.</para>
        /// </param>
        /// <param name="index">
        ///   <para>The zero-based index in <paramref name="array" /> at which copying begins.</para>
        /// </param>
        public void CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
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
