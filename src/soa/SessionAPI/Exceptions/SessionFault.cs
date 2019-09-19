// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Define the data contract for the session failure.
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/session/")]
    [Serializable]
    public class SessionFault
    {
        /// <summary>
        /// Gets the action for session fault
        /// </summary>
        public const string Action = "http://hpc.microsoft.com/session/SessionFault";

        #region private fields
        /// <summary>
        /// the fault code.
        /// </summary>
        private int faultCode;

        /// <summary>
        /// the fault reason
        /// </summary>
        private string faultReason;

        /// <summary>
        /// the fault context.
        /// </summary>
        private object[] faultContext;
        #endregion

        /// <summary>
        /// Initializes a new instance of the SessionFault class
        /// </summary>
        /// <param name="faultCode">the fault code.</param>
        /// <param name="reason">the fault reason.</param>
        /// <param name="context">the fault context.</param>
        public SessionFault(int faultCode, string reason, params object[] context)
        {
            this.faultCode = faultCode;
            this.faultReason = reason;
            this.faultContext = context;
        }

        #region public properties
        /// <summary>
        /// Gets the fault code.
        /// </summary>
        [DataMember]
        public int Code
        {
            get
            {
                return this.faultCode;
            }

            set
            {
                this.faultCode = value;
            }
        }

        /// <summary>
        /// Gets the fault reason.
        /// </summary>
        [DataMember]
        public string Reason
        {
            get
            {
                return this.faultReason;
            }

            set
            {
                this.faultReason = value;
            }
        }

        /// <summary>
        /// Gets the fault context.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "No copies are made")]
        public object[] Context
        {
            get
            {
                return this.faultContext;
            }

            set
            {
                this.faultContext = value;
            }
        }
        #endregion
    }
}
