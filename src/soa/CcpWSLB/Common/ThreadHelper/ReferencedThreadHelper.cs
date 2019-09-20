// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common.ThreadHelper
{
    /// <summary>
    /// Provide root entry for thread pool thread and async call back thread
    /// Provide reference counting pattern for resource manage
    /// </summary>
    /// <typeparam name="T">
    /// type of the callback state object
    /// object for WaitCallback
    /// IAsyncResult for AsyncCallback
    /// </typeparam>
    internal abstract class ReferencedThreadHelper<T>
    {
        /// <summary>
        /// Stores the ref obj
        /// </summary>
        protected ReferenceObject refObj;

        public ReferencedThreadHelper(ReferenceObject refObj)
        {
            this.refObj = refObj;
        }
    }
}
