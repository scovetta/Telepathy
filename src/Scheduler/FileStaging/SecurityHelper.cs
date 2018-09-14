//--------------------------------------------------------------------------
// <copyright file="SecurityHelper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This class uses P/Invoke in order to call Windows Security APIs.
//     It provides a helper method called GetInfoForSDDL that looks up
//     name information from an SDDL.
// </summary>
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Hpc.Azure.FileStaging
{
    /// <summary>
    /// This class uses P/Invoke in order to call Windows Security APIs. It provides a helper method 
    /// called GetInfoForSDDL that looks up name information from an SDDL.
    /// </summary>
    class SecurityHelper
    {
        /// <summary>
        /// These string constants define how security-related strings are formatted.
        /// </summary>
        public const string upnFormat = "{0}@{1}";
        public const string computerAccountFormat = "{0}$";

        /// <summary>
        /// Produces a string for the domain and the string for the username that corresponds to the user account referenced by an SDDL.
        /// </summary>
        /// <param name="userSDDL"></param>
        /// <param name="domainName"></param>
        /// <param name="userName"></param>
        public static void GetInfoForSDDL(string userSDDL, out string domainName, out string userName)
        {
            const int POLICY_ALL_ACCESS = 0x00F0FFF;
            const int STATUS_SUCCESS = 0;

            IntPtr sid = IntPtr.Zero, policy = IntPtr.Zero, domainsPtr = IntPtr.Zero, userPtr = IntPtr.Zero;
            try
            {
                //
                // LsaOpenPolicy requires that its ObjectAttributes parameter is zeroed out.
                //
                LSA_OBJECT_ATTRIBUTES attributes;
                attributes.RootDirectory = IntPtr.Zero;
                attributes.ObjectName = IntPtr.Zero;
                attributes.Attributes = 0;
                attributes.SecurityDescriptor = IntPtr.Zero;
                attributes.SecurityQualityOfService = IntPtr.Zero;
                attributes.Length = (uint)Marshal.SizeOf(typeof(LSA_OBJECT_ATTRIBUTES));

                //
                // Convert the SDDL (string Sid) to a SID object
                //
                if (!ConvertStringSidToSid(userSDDL, out sid))
                {
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.OnPremise_InteropFailed, "ConvertStringSidToSid", Marshal.GetLastWin32Error()));
                }

                //
                // Open a policy on the local machine
                //
                uint ntStatus = LsaOpenPolicy(IntPtr.Zero, ref attributes, POLICY_ALL_ACCESS, out policy);
                if (ntStatus != STATUS_SUCCESS)
                {
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.OnPremise_InteropFailed, "LsaOpenPolicy", LsaNtStatusToWinError(ntStatus)));
                }

                //
                // Look up the names that correspond to the SID
                //
                ntStatus = LsaLookupSids(policy, 1, ref sid, out domainsPtr, out userPtr);
                if (ntStatus != STATUS_SUCCESS)
                {
                    throw new Exception(string.Format(CultureInfo.CurrentCulture, Resources.OnPremise_InteropFailed, "LsaLookupSids", LsaNtStatusToWinError(ntStatus)));
                }

                //
                // Use Marshal.PtrToStructure to create managed objects from the unmanaged heap
                //
                LSA_REFERENCED_DOMAIN_LIST domains = (LSA_REFERENCED_DOMAIN_LIST)Marshal.PtrToStructure(domainsPtr, typeof(LSA_REFERENCED_DOMAIN_LIST));
                LSA_TRUST_INFORMATION domain = (LSA_TRUST_INFORMATION)Marshal.PtrToStructure(domains.Domains, typeof(LSA_TRUST_INFORMATION));
                LSA_TRANSLATED_NAME user = (LSA_TRANSLATED_NAME)Marshal.PtrToStructure(userPtr, typeof(LSA_TRANSLATED_NAME));

                //
                // Capture the domain and user names from the newly created objects
                // Bug 10438: Sometimes the string contains extra characters not accounted for by the length field
                // of the LAS_UNICODE_STRING struct. These need to be chopped.
                //
                byte[] domainNameBytes = Encoding.Unicode.GetBytes(domain.Name.Buffer);
                byte[] userNameBytes = Encoding.Unicode.GetBytes(user.Name.Buffer);
                domainName = new string(Encoding.Unicode.GetChars(domainNameBytes, 0, domain.Name.Length));
                userName = new string(Encoding.Unicode.GetChars(userNameBytes, 0, user.Name.Length));
            }
            finally
            {
                //
                // Free the memory allocated by security APIs
                //
                if (sid != IntPtr.Zero)
                {
                    LocalFree(sid);
                }

                if (policy != IntPtr.Zero)
                {
                    LsaClose(policy);
                }

                if (domainsPtr != IntPtr.Zero)
                {
                    LsaFreeMemory(domainsPtr);
                }

                if (userPtr != IntPtr.Zero)
                {
                    LsaFreeMemory(userPtr);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_OBJECT_ATTRIBUTES
        {
            public uint Length;
            public IntPtr RootDirectory;
            public IntPtr ObjectName;
            public uint Attributes;
            public IntPtr SecurityDescriptor;
            public IntPtr SecurityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_REFERENCED_DOMAIN_LIST
        {
            public uint Entries;
            public IntPtr Domains;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_TRUST_INFORMATION
        {
            public LSA_UNICODE_STRING Name;
            public IntPtr Sid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_TRANSLATED_NAME
        {
            public int Use;
            public LSA_UNICODE_STRING Name;
            public int DomainIndex;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct LSA_UNICODE_STRING
        {
            public ushort Length;
            public ushort MaximumLength;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Buffer;
        }

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern uint LsaOpenPolicy(IntPtr SystemName, ref LSA_OBJECT_ATTRIBUTES ObjectAttributes, int AccessMask, out IntPtr PolicyHandle);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern uint LsaLookupSids(IntPtr PolicyHandle, uint count, ref IntPtr buffer, out IntPtr domainList, out IntPtr nameList);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern bool ConvertStringSidToSid(string StringSid, out IntPtr Sid);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = false, ExactSpelling = false)]
        private static extern uint LsaNtStatusToWinError(uint status);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern uint LsaClose(IntPtr ObjectHandle);

        [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern int LsaFreeMemory(IntPtr Buffer);

        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = false)]
        private static extern IntPtr LocalFree(IntPtr hMem);
    }
}
