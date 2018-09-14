//------------------------------------------------------------------------------
// <copyright file="BlobTransferBeforeQueueCallback.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Delegate for before transfer callback.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.BlobTransferCallbacks
{
    /// <summary>
    /// Delegate for before transfer callback.
    /// </summary>
    /// <param name="sourcePath">Source path of the file to be transferred.</param>
    /// <param name="destinationPath">Destination path of the file to be transferred.</param>
    /// <returns>True if the file should be transferred; otherwise false.</returns>
    public delegate bool BlobTransferBeforeQueueCallback(
        string sourcePath, 
        string destinationPath);
}