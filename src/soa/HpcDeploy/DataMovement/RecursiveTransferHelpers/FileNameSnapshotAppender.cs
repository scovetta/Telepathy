//------------------------------------------------------------------------------
// <copyright file="FileNameSnapshotAppender.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      File name resolver class for appending snapshot time to a file name.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System.Collections.Generic;

    /// <summary>
    /// File name resolver class for appending snapshot time to a file name.
    /// </summary>
    internal class FileNameSnapshotAppender : IFileNameResolver
    {
        private HashSet<string> fileNameSet = new HashSet<string>();

        public string ResolveFileName(FileEntry sourceEntry)
        {
            string uniqueFileName = FileNameResolver.ResolveFileNameConflict(
                Utils.AppendSnapShotToFileName(sourceEntry.RelativePath, sourceEntry.SnapshotTime),
                this.fileNameSet.Contains,
                delegate(string fileName, string extension, int count)
                {
                    return string.Format("{0} ({1}){2}", fileName, count, extension);
                });

            this.fileNameSet.Add(uniqueFileName);

            return uniqueFileName;
        }
    }
}
