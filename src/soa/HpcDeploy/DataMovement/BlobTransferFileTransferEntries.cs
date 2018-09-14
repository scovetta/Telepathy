//------------------------------------------------------------------------------
// <copyright file="BlobTransferFileTransferEntries.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Represents the list of files in the restartable mode state.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents the list of files in the restartable mode state.
    /// </summary>
    public class BlobTransferFileTransferEntries : IDictionary<string, BlobTransferFileTransferEntry>, IDictionary
    {
        private ConcurrentDictionary<string, BlobTransferFileTransferEntry> dictionary = new ConcurrentDictionary<string, BlobTransferFileTransferEntry>();

        bool IDictionary.IsFixedSize
        {
            get
            {
                return ((IDictionary)this.dictionary).IsFixedSize;
            }            
        }

        bool IDictionary.IsReadOnly
        {
            get 
            {
                return ((IDictionary)this.dictionary).IsReadOnly;
            }
        }

        ICollection IDictionary.Keys
        {
            get 
            {
                return ((IDictionary)this.dictionary).Keys;    
            }
        }

        ICollection IDictionary.Values
        {            
            get 
            {
                return ((IDictionary)this.dictionary).Values;
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return ((ICollection)this.dictionary).IsSynchronized;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return ((ICollection)this.dictionary).SyncRoot;
            }
        }

        bool ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>.IsReadOnly
        {
            get
            {
                return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).IsReadOnly;
            }
        }

        /// <summary>
        /// Gets the number of key/value pairs contained in the <see
        /// cref="BlobTransferFileTransferEntries"/>.
        /// </summary> 
        /// <exception cref="T:System.OverflowException">The dictionary contains too many
        /// elements.</exception> 
        /// <value>The number of key/value pairs contained in the <see 
        /// cref="BlobTransferFileTransferEntries"/>.</value>
        /// <remarks>Count has snapshot semantics and represents the number of items in the <see 
        /// cref="BlobTransferFileTransferEntries"/>
        /// at the moment when Count was accessed.</remarks>
        public int Count
        {
            get
            {
                return this.dictionary.Count;
            }
        }

        /// <summary>
        /// Gets a collection containing the keys in the <see 
        /// cref="T:System.Collections.Generic.Dictionary{string,BlobTransferFileTransferEntry}"/>.
        /// </summary>
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{string}"/> containing the keys in the
        /// <see cref="T:System.Collections.Generic.Dictionary{string,BlobTransferFileTransferEntry}"/>.</value>
        public ICollection<string> Keys
        {
            get
            {
                return this.dictionary.Keys;
            }
        }

        /// <summary>
        /// Gets a collection containing the values in the <see
        /// cref="T:System.Collections.Generic.Dictionary{string,BlobTransferFileTransferEntry}"/>.
        /// </summary> 
        /// <value>An <see cref="T:System.Collections.Generic.ICollection{BlobTransferFileTransferEntry}"/> containing the values in
        /// the 
        /// <see cref="T:System.Collections.Generic.Dictionary{string,BlobTransferFileTransferEntry}"/>.</value> 
        public ICollection<BlobTransferFileTransferEntry> Values
        {
            get
            {
                return this.dictionary.Values;
            }
        }

        /// <summary> 
        /// Gets a value indicating whether the <see cref="BlobTransferFileTransferEntries"/> is empty. 
        /// </summary>
        /// <value>True if the <see cref="BlobTransferFileTransferEntries"/> is empty; otherwise, 
        /// false.</value>
        public bool IsEmpty
        {
            get
            {
                return this.dictionary.IsEmpty;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return ((IDictionary)this.dictionary)[key];
            }

            set
            {
                ((IDictionary)this.dictionary)[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key. 
        /// </summary> 
        /// <param name="key">The key of the value to get or set.</param>
        /// <value>The value associated with the specified key. If the specified key is not found, a get 
        /// operation throws a
        /// <see cref="T:Sytem.Collections.Generic.KeyNotFoundException"/>, and a set operation creates a new
        /// element with the specified key.</value>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        /// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The property is retrieved and 
        /// <paramref name="key"/> 
        /// does not exist in the collection.</exception>
        /// <returns>Return the BlobTransferFileTransferEntry stored with the specified key.</returns>
        public BlobTransferFileTransferEntry this[string key]
        {
            get
            {
                return this.dictionary[key];
            }

            set
            {
                this.dictionary[key] = value;
            }
        }

        void IDictionary.Remove(object key)
        {
            ((IDictionary)this.dictionary).Remove(key);
        }

        void IDictionary.Add(object key, object value)
        {
            ((IDictionary)this.dictionary).Add(key, value);            
        }

        void IDictionary.Clear()
        {
            ((IDictionary)this.dictionary).Clear();
        }

        bool IDictionary.Contains(object key)
        {
            return ((IDictionary)this.dictionary).Contains(key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return ((IDictionary)this.dictionary).GetEnumerator();
        }

        void ICollection.CopyTo(System.Array array, int index)
        {
            ((ICollection)this.dictionary).CopyTo(array, index);            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.dictionary).GetEnumerator();            
        }

        void IDictionary<string, BlobTransferFileTransferEntry>.Add(string key, BlobTransferFileTransferEntry value)
        {
            ((IDictionary<string, BlobTransferFileTransferEntry>)this.dictionary).Add(key, value);            
        }

        bool IDictionary<string, BlobTransferFileTransferEntry>.Remove(string key)
        {
            return ((IDictionary<string, BlobTransferFileTransferEntry>)this.dictionary).Remove(key);            
        }

        void ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>.Add(KeyValuePair<string, BlobTransferFileTransferEntry> item)
        {
            ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Add(item);            
        }

        bool ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>.Contains(KeyValuePair<string, BlobTransferFileTransferEntry> item)
        {
            return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>.CopyTo(KeyValuePair<string, BlobTransferFileTransferEntry>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>.Remove(KeyValuePair<string, BlobTransferFileTransferEntry> item)
        {
            return ((ICollection<KeyValuePair<string, BlobTransferFileTransferEntry>>)this.dictionary).Remove(item);
        }

        /// <summary> 
        /// Determines whether the <see cref="BlobTransferFileTransferEntries"/> contains the specified
        /// key.
        /// </summary>
        /// <param name="key">The key to locate in the <see cref="BlobTransferFileTransferEntries"/>.</param>
        /// <returns>true if the <see cref="BlobTransferFileTransferEntries"/> contains an element with 
        /// the specified key; otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference
        /// (Nothing in Visual Basic).</exception> 
        public bool ContainsKey(string key)
        {
            return this.dictionary.ContainsKey(key);            
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see 
        /// cref="BlobTransferFileTransferEntries"/>. 
        /// </summary>
        /// <param name="key">The key of the value to get.</param> 
        /// <param name="value">When this method returns, <paramref name="value"/> contains the object from
        /// the
        /// <see cref="BlobTransferFileTransferEntries"/> with the specified key or null,
        /// if the operation failed.</param> 
        /// <returns>true if the key was found in the <see cref="BlobTransferFileTransferEntries"/>;
        /// otherwise, false.</returns> 
        /// <exception cref="T:System.ArgumentNullException"><paramref name="key"/> is a null reference 
        /// (Nothing in Visual Basic).</exception>
        public bool TryGetValue(string key, out BlobTransferFileTransferEntry value)
        {
            return this.dictionary.TryGetValue(key, out value);
        }

        /// <summary> 
        /// Removes all keys and values from the <see cref="BlobTransferFileTransferEntries"/>.
        /// </summary> 
        public void Clear()
        {
            this.dictionary.Clear();            
        }

        /// <summary>Returns an enumerator that iterates through the <see
        /// cref="BlobTransferFileTransferEntries"/>.</summary>
        /// <returns>An enumerator for the <see cref="BlobTransferFileTransferEntries"/>.</returns>
        /// <remarks> 
        /// The enumerator returned from the dictionary is safe to use concurrently with
        /// reads and writes to the dictionary, however it does not represent a moment-in-time snapshot 
        /// of the dictionary.  The contents exposed through the enumerator may contain modifications 
        /// made to the dictionary after <see cref="GetEnumerator"/> was called.
        /// </remarks> 
        public IEnumerator<KeyValuePair<string, BlobTransferFileTransferEntry>> GetEnumerator()
        {
            return this.dictionary.GetEnumerator();
        }
    }
}