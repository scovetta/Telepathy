//------------------------------------------------------------------------------
// <copyright file="NativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">colinw</owner>
// <securityReview name="colinw" date="1-25-06"/>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc {

    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security.Permissions;
    using Microsoft.Win32.SafeHandles;

    [HostProtection(MayLeakOnAbort = true)]
    internal static partial class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetCurrentProcess();

        public const int SE_PRIVILEGE_ENABLED = 2;
        public const int SE_PRIVILEGE_REMOVED = 4;
        public const int SE_PRIVILEGE_DISABLED = 0;
        

        [DllImport("Advapi32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool AdjustTokenPrivileges(
            HandleRef TokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            TokenPrivileges NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength
        );

        [DllImport("Advapi32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPTStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPTStr)] string lpName, out LUID lpLuid);

        [DllImport("Advapi32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool OpenProcessToken(HandleRef ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        [DllImport("Kernel32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(HandleRef handle);
        [StructLayout(LayoutKind.Sequential)]
        public  struct LUID {
            public  int LowPart;
            public  int HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class TokenPrivileges {
            public  int PrivilegeCount = 1;
            public  LUID Luid;
            public  int Attributes;
        }
    }
}

