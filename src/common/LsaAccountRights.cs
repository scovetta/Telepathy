//------------------------------------------------------------------------------
// <copyright file="LsaAccountRights.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    #region Using directives

    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    #endregion

    /// <summary>
    /// The LsaAccountRights class implements some common functionality for 
    /// manipulating account rights for user accounts
    /// </summary>
    public static class LsaAccountRights
    {
        /// <summary>
        /// Security right name for log on interactively
        /// </summary>
        private const string InteractiveLogonName = "SeInteractiveLogonRight";

        /// <summary>
        /// Grant interactive log on to the specified user
        /// </summary>
        /// <param name="userName">Name of the user account</param>
        /// <param name="addRight">Whether to add or remove the right</param>
        public static void AdjustInteractiveLogonRight(string userName, bool addRight)
        {
            NativeMethods.LSA_UNICODE_STRING sysName = new NativeMethods.LSA_UNICODE_STRING();
            NativeMethods.LSA_OBJECT_ATTRIBUTES objAttrs = new NativeMethods.LSA_OBJECT_ATTRIBUTES();
            IntPtr polHandle = IntPtr.Zero;
            IntPtr userSid = IntPtr.Zero;

            try
            {
                uint ntStatus = NativeMethods.LsaOpenPolicy(
                    ref sysName,
                    ref objAttrs,
                    NativeMethods.POLICY_LOOKUP_NAMES | NativeMethods.POLICY_CREATE_ACCOUNT,
                    out polHandle);

                if (ntStatus != NativeMethods.LSA_STATUS_SUCCESS)
                {
                    uint status = ResolveLsaStatus(ntStatus);
                    throw new Exception(string.Format("Failed to open LSA policy handle : {0}.", status));
                }

                userSid = AccountNameToNativeSid(userName);

                NativeMethods.LSA_UNICODE_STRING[] userRights = new NativeMethods.LSA_UNICODE_STRING[1];
                userRights[0] = new NativeMethods.LSA_UNICODE_STRING();
                userRights[0].Buffer = Marshal.StringToHGlobalUni(InteractiveLogonName);
                userRights[0].Length = (ushort)(InteractiveLogonName.Length * UnicodeEncoding.CharSize);
                userRights[0].MaximumLength = (ushort)((InteractiveLogonName.Length + 1) * UnicodeEncoding.CharSize);

                if (addRight)
                {
                    ntStatus = NativeMethods.LsaAddAccountRights(polHandle, userSid, userRights, 1);
                }
                else
                {
                    ntStatus = NativeMethods.LsaRemoveAccountRights(polHandle, userSid, false, userRights, 1);
                }

                if (ntStatus != NativeMethods.LSA_STATUS_SUCCESS)
                {
                    // Only throw on failures to add or if remove fails for some other reason 
                    // that the SID was missing from the list
                    if (addRight || ntStatus != NativeMethods.STATUS_OBJECT_NAME_NOT_FOUND)
                    {
                        uint status = ResolveLsaStatus(ntStatus);
                        throw new Exception(string.Format(
                            "Failed to {0} interactive log on for account {1} : {2}.",
                            addRight ? "grant" : "revoke",
                            userName,
                            status));
                    }
                }
            }
            finally
            {
                try
                {
                    if (userSid != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(userSid);
                    }

                    if (polHandle != IntPtr.Zero)
                    {
                        NativeMethods.LsaClose(polHandle);
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Helper method to get the sid for an account name on a remote machine
        /// </summary>
        /// <param name="accountName">Account to get SID for (can be group or user)</param>        
        /// <returns>The SID for the account</returns>
        private static IntPtr AccountNameToNativeSid(string accountName)
        {
            string sidString = string.Empty;
            IntPtr sid = IntPtr.Zero;
            uint numBytes = 0;
            StringBuilder domainName = new StringBuilder();
            uint domainNameCap = (uint)domainName.Capacity;
            NativeMethods.SID_NAME_USE sidUse;
            int err = NativeMethods.ERROR_SUCCESS;

            try
            {
                // Try to get SID for local group
                if (!NativeMethods.LookupAccountName(
                    null, 
                    accountName, 
                    sid, 
                    ref numBytes, 
                    domainName, 
                    ref domainNameCap, 
                    out sidUse))
                {
                    // Should fail with insufficient buffer
                    err = Marshal.GetLastWin32Error();
                    
                    if (err == NativeMethods.ERROR_INSUFFICIENT_BUFFER)
                    {
                        sid = Marshal.AllocHGlobal((int)numBytes);
                        domainName.EnsureCapacity((int)domainNameCap);
                        err = NativeMethods.ERROR_SUCCESS;
                        
                        if (!NativeMethods.LookupAccountName(
                            null, 
                            accountName, 
                            sid, 
                            ref numBytes, 
                            domainName, 
                            ref domainNameCap, 
                            out sidUse))
                        {
                            err = Marshal.GetLastWin32Error();
                            throw new Exception(
                                string.Format("Failed to get SID for account {0}: {1}.", accountName, err));
                        }
                    }
                    else
                    {
                        throw new Exception(
                            string.Format("Failed to look up SID for account {0}: {1}.", accountName, err));
                    }
                }
                else if (sid == IntPtr.Zero)
                {
                    // No error, but no SID, so fail
                    throw new Exception(
                        string.Format("Could not resolve SID for account {0} (null SID).", accountName));
                }

                return sid;
            }
            catch (Exception)
            {
                // we're leaving this method with an error so clean up our allocation if it was done.
                if (sid != IntPtr.Zero)
                { 
                    Marshal.FreeHGlobal(sid); 
                }

                throw;
            }
        }

        /// <summary>
        /// Helper function to convert LSA status codes (ala NT Status) to Win32 equivalent
        /// </summary>
        /// <param name="ntStatus">NT status value</param>
        /// <returns>The Win32 status</returns>
        private static uint ResolveLsaStatus(uint ntStatus)
        {
            uint status = NativeMethods.LsaNtStatusToWinError(ntStatus);
            
            if (status == NativeMethods.ERROR_MR_MID_NOT_FOUND)
            {
                status = ntStatus;
            }

            return status;
        }
    }
}