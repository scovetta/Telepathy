//------------------------------------------------------------------------------
// <copyright file="Ticket.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">nzeng</owner>
// <securityReview name="nzeng" date="4-11-06"/>
//------------------------------------------------------------------------------
#define TRACE

#region Using directives
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

#endregion

namespace Microsoft.Hpc
{
    public sealed class Ticket
    {
        #region PublicMethods

        public static void Purge(string serverName, string realmName)
        {
            UInt32 packageID = 0;
            IntPtr handle = IntPtr.Zero;

            try
            {
                // get connection handle and lsa package
                LookupPackageConnection(out handle, out packageID);
                // purge tickets
                PurgeTickets(handle, packageID, serverName, realmName);
            }
            finally
            {
                if (handle != IntPtr.Zero)
                {
                    NativeMethods.LsaDeregisterLogonProcess(handle);
                }
            }
        }

        private static void LookupPackageConnection(out IntPtr handle, out UInt32 packageID)
        {
            handle = IntPtr.Zero;
            packageID = 0;
            UInt32 ntStatus = 0;

            //
            // Establish an untrusted connection to LSA server
            //
            ntStatus = NativeMethods.LsaConnectUntrusted(out handle);
            CheckNtStatus(ntStatus);

            //
            // Obtain ID for Kerberos authentication package
            //
            NativeMethods.LSA_STRING lsaName = new NativeMethods.LSA_STRING();
            lsaName.Buffer = NativeMethods.MICROSOFT_KERBEROS_NAME_A;
            lsaName.Length = (UInt16)lsaName.Buffer.Length;
            lsaName.MaximumLength = (UInt16)(lsaName.Length + 1);

            ntStatus = NativeMethods.LsaLookupAuthenticationPackage(handle,    // [IN] LSA Handle
                                                      lsaName,                 // [IN] Package Name
                                                      out packageID);          // [OUT] Package ID
            CheckNtStatus(ntStatus);
        }

        [System.Security.Permissions.PermissionSetAttribute(System.Security.Permissions.SecurityAction.Demand, Name="FullTrust")]
        private static void PurgeTickets(IntPtr handle, UInt32 packageID, string serverName, string realmName)
        {
            UInt32 ntStatus = 0;
            UInt32 ntSubStatus;
            IntPtr response;
            UInt32 responseSize;
            UInt32 requestSize = (UInt32)Marshal.SizeOf(typeof(NativeMethods.KERB_PURGE_TKT_CACHE_REQUEST));

            NativeMethods.KERB_PURGE_TKT_CACHE_REQUEST cacheRequest = new NativeMethods.KERB_PURGE_TKT_CACHE_REQUEST();
            if (string.IsNullOrEmpty(serverName))
            {
                cacheRequest.ServerName = null;
            }
            else
            {
                cacheRequest.ServerName = new NativeMethods.UNICODE_STRING();
                cacheRequest.ServerName.Length = (ushort)serverName.Length;
                cacheRequest.ServerName.MaximumLength = (ushort)(serverName.Length + 1);
                cacheRequest.ServerName.Buffer = serverName;
            }
            if (string.IsNullOrEmpty(realmName))
            {
                cacheRequest.RealmName = null;
            }
            else
            {
                cacheRequest.RealmName = new NativeMethods.UNICODE_STRING();
                cacheRequest.RealmName.Length = (ushort)realmName.Length;
                cacheRequest.RealmName.MaximumLength = (ushort)(realmName.Length + 1);
                cacheRequest.RealmName.Buffer = realmName;
            }
            cacheRequest.MessageType = NativeMethods.KERB_PROTOCOL_MESSAGE_TYPE.KerbPurgeTicketCacheMessage;
            cacheRequest.LogonId = new NativeMethods.LUID();
            cacheRequest.LogonId.LowPart = 0;          // LUID, zero indicates 
            cacheRequest.LogonId.HighPart = 0;         // current logon session

            ntStatus = NativeMethods.LsaCallAuthenticationPackage(
                                    handle,                  // [IN] LSA connection handle
                                    packageID,               // [IN] Kerberos package ID
                                    cacheRequest,            // [IN] Request message
                                    requestSize,             // [IN] Message length
                                    out response,            // [OUT] Response buffer
                                    out responseSize,        // [OUT] Response length
                                    out ntSubStatus);

            CheckNtStatus(ntStatus);
            CheckNtStatus(ntSubStatus);
        }

        private static void CheckNtStatus(UInt32 status)
        {
            if (status != NativeMethods.LSA_STATUS_SUCCESS)
            {
                int errorCode = (int)NativeMethods.LsaNtStatusToWinError(status);
                if (errorCode != 0)
                {
                    throw new Win32Exception(errorCode);
                }
            }
        }

        #endregion // PublicMethods
    }
}

