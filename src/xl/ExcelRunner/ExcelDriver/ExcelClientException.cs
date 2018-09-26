//------------------------------------------------------------------------------
// <copyright file="ExcelClientException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     Exception thrown by ExcelClient component.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Runtime.Serialization;
    using System.Text;

	/// <summary>
	///   <para>Indicates that the <see cref="Microsoft.Hpc.Excel.ExcelClient" /> object encountered an error with a nonspecific cause.</para>
	/// </summary>
    [Serializable]
    public class ExcelClientException : Exception
    {
		/// <summary>
		///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Excel.ExcelClientException" /> class with the specified error message.</para>
		/// </summary>
		/// <param name="message">
		///   <para>A string that specifies a message that describe the error.</para>
		/// </param>
		/// <remarks>
		///   <para>This constructor calls the <see cref="System.Exception.#ctor(System.String)" /> constructor.</para>
		/// </remarks>
		/// <seealso cref="System.Exception.#ctor(System.String)" />
        public ExcelClientException(string message) 
            : base(message)
        {
        }

		/// <summary>
		///   <para>Initializes a new instance of the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClientException" /> class with the specified error message and a reference to the inner exception that is the cause of this exception.</para> 
		/// </summary>
		/// <param name="message">
		///   <para>A string that specifies a message that describe the error.</para>
		/// </param>
		/// <param name="innerException">
		///   <para>A 
		/// 
		/// <see cref="System.Exception" /> object that specifies the inner exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</para> 
		/// </param>
		/// <remarks>
		///   <para>This constructor calls the <see cref="System.Exception.#ctor(System.String,System.Exception)" /> constructor.</para>
		/// </remarks>
		/// <seealso cref="System.Exception.#ctor(System.String,System.Exception)" />
        public ExcelClientException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }

		/// <summary>
		///   <para>Initializes a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClientException" /> class without specifying an error message, inner exception, or serialized data.</para>
		/// </summary>
		/// <remarks>
		///   <para>This constructor calls the <see cref="System.Exception.#ctor" /> constructor.</para>
		/// </remarks>
		/// <seealso cref="System.Exception.#ctor" />
        public ExcelClientException() 
            : base()
        {
        }

		/// <summary>
		///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Excel.ExcelClientException" /> class with serialized data.</para>
		/// </summary>
		/// <param name="info">
		///   <para>The 
		/// 
		/// <see cref="System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized object data about the exception being thrown.</para> 
		/// </param>
		/// <param name="context">
		///   <para>The 
		/// 
		/// <see cref="System.Runtime.Serialization.StreamingContext" /> structure that contains contextual information about the source or destination.</para> 
		/// </param>
		/// <remarks>
		///   <para>This constructor calls the 
		/// 
		/// <see cref="System.Exception.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" /> constructor.</para> 
		/// </remarks>
		/// <seealso cref="System.Exception.#ctor(System.Runtime.Serialization.SerializationInfo,System.Runtime.Serialization.StreamingContext)" />
        public ExcelClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
