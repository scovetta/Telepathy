//------------------------------------------------------------------------------
// <copyright file="BlobTransferOverwritePromptCallback.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Delegate for overwrite file prompt callback.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.BlobTransferCallbacks
{
    /// <summary>
    /// Delegate for overwrite file prompt callback.
    /// </summary>
    /// <param name="sourcePath">Path of the source file used to overwrite the destination.</param>
    /// <param name="destinationPath">Path of the file to be overwritten.</param>
    /// <returns>True if the file should be overwritten; otherwise false.</returns>
    public delegate bool BlobTransferOverwritePromptCallback(
        string sourcePath,
        string destinationPath);
}