//------------------------------------------------------------------------------
// <copyright file="LsaNativeMethods.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [HostProtection(MayLeakOnAbort = true)]
    internal static partial class NativeMethods
    {   
        public const string MICROSOFT_KERBEROS_NAME_A = "Kerberos";

        public const UInt32 LSA_STATUS_SUCCESS = 0;
        public const UInt32 STATUS_ACCESS_DENIED = 0xC0000022;
        public const UInt32 STATUS_INSUFFICIENT_RESOURCES = 0xC000009A;
        public const UInt32 STATUS_INVALID_HANDLE = 0xC0000008;
        public const UInt32 STATUS_INTERNAL_DB_ERROR = 0xC0000158;
        public const UInt32 STATUS_INVALID_SERVER_STATE = 0xC00000DC;
        public const UInt32 STATUS_INVALID_PARAMETER = 0xC000000D;
        public const UInt32 STATUS_NO_SUCH_PRIVILEGE = 0xC0000060;
        public const UInt32 STATUS_OBJECT_NAME_NOT_FOUND = 0xC0000034;
        public const UInt32 STATUS_UNSUCCESSFUL = 0xC0000001;

        [DllImport("Secur32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern UInt32 LsaConnectUntrusted(
            out IntPtr LsaHandle
        );

        [DllImport("Secur32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern UInt32 LsaDeregisterLogonProcess(
            IntPtr LsaHandle
        );

        [DllImport("Secur32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern UInt32 LsaLookupAuthenticationPackage(
            IntPtr LsaHandle,
            LSA_STRING PackageName,
            out UInt32 AuthenticationPackage
        );

        [DllImport("Secur32.dll", EntryPoint = "LsaCallAuthenticationPackage", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern UInt32 LsaCallAuthenticationPackage(
          IntPtr LsaHandle,
          UInt32 AuthenticationPackage,
          KERB_PURGE_TKT_CACHE_REQUEST ProtocolSubmitBuffer,
          UInt32 SubmitBufferLength,
          out IntPtr ProtocolReturnBuffer,
          out UInt32 ReturnBufferLength,
          out UInt32 ProtocolStatus
        );

        [DllImport("Secur32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern UInt32 LsaFreeReturnBuffer(
            IntPtr Buffer
        );


        [DllImport("Advapi32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern UInt32 LsaNtStatusToWinError(
            UInt32 Status
        );

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public class LSA_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Buffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public class UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength = 1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Buffer;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public class KERB_PURGE_TKT_CACHE_REQUEST
        {
            public KERB_PROTOCOL_MESSAGE_TYPE MessageType;
            public LUID LogonId;
            public UNICODE_STRING ServerName;
            public UNICODE_STRING RealmName;
        }

        public enum KERB_PROTOCOL_MESSAGE_TYPE
        {
            KerbDebugRequestMessage = 0,
            KerbQueryTicketCacheMessage,
            KerbChangeMachinePasswordMessage,
            KerbVerifyPacMessage,
            KerbRetrieveTicketMessage,
            KerbUpdateAddressesMessage,
            KerbPurgeTicketCacheMessage,
            KerbChangePasswordMessage,
            KerbRetrieveEncodedTicketMessage,
            KerbDecryptDataMessage,
            KerbAddBindingCacheEntryMessage,
            KerbSetPasswordMessage,
            KerbSetPasswordExMessage,
            KerbVerifyCredentialsMessage,
            KerbQueryTicketCacheExMessage,
            KerbPurgeTicketCacheExMessage,
            KerbRefreshSmartcardCredentialsMessage,
            KerbAddExtraCredentialsMessage,
            KerbQuerySupplementalCredentialsMessage
        }

        /// <summary>
        /// LSA Account Rights management
        /// </summary>
        /// <summary>
        /// SID Usage Enum
        /// </summary>
        public enum SID_NAME_USE
        {
            SidTypeUser = 1,
            SidTypeGroup,
            SidTypeDomain,
            SidTypeAlias,
            SidTypeWellKnownGroup,
            SidTypeDeletedAccount,
            SidTypeInvalid,
            SidTypeUnknown,
            SidTypeComputer
        }

        /// <summary>
        /// Get SID for account name
        /// </summary>
        /// <param name="lpSystemName">Computer name</param>
        /// <param name="lpAccountName">Account name</param>
        /// <param name="Sid">Security ID</param>
        /// <param name="cbSid">Number of bytes needed to hold the SID</param>
        /// <param name="ReferencedDomainName">Domain name reference by SID</param>
        /// <param name="cchReferencedDomainName">Number of bytes needed to hold the domain</param>
        /// <param name="peUse">Account type</param>
        /// <returns>error flag</returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool LookupAccountName(
            string lpSystemName,
            string lpAccountName,
            IntPtr Sid,
            ref uint cbSid,
            StringBuilder ReferencedDomainName,
            ref uint cchReferencedDomainName,
            out SID_NAME_USE peUse);

        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_UNICODE_STRING
        {
            public UInt16 Length;
            public UInt16 MaximumLength;
            public IntPtr Buffer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LSA_OBJECT_ATTRIBUTES
        {
            public uint Length;
            public IntPtr RootDirectory;
            public LSA_UNICODE_STRING ObjectName;
            public UInt32 Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        public const uint POLICY_LOOKUP_NAMES = 0x00000800;
        public const uint POLICY_CREATE_ACCOUNT = 0x00000010;

        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern UInt32 LsaOpenPolicy(
            ref LSA_UNICODE_STRING SystemName,
            ref LSA_OBJECT_ATTRIBUTES ObjectAttributes,
            uint DesiredAccess,
            out IntPtr PolicyHandle);

        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern UInt32 LsaAddAccountRights(
            IntPtr PolicyHandle,
            IntPtr AccountSid,
            LSA_UNICODE_STRING[] UserRights,
            int CountOfRights);

        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern UInt32 LsaRemoveAccountRights(
            IntPtr PolicyHandle,
            IntPtr AccountSid,
            bool AllRights,
            LSA_UNICODE_STRING[] UserRights,
            int CountOfRights);

        [DllImport("advapi32.dll", PreserveSig = true)]
        public static extern UInt32 LsaClose(
            IntPtr PolicyHandle);
    }
}

