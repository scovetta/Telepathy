// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Exceptions
{
    using System;

    /// <summary>
    ///   <para>Contains arguments for a SOA error event.</para>
    /// </summary>
    public class SOAEventArgs : EventArgs
    {
        private int faultCode;

        /// <summary>
        ///   <para>Gets an integer that specifies the error that occurred.</para>
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.Int32" /> that specifies the error that occurred..</para>
        /// </value>
        /// <remarks>
        ///   <para>The error code will be one of the values defined in the <see cref="SOAFaultCode" /> class.</para>
        /// </remarks>
        public int FaultCode
        {
            get { return this.faultCode; }
        }

        /// <summary>
        ///   <para>Creates a new <see cref="SOAEventArgs" /> object with the specified error code.</para>
        /// </summary>
        /// <param name="errorCode">
        ///   <para>A <see cref="SOAFaultCode" /> value that indicates the error that occurred.</para>
        /// </param>
        public SOAEventArgs(int errorCode)
        {
            this.faultCode = errorCode;
        }
    }
}
