//------------------------------------------------------------------------------
// <copyright file="GlobalMemoryStatusNativeMethods.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Interop methods for access global memory information.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System.Runtime.InteropServices;

    internal class GlobalMemoryStatusNativeMethods
    {
        private MEMORYSTATUSEX memStatus;

        public GlobalMemoryStatusNativeMethods()
        {
            this.memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(this.memStatus))
            {
                this.TotalPhysicalMemory = this.memStatus.ullTotalPhys;
                this.AvailablePhysicalMemory = this.memStatus.ullAvailPhys;
            }
        }

        public ulong TotalPhysicalMemory
        {
            get;
            private set;
        }

        public ulong AvailablePhysicalMemory
        {
            get;
            private set;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }
    }
}
