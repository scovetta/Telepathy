//------------------------------------------------------------------------------
// <copyright file="ErrorFileEntry.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Class inherit from FileEntry to indicate failures.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;

    /// <summary>
    /// Class inherit from FileEntry to indicate failures.
    /// </summary>
    internal class ErrorFileEntry : FileEntry
    {
        /// <summary>
        /// Exception received when loading blobs.
        /// </summary>
        public readonly Exception Exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorFileEntry" /> class.
        /// </summary>
        /// <param name="ex">Exception to restore.</param>
        public ErrorFileEntry(Exception ex)
            : base(null, null, null)
        {
            this.Exception = ex;
        }
    }
}
