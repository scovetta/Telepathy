// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;

    /// <summary>
    /// Exception only used by parameter
    /// </summary>
    internal class ParameterException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ParameterException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ParameterException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ParameterException class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ParameterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
