//-----------------------------------------------------------------------
// <copyright file="RequestStorageException.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     This exception is specific for request queue issue.
// </summary>
//-----------------------------------------------------------------------

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This exception is specific for request queue issue.
    /// </summary>
    internal class RequestStorageException : StorageException
    {
        /// <summary>
        /// Initializes a new instance of the RequestStorageException class.
        /// </summary>
        /// <param name="e">original StorageException</param>
        public RequestStorageException(StorageException e)
            : base(e.RequestInformation, e.Message, e.InnerException)
        {
        }
    }
}
