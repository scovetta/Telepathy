//------------------------------------------------------------------------------
// <copyright file="FileNativeMethods.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Native methods for accessing files
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.Internal
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32.SafeHandles;

    internal static class FileNativeMethods
    {
        [Flags]
        public enum NativeFileAccess : uint
        {
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000
        }

        [Flags]
        public enum NativeFileShare : uint
        {
            FILE_SHARE_NONE = 0x0,
            FILE_SHARE_READ = 0x1,
            FILE_SHARE_WRITE = 0x2,
            FILE_SHARE_DELETE = 0x4,
        }

        public enum NativeFileMode : uint
        {
            CREATE_NEW = 1,
            CREATE_ALWAYS = 2,
            OPEN_EXISTING = 3,
            OPEN_ALWAYS = 4,
            TRUNCATE_EXSTING = 5
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReadFile(
            SafeFileHandle hFile,
            Byte[] aBuffer,
            UInt32 cbToRead,
            ref UInt32 cbThatWereRead,
            IntPtr pOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WriteFile(
            SafeFileHandle hFile,
            Byte[] aBuffer,
            UInt32 cbToWrite,
            ref UInt32 cbThatWereWritten,
            IntPtr pOverlapped);

        [DllImport("kernel32", SetLastError = true)]
        internal static extern Int32 CloseHandle(
            SafeFileHandle handle);

        // IO error codes
        internal const int ErrorFileExists = 0x50;
        internal const int ErrorDiskFull = 0x70;
        internal const int ErrorFileNotFound = 0x2;
        internal const int ErrorPathNotFound = 0x3;
        internal const int ErrorAccessDenied = 0x5;
        internal const int ErrorSharingViolation = 0x20;
        internal const int ErrorNetWorkPathNotFound = 0x33;
        internal const int ErrorNetworkPathNotFound2 = 0x35;
        internal const int ErrorNetworkBusy = 0x36;
        internal const int ErrorNetworkUnexpected = 0x3b;
        internal const int ErrorNetNameDeleted = 0x40;
        internal const int ErrorNetworkAccessDenied = 0x41;
        internal const int ErrorBadNetworkName = 0x43;
        internal const int ErrorNoSystemResource = 0x5aa;
        internal const int ErrorWorkingSetQuota = 0x5ad;
    }
}
