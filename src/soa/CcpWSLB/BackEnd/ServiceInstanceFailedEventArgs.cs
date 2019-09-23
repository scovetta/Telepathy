// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.BackEnd
{
    using System;

    using Microsoft.Telepathy.Session.Exceptions;

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
