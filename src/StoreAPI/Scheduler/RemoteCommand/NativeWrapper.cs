using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.AccessControl;
using System.IO;

namespace Microsoft.Hpc.Scheduler
{
    class NativeWrapper
    {
        internal const int PIPE_ACCESS_INBOUND = 0x00000001;
        internal const int PIPE_ACCESS_OUTBOUND = 0x00000002;
        internal const int PIPE_ACCESS_DUPLEX = 0x00000003;

        internal const int PIPE_WAIT = 0x00000000;
        internal const int PIPE_NOWAIT = 0x00000001;
        internal const int PIPE_READMODE_BYTE = 0x00000000;
        internal const int PIPE_READMODE_MESSAGE = 0x00000002;
        internal const int PIPE_TYPE_BYTE = 0x00000000;
        internal const int PIPE_TYPE_MESSAGE = 0x00000004;
        internal const uint FILE_FLAG_WRITE_THROUGH = 0x80000000;


        internal const int PIPE_SINGLE_INSTANCES = 1;
        internal const int PIPE_UNLIMITED_INSTANCES = 255;
        internal const int FILE_FLAG_OVERLAPPED = 0x40000000;

        internal const int ERROR_IO_PENDING = 997;
        internal const int ERROR_PIPE_CONNECTED = 535;
        internal const int ERROR_FILE_NOT_FOUND = 2;

        [Flags]
        internal enum PipeAccessRights
        {
            // No None field - An ACE with the value 0 cannot grant nor deny.
            ReadData = 0x000001,
            WriteData = 0x000002,

            // Not that all client named pipes require ReadAttributes access even if the user does not specify it.
            // (This is because CreateFile slaps on the requirement before calling NTCreateFile (at least in WinXP SP2)).
            ReadAttributes = 0x000080,
            WriteAttributes = 0x000100,

            // These aren't really needed since there is no operation that requires this access, but they are left here
            // so that people can specify ACLs that others can open by specifying a PipeDirection rather than a
            // PipeAccessRights (PipeDirection.In/Out maps to GENERIC_READ/WRITE access).
            ReadExtendedAttributes = 0x000008,
            WriteExtendedAttributes = 0x000010,

            CreateNewInstance = 0x000004, // AppendData

            // Again, this is not needed but it should be here so that our FullControl matches windows.
            Delete = 0x010000,

            ReadPermissions = 0x020000,
            ChangePermissions = 0x040000,
            TakeOwnership = 0x080000,
            Synchronize = 0x100000,

            FullControl = ReadData | WriteData | ReadAttributes | ReadExtendedAttributes |
                                           WriteAttributes | WriteExtendedAttributes | CreateNewInstance |
                                           Delete | ReadPermissions | ChangePermissions | TakeOwnership |
                                           Synchronize,

            Read = ReadData | ReadAttributes | ReadExtendedAttributes | ReadPermissions,
            Write = WriteData | WriteAttributes | WriteExtendedAttributes, // | CreateNewInstance, For security, I really don't this CreateNewInstance belongs here.
            ReadWrite = Read | Write,

            // These are somewhat similar to what you get if you use PipeDirection:
            //In                           = ReadData | ReadAttributes | ReadExtendedAttributes | ReadPermissions,
            //Out                          = WriteData | WriteAttributes | WriteExtendedAttributes | ChangePermissions | CreateNewInstance | ReadAttributes, // NOTE: Not sure if ReadAttributes should really be here
            //InOut                        = In | Out,

            AccessSystemSecurity = 0x01000000, // Allow changes to SACL.
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class Overlapped
        {
            internal IntPtr InternalLow;
            internal IntPtr InternalHigh;
            internal int OffsetLow;
            internal int OffsetHigh;
            internal IntPtr EventHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_ATTRIBUTES
        {
            internal int nLength;
            internal IntPtr pSecurityDescriptor;
            internal int bInheritHandle;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern SafeFileHandle CreateNamedPipe(
            string name,
            uint openMode,
            int pipeMode,
            int maxInstances,
            int outBufSize,
            int inBufSize,
            int timeout,
            ref SECURITY_ATTRIBUTES lpPipeAttributes
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ConnectNamedPipe(
            SafeFileHandle hNamedPipe,
            Overlapped lpOverlapped);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FlushFileBuffers(SafeFileHandle hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DisconnectNamedPipe(SafeFileHandle hPipeHandle);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            string lpFileName,
            FileSystemRights dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr securityAttrs,
            FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool WaitNamedPipe(string lpPipeName, int timeout);
    }
}
