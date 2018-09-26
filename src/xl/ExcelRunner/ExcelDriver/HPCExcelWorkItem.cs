//------------------------------------------------------------------------------
// <copyright file="HPCExcelWorkItem.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Generic data type to package data in for sending requests to 
//      ExcelService
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

	/// <summary>
	///   <para>Represents a list of parameter values that you want to pass to an 
	/// Excel macro when performing a calculation for a workbook or a list of results from a calculation.</para>
	/// </summary>
	/// <remarks>
	///   <para>A work item consists of an array of objects that represent the parameter values that you want to pass to an 
	/// Excel macro or a list of results from a calculation that an Excel macro performed. The array cannot consist of more than 30 objects.</para>
	/// </remarks>
	/// <seealso cref="System.Object" />
    [Serializable]
    public class WorkItem : ISerializable
    {
        /// <summary>
        /// Maximum number of elements that Excel will accept.
        /// </summary>
        private readonly int MAXELEMENTS = 30;

        /// <summary>
        /// Name of serialized value holding macro parameters
        /// </summary>
        private readonly string ELEMENTSID = "elements";

        /// <summary>
        /// Name of serialized value holding the version identifier
        /// </summary>
        private readonly string VERSIONID = "version";

        /// <summary>
        /// Current Version of WorkItem Microsoft.Hpc.Excel.WorkItem
        /// </summary>
        private readonly double MYVERSION = 1.0;

        /// <summary>
        /// Internal data structure representing the data in the work item
        /// </summary>
        private List<object> workItemElements;

		/// <summary>
		///   <para>Initializes a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> class without information about any values for macro parameters.</para>
		/// </summary>
		/// <remarks>
		///   <para>To initialize a 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object with a set of objects that specify the values to pass in for the parameters of an Excel macro, use the  
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor(System.Object[])" /> form of the constructor.</para>
		///   <para>You create a new 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object when you want to specify parameter values for an Excel macro in a 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest" /> object. </para>
		///   <para>When you want to get 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> objects that represent the results of a calculation, use the 
		/// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}" /> method or implement a 
		/// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate to get the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse" /> objects that represent the responses to the calculation requests, and then deserialize the contents of the  
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse.CalculateResult" /> field in those responses with the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.Deserialize(System.Byte[])" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.#ctor(System.Object[])" />
        public WorkItem()
        {
            this.workItemElements = new List<object>(this.MAXELEMENTS);
        }

		/// <summary>
		///   <para>Initializes a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> class with an array of objects that specify values for the parameters of an Excel macro.</para>
		/// </summary>
		/// <param name="elements">
		///   <para>An array of objects that specify that values that you want to pass in for the parameters of an Excel macro.</para>
		/// </param>
		/// <remarks>
		///   <para>The array cannot consist of more than 30 objects.</para>
		///   <para>To initialize a 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor" /> object without any  objects, use the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor" /> form of the constructor.</para>
		///   <para>You create a new 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object when you want to specify parameter values for an Excel macro in a 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest" /> object. </para>
		///   <para>When you want to get 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> objects that represent the results of a calculation, use the 
		/// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses{T}" /> method or implement a 
		/// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{T}" /> delegate to get the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse" /> objects that represent the responses to the calculation requests, and then deserialize the contents of the  
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse.CalculateResult" /> field in those responses with the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.Deserialize(System.Byte[])" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.#ctor" />
        public WorkItem(object[] elements)
        {
            // Check if list contains a valid number of elements.
            if (elements.Length > this.MAXELEMENTS)
            {
                throw new System.NotSupportedException(Resources.HPCExcelWorkItem_MaxSize); 
            }

            // Add each element in parameter to the internal list
            this.workItemElements = new List<object>(this.MAXELEMENTS);
            foreach (object obj in elements)
            {
                this.workItemElements.Add(obj);
            }
        }

        /// <summary>
        ///   <para>Initializes a new instance of the WorkItem class when being deserialized.</para>
        /// </summary>
        /// <param name="info">
        ///   <para>Information contained in deserialized work item.</para>
        /// </param>
        /// <param name="context">
        ///   <para>Source/Destination of serialization stream.</para>
        /// </param>
        protected WorkItem(SerializationInfo info, StreamingContext context)
        {
            // Check if incoming version is greater than or equal to implementation version
            string incomingVersion = info.GetValue(this.VERSIONID, typeof(string)).ToString();
            if (double.Parse(incomingVersion, System.Globalization.CultureInfo.InvariantCulture) >= this.MYVERSION)
            {
                this.workItemElements = (List<object>)info.GetValue(this.ELEMENTSID, typeof(List<object>));
            }
            else
            {
                this.workItemElements = new List<object>(this.MAXELEMENTS);
            }
        }

		/// <summary>
		///   <para>Serializes the specified 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object as an array of bytes that you can use as part of a 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest" /> object.</para>
		/// </summary>
		/// <param name="wi">
		///   <para>The <see cref="Microsoft.Hpc.Excel.WorkItem" /> object that you want to serialize.</para>
		/// </param>
		/// <returns>
		///   <para>An array of <see cref="System.Byte" /> objects that represents the specified <see cref="Microsoft.Hpc.Excel.WorkItem" />.</para>
		/// </returns>
		/// <remarks>
		///   <para>Use the method to transform a 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> into a form that you can use in a 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest" /> object when sending requests to perform calculations to the built-in Excel SOA service.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest.#ctor(System.String,System.Byte[],System.Nullable{System.DateTime})" />
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.Deserialize(System.Byte[])" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.CalculateRequest.inputs" />
        public static byte[] Serialize(WorkItem wi)
        {
            byte[] retVal;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, wi);
            retVal = stream.GetBuffer();
            stream.Close();
            return retVal;
        }

		/// <summary>
		///   <para>Deserializes the specified string by converting it to the equivalent <see cref="Microsoft.Hpc.Excel.WorkItem" /> object.</para>
		/// </summary>
		/// <param name="serializedWI">
		///   <para>An array of <see cref="System.Byte" /> objects that represents a <see cref="Microsoft.Hpc.Excel.WorkItem" /> object.</para>
		/// </param>
		/// <returns>
		///   <para>A <see cref="Microsoft.Hpc.Excel.WorkItem" /> object that corresponds to the specified array of bytes.</para>
		/// </returns>
		/// <exception cref="System.Runtime.Serialization.SerializationException">
		///   <para>Indicates that the specified array of bytes does not represent a valid serialized 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object.</para>
		/// </exception>
		/// <remarks>
		///   <para>Use this method to deserialize the array of bytes in the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse.CalculateResult" /> field as a 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object that contains information about the results of a calculation of an Excel workbook.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse.CalculateResult" />
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.Serialize(Microsoft.Hpc.Excel.WorkItem)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.CalculateResponse" />
        public static WorkItem Deserialize(byte[] serializedWI)
        {
            WorkItem retVal;
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(serializedWI);
            retVal = (WorkItem) formatter.Deserialize(stream);
            stream.Close();
            return retVal;
        }
        
		/// <summary>
		///   <para>Inserts an object of the specified type at the specified position in the array of objects that the WorkItem object contains.</para>
		/// </summary>
		/// <param name="ordinal">
		///   <para>The position at which you want to insert the object in the list of objects that the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor" /> object contains. This position is a zero-based index.</para>
		/// </param>
		/// <param name="value">
		///   <para>An object or value of the type that the type parameter specifies.</para>
		/// </param>
		/// <typeparam name="T">
		///   <para>The type of the object that you want to insert into the array.</para>
		/// </typeparam>
		/// <remarks>
		///   <para>Use this method to add objects to the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor" /> object if you did not add objects for all of the macro parameters when you created the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem.#ctor" /> object.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.#ctor" />
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.Get{T}(System.Int32)" />
		/// <seealso cref="Microsoft.Hpc.Excel.WorkItem.GetAll" />
        public void Insert<T>(int ordinal, T value)
        {
            this.workItemElements.Insert(ordinal, (object)value);
        }

		/// <summary>
		///   <para>Gets the object at the specified position in the list of objects that the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object contains.</para>
		/// </summary>
		/// <param name="ordinal">
		///   <para>The position of the object that you want to get in the list of objects that the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> object contains. This position is a zero-based index.</para>
		/// </param>
		/// <typeparam name="T">
		///   <para>The type of object that you want to get.</para>
		/// </typeparam>
		/// <returns>
		///   <para>The object with the type that the type parameter specifies that is at the specified position in the 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" />.</para>
		/// </returns>
		/// <exception cref="System.InvalidCastException">
		///   <para>The item at the specified position in the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.WorkItem" /> is not the same type as the type parameter specifies and cannot be converted to the type that the type parameter specifies.</para> 
		/// </exception>
        public T Get<T>(int ordinal)
        {
            return (T)this.workItemElements[ordinal];
        }

		/// <summary>
		///   <para>Gets all of the objects that the <see cref="Microsoft.Hpc.Excel.WorkItem" /> object contains.</para>
		/// </summary>
		/// <returns>
		///   <para>An array of the objects that the <see cref="Microsoft.Hpc.Excel.WorkItem" /> object contains.</para>
		/// </returns>
        public object[] GetAll()
        {
            return this.workItemElements.ToArray();
        }

        /// <summary>
        ///   <para>Provides interface-defined serialization method.</para>
        /// </summary>
        /// <param name="info">
        ///   <para>Information to be contained in deserialized work item.</para>
        /// </param>
        /// <param name="context">
        ///   <para>Source/Destination of serialization stream.</para>
        /// </param>
        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.GetObjectData(info, context);
        }

        /// <summary>
        ///   <para>Provides serialization behavior.</para>
        /// </summary>
        /// <param name="info">
        ///   <para>Information to be contained in deserialized work item.</para>
        /// </param>
        /// <param name="context">
        ///   <para>Source/Destination of serialization stream.</para>
        /// </param>
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(this.VERSIONID, this.MYVERSION);
            info.AddValue(this.ELEMENTSID, this.workItemElements);
        }
    } // class
} // namespace
