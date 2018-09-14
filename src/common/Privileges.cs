//------------------------------------------------------------------------------
// <copyright file="Priviligest.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">colinw</owner>
// <securityReview name="colinw" date="2-22-06"/>
//------------------------------------------------------------------------------
#define TRACE

#region Using directives
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
#endregion

namespace Microsoft.Hpc
{
    public sealed class Privileges
    {
        #region PrivateFields
        #endregion // PrivateFields

        #region Constructors
        private Privileges()
        {
        }
        #endregion // Constructors

        #region Properties

        // SE_CREATE_TOKEN_NAME
        public const String CreateToken = "SeCreateTokenPrivilege";
        // SE_ASSIGNPRIMARYTOKEN_NAME
        public const String AssignPrimaryToken = "SeAssignPrimaryTokenPrivilege";
        // SE_LOCK_MEMORY_NAME
        public const String LockMemory = "SeLockMemoryPrivilege";
        // SE_INCREASE_QUOTA_NAME
        public const String IncreaseQuota = "SeIncreaseQuotaPrivilege";
        // SE_UNSOLICITED_INPUT_NAME
        public const String UnsolicitedInput = "SeUnsolicitedInputPrivilege";
        // SE_MACHINE_ACCOUNT_NAME
        public const String MachineAccount = "SeMachineAccountPrivilege";
        // SE_TCB_NAME
        public const String Tcb = "SeTcbPrivilege";
        // SE_SECURITY_NAME
        public const String Security = "SeSecurityPrivilege";
        // SE_TAKE_OWNERSHIP_NAME
        public const String TakeOwnership = "SeTakeOwnershipPrivilege";
        // SE_LOAD_DRIVER_NAME
        public const String LoadDriver = "SeLoadDriverPrivilege";
        // SE_SYSTEM_PROFILE_NAME
        public const String SystemProfile = "SeSystemProfilePrivilege";
        // SE_SYSTEMTIME_NAME
        public const String SystemTime = "SeSystemtimePrivilege";
        // SE_PROF_SINGLE_PROCESS_NAME
        public const String ProfileSingleProcess = "SeProfileSingleProcessPrivilege";
        // SE_INC_BASE_PRIORITY_NAME
        public const String IncreaseBasePriority = "SeIncreaseBasePriorityPrivilege";
        // SE_CREATE_PAGEFILE_NAME
        public const String CreatePageFile = "SeCreatePagefilePrivilege";
        // SE_CREATE_PERMANENT_NAME
        public const String CreatePermanent = "SeCreatePermanentPrivilege";
        // SE_BACKUP_NAME
        public const String Backup = "SeBackupPrivilege";
        // SE_RESTORE_NAME
        public const String Restore = "SeRestorePrivilege";
        // SE_SHUTDOWN_NAME
        public const String Shutdown = "SeShutdownPrivilege";
        // SE_DEBUG_NAME
        public const String Debug = "SeDebugPrivilege";
        // SE_AUDIT_NAME
        public const String Audit = "SeAuditPrivilege";
        // SE_SYSTEM_ENVIRONMENT_NAME
        public const String SystemEnvironment = "SeSystemEnvironmentPrivilege";
        // SE_CHANGE_NOTIFY_NAME
        public const String ChangeNotify = "SeChangeNotifyPrivilege";
        // SE_REMOTE_SHUTDOWN_NAME
        public const String RemoteShutdown = "SeRemoteShutdownPrivilege";
        // SE_UNDOCK_NAME
        public const String Undock = "SeUndockPrivilege";
        // SE_SYNC_AGENT_NAME
        public const String SyncAgent = "SeSyncAgentPrivilege";
        // SE_ENABLE_DELEGATION_NAME
        public const String EnableDelegation = "SeEnableDelegationPrivilege";
        // SE_MANAGE_VOLUME_NAME
        public const String ManageVolume = "SeManageVolumePrivilege";
        // SE_IMPERSONATE_NAME
        public const String Impersonate = "SeImpersonatePrivilege";
        // SE_CREATE_GLOBAL_NAME
        public const String CreateGlobal = "SeCreateGlobalPrivilege";
        // SE_TRUSTED_CREDMAN_ACCESS_NAME
        public const String TrustedCredManAccess = "SeTrustedCredManAccessPrivilege";
        // SE_RELABEL_NAME
        public const String Relabel = "SeRelabelPrivilege";
        // SE_INC_WORKING_SET_NAME
        public const String IncreaseWorkingSet = "SeIncreaseWorkingSetPrivilege";
        // SE_TIME_ZONE_NAME
        public const String TimeZone = "SeTimeZonePrivilege";
        // SE_CREATE_SYMBOLIC_LINK_NAME
        public const String CreateSymbolicLink = "SeCreateSymbolicLinkPrivilege";

        #endregion // Properties

        #region InternalPropertiesAndMethods
        #endregion  // InternalPropertiesAndMethods

        #region PublicMethods


