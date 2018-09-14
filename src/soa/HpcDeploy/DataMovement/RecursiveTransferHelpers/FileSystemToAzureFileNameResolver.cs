//------------------------------------------------------------------------------
// <copyright file="FileSystemToAzureFileNameResolver.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      File name resolver class for translating Windows file names to Azure file names to.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    /// <summary>
    /// File name resolver class for translating Windows file names to Azure file names to.
    /// </summary>
    internal class FileSystemToAzureFileNameResolver : IFileNameResolver
    {
        public string ResolveFileName(FileEntry sourceEntry)
        {
            return sourceEntry.RelativePath.Replace('\\', '/');
        }
    }
}
