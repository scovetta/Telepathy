//-----------------------------------------------------------------------
// <copyright file="RetryPolicy.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Represents the retry policy for retry helper</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the retry policy for retry helper
    /// </summary>
    internal class RetryPolicy
    {
        /// <summary>
        /// Stores the default retry count
        /// </summary>
        private const int DefaultRetryCount = 3;

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class
        /// </summary>
        public RetryPolicy() : this(DefaultRetryCount) { }

        /// <summary>
        /// Initializes a new instance of the RetryPolicy class
        /// </summary>
        /// <param name="retryCount">indicating the retry count</param>
        public RetryPolicy(int retryCount)
        {
            this.RetryCount = retryCount;
            this.NoIncreaseRetryCountExceptionList = new List<Type>();
        }

        /// <summary>
        /// Gets or sets the retry count
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the list of exception types which should
        /// not increase retry count
        /// </summary>
        public List<Type> NoIncreaseRetryCountExceptionList { get; set; }
    }
}