        public static void DisableDefaultPrivileges()
        {            
            // By default System processes have extra privileges that the CCP services do not
            // require. Unfortunately we cannot turn them off but we can disable them so it is more
            // tedious for an attacker to use these privileges or for our CCP software to use them
            // due to error.

            string[] PrivilegeNames = new string[] {
                "SeAuditPrivilege",
                "SeCreateGlobalPrivilege",
                "SeCreatePagefilePrivilege",
                "SeCreatePermanentPrivilege",
                "SeDebugPrivilege",
                "SeLockMemoryPrivilege",
                "SeProfileSingleProcessPrivilege",
                "SeManageVolumePrivilege",
                "SeTcbPrivilege",
                "SeIncreaseBasePriorityPrivilege"
            };

            DisablePrivileges(PrivilegeNames);
        }

        //
        public static void RemoveDefaultPrivilegesForScheduler()
        {
            // By default System processes have extra privileges that the CCP services do not
            // require. We are removing them here

            string[] PrivilegeNames = new string[] {
                "SeAuditPrivilege",
                "SeCreateGlobalPrivilege",
                "SeCreatePagefilePrivilege",
                "SeCreatePermanentPrivilege",
                "SeDebugPrivilege",
                "SeLockMemoryPrivilege",
                "SeProfileSingleProcessPrivilege",
                "SeManageVolumePrivilege",                
                "SeIncreaseBasePriorityPrivilege"
            };

            DisablePrivileges(PrivilegeNames);
        }

        
        /// <summary>
        /// Remove Privileges listed in string array        
        /// NOTE: This is actually removing privileges since SE_PRIVILEGE_DISABLED used to be set to 4 which 
        /// in winnt.h is actually SE_PRIVILEGE_REMOVED which removes the privilege and does not allow us to enable it afterwards
        /// </summary>
        /// <param name="PrivilegeNames"></param>
        public static void DisablePrivileges(string[] PrivilegeNames)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                if (!NativeMethods.OpenProcessToken(
                        new HandleRef(null, NativeMethods.GetCurrentProcess()),
                        (int)TokenAccessLevels.AdjustPrivileges,
                        out tokenHandle))
                {
                    return;
                }

                NativeMethods.TokenPrivileges tp = new NativeMethods.TokenPrivileges();
                tp.PrivilegeCount = 1;
                tp.Attributes = NativeMethods.SE_PRIVILEGE_REMOVED;
                foreach (string privilege in PrivilegeNames)
                {
                    if (NativeMethods.LookupPrivilegeValue(null, privilege, out tp.Luid))
                    {
                        // AdjustTokenPrivileges can return true even if it didn't succeed (when ERROR_NOT_ALL_ASSIGNED is returned).
                        NativeMethods.AdjustTokenPrivileges(new HandleRef(null, tokenHandle), false, tp, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(new HandleRef(null, tokenHandle));
                }
            }
        }

        /// <summary>
        /// This method actually disables the privileges in the string array.
        /// This can be used to disable privileges that a service wants to enable later
        /// </summary>
        /// <param name="PrivilegeNames"></param>
        
        public static void DisablePrivileges2(string[] PrivilegeNames)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                if (!NativeMethods.OpenProcessToken(
                        new HandleRef(null, NativeMethods.GetCurrentProcess()),
                        (int)TokenAccessLevels.AdjustPrivileges,
                        out tokenHandle))
                {
                    return;
                }

                NativeMethods.TokenPrivileges tp = new NativeMethods.TokenPrivileges();
                tp.PrivilegeCount = 1;
                tp.Attributes = NativeMethods.SE_PRIVILEGE_DISABLED;
                foreach (string privilege in PrivilegeNames)
                {
                    if (NativeMethods.LookupPrivilegeValue(null, privilege, out tp.Luid))
                    {
                        // AdjustTokenPrivileges can return true even if it didn't succeed (when ERROR_NOT_ALL_ASSIGNED is returned).
                        NativeMethods.AdjustTokenPrivileges(new HandleRef(null, tokenHandle), false, tp, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(new HandleRef(null, tokenHandle));
                }
            }
        }



        /// <summary>
        /// Enables Privileges listed in string array
        /// </summary>
        /// <param name="PrivilegeNames"></param>
        public static void EnablePrivileges(string[] PrivilegeNames)
        {
            IntPtr tokenHandle = IntPtr.Zero;
            try
            {
                if (!NativeMethods.OpenProcessToken(
                        new HandleRef(null, NativeMethods.GetCurrentProcess()),
                        (int)TokenAccessLevels.AdjustPrivileges,
                        out tokenHandle))
                {
                    return;
                }

                NativeMethods.TokenPrivileges tp = new NativeMethods.TokenPrivileges();
                tp.PrivilegeCount = 1;
                tp.Attributes = NativeMethods.SE_PRIVILEGE_ENABLED;
                foreach (string privilege in PrivilegeNames)
                {
                    if (NativeMethods.LookupPrivilegeValue(null, privilege, out tp.Luid))
                    {
                        // AdjustTokenPrivileges can return true even if it didn't succeed (when ERROR_NOT_ALL_ASSIGNED is returned).
                        NativeMethods.AdjustTokenPrivileges(new HandleRef(null, tokenHandle), false, tp, 0, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
            finally
            {
                if (tokenHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(new HandleRef(null, tokenHandle));
                }
            }
        }
        #endregion // PublicMethods
        #region PrivateMethods
        #endregion // PrivateMethods
    }
}

