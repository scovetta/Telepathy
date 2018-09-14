//------------------------------------------------------------------------------
// <copyright file="SessionException.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       The structure as the parameter to attach a session
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;
    using System.Runtime.Serialization;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Represents an exception that occurs when a session error occurs.</para>
    /// </summary>
    [Serializable]
    public class SessionException : Exception
    {
        private int errorCode = SOAFaultCode.UnknownError;

        internal SessionException(Exception e)
            : base(SR.ExceptionInCreatingSession, e)
        {
        }

        internal SessionException(string msg)
            : base(msg)
        {
        }

        internal SessionException(int errorCode, string msg)
            : base(msg)
        {
            this.errorCode = errorCode;
        }

        internal SessionException(int errorCode, string msg, Exception innerException)
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
        protected SessionException(SessionInfo info, StreamingContext context)
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
