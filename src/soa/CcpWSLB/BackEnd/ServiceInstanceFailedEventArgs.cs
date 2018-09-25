//------------------------------------------------------------------------------
// <copyright file="ServiceInstanceFailedEventArgs.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Service instance failed event args
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// service instance failed event args
    /// </summary>
    class ServiceInstanceFailedEventArgs : EventArgs
    {
        #region private fields
        /// <summary> session fault that indicate the fail reason </summary>
        private SessionFault sessionFaultField;
        #endregion

        /// <summary>
        /// Create a new instance of the ServiceInstanceFailedEventArgs class
        /// </summary>
        /// <param name="sessionFault">session fault that indicates the fail reason</param>
        public ServiceInstanceFailedEventArgs(SessionFault sessionFault)
        {
            this.sessionFaultField = sessionFault;
        }

        /// <summary>
        /// Gets the session fault
        /// </summary>
        public SessionFault Fault
        {
            get
            {
                return this.sessionFaultField;
            }
        }
    }
}
