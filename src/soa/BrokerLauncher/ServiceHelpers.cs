// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Runtime.InteropServices;

    using Microsoft.Win32.SafeHandles;

    [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    internal sealed class SafeGlobalMemoryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeGlobalMemoryHandle()
            : base(true)
        {
        }

        private SafeGlobalMemoryHandle(IntPtr handle)
            : base(true)
        {
            base.SetHandle(handle);
        }

        internal SafeGlobalMemoryHandle(int size)
            : base(true)
        {
            base.SetHandle(Marshal.AllocHGlobal(size));
        }

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(this.handle);
            return true;
        }

        internal static SafeGlobalMemoryHandle InvalidHandle
        {
            get
            {
                return new SafeGlobalMemoryHandle(IntPtr.Zero);
            }
        }

        internal object ToStructure(System.Type type)
        {
            return Marshal.PtrToStructure(this.handle, type);
        }
    }

    internal sealed class SafeServiceHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeServiceHandle()
            : base(true)
        {
        }

        private SafeServiceHandle(IntPtr handle)
            : base(true)
        {
            base.SetHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            return ServiceNativeMethods.CloseServiceHandle(this.handle);
        }

        internal static SafeServiceHandle InvalidHandle
        {
            get
            {
                return new SafeServiceHandle(IntPtr.Zero);
            }
        }
    }

    internal static class ServiceNativeMethods
    {
        internal enum SC_MANAGER_ACCESS : uint
        {
            //standard rights
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = READ_CONTROL,
            STANDARD_RIGHTS_WRITE = READ_CONTROL,
            STANDARD_RIGHTS_EXECUTE = READ_CONTROL,

            //rights specific to SCM
            SC_MANAGER_CONNECT = 0x0001,
            SC_MANAGER_CREATE_SERVICE = 0x0002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x0004,
            SC_MANAGER_LOCK = 0x0008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x0010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x0020,
            SC_MANAGER_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED
                                        | SC_MANAGER_CONNECT
                                        | SC_MANAGER_CREATE_SERVICE
                                        | SC_MANAGER_ENUMERATE_SERVICE
                                        | SC_MANAGER_LOCK
                                        | SC_MANAGER_QUERY_LOCK_STATUS
                                        | SC_MANAGER_MODIFY_BOOT_CONFIG,

            //generic access rights
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,
        }

        internal enum SERVICE_ACCESS : uint
        {
            //standard rights
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,
            STANDARD_RIGHTS_REQUIRED = 0x000F0000,
            STANDARD_RIGHTS_READ = READ_CONTROL,
            STANDARD_RIGHTS_WRITE = READ_CONTROL,
            STANDARD_RIGHTS_EXECUTE = READ_CONTROL,

            //rights specific to service
            SERVICE_QUERY_CONFIG = 0x0001,
            SERVICE_CHANGE_CONFIG = 0x0002,
            SERVICE_QUERY_STATUS = 0x0004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x0008,
            SERVICE_START = 0x0010,
            SERVICE_STOP = 0x0020,
            SERVICE_PAUSE_CONTINUE = 0x0040,
            SERVICE_INTERROGATE = 0x0080,
            SERVICE_USER_DEFINED_CONTROL = 0x0100,
            SERVICE_ALL_ACCESS = STANDARD_RIGHTS_REQUIRED |
                                    SERVICE_QUERY_CONFIG |
                                    SERVICE_CHANGE_CONFIG |
                                    SERVICE_QUERY_STATUS |
                                    SERVICE_ENUMERATE_DEPENDENTS |
                                    SERVICE_START |
                                    SERVICE_STOP |
                                    SERVICE_PAUSE_CONTINUE |
                                    SERVICE_INTERROGATE |
                                    SERVICE_USER_DEFINED_CONTROL,

            //generic rights
            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,
        }

        internal enum SERVICE_TYPE : uint
        {
            SERVICE_NO_CHANGE = 0xffffffff,
            SERVICE_KERNEL_DRIVER = 0x00000001,
            SERVICE_FILE_SYSTEM_DRIVER = 0x00000002,
            SERVICE_ADAPTER = 0x00000004,
            SERVICE_RECOGNIZER_DRIVER = 0x00000008,
            SERVICE_DRIVER = SERVICE_KERNEL_DRIVER | SERVICE_FILE_SYSTEM_DRIVER | SERVICE_RECOGNIZER_DRIVER,

            SERVICE_WIN32_OWN_PROCESS = 0x00000010,
            SERVICE_WIN32_SHARE_PROCESS = 0x00000020,
            SERVICE_WIN32 = SERVICE_WIN32_OWN_PROCESS | SERVICE_WIN32_SHARE_PROCESS,

            SERVICE_INTERACTIVE_PROCESS = 0x00000100,
        }

        internal enum SERVICE_STARTTYPE : uint
        {
            SERVICE_NO_CHANGE = 0xffffffff,
            SERVICE_BOOT_START = 0x00000000,
            SERVICE_SYSTEM_START = 0x00000001,
            SERVICE_AUTO_START = 0x00000002,
            SERVICE_DEMAND_START = 0x00000003,
            SERVICE_DISABLED = 0x00000004,
        }

        internal enum SERVICE_ERRORCONTROL : uint
        {
            SERVICE_NO_CHANGE = 0xffffffff,
            SERVICE_ERROR_IGNORE = 0x00000000,
            SERVICE_ERROR_NORMAL = 0x00000001,
            SERVICE_ERROR_SEVERE = 0x00000002,
            SERVICE_ERROR_CRITICAL = 0x00000003,
        }

        internal enum SERVICE_STATE : uint
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        internal enum SERVICE_CONTROL_ACCEPTED : uint
        {
            SERVICE_ACCEPT_STOP = 0x00000001,
            SERVICE_ACCEPT_PAUSE_CONTINUE = 0x00000002,
            SERVICE_ACCEPT_SHUTDOWN = 0x00000004,
            SERVICE_ACCEPT_PARAMCHANGE = 0x00000008,
            SERVICE_ACCEPT_NETBINDCHANGE = 0x00000010,
            SERVICE_ACCEPT_HARDWAREPROFILECHANGE = 0x00000020,
            SERVICE_ACCEPT_POWEREVENT = 0x00000040,
            SERVICE_ACCEPT_SESSIONCHANGE = 0x00000080,
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct QUERY_SERVICE_CONFIG
        {
            internal SERVICE_TYPE serviceType;
            internal SERVICE_STARTTYPE startType;
            internal SERVICE_ERRORCONTROL errorControl;
            internal string binaryPathName;
            internal string loadOrderGroup;
            internal uint dwTagId;
            internal string dependencies;
            internal string serviceStartName;
            internal string displayName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SERVICE_STATUS
        {
            internal SERVICE_TYPE serviceType;
            internal SERVICE_STATE currentState;
            internal SERVICE_CONTROL_ACCEPTED controlsAccepted;
            internal uint win32ExitCode;
            internal uint serviceSpecificExitCode;
            internal uint checkPoint;
            internal uint waitHint;
        }

        internal const int ERROR_INSUFFICIENT_BUFFER = 122;

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeServiceHandle OpenSCManager(string machineName, string databaseName, SC_MANAGER_ACCESS desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern SafeServiceHandle OpenService(SafeServiceHandle manager, string serviceName, SERVICE_ACCESS desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool QueryServiceConfig(SafeServiceHandle service, SafeGlobalMemoryHandle pServiceConfig, uint bufSize, out uint bytesNeeded);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ChangeServiceConfig(SafeServiceHandle service, SERVICE_TYPE serviceType, SERVICE_STARTTYPE startType,
            SERVICE_ERRORCONTROL errorControl, String binaryPathName, String loadOrderGroup, IntPtr pdwTagId, String dependencies, String serviceStartName,
            String password, String displayName);

    }

    internal static class ServiceHelpers
    {
        /// <summary>
        /// Query start type of a service on local machine
        /// </summary>
        /// <returns></returns>
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        internal static ServiceNativeMethods.SERVICE_STARTTYPE GetServiceStartType(string serviceName, string server = null)
        {
            ServiceNativeMethods.QUERY_SERVICE_CONFIG config = new ServiceNativeMethods.QUERY_SERVICE_CONFIG();

            //open service control manager
            using (SafeServiceHandle scManager = ServiceNativeMethods.OpenSCManager(server, null, ServiceNativeMethods.SC_MANAGER_ACCESS.GENERIC_READ))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (scManager.IsInvalid)
                {
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }

                // open service
                using (SafeServiceHandle service = ServiceNativeMethods.OpenService(scManager, serviceName, ServiceNativeMethods.SERVICE_ACCESS.GENERIC_READ))
                {
                    errorCode = Marshal.GetLastWin32Error();
                    if (service.IsInvalid)
                    {
                        throw new System.ComponentModel.Win32Exception(errorCode);
                    }

                    //query size of buffer needed for configuration information
                    uint bufSize = 0;
                    uint bytesNeeded;
                    using (SafeGlobalMemoryHandle invalidHandle = SafeGlobalMemoryHandle.InvalidHandle)
                    {
                        if (!ServiceNativeMethods.QueryServiceConfig(service, invalidHandle, bufSize, out bytesNeeded))
                        {
                            errorCode = Marshal.GetLastWin32Error();
                            if (errorCode != ServiceNativeMethods.ERROR_INSUFFICIENT_BUFFER)
                            {
                                throw new System.ComponentModel.Win32Exception(errorCode);
                            }
                        }
                    }

                    //allocate buffer and query configurations
                    using (SafeGlobalMemoryHandle buffer = new SafeGlobalMemoryHandle((int)bytesNeeded))
                    {   
                        bufSize = bytesNeeded;
                        if (!ServiceNativeMethods.QueryServiceConfig(service, buffer, bufSize, out bytesNeeded))
                        {
                            errorCode = Marshal.GetLastWin32Error();
                            throw new System.ComponentModel.Win32Exception(errorCode);
                        }

                        config = (ServiceNativeMethods.QUERY_SERVICE_CONFIG)buffer.ToStructure(typeof(ServiceNativeMethods.QUERY_SERVICE_CONFIG));
                    }
                }
            }

            return config.startType;
        }

        /// <summary>
        /// Set service start type.
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="type"></param>
        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        internal static void SetServiceStartType(string serviceName, ServiceNativeMethods.SERVICE_STARTTYPE type)
        {
            //open service control manager
            using (SafeServiceHandle scManager = ServiceNativeMethods.OpenSCManager(null, null, ServiceNativeMethods.SC_MANAGER_ACCESS.GENERIC_READ))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (scManager.IsInvalid)
                {
                    throw new System.ComponentModel.Win32Exception(errorCode);
                }

                // open service
                using (SafeServiceHandle service = ServiceNativeMethods.OpenService(scManager, serviceName, ServiceNativeMethods.SERVICE_ACCESS.GENERIC_WRITE))
                {
                    errorCode = Marshal.GetLastWin32Error();
                    if (service.IsInvalid)
                    {
                        throw new System.ComponentModel.Win32Exception(errorCode);
                    }

                    // change start type
                    if (!ServiceNativeMethods.ChangeServiceConfig(service, ServiceNativeMethods.SERVICE_TYPE.SERVICE_NO_CHANGE, type,
                            ServiceNativeMethods.SERVICE_ERRORCONTROL.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, null, null, null, null))
                    {
                        errorCode = Marshal.GetLastWin32Error();
                        throw new System.ComponentModel.Win32Exception(errorCode);
                    }
                }
            }
        }
    }
}