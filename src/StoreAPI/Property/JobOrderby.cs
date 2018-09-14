using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para>Contains a list of <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> objects.</para>
    /// </summary>
    public interface IJobOrderByList : IEnumerable<JobOrderBy>
    {
        /// <summary>
        ///   <para>Adds a <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to the list.</para>
        /// </summary>
        /// <param name="orderBy">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to add to the list.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.IJobOrderByList" /> interface.</para>
        /// </returns>
        IJobOrderByList Add(JobOrderBy orderBy);
        /// <summary>
        ///   <para>Converts the object to an integer.</para>
        /// </summary>
        /// <returns>
        ///   <para>An integer that represented the converted list.</para>
        /// </returns>
        int ToInt();
        /// <summary>
        ///   <para>Retrieves the number of items in the list.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items in the list.</para>
        /// </value>
        int Count { get; }
    }


    /// <summary>
    ///   <para>Used to specify the preference that you want the scheduler to use when it decides on which nodes to 
    /// run your job. For example, schedule the job on nodes with the most amount of memory and the least number of cores. </para>
    /// </summary>
    /// <remarks>
    ///   <para>When the scheduler schedules a job, it creates a list of nodes on which the job can run. By default, the list is sorted first by those nodes with the most number of 
    /// cores and then by those nodes with the most amount of memory. The job is scheduled on nodes starting at the top of the list. To specify the scheduling preference for your job, set the  
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OrderBy" /> property (do not use this class).</para>
    /// </remarks>
    [Serializable]
    public class JobOrderBy : IJobOrderByList
    {
        /// <summary>
        ///   <para>Defines the properties that you can specify when telling the 
        /// scheduler your preference for determining on which nodes to schedule the job. </para>
        /// </summary>
        public enum OrderByProperty : byte
        {
            /// <summary>
            ///   <para>Declares that there is no preference for determining on which nodes to schedule the job.</para>
            /// </summary>
            None = 0,
            /// <summary>
            ///   <para>Give preference to memory.</para>
            /// </summary>
            Memory = 1,
            /// <summary>
            ///   <para>Give preference to cores.</para>
            /// </summary>
            Cores = 2,
        }

        /// <summary>
        ///   <para>Defines the order in which the resources list is sorted.</para>
        /// </summary>
        public enum SortOrder : byte
        {
            /// <summary>
            ///   <para>Sort the resources list in descending order.</para>
            /// </summary>
            Descending = 0x0,
            /// <summary>
            ///   <para>Sort the resources list in ascending order.</para>
            /// </summary>
            Ascending = 0x80,
        }

        OrderByProperty _property;
        SortOrder _order;

        internal JobOrderBy(OrderByProperty property, SortOrder order)
        {
            _property = property;
            _order = order;
        }

        /// <summary>
        ///   <para>The property that you want the scheduler to give preference to when creating the list of nodes on which the job can run.</para>
        /// </summary>
        /// <value>
        ///   <para>The property to give preference to. For possible values, see 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy.OrderByProperty" />.</para>
        /// </value>
        public OrderByProperty Property
        {
            get { return _property; }
        }

        /// <summary>
        ///   <para>Retrieves the order in which the preference is sorted.</para>
        /// </summary>
        /// <value>
        ///   <para>The order in which the preference specified in <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy.Property" /> is sorted
        /// For possible values, see <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy.SortOrder" />.</para>
        /// </value>
        public SortOrder Order
        {
            get { return _order; }
        }

        /// <summary>
        ///   <para>Parses the specified string and uses its parts to create a <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object.</para>
        /// </summary>
        /// <param name="text">
        ///   <para>A string that contains the preferences in the form “[-][Core|Memory]”.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that contains the preference.</para>
        /// </returns>
        public static JobOrderBy Parse(string text)
        {
            text = text.Trim();

            SortOrder order = SortOrder.Descending;
            if (text.StartsWith("-"))
            {
                order = SortOrder.Ascending;
                text = text.Substring(1);
            }

            OrderByProperty column = (OrderByProperty)Enum.Parse(typeof(OrderByProperty), text, true);
            return new JobOrderBy(column, order);
        }

        /// <summary>
        ///   <para>Determines if the specified object is equal to this object.</para>
        /// </summary>
        /// <param name="obj">
        ///   <para>The object to compare with this object.</para>
        /// </param>
        /// <returns>
        ///   <para>Is true if the objects are equal; otherwise, false.</para>
        /// </returns>
        public override bool Equals(object obj)
        {
            JobOrderBy that = obj as JobOrderBy;
            return that != null && this._property == that._property && this._order == that._order;
        }

        /// <summary>
        ///   <para>Retrieves a unique hash code for the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A unique hash code for the object.</para>
        /// </returns>
        public override int GetHashCode()
        {
            return this.ToByte();
        }

        /// <summary>
        ///   <para>Retrieves a formatted string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A formatted string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            return (_order == SortOrder.Ascending ? "-" : string.Empty) + _property.ToString();
        }

        /// <summary>
        ///   <para>Converts this object to a byte.</para>
        /// </summary>
        /// <returns>
        ///   <para>A byte that represents this object.</para>
        /// </returns>
        public byte ToByte()
        {
            return (byte)((byte)_property | (byte)_order);
        }

        /// <summary>
        ///   <para>Converts the specified byte into a JobOrderBy object.</para>
        /// </summary>
        /// <param name="b">
        ///   <para>The byte that you want to convert into a JobOrderBy object.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that is equivalent to the specified byte.</para>
        /// </returns>
        public static JobOrderBy FromByte(byte b)
        {
            JobOrderBy result = new JobOrderBy((OrderByProperty)(b & 0x0f), (SortOrder)(b & 0xf0));

            if (result.Order != SortOrder.Ascending && result.Order != SortOrder.Descending)
                throw new ArgumentException();
            if (result.Property != OrderByProperty.Cores && result.Property != OrderByProperty.Memory)
                throw new ArgumentException();

            return result;
        }

        public static explicit operator Byte(JobOrderBy orderby)
        {
            return orderby.ToByte();
        }

        static JobOrderBy memoryAsc = new JobOrderBy(OrderByProperty.Memory, SortOrder.Ascending);
        static JobOrderBy memoryDesc = new JobOrderBy(OrderByProperty.Memory, SortOrder.Descending);
        static JobOrderBy processorsAsc = new JobOrderBy(OrderByProperty.Cores, SortOrder.Ascending);
        static JobOrderBy processorsDesc = new JobOrderBy(OrderByProperty.Cores, SortOrder.Descending);

        static Dictionary<byte, JobOrderBy> instanceTable = new Dictionary<byte, JobOrderBy>();

        static JobOrderBy()
        {
            instanceTable.Add((byte)memoryAsc, memoryAsc);
            instanceTable.Add((byte)memoryDesc, memoryDesc);
            instanceTable.Add((byte)processorsAsc, processorsAsc);
            instanceTable.Add((byte)processorsDesc, processorsDesc);
        }

        /// <summary>
        ///   <para>Creates a 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that you use to specify your preference for scheduling jobs on nodes in the cluster.</para> 
        /// </summary>
        /// <param name="prop">
        ///   <para>The resource property to give preference to when scheduling resources for the job. For possible properties that you can specify, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy.OrderByProperty" /> enumeration.</para>
        /// </param>
        /// <param name="order">
        ///   <para>The order in which to sort the resource property in the list of nodes that can run the job. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy.OrderByProperty" /> enumeration.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that contains the preference.</para>
        /// </returns>
        public static JobOrderBy CreateOrderBy(OrderByProperty prop, SortOrder order)
        {
            JobOrderBy orderby;
            if (instanceTable.TryGetValue((byte)((byte)prop | (byte)order), out orderby))
            {
                return orderby;
            }

            throw new ArgumentException("Unknown orderby");
        }

        /// <summary>
        ///   <para>Retrieves an object that defines a preference for using the amount of memory on a node 
        /// when generating the list of nodes on which the job is scheduled. The list is sorted in ascending order.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that defines a preference for scheduling the job on nodes with the least amount of memory first.</para> 
        /// </value>
        public static JobOrderBy MemoryAsc
        {
            get { return memoryAsc; }
        }

        /// <summary>
        ///   <para>Retrieves an object that defines a preference for using the amount of memory on a node 
        /// when generating the list of nodes on which the job is scheduled. The list is sorted in descending order.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that defines a preference for scheduling the job on nodes with the most amount of memory first.</para> 
        /// </value>
        public static JobOrderBy MemoryDesc
        {
            get { return memoryDesc; }
        }

        /// <summary>
        ///   <para>Retrieves an object that defines a preference for using the number of cores on a node 
        /// when generating the list of nodes on which the job is scheduled. The list is sorted in ascending order.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that defines a preference for scheduling the job on nodes with the least number of cores first.</para> 
        /// </value>
        public static JobOrderBy ProcessorsAsc
        {
            get { return processorsAsc; }
        }

        /// <summary>
        ///   <para>Retrieves an object that defines a preference for using the number of cores on a node 
        /// when generating the list of nodes on which the job is scheduled. The list is sorted in descending order.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that defines a preference for scheduling the job on nodes with the most number of cores first.</para> 
        /// </value>
        public static JobOrderBy ProcessorsDesc
        {
            get { return processorsDesc; }
        }

        #region IJobOrderByList Members

        /// <summary>
        ///   <para>Returns a new preference list that includes the preferences from this object and the specified preference.</para>
        /// </summary>
        /// <param name="orderBy">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object that contains the preference to add to the list.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.IJobOrderByList" /> interface that contains the list of preferences.</para>
        /// </returns>
        public IJobOrderByList Add(JobOrderBy orderBy)
        {
            return new JobOrderByList().Add(this).Add(orderBy);
        }

        /// <summary>
        ///   <para>Retrieves the number of preferences in the order by list.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of preferences in the list.</para>
        /// </value>
        public int Count
        {
            get { return 1; }
        }

        /// <summary>
        ///   <para>Converts this object to an integer.</para>
        /// </summary>
        /// <returns>
        ///   <para>An integer that represents this object.</para>
        /// </returns>
        public int ToInt()
        {
            return ToByte();
        }

        #endregion

        #region IEnumerable<JobOrderBy> Members

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the list of preferences.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.Generic.IEnumerator{T}" /> interface that you use to enumerate the items in the list.</para>
        /// </returns>
        public IEnumerator<JobOrderBy> GetEnumerator()
        {
            List<JobOrderBy> list = new List<JobOrderBy>();
            list.Add(this);
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the list of preferences.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you can use to enumerate the items in the list.</para>
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new JobOrderBy[] { this }.GetEnumerator();
        }

        #endregion
    }

    /// <summary>
    ///   <para>Contains a list of <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> objects.</para>
    /// </summary>
    [Serializable]
    public class JobOrderByList : IJobOrderByList
    {
        const byte PropertyMask = 0x7f;
        const byte OrderMask = 0x80;
        const int MaxSize = 4;

        List<JobOrderBy> list = new List<JobOrderBy>();

        #region IJobOrderByList Members

        /// <summary>
        ///   <para>Adds a <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to the list.</para>
        /// </summary>
        /// <param name="orderBy">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to add to the list.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.IJobOrderByList" /> interface.</para>
        /// </returns>
        IJobOrderByList IJobOrderByList.Add(JobOrderBy orderBy)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        ///   <para>Adds a <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to the list.</para>
        /// </summary>
        /// <param name="item">
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> object to add to the list.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="Microsoft.Hpc.Scheduler.Properties.IJobOrderByList" /> interface.</para>
        /// </returns>
        public IJobOrderByList Add(JobOrderBy item)
        {
            if (list.Count >= MaxSize)
            {
                throw new InvalidOperationException("List is full");
            }

            foreach (JobOrderBy orderby in list)
            {
                if (orderby.Property == item.Property)
                {
                    throw new InvalidOperationException(string.Format("Job orderby for property {0} already exists in the list", item.Property));
                }
            }

            list.Add(item);

            return this;
        }

        /// <summary>
        ///   <para>Retrieves the number of items in the list.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of items in the list.</para>
        /// </value>
        public int Count
        {
            get { return list.Count; }
        }

        /// <summary>
        ///   <para>Converts the object to an integer.</para>
        /// </summary>
        /// <returns>
        ///   <para>An integer that represented the converted list.</para>
        /// </returns>
        public int ToInt()
        {
            Debug.Assert(list.Count <= MaxSize);

            byte[] bytes = new byte[4];
            for (int i = 0; i < list.Count; i++)
            {
                bytes[i] = (byte)list[i];
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        #endregion

        #region IEnumerable<JobOrderBy> Members

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the list of preferences.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.Generic.IEnumerator{T}" /> interface that you use to enumerate the items in the list.</para>
        /// </returns>
        public IEnumerator<JobOrderBy> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        ///   <para>Retrieves an enumerator that you can use to enumerate the items in the list of preferences.</para>
        /// </summary>
        /// <returns>
        ///   <para>An <see cref="System.Collections.IEnumerator" /> interface that you can use to enumerate the items in the list.</para>
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        /// <summary>
        ///   <para>Converts the specified integer back into a preferences list.</para>
        /// </summary>
        /// <param name="number">
        ///   <para>A number from a previous call to <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderByList.ToInt" />.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderByList" /> object that contains the list of preferences.</para>
        /// </returns>
        public static JobOrderByList FromInt(int number)
        {
            JobOrderByList orderbys = new JobOrderByList();

            if (number == 0)
            {
                return orderbys;
            }

            byte[] bytes = BitConverter.GetBytes(number);
            foreach (byte b in bytes)
            {
                JobOrderBy.OrderByProperty property = (JobOrderBy.OrderByProperty)(b & PropertyMask);
                JobOrderBy.SortOrder order = (JobOrderBy.SortOrder)(b & OrderMask);

                if (property != 0)
                {
                    orderbys.Add(new JobOrderBy(property, order));
                }
            }

            return orderbys;
        }

        /// <summary>
        ///   <para>Parses the specified string and uses its parts to create one or more  
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderBy" /> objects that are added to the list.</para>
        /// </summary>
        /// <param name="text">
        ///   <para>A string that contains the preferences in the form “[-][Core|Memory][,[-][Core|Memory]]”.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.Properties.JobOrderByList" /> object that contains the preferences.</para>
        /// </returns>
        public static JobOrderByList Parse(string text)
        {
            JobOrderByList orderbys = new JobOrderByList();
            text = text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                return orderbys;
            }

            foreach (string item in text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                orderbys.Add(JobOrderBy.Parse(item));
            }

            return orderbys;
        }

        /// <summary>
        ///   <para>Retrieves a formatted string that represents the object.</para>
        /// </summary>
        /// <returns>
        ///   <para>A formatted string that represents the object.</para>
        /// </returns>
        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    text.Append(',');
                }
                text.Append(list[i]);
            }

            return text.ToString();
        }

        public static explicit operator JobOrderByList(int number)
        {
            return FromInt(number);
        }

        public static explicit operator Int32(JobOrderByList orderbys)
        {
            return orderbys.ToInt();
        }
    }
}
