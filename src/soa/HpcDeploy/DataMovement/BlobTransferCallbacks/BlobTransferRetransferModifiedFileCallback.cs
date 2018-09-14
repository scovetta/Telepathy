//------------------------------------------------------------------------------
// <copyright file="BlobTransferRetransferModifiedFileCallback.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Delegate for a prompt callback used to determine whether 
//      to retransfer the whole file or just fail it
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.BlobTransferCallbacks
{
    /// <summary>
    /// Delegate for callback used to determine whether to retransfer the whole file or just fail it
    /// if source has been changed since last unfinished transfer.
    /// </summary>
    /// <param name="sourcePath">Path of the file to be transferred.</param>
    /// <returns>True if the file should be retransferred; otherwise false.</returns>
    public delegate bool BlobTransferRetransferModifiedFileCallback(
        string sourcePath);
}
