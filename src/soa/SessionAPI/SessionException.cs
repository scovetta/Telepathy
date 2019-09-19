// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Represents an exception that occurs when a session error occurs.</para>
    /// </summary>
    [Serializable]
    public class SessionException : Exception
    {
        private int errorCode = SOAFaultCode.UnknownError;

        public SessionException(Exception e)
            : base(SR.ExceptionInCreatingSession, e)
        {
        }

        public SessionException(string msg)
            : base(msg)
        {
        }

        public SessionException(int errorCode, string msg)
            : base(msg)
        {
            this.errorCode = errorCode;
        }

        public SessionException(int errorCode, string msg, Exception innerException)
            : base(msg, innerException)
        {
            this.errorCode = errorCode;
        }

        /// <summary>
        /// Constructor of <see cref="SessionException"/>
        /// </summary>
        /// <param name="msg">
        ///   <para />
        /// </param>
        /// <param name="e">
        ///   <para />
        /// </param>
        public SessionException(string msg, Exception e)
            : base(msg, e)
        {
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="info">
        ///   <para />
        /// </param>
        /// <param name="context">
        ///   <para />
        /// </param>
        public SessionException(SessionInfo info, StreamingContext context)
        {
        }

        /// <summary>
        ///   <para>Gets the error code for the session-specific error.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="System.Int32" /> that contains the error code for the session-specific error.</para>
        /// </value>
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
    }
}
