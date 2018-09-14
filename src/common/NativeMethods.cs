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

    using BOOL = System.Int32;
    using DWORD = System.UInt32;
    using ULONG = System.UInt32;

    [HostProtection(MayLeakOnAbort = true)]
    internal static partial class NativeMethods
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        public const int TOKEN_TYPE_TokenPrimary = 1;
        public const int TOKEN_TYPE_TokenImpersonation = 2;

        public const int TOKEN_ALL_ACCESS   = 0x000f01ff;
        public const int TOKEN_EXECUTE      = 0x00020000;
        public const int TOKEN_READ         = 0x00020008;
        public const int TOKEN_IMPERSONATE  = 0x00000004;

        public const int PIPE_ACCESS_INBOUND = 0x00000001;
        public const int PIPE_ACCESS_OUTBOUND = 0x00000002;
        public const int PIPE_ACCESS_DUPLEX = 0x00000003;

        public const int PIPE_WAIT = 0x00000000;
        public const int PIPE_NOWAIT = 0x00000001;
        public const int PIPE_READMODE_BYTE = 0x00000000;
        public const int PIPE_READMODE_MESSAGE = 0x00000002;
        public const int PIPE_TYPE_BYTE = 0x00000000;
        public const int PIPE_TYPE_MESSAGE = 0x00000004;

        public const int PIPE_SINGLE_INSTANCES = 1;
        public const int PIPE_UNLIMITED_INSTANCES = 255;

        public const int FILE_FLAG_OVERLAPPED = 0x40000000;

        public const int STARTF_USESHOWWINDOW = 0x00000001;
        public const int STARTF_USESTDHANDLES = 0x00000100;

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        public class STARTUPINFO : IDisposable
        {
            internal int cb;
            internal IntPtr lpReserved = IntPtr.Zero;
            internal String lpDesktop;
            internal IntPtr lpTitle = IntPtr.Zero;
            internal int dwX;
            internal int dwY;
            internal int dwXSize;
            internal int dwYSize;
            internal int dwXCountChars;
            internal int dwYCountChars;
            internal int dwFillAttribute;
            internal int dwFlags;
            internal short wShowWindow;
            internal short cbReserved2;
            internal IntPtr lpReserved2 = IntPtr.Zero;
            internal SafeFileHandle hStdInput = new SafeFileHandle(IntPtr.Zero, false);
            internal SafeFileHandle hStdOutput = new SafeFileHandle(IntPtr.Zero, false);
            internal SafeFileHandle hStdError = new SafeFileHandle(IntPtr.Zero, false);
            bool disposed;

            public  STARTUPINFO()
            {
                cb = Marshal.SizeOf(this);
            }

            // Disposable types with unmanaged resources implement a finalizer.
            ~STARTUPINFO()
            {
                Dispose(false);
            }

            private void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    // Dispose of resources held by this instance.
                    // close the handles created for child process
                    if (hStdInput != null && !hStdInput.IsInvalid)
                    {
                        hStdInput.Close();
                        hStdInput = null;
                    }

                    SafeFileHandle savedhStdOutput = hStdOutput;

                    if (hStdOutput != null && !hStdOutput.IsInvalid)
                    {
                        hStdOutput.Close();
                        hStdOutput = null;
                    }

                    if (hStdError != null && !hStdError.IsInvalid)
                    {
                        //if the stdout and stderr handles are the same safe file handle
                        //do not close it twice
                        if (hStdError != savedhStdOutput)
                        {
                            hStdError.Close();
                        }
                        hStdError = null;
                    }

                    disposed = true;
                }
            }

            public void Dispose()
            {
                Dispose(true);

                // Suppress finalization of this disposed instance.
                GC.SuppressFinalize(this);
            }

        }

        [StructLayout(LayoutKind.Sequential)]
        public class PROCESS_INFORMATION : IDisposable
        {
            // The handles in PROCESS_INFORMATION are initialized in unmanaged functions.
            // We can't use SafeHandle here because Interop doesn't support [out] SafeHandles in structures/classes yet.

            // NOTE: IF YOU WANT TO USE THE HANDLES AFTER THE PROCESS_INFORMATION OBJECT HAS BEEN DISPOSED
            // EITHER DUPLICATE THE HANDLE OR ELSE ZERO OUT THE HANDLE YOU DO NOT WANT CLOSED AT DISPOSE TIME
            // THEN IT IS THE CREATOR'S RESPONSIBILITY TO CLOSE THE HANDLE PROPERLY
            internal IntPtr hProcess = IntPtr.Zero;
            internal IntPtr hThread = IntPtr.Zero;
            public int dwProcessId;
            public int dwThreadId;
            // bool disposed;

            // Note this will guarantee we will always free the handles
            // so unless you duplicate the handles from PROCESS_INFORMATION class
            // do not close those handles.
            ~PROCESS_INFORMATION()
            {
                Close();
            }

            public void Dispose()
            {
                Close();
                GC.SuppressFinalize(this);
            }

            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
            public void Close()
            {

                if (hProcess != (IntPtr)0 && hProcess != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                {
                    Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(this, hProcess));
                    hProcess = NativeMethods.INVALID_HANDLE_VALUE;
                }

                if (hThread != (IntPtr)0 && hThread != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                {
                    Microsoft.Win32.SafeNativeMethods.CloseHandle(new HandleRef(this, hThread));
                    hThread = NativeMethods.INVALID_HANDLE_VALUE;
                }
            }

        }

        public const int STILL_ACTIVE = 0x00000103;

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetExitCodeProcess(SafeProcessHandle processHandle, out int exitCode);

        public const int STD_INPUT_HANDLE = -10;
        public const int STD_OUTPUT_HANDLE = -11;
        public const int STD_ERROR_HANDLE = -12;

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TerminateProcess(SafeProcessHandle processHandle, int exitCode);


        public const int CTRL_C_EVENT = 0;
        public const int CTRL_BREAK_EVENT = 1;

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GenerateConsoleCtrlEvent(int dwCtrlEvent, int dwProcessGroupId);

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeWaitHandle CreateEvent(SECURITY_ATTRIBUTES lpSecurityAttributes, bool isManualReset, bool initialState, string name);

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetEvent(SafeWaitHandle handle);

        //public readonly static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool CreateProcessAsUser(
            HandleRef hToken,
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            [MarshalAs(UnmanagedType.Bool)]bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true, BestFitMapping = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            [MarshalAs(UnmanagedType.Bool)]bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            STARTUPINFO lpStartupInfo,
            PROCESS_INFORMATION lpProcessInformation
        );

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public static extern SafeProcessHandle OpenProcess(int access, [MarshalAs(UnmanagedType.Bool)]bool inherit, int processId);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public static extern int ResumeThread(SafeThreadHandle handle);

        public const int DUPLICATE_CLOSE_SOURCE = 1;
        public const int DUPLICATE_SAME_ACCESS  = 2;

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true, BestFitMapping=false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            HandleRef hTargetProcess,
            out SafeWaitHandle targetHandle,
            int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)]bool bInheritHandle,
            int dwOptions
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Ansi, SetLastError = true, BestFitMapping = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateHandle(
            HandleRef hSourceProcessHandle,
            SafeHandle hSourceHandle,
            SafeProcessHandle hTargetProcess,
            out IntPtr targetHandle,
            int dwDesiredAccess,
            [MarshalAs(UnmanagedType.Bool)]bool bInheritHandle,
            int dwOptions
        );

        [DllImport(ExternDll.Userenv, CharSet=CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateEnvironmentBlock(out IntPtr env, HandleRef hToken, [MarshalAs(UnmanagedType.Bool)]bool fInherit);

        [DllImport(ExternDll.Userenv, CharSet=CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyEnvironmentBlock(IntPtr env);

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            IntPtr lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out SafeTokenHandle TokenHandle
        );

        public enum TokenInformationClass : int
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }
/*
  public struct TOKEN_ELEVATION
        {
            public UInt32 TokenIsElevated;
        }
*/
        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull = 2,
            TokenElevationTypeLimited = 3
        }
/*
        public struct TOKEN_LINKED_TOKEN
        {
            public IntPtr LinkedToken;
        }
*/
        [DllImport(ExternDll.Advapi32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetTokenInformation(
            IntPtr TokenHandle,
            TokenInformationClass TokenInformationClass,
            IntPtr TokenInformation,
            uint TokenInformationLength,
            out uint ReturnLength);

        [Flags]
        public enum TokenAccessLevels
        {
            SameAccessLevels = 0,
            AssignPrimary = 0x00000001,
            Duplicate = 0x00000002,
            Impersonate = 0x00000004,
            Query = 0x00000008,
            QuerySource = 0x00000010,
            AdjustPrivileges = 0x00000020,
            AdjustGroups = 0x00000040,
            AdjustDefault = 0x00000080,
            AdjustSessionId = 0x00000100,

            Read = 0x00020000 | Query,

            Write = 0x00020000 | AdjustPrivileges | AdjustGroups | AdjustDefault,

            AllAccess = 0x000F0000 |
                AssignPrimary |
                Duplicate |
                Impersonate |
                Query |
                QuerySource |
                AdjustPrivileges |
                AdjustGroups |
                AdjustDefault |
                AdjustSessionId,

            MaximumAllowed = 0x02000000
        }

        public enum SecurityImpersonationLevel
        {
            Anonymous = 0,
            Identification = 1,
            Impersonation = 2,
            Delegation = 3,
        }

        public enum TokenType
        {
            Primary = 1,
            Impersonation = 2,
        }

        [DllImport(ExternDll.Advapi32, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DuplicateTokenEx(
            [In]    SafeTokenHandle ExistingToken,
            [In]    TokenAccessLevels DesiredAccess,
            [In]    IntPtr TokenAttributes,
            [In]    SecurityImpersonationLevel ImpersonationLevel,
            [In]    TokenType TokenType,
            [In, Out] ref SafeTokenHandle NewToken);

        public const int LMEM_FIXED = 0x0000;
        public const int LMEM_ZEROINIT = 0x0040;
        public const int LPTR = (LMEM_FIXED | LMEM_ZEROINIT);
/*
        [DllImport(ExternDll.Kernel32, CharSet = CharSet.Auto, SetLastError = true)]
        public static extern
        SafeLocalMemHandle LocalAlloc(
            [In] int uFlags,
            [In] IntPtr sizetdwBytes);
*/
        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public struct PROFILEINFO
        {
            public int dwSize;
            public int dwFlags;
            public String lpUserName;
            public String lpProfilePath;
            public String lpDefaultPath;
            public String lpServerName;
            public String lpPolicyPath;
            internal IntPtr hProfile;
        }
        public const int PI_NOUI          = 0x00000001;
        public const int PI_APPLYPOLICY   = 0x00000002;

        [DllImport(ExternDll.Userenv, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool LoadUserProfile(
            SafeTokenHandle hToken,
            ref PROFILEINFO lpProfileInfo);

        [DllImport(ExternDll.Userenv, SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnloadUserProfile(
            SafeTokenHandle hToken,
            IntPtr hProfile);


        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        public const int NtPerfCounterSizeDword = 0x00000000;
        public const int NtPerfCounterSizeLarge = 0x00000100;

        public const int SHGFI_USEFILEATTRIBUTES = 0x000000010;  // use passed dwFileAttribute
        public const int SHGFI_TYPENAME = 0x000000400;

        public const int NtQueryProcessBasicInfo = 0;
        public const int NtQuerySystemProcessInformation = 5;

        public const int SEE_MASK_CLASSNAME = 0x00000001;    // Note CLASSKEY overrides CLASSNAME
        public const int SEE_MASK_CLASSKEY = 0x00000003;
        public const int SEE_MASK_IDLIST = 0x00000004;    // Note INVOKEIDLIST overrides IDLIST
        public const int SEE_MASK_INVOKEIDLIST = 0x0000000c;
        public const int SEE_MASK_ICON = 0x00000010;
        public const int SEE_MASK_HOTKEY = 0x00000020;
        public const int SEE_MASK_NOCLOSEPROCESS = 0x00000040;
        public const int SEE_MASK_CONNECTNETDRV = 0x00000080;
        public const int SEE_MASK_FLAG_DDEWAIT = 0x00000100;
        public const int SEE_MASK_DOENVSUBST = 0x00000200;
        public const int SEE_MASK_FLAG_NO_UI = 0x00000400;
        public const int SEE_MASK_UNICODE = 0x00004000;
        public const int SEE_MASK_NO_CONSOLE = 0x00008000;
        public const int SEE_MASK_ASYNCOK = 0x00100000;

        public const int TH32CS_SNAPHEAPLIST = 0x00000001;
        public const int TH32CS_SNAPPROCESS = 0x00000002;
        public const int TH32CS_SNAPTHREAD = 0x00000004;
        public const int TH32CS_SNAPMODULE = 0x00000008;
        public const int TH32CS_INHERIT = unchecked((int)0x80000000);


        public const int PROCESS_TERMINATE = 0x0001;
        public const int PROCESS_CREATE_THREAD = 0x0002;
        public const int PROCESS_SET_SESSIONID = 0x0004;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_VM_READ = 0x0010;
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_DUP_HANDLE = 0x0040;
        public const int PROCESS_CREATE_PROCESS = 0x0080;
        public const int PROCESS_SET_QUOTA = 0x0100;
        public const int PROCESS_SET_INFORMATION = 0x0200;
        public const int PROCESS_QUERY_INFORMATION = 0x0400;
        public const int PROCESS_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0xFFF;


        public const int THREAD_TERMINATE = 0x0001;
        public const int THREAD_SUSPEND_RESUME = 0x0002;
        public const int THREAD_GET_CONTEXT = 0x0008;
        public const int THREAD_SET_CONTEXT = 0x0010;
        public const int THREAD_SET_INFORMATION = 0x0020;
        public const int THREAD_QUERY_INFORMATION = 0x0040;
        public const int THREAD_SET_THREAD_TOKEN = 0x0080;
        public const int THREAD_IMPERSONATE = 0x0100;
        public const int THREAD_DIRECT_IMPERSONATION = 0x0200;

        //public static readonly IntPtr HKEY_LOCAL_MACHINE = unchecked((IntPtr)(int)0x80000002);
        public const int REG_BINARY = 3;
        public const int REG_MULTI_SZ = 7;

        public const int KEY_QUERY_VALUE        = 0x0001;
        public const int KEY_ENUMERATE_SUB_KEYS = 0x0008;
        public const int KEY_NOTIFY             = 0x0010;

        public const int KEY_READ               =((STANDARD_RIGHTS_READ |
                                                           KEY_QUERY_VALUE |
                                                           KEY_ENUMERATE_SUB_KEYS |
                                                           KEY_NOTIFY)
                                                          &
                                                          (~SYNCHRONIZE));
        public const int ERROR_SUCCESS = 0;
        public const int ERROR_FILE_NOT_FOUND = 2;
        public const int ERROR_PATH_NOT_FOUND = 3;
        public const int ERROR_ACCESS_DENIED = 5;
        public const int ERROR_INVALID_HANDLE = 6;
        public const int ERROR_NOT_ENOUGH_MEMORY = 8;
        public const int ERROR_BAD_LENGTH = 24;
        public const int ERROR_SHARING_VIOLATION = 32;
        public const int ERROR_HANDLE_EOF = 38;
        public const int ERROR_FILE_EXISTS = 80;
        public const int ERROR_INVALID_PARAMETER = 87;
        public const int ERROR_BROKEN_PIPE = 109;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const int ERROR_ALREADY_EXISTS = 183;
        public const int ERROR_FILENAME_EXCED_RANGE = 206;  // filename too long.
        public const int ERROR_NO_DATA = 232;
        public const int ERROR_MORE_DATA = 234;
        public const int ERROR_PARTIAL_COPY = 299;
        public const int ERROR_MR_MID_NOT_FOUND = 317;
        public const int ERROR_OPERATION_ABORTED = 995;
        public const int ERROR_IO_INCOMPLETE = 996;
        public const int ERROR_IO_PENDING = 997;
        public const int ERROR_NO_TOKEN = 1008;
        public const int ERROR_COUNTER_TIMEOUT = 1121;
        public const int ERROR_CANCELLED = 1223;
        public const int ERROR_NO_ASSOCIATION = 1155;
        public const int ERROR_DDE_FAIL = 1156;
        public const int ERROR_DLL_NOT_FOUND = 1157;
        public const int ERROR_NO_SUCH_LOGON_SESSION = 1312;
        public const int RPC_S_SERVER_UNAVAILABLE = 1722;
        public const int RPC_S_CALL_FAILED = 1726;


        public const int PDH_NO_DATA = unchecked((int) 0x800007D5);
        public const int PDH_CALC_NEGATIVE_DENOMINATOR = unchecked((int) 0x800007D6);
        public const int PDH_CALC_NEGATIVE_VALUE = unchecked((int) 0x800007D8);


        public const int SE_ERR_FNF = 2;
        public const int SE_ERR_PNF = 3;
        public const int SE_ERR_ACCESSDENIED = 5;
        public const int SE_ERR_OOM = 8;
        public const int SE_ERR_DLLNOTFOUND = 32;
        public const int SE_ERR_SHARE = 26;
        public const int SE_ERR_ASSOCINCOMPLETE = 27;
        public const int SE_ERR_DDETIMEOUT = 28;
        public const int SE_ERR_DDEFAIL = 29;
        public const int SE_ERR_DDEBUSY = 30;
        public const int SE_ERR_NOASSOC = 31;

        //public const int SE_PRIVILEGE_ENABLED = 2;
        //public const int SE_PRIVILEGE_DISABLED = 4;

        public const int LOGON32_PROVIDER_DEFAULT = 0;
        public const int LOGON32_LOGON_INTERACTIVE = 2;
        public const int LOGON32_LOGON_NETWORK = 3;
        public const int LOGON32_LOGON_BATCH = 4;
        public const int LOGON32_LOGON_SERVICE = 5;
        public const int LOGON32_LOGON_UNLOCK = 7;
        public const int LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
        public const int LOGON32_LOGON_NEW_CREDENTIALS = 9;

        public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
        public const int TOKEN_QUERY = 0x08;

        public const int DEBUG_PROCESS                  = 0x00000001;
        public const int DEBUG_ONLY_THIS_PROCESS        = 0x00000002;
        public const int CREATE_SUSPENDED               = 0x00000004;
        public const int DETACHED_PROCESS               = 0x00000008;
        public const int CREATE_NEW_CONSOLE             = 0x00000010;
        public const int NORMAL_PRIORITY_CLASS          = 0x00000020;
        public const int IDLE_PRIORITY_CLASS            = 0x00000040;
        public const int HIGH_PRIORITY_CLASS            = 0x00000080;
        public const int REALTIME_PRIORITY_CLASS        = 0x00000100;
        public const int CREATE_NEW_PROCESS_GROUP       = 0x00000200;
        public const int CREATE_UNICODE_ENVIRONMENT     = 0x00000400;
        public const int CREATE_SEPARATE_WOW_VDM        = 0x00000800;
        public const int CREATE_SHARED_WOW_VDM          = 0x00001000;
        public const int CREATE_FORCEDOS                = 0x00002000;
        public const int BELOW_NORMAL_PRIORITY_CLASS    = 0x00004000;
        public const int ABOVE_NORMAL_PRIORITY_CLASS    = 0x00008000;
        public const int CREATE_BREAKAWAY_FROM_JOB      = 0x01000000;
        public const int CREATE_DEFAULT_ERROR_MODE      = 0x04000000;
        public const int CREATE_NO_WINDOW               = 0x08000000;
        public const int PROFILE_USER                   = 0x10000000;
        public const int PROFILE_KERNEL                 = 0x20000000;
        public const int PROFILE_SERVER                 = 0x40000000;

        //
        // Desktops and Workstations
        //

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public extern static SafeDesktopHandle CreateDesktop(
            string lpszDesktop,
            IntPtr lpszDevice,
            IntPtr pDevmode,
            uint dwFlags,
            uint dwDesiredAccess,
            IntPtr lpsa
        );

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool CloseDesktop(
            IntPtr hDesktop
        );

        //
        // access types
        //
        public const int  DELETE                           = 0x00010000;
        public const int  READ_CONTROL                     = 0x00020000;
        public const int  WRITE_DAC                        = 0x00040000;
        public const int  WRITE_OWNER                      = 0x00080000;
        public const int  SYNCHRONIZE                      = 0x00100000;

        public const int  STANDARD_RIGHTS_REQUIRED         = 0x000F0000;

        public const int  STANDARD_RIGHTS_READ             = READ_CONTROL;
        public const int  STANDARD_RIGHTS_WRITE            = READ_CONTROL;
        public const int  STANDARD_RIGHTS_EXECUTE          = READ_CONTROL;

        public const int  STANDARD_RIGHTS_ALL              = 0x001F0000;

        public const int  SPECIFIC_RIGHTS_ALL              = 0x0000FFFF;

        //
        // AccessSystemAcl access type
        //

        public const int  ACCESS_SYSTEM_SECURITY           = 0x01000000;

        //
        // MaximumAllowed access type
        //

        public const int  MAXIMUM_ALLOWED                  = 0x02000000;

        //
        //  These are the generic rights.
        //

        public const uint  GENERIC_READ                     = 0x80000000;
        public const uint  GENERIC_WRITE                    = 0x40000000;
        public const uint  GENERIC_EXECUTE                  = 0x20000000;
        public const uint  GENERIC_ALL                      = 0x10000000;
        public const uint EVENT_ALL_ACCESS                  = 0x001F0003;

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public extern static SafeWindowStationHandle CreateWindowStation(
            string pwinsta,
            uint dwReserved,
            uint dwDesiredAccess,
            IntPtr lpsa
        );

        [DllImport(ExternDll.User32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool CloseWindowStation(
            IntPtr hWinsta
        );

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool SetProcessWindowStation(
            IntPtr hWinSta
        );

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool SetProcessWindowStation(
            SafeWindowStationHandle hWinSta
        );

        // http://windowssdk.msdn.microsoft.com/library/en-us/dllproc/base/getprocesswindowstation.asp states
        // handle returned should not be closed so don't use SafeHandle!
        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public extern static IntPtr GetProcessWindowStation();

        //
        // Define all access to windows objects
        // From windows\gina\winlogon\secutil.c
        //

        public const int DESKTOP_ALL = DESKTOP_READOBJECTS | DESKTOP_CREATEWINDOW |
                     DESKTOP_CREATEMENU      | DESKTOP_HOOKCONTROL      |
                     DESKTOP_JOURNALRECORD   | DESKTOP_JOURNALPLAYBACK  |
                     DESKTOP_ENUMERATE       | DESKTOP_WRITEOBJECTS     |
                     DESKTOP_SWITCHDESKTOP   | STANDARD_RIGHTS_REQUIRED;

        public const int  DESKTOP_READOBJECTS        = 0x00000001;
        public const int  DESKTOP_CREATEWINDOW       = 0x00000002;
        public const int  DESKTOP_CREATEMENU         = 0x00000004;
        public const int  DESKTOP_HOOKCONTROL        = 0x00000008;
        public const int  DESKTOP_JOURNALRECORD      = 0x00000010;
        public const int  DESKTOP_JOURNALPLAYBACK    = 0x00000020;
        public const int  DESKTOP_ENUMERATE          = 0x00000040;
        public const int  DESKTOP_WRITEOBJECTS       = 0x00000080;
        public const int  DESKTOP_SWITCHDESKTOP      = 0x00000100;


        public const int WINSTA_ALL  = WINSTA_ENUMDESKTOPS     | WINSTA_READATTRIBUTES    |
                     WINSTA_ACCESSCLIPBOARD  | WINSTA_CREATEDESKTOP     |
                     WINSTA_WRITEATTRIBUTES  | WINSTA_ACCESSGLOBALATOMS |
                     WINSTA_EXITWINDOWS      | WINSTA_ENUMERATE         |
                     WINSTA_READSCREEN       |
                     STANDARD_RIGHTS_REQUIRED;

        public const int  WINSTA_ENUMDESKTOPS         = 0x0001;
        public const int  WINSTA_READATTRIBUTES       = 0x0002;
        public const int  WINSTA_ACCESSCLIPBOARD      = 0x0004;
        public const int  WINSTA_CREATEDESKTOP        = 0x0008;
        public const int  WINSTA_WRITEATTRIBUTES      = 0x0010;
        public const int  WINSTA_ACCESSGLOBALATOMS    = 0x0020;
        public const int  WINSTA_EXITWINDOWS          = 0x0040;
        public const int  WINSTA_ENUMERATE            = 0x0100;
        public const int  WINSTA_READSCREEN           = 0x0200;


        public const int SE_WINDOW_OBJECT = 7;

        public const int OWNER_SECURITY_INFORMATION     = 0x00000001;
        public const int GROUP_SECURITY_INFORMATION     = 0x00000002;
        public const int DACL_SECURITY_INFORMATION      = 0x00000004;
        public const int SACL_SECURITY_INFORMATION      = 0x00000008;

        [DllImport(ExternDll.Advapi32, EntryPoint = "GetSecurityInfo", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint GetSecurityInfoByHandle(
            SafeHandle handle,
            uint objectType,
            uint securityInformation,
            out IntPtr sidOwner,
            out IntPtr sidGroup,
            out IntPtr dacl,
            out IntPtr sacl,
            out IntPtr securityDescriptor);

        [DllImport(ExternDll.Advapi32,EntryPoint = "GetSecurityDescriptorLength", CallingConvention = CallingConvention.Winapi, SetLastError = false, CharSet = CharSet.Unicode)]
        public static extern DWORD GetSecurityDescriptorLength(
            IntPtr byteArray);

        [DllImport(ExternDll.Kernel32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static extern IntPtr LocalFree(IntPtr handle);

        [DllImport(ExternDll.Advapi32, EntryPoint = "SetSecurityInfo", CallingConvention = CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern DWORD SetSecurityInfoByHandle(
            SafeHandle handle,
            DWORD objectType,
            DWORD securityInformation,
            byte[] owner,
            byte[] group,
            byte[] dacl,
            byte[] sacl);

        //
        // Job Objects
        //
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public extern static SafeJobHandle CreateJobObject(
            IntPtr lpJobAttributes,
            string lpName
        );

        public const int JOB_OBJECT_ASSIGN_PROCESS          = 0x0001;
        public const int JOB_OBJECT_SET_ATTRIBUTES          = 0x0002;
        public const int JOB_OBJECT_QUERY                   = 0x0004;
        public const int JOB_OBJECT_TERMINATE               = 0x0008;
        public const int JOB_OBJECT_SET_SECURITY_ATTRIBUTES = 0x0010;
        public const int JOB_OBJECT_ALL_ACCESS              = (STANDARD_RIGHTS_REQUIRED | SYNCHRONIZE | 0x1F);

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool AssignProcessToJobObject(
            SafeJobHandle hJob,
            SafeProcessHandle hProcess
        );

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool TerminateJobObject(
            SafeJobHandle hJob,
            int uExitCode
        );

        //
        // Basic Limits
        //
        public const int JOB_OBJECT_LIMIT_WORKINGSET = 0x00000001;
        public const int JOB_OBJECT_LIMIT_PROCESS_TIME = 0x00000002;
        public const int JOB_OBJECT_LIMIT_JOB_TIME = 0x00000004;
        public const int JOB_OBJECT_LIMIT_ACTIVE_PROCESS = 0x00000008;
        public const int JOB_OBJECT_LIMIT_AFFINITY = 0x00000010;
        public const int JOB_OBJECT_LIMIT_PRIORITY_CLASS = 0x00000020;
        public const int JOB_OBJECT_LIMIT_PRESERVE_JOB_TIME = 0x00000040;
        public const int JOB_OBJECT_LIMIT_SCHEDULING_CLASS = 0x00000080;
        public const int JOB_OBJECT_LIMIT_SUBSET_AFFINITY = 0x00004000;

        //
        // Extended Limits
        //
        public const int JOB_OBJECT_LIMIT_PROCESS_MEMORY = 0x00000100;
        public const int JOB_OBJECT_LIMIT_JOB_MEMORY = 0x00000200;
        public const int JOB_OBJECT_LIMIT_DIE_ON_UNHANDLED_EXCEPTION = 0x00000400;
        public const int JOB_OBJECT_LIMIT_BREAKAWAY_OK = 0x00000800;
        public const int JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK = 0x00001000;
        public const int JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000;

        public const int JOB_OBJECT_LIMIT_RESERVED2 = 0x00004000;
        public const int JOB_OBJECT_LIMIT_RESERVED3 = 0x00008000;
        public const int JOB_OBJECT_LIMIT_RESERVED4 = 0x00010000;
        public const int JOB_OBJECT_LIMIT_RESERVED5 = 0x00020000;
        public const int JOB_OBJECT_LIMIT_RESERVED6 = 0x00040000;


        public const int JOB_OBJECT_LIMIT_VALID_FLAGS = 0x0007ffff;

        public const int JOB_OBJECT_BASIC_LIMIT_VALID_FLAGS = 0x000000ff;
        public const int JOB_OBJECT_EXTENDED_LIMIT_VALID_FLAGS = 0x00003fff;
        public const int JOB_OBJECT_RESERVED_LIMIT_VALID_FLAGS = 0x0007ffff;
        public const int JobObjectBasicAccountingInformation = 1;

        [StructLayout(LayoutKind.Sequential)]
        public struct JobObjectBasicInformation
        {
            public Int64 TotalUserTime;
            public Int64 TotalKernelTime;
            public Int64 ThisPeriodTotalUserTime;
            public Int64 ThisPeriodTotalKernelTime;
            public UInt32 TotalPageFaultCount;
            public UInt32 TotalProcesses;
            public UInt32 ActiveProcesses;
            public UInt32 TotalTerminatedProcesses;
        }

        public const int JobObjectBasicAndIoAccountingInformation = 8;
        [StructLayout(LayoutKind.Sequential)]
        public struct JobObjectBasicAndIoInformation
        {
            public Int64 TotalUserTime;
            public Int64 TotalKernelTime;
            public Int64 ThisPeriodTotalUserTime;
            public Int64 ThisPeriodTotalKernelTime;
            public UInt32 TotalPageFaultCount;
            public UInt32 TotalProcesses;
            public UInt32 ActiveProcesses;
            public UInt32 TotalTerminatedProcesses;
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
        }

        public const int JobObjectExtendedLimitInformationQuery = 9;
        public const int JobObjectExtendedLimitInformationSet = 9;
        [StructLayout(LayoutKind.Sequential)]
        public struct JobObjectExtendedLimitInformation
        {
            public Int64 PerProcessUserTimeLimit;
            public Int64 PerJobUserTimeLimit;
            public UInt32 LimitFlags;
            internal UIntPtr MinimumWorkingSetSize;
            internal UIntPtr MaximumWorkingSetSize;
            public UInt32 ActiveProcessLimit;
            internal IntPtr Affinity;
            public UInt32 PriorityClass;
            public UInt32 SchedulingClass;
            public UInt64 ReadOperationCount;
            public UInt64 WriteOperationCount;
            public UInt64 OtherOperationCount;
            public UInt64 ReadTransferCount;
            public UInt64 WriteTransferCount;
            public UInt64 OtherTransferCount;
            internal UIntPtr ProcessMemoryLimit;
            internal UIntPtr JobMemoryLimit;
            internal UIntPtr PeakProcessMemoryUsed;
            internal UIntPtr PeakJobMemoryUsed;
        }

        public const int QueryJobObjectBasicProcessIdList = 3;
        [StructLayout(LayoutKind.Sequential)]
        public struct JobObjectBasicProcessIdListHeader
        {
            public UInt32 NumberOfAssignedProcesses;
            public UInt32 NumberOfProcessIdsInList;
        }

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool QueryInformationJobObject(
            SafeJobHandle hJob,
            int query,
            out JobObjectBasicInformation info,
            int size,
            out int returnedSize
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool QueryInformationJobObject(
            SafeJobHandle hJob,
            int query,
            out JobObjectExtendedLimitInformation info,
            int size,
            out int returnedSize
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool QueryInformationJobObject(
            SafeJobHandle hJob,
            int query,
            [MarshalAs(UnmanagedType.LPArray)]IntPtr[] info,
            int size,
            out int returnedSize
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool SetInformationJobObject(
            SafeJobHandle hJob,
            int informationClass,
            ref JobObjectExtendedLimitInformation info,
            int size
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreatePipe(
            out SafeFileHandle hReadPipe,
            out SafeFileHandle hWritePipe,
            SECURITY_ATTRIBUTES sa,
            int nSize
        );

        [Flags]
        public enum PipeAccessRights
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
        public class SECURITY_ATTRIBUTES
        {
            public int nLength = 12;
            public SafeLocalMemHandle lpSecurityDescriptor = new SafeLocalMemHandle(IntPtr.Zero, false);
            public bool bInheritHandle; // = false;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetHandleInformation(
            SafeHandle hObject,
            uint dwMask,
            uint dwFlags
        );

        public const uint  HANDLE_FLAG_INHERIT             = 0x00000001;
        public const uint  HANDLE_FLAG_PROTECT_FROM_CLOSE  = 0x00000002;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr securityAttrs,
            FileMode dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        public const int FILE_READ_DATA = 1;
        public const int FILE_WRITE_DATA = 2;

        [Serializable, ComVisible(true), Flags]
        public enum FileShare
        {
            // Fields
            Delete = 4,
            Inheritable = 0x10,
            None = 0,
            Read = 1,
            ReadWrite = 3,
            Write = 2
        }

        [Serializable, ComVisible(true)]
        public enum FileMode
        {
            // Fields
            Append = 6,
            Create = 2,
            CreateNew = 1,
            Open = 3,
            OpenOrCreate = 4,
            Truncate = 5
        }



        //
        // COM Impersonation
        //
        [DllImport(ExternDll.Ole32, CharSet=CharSet.Unicode, SetLastError=false, PreserveSig=false)]
        public static extern void CoImpersonateClient();

        [DllImport(ExternDll.Ole32, CharSet=CharSet.Unicode, SetLastError=false, PreserveSig=false)]
        public static extern void CoRevertToSelf();

        [DllImport(ExternDll.Rpcrt4, SetLastError=false, PreserveSig=false)]
        public static extern int RpcImpersonateClient(IntPtr rpcContext);

        [DllImport(ExternDll.Rpcrt4, SetLastError=false, PreserveSig=false)]
        public static extern int RpcRevertToSelf();

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern ErrorModes SetErrorMode(ErrorModes uMode);

#if NEVER
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Ansi, SetLastError=true)]
        public  static extern IntPtr GetStdHandle(int whichHandle);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern int GetThreadPriority(SafeThreadHandle handle);
        [DllImport(ExternDll.Ntdll, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public  static extern int NtQuerySystemInformation(int query, IntPtr dataPtr, int size, out int returnedSize);
        [DllImport(ExternDll.Ntdll, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public  static extern int NtQueryInformationProcess(SafeProcessHandle processHandle, int query, NtProcessBasicInfo info, int size, int[] returnedSize);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern SafeThreadHandle OpenThread(int access, bool inherit, int threadId);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetExitCodeProcess(HandleRef processHandle, out int exitCode);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetProcessTimes(SafeProcessHandle handle, out long creation, out long exit, out long kernel, out long user);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto)]
        public  static extern int GetCurrentProcessId();
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetPriorityClass(SafeProcessHandle handle, int priorityClass);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern int GetPriorityClass(SafeProcessHandle handle);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetProcessAffinityMask(SafeProcessHandle handle, out IntPtr processMask, out IntPtr systemMask);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetThreadPriorityBoost(SafeThreadHandle handle, out bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetProcessPriorityBoost(SafeProcessHandle handle, out bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool GetProcessWorkingSetSize(SafeProcessHandle handle, out IntPtr min, out IntPtr max);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetProcessAffinityMask(SafeProcessHandle handle, IntPtr mask);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetProcessPriorityBoost(SafeProcessHandle handle, bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetProcessWorkingSetSize(SafeProcessHandle handle, IntPtr min, IntPtr max);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern int SetThreadIdealProcessor(SafeThreadHandle handle, int processor);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetThreadPriority(SafeThreadHandle handle, int priority);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool SetThreadPriorityBoost(SafeThreadHandle handle, bool disabled);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern IntPtr SetThreadAffinityMask(SafeThreadHandle handle, HandleRef mask);
        [DllImport(ExternDll.Psapi, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern bool EnumProcesses(int[] processIds, int size, out int needed);
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern int GetConsoleCP();
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  static extern int GetConsoleOutputCP();
#endif
        public const UInt32 INFINITE = 0xFFFFFFFF;
        public const UInt32 WAIT_ABANDONED = 0x00000080;
        public const UInt32 WAIT_OBJECT_0 = 0x00000000;
        public const UInt32 WAIT_TIMEOUT = 0x00000102;

        [DllImport(ExternDll.Kernel32, ExactSpelling = true, SetLastError = true)]
        public static extern int WaitForSingleObject(HandleRef handle, UInt32 timeout);

        [DllImport(ExternDll.Kernel32, ExactSpelling = true, SetLastError = true)]
        public extern static int WaitForSingleObject(IntPtr handle, UInt32 milliseconds);
#if NEVER
        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        public  static extern SafeFileHandle CreateNamedPipe(
            string name,
            int openMode,
            int pipeMode,
            int maxInstances,
            int outBufSize,
            int inBufSize,
            int timeout,
            IntPtr lpPipeAttributes
        );

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool AdjustTokenPrivileges(
            HandleRef TokenHandle,
            [MarshalAs(UnmanagedType.Bool)]bool DisableAllPrivileges,
            TokenPrivileges NewState,
            int BufferLength,
            IntPtr PreviousState,
            IntPtr ReturnLength
        );

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true, BestFitMapping=false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool LookupPrivilegeValue([MarshalAs(UnmanagedType.LPTStr)] string lpSystemName, [MarshalAs(UnmanagedType.LPTStr)] string lpName, out LUID lpLuid);

        [DllImport(ExternDll.Advapi32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public  static extern bool OpenProcessToken(HandleRef ProcessHandle, int DesiredAccess, out IntPtr TokenHandle);

        [DllImport(ExternDll.Kernel32, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(HandleRef handle);

        [StructLayout(LayoutKind.Sequential)]
        public class NtModuleInfo {
            public  IntPtr BaseOfDll = IntPtr.Zero;
            public  int SizeOfImage;
            public  IntPtr EntryPoint = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class WinProcessEntry {
            public  int dwSize;
            public  int cntUsage;
            public  int th32ProcessID;
            public  IntPtr th32DefaultHeapID = IntPtr.Zero;
            public  int th32ModuleID;
            public  int cntThreads;
            public  int th32ParentProcessID;
            public  int pcPriClassBase;
            public  int dwFlags;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            //public  string fileName;
            //byte fileName[260];
            public  const int sizeofFileName = 260;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class WinThreadEntry {
            public  int dwSize;
            public  int cntUsage;
            public  int th32ThreadID;
            public  int th32OwnerProcessID;
            public  int tpBasePri;
            public  int tpDeltaPri;
            public  int dwFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class WinModuleEntry {  // MODULEENTRY32
            public  int dwSize;
            public  int th32ModuleID;
            public  int th32ProcessID;
            public  int GlblcntUsage;
            public  int ProccntUsage;
            public  IntPtr modBaseAddr = IntPtr.Zero;
            public  int modBaseSize;
            public  IntPtr hModule = IntPtr.Zero;
            //byte moduleName[256];
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=256)]
            //public  string moduleName;
            //[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            //public  string fileName;
            //byte fileName[260];
            public  const int sizeofModuleName = 256;
            public  const int sizeofFileName = 260;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ShellExecuteInfo {
            public  int cbSize;
            public  int fMask;
            public  IntPtr hwnd  = IntPtr.Zero;
            public  IntPtr lpVerb = IntPtr.Zero;
            public  IntPtr lpFile = IntPtr.Zero;
            public  IntPtr lpParameters = IntPtr.Zero;
            public  IntPtr lpDirectory = IntPtr.Zero;
            public  int nShow;
            public  IntPtr hInstApp = IntPtr.Zero;
            public  IntPtr lpIDList = IntPtr.Zero;
            public  IntPtr lpClass = IntPtr.Zero;
            public  IntPtr hkeyClass = IntPtr.Zero;
            public  int dwHotKey;
            public  IntPtr hIcon = IntPtr.Zero;
            public  IntPtr hProcess = IntPtr.Zero;

            public  ShellExecuteInfo() {
                cbSize = Marshal.SizeOf(this);
            }
        }

        // NT definition
        // typedef struct _PROCESS_BASIC_INFORMATION {
        //    NTSTATUS ExitStatus; (LONG)
        //    PPEB PebBaseAddress;
        //    ULONG_PTR AffinityMask;
        //    KPRIORITY BasePriority;  (LONG)
        //    ULONG_PTR UniqueProcessId;
        //    ULONG_PTR InheritedFromUniqueProcessId;
        //} PROCESS_BASIC_INFORMATION;

        [StructLayout(LayoutKind.Sequential)]
        public class NtProcessBasicInfo {
            public  int ExitStatus;
            public  IntPtr PebBaseAddress = IntPtr.Zero;
            public  IntPtr AffinityMask = IntPtr.Zero;
            public  int BasePriority;
            public  IntPtr UniqueProcessId = IntPtr.Zero;
            public  IntPtr InheritedFromUniqueProcessId = IntPtr.Zero;
        }

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

        [DllImport(ExternDll.User32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public  extern static SafeWindowStationHandle OpenWindowStation(
            string pwinsta,
            bool fInherit,
            uint dwDesiredAccess
        );

        public const int JobObjectBasicLimitInformationQuery = 2;
        public const int JobObjectBasicLimitInformationSet = 2;
        [StructLayout(LayoutKind.Sequential)]
        public  struct JobObjectBasicLimitInformation
        {
            public  Int64 PerProcessUserTimeLimit;
            public  Int64 PerJobUserTimeLimit;
            public  UInt32 LimitFlags;
            public  UIntPtr MinimumWorkingSetSize;
            public  UIntPtr MaximumWorkingSetSize;
            public  UInt32 ActiveProcessLimit;
            public  IntPtr Affinity;
            public  UInt32 PriorityClass;
            public  UInt32 SchedulingClass;
        }

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public  extern static bool QueryInformationJobObject(
            SafeJobHandle hJob,
            int query,
            out JobObjectBasicAndIoInformation info,
            int size,
            out int returnedSize
        );

        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public  extern static bool QueryInformationJobObject(
            SafeJobHandle hJob,
            int query,
            out JobObjectBasicLimitInformation info,
            int size,
            out int returnedSize
        );

        [DllImport(ExternDll.Kernel32, CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
        public extern static SafeJobHandle OpenJobObject(
            int dwDesiredAccess,
            bool bInheritHandles,
            string lpName
        );

#endif

        public const uint WTS_NO_ACTIVE_CONSOLE_SESSION = 0xffffffff;
        [DllImport(ExternDll.Kernel32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = false)]
        public extern static UInt32 WTSGetActiveConsoleSessionId();

        [DllImport(ExternDll.Wtsapi32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool WTSQueryUserToken(UInt32 SessionId, out SafeTokenHandle Token);

        [DllImport(ExternDll.Wtsapi32, EntryPoint = "WTSOpenServerW", SetLastError = true,
        CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern SafeWtsHandle WTSOpenServer([MarshalAs(UnmanagedType.LPTStr)] string ServerName);

        [DllImport(ExternDll.Wtsapi32, EntryPoint = "WTSCloseServer", SetLastError = false,
         CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern void WTSCloseServer(IntPtr ServerHandle);

        [DllImport(ExternDll.Wtsapi32, EntryPoint = "WTSEnumerateSessionsW", SetLastError = true,
         CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool _WTSEnumerateSessions(SafeWtsHandle ServerHandle, UInt32 reserved, UInt32 version, out IntPtr pSessionInfo, out UInt32 Count);

        [DllImport(ExternDll.Wtsapi32, SetLastError = false,
         CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern void WTSFreeMemory(IntPtr pSessionInfo);

        [DllImport(ExternDll.Wtsapi32, CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool WTSLogoffSession(IntPtr ServerHandle, UInt32 SessionId, [MarshalAs(UnmanagedType.Bool)]bool bWait);

        public enum WTS_ShutDown_Flag
        {
            WTS_WSD_LOGOFF = 0x00000001,  // log off all users except current user; deletes WinStations (a reboot is required to recreate the WinStations)
            WTS_WSD_SHUTDOWN = 0x00000002, // shutdown system
            WTS_WSD_REBOOT = 0x00000004, // shutdown and reboot
            WTS_WSD_POWEROFF = 0x00000008,  // shutdown and power off (on machines that support power off through software)
            WTS_WSD_FASTREBOOT = 0x00000010  // reboot without logging users off or shutting down
        }

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
        }

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,              // User logged on to WinStation
            WTSConnected,           // WinStation connected to client
            WTSConnectQuery,        // In the process of connecting to client
            WTSShadow,              // Shadowing another WinStation
            WTSDisconnected,        // WinStation logged on without client
            WTSIdle,                // Waiting for client to connect
            WTSListen,              // WinStation is listening for connection
            WTSReset,               // WinStation is being reset
            WTSDown,                // WinStation is down due to error
            WTSInit,                // WinStation in initialization
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WTS_SESSION_INFO
        {
            public UInt32 SessionId;             // session id

            [MarshalAs(UnmanagedType.LPWStr)]
            public string pWinStationName;      // name of WinStation this session is connected to

            public WTS_CONNECTSTATE_CLASS State; // connection state (see enum)
        }

        public static bool WTSEnumerateSessions(SafeWtsHandle ServerHandle, out WTS_SESSION_INFO[] SessionInfo)
        {
            WTS_SESSION_INFO[] retValue;
            uint count;
            IntPtr buf = IntPtr.Zero;

            try
            {
                bool succ = _WTSEnumerateSessions(ServerHandle, 0, 1, out buf, out count);

                if (succ)
                {
                    IntPtr current = buf;
                    int datasize = Marshal.SizeOf(new WTS_SESSION_INFO());
                    retValue = new WTS_SESSION_INFO[count];
                    for (int i = 0; i < count; i++)
                    {
                        retValue[i] = (WTS_SESSION_INFO)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFO));
                        current += datasize;
                    }

                    SessionInfo = retValue;
                    return true;
                }
                else
                {
                    SessionInfo = null;
                    return false;
                }
            }
            finally
            {
                if (buf != IntPtr.Zero)
                {
                    WTSFreeMemory(buf);
                }
            }
        }

        //
        // Logon to console
        //
        [DllImport("HpcCredentialProviderClient.dll", SetLastError = false, CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.I4)]
        public extern static Int32 LogonToConsole(
            string username,
            IntPtr password);
    }
}
