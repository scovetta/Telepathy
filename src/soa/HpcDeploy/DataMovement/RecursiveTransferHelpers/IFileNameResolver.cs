//------------------------------------------------------------------------------
// <copyright file="IFileNameResolver.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      FileNameResolver interface.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    /// <summary>
    /// FileNameResolver interface.
    /// </summary>
    internal interface IFileNameResolver
    {
        string ResolveFileName(FileEntry sourceEntry);
    }
}
