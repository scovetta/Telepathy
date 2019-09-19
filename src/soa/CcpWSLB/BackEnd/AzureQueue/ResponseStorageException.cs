// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.BackEnd
{
    using Microsoft.WindowsAzure.Storage;

    /// <summary>
    /// This exception is specific for response queue issue.
    /// </summary>
    internal class ResponseStorageException : StorageException
    {
        /// <summary>
        /// Initializes a new instance of the ResponseStorageException class.
        /// </summary>
        /// <param name="e">original StorageException</param>
        /// <param name="storageName">response storage name</param>
        public ResponseStorageException(StorageException e, string storageName)
            : base(e.RequestInformation, e.Message, e.InnerException)
        {
            this.ResponseStorageName = storageName;
        }

        /// <summary>
        /// Gets the response storage name.
        /// </summary>
        public string ResponseStorageName
        {
            get;
            private set;
        }
    }
}
