//------------------------------------------------------------------------------
// <copyright file="ResponseErrorArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      An EventArgs type which contains an exception as the event data.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    
	/// <summary>
	///   <para>Contains data about errors that occurred in getting responses from the HPC cluster for calculation requests.</para>
	/// </summary>
	/// <remarks>
	///   <para>To be notified of errors in responses to calculation requests, add a delegate to the 
	/// 
	/// <see cref="Microsoft.Hpc.Excel.ExcelClient.ErrorHandler" /> event. This delegate must include parameters for the object that sent the event and for a  
	/// <see cref="Microsoft.Hpc.Excel.ResponseErrorEventArgs" /> object. The following code example shows the signature for such a delegate.</para>
	///   <code>private void ErrorHandler(object sender, ResponseErrorEventArgs e)</code>
	/// </remarks>
	/// <seealso cref="Microsoft.Hpc.Excel.ResponseErrorEventArgs" />
	/// <seealso cref="System.EventArgs" />
    public class ResponseErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Exception causing the event.
        /// </summary>
        private Exception respException;

		/// <summary>
		///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Excel.ResponseErrorEventArgs" /> class with the specified exception.</para>
		/// </summary>
		/// <param name="ex">
		///   <para>A <see cref="System.Exception" /> object that represents the exception that caused the error.</para>
		/// </param>
        public ResponseErrorEventArgs(Exception ex) 
        {
            this.respException = ex;
        }

		/// <summary>
		///   <para>Gets the exception that the response handler received from the broker.</para>
		/// </summary>
		/// <value>
		///   <para>A <see cref="System.Exception" /> object that represents the exception that the response handler received from the broker.</para>
		/// </value>
        public Exception ResponseException
        {
            get { return this.respException; }
        }
    }
}
