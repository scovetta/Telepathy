//------------------------------------------------------------------------------
// <copyright file="EnumerateDirectoryHelper.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Interop methods for enumerating files and directory.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.RecursiveTransferHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using Microsoft.Win32.SafeHandles;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;

    /// <summary>
    /// Interop methods for enumerating files and directory.
    /// </summary>
    internal static class EnumerateDirectoryHelper
    {
        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory that match the specified 
        /// search pattern, using a value to determine whether to search subdirectories.
        /// Folder permission will be checked for those folders containing found files.
        /// Difference with Directory.GetFiles/EnumerateFiles: Junctions and folders not accessible will be ignored.
        /// </summary>
        /// <param name="path">The directory to search. </param>
        /// <param name="searchPattern">The search string to match against the names of files in path. The parameter 
        /// cannot end in two periods ("..") or contain two periods ("..") followed by DirectorySeparatorChar or 
        /// AltDirectorySeparatorChar, nor can it contain any of the characters in InvalidPathChars. </param>
        /// <param name="searchOption">One of the values of the SearchOption enumeration that specifies whether 
        /// the search operation should include only the current directory or should include all subdirectories.
        /// The default value is TopDirectoryOnly.</param>
        /// <param name="cancellationTokenSource">CancellationTokenSource for AzureStorageLocation to register cancellation handler to.</param>
        /// <returns>An enumerable collection of file names in the directory specified by path and that match 
        /// searchPattern and searchOption.</returns>
        public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption, CancellationTokenSource cancellationTokenSource)
        {
            CancellationChecker cancellationChecker = new CancellationChecker();
            using (CancellationTokenRegistration tokenRegistration = cancellationTokenSource.Token.Register(cancellationChecker.Cancel))
            {
                if ((searchOption != SearchOption.TopDirectoryOnly) && (searchOption != SearchOption.AllDirectories))
                {
                    throw new ArgumentOutOfRangeException("searchOption");
                }

                // Remove whitespaces in the end.
                searchPattern = searchPattern.TrimEnd();
                if (searchPattern.Length == 0)
                {
                    // Returns an empty string collection.
                    return new List<string>();
                }

                // To support patterns like "folderA\" aiming at listing files under some folder.
                if ("." == searchPattern)
                {
                    searchPattern = "*";
                }

                cancellationChecker.CheckCancellation();
                CheckSearchPattern(searchPattern);

                cancellationChecker.CheckCancellation();

                // Check path permissions.
                string fullPath = Path.GetFullPath(path);
                CheckPathDiscoveryPermission(fullPath);

                string patternDirectory = Path.GetDirectoryName(searchPattern);
                if (!string.IsNullOrEmpty(patternDirectory))
                {
                    CheckPathDiscoveryPermission(Path.Combine(fullPath, patternDirectory));
                }

                string fullPathWithPattern = Path.Combine(fullPath, searchPattern);

                // To support patterns like "folderA\" aiming at listing files under some folder.
                char lastC = fullPathWithPattern[fullPathWithPattern.Length - 1];
                if (Path.DirectorySeparatorChar == lastC ||
                    Path.AltDirectorySeparatorChar == lastC ||
                    Path.VolumeSeparatorChar == lastC)
                {
                    fullPathWithPattern = fullPathWithPattern + '*';
                }

                string directoryName = AppendDirectorySeparator(Path.GetDirectoryName(fullPathWithPattern));
                string filePattern = fullPathWithPattern.Substring(directoryName.Length);

                if (!Directory.Exists(directoryName))
                {
                    throw new DirectoryNotFoundException(
                        string.Format("Could not find a part of the path '{0}'.", directoryName));
                }

                cancellationChecker.CheckCancellation();
                return InternalEnumerateFiles(directoryName, filePattern, searchOption, cancellationChecker);
            }
        }

        private static IEnumerable<string> InternalEnumerateFiles(string directoryName, string filePattern, SearchOption searchOption, CancellationChecker cancellationChecker)
        {
            Queue<string> folders = new Queue<string>();
            folders.Enqueue(directoryName);

            while (folders.Count > 0)
            {
                string folder = AppendDirectorySeparator(folders.Dequeue());

                cancellationChecker.CheckCancellation();

                try
                {
                    CheckPathDiscoveryPermission(folder);
                }
                catch (SecurityException)
                {
                    // Ignore this folder if we have no right to discovery it.
                    continue;
                }

                WIN32_FIND_DATA findFileData;

                // Load files directly under this folder.
                using (SafeFindHandle findHandle = FindFirstFile(folder + filePattern, out findFileData))
                {
                    if (!findHandle.IsInvalid)
                    {
                        do
                        {
                            cancellationChecker.CheckCancellation();

                            if (FileAttributes.Directory != (findFileData.FileAttributes & FileAttributes.Directory))
                            {
                                yield return Path.Combine(folder, findFileData.FileName);
                            }
                        }
                        while (FindNextFile(findHandle, out findFileData));
                    }
                }

                if (SearchOption.AllDirectories == searchOption)
                {
                    // Add sub-folders.
                    using (SafeFindHandle findHandle = FindFirstFile(folder + '*', out findFileData))
                    {
                        if (!findHandle.IsInvalid)
                        {
                            do
                            {
                                cancellationChecker.CheckCancellation();

                                if (FileAttributes.Directory == (findFileData.FileAttributes & FileAttributes.Directory) &&
                                    !findFileData.FileName.Equals(@".") &&
                                    !findFileData.FileName.Equals(@".."))
                                {
                                    // TODO: Ignore junction point or not. Make it configurable.
                                    if (FileAttributes.ReparsePoint != (findFileData.FileAttributes & FileAttributes.ReparsePoint))
                                    {
                                        folders.Enqueue(Path.Combine(folder, findFileData.FileName));
                                    }
                                }
                            }
                            while (FindNextFile(findHandle, out findFileData));
                        }
                    }
                }
            }
        }

        private static string AppendDirectorySeparator(string dir)
        {
            char lastC = dir[dir.Length - 1];
            if (Path.DirectorySeparatorChar != lastC && Path.AltDirectorySeparatorChar != lastC)
            {
                dir = dir + Path.DirectorySeparatorChar;
            }

            return dir;
        }

        private static void CheckSearchPattern(string searchPattern)
        {
            while (true)
            {
                int index = searchPattern.IndexOf("..", StringComparison.Ordinal);

                if (-1 == index)
                {
                    return;
                }

                index += 2;

                if (searchPattern.Length == index ||
                    searchPattern[index] == Path.DirectorySeparatorChar ||
                    searchPattern[index] == Path.AltDirectorySeparatorChar)
                {
                    throw new ArgumentException(
                        "Search pattern cannot contain \"..\" to move up directories" +
                        "and can be contained only internally in file/directory names, " +
                        "as in \"a..b\"");
                }

                searchPattern = searchPattern.Substring(index);
            }
        }

        [SuppressMessage("Microsoft.Security", 
            "CA2103:ReviewImperativeSecurity", 
            Justification = "Used to check if user has access to a particular folder so we can skip over folders to which the user does not have access.")]
        private static void CheckPathDiscoveryPermission(string dir)
        {
            string checkDir = AppendDirectorySeparator(dir) + '.';

            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, checkDir).Demand();
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFindHandle FindFirstFile(string fileName, out WIN32_FIND_DATA findFileData);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextFile(SafeHandle findFileHandle, out WIN32_FIND_DATA findFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(SafeHandle findFileHandle);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto), BestFitMapping(false)]
        private struct WIN32_FIND_DATA
        {
            public FileAttributes FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public int Reserved0;
            public int Reserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string FileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string AlternateFileName;
        }

        private sealed class SafeFindHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            [SecurityCritical]
            internal SafeFindHandle()
                : base(true)
            {
            }

            protected override bool ReleaseHandle()
            {
                if (!(IsInvalid || IsClosed))
                {
                    return FindClose(this);
                }

                return IsInvalid || IsClosed;
            }

            protected override void Dispose(bool disposing)
            {
                if (!(IsInvalid || IsClosed))
                {
                    FindClose(this);
                }

                base.Dispose(disposing);
            }
        }
    }
}
