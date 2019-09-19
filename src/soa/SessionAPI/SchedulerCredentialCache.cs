// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#region Using directives

using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Text;
using Microsoft.Win32;
using System.Security.AccessControl;
using System.Security.Principal;

#endregion

namespace Microsoft.Hpc.Scheduler
{

// Security review: jvert 1-09-06

    static class CredentialCache
    {
        internal static byte[] LookupCredential(string targetCluster, ref string username)
        {
            try
            {
                // Try to normalize the username
                username = Sid2Name(Name2Sid(username));
            }
            catch
            {
                username = null;
            }

            //
            // Try to open the specified registry key
            //
            using(RegistryKey clusterKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\HPC\CachedCredentials\" + targetCluster))
            {
                if (clusterKey == null) {
                    //
                    // There are no credentials cached for this server
                    //
                    return null;
                }

                //
                // If a username was not specified, then there must be only one value and its name
                // will be the username we return.
                //
                byte[] credential;
                if (String.IsNullOrEmpty(username)) {
                    string[] values = clusterKey.GetValueNames();

                    if (values.Length != 1) 
                    {
                        return null;
                    }

                    credential = (byte [])clusterKey.GetValue(values[0]);
                    if (credential != null) {
                        username = values[0];
                    }

                } else {

                    //
                    // A username was specified so we use that as the value name to return
                    //
                    credential = (byte [])clusterKey.GetValue(username);
                }

                return credential;
            }
        }

        internal static void CacheCredential(string targetCluster, string username, byte[] credential)
        {
            try
            {
                // Try to normalize the username
                username = Sid2Name(Name2Sid(username));
            }
            catch
            {
                return;
            }

            //
            // Algorithm we use here is simple - just set the information into the registry using
            // the username as the value name and the credential as the value data
            //

            // V3SP2 bug 11110
            // Need to create the CachedCredential subkey first (if not exists), to make sure it 
            // inherits its parent's privilege

            RegistryKey cachedCredentialKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\HPC\CachedCredentials");

            if (cachedCredentialKey == null)
            {
                cachedCredentialKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\HPC\CachedCredentials");
            }

            using (cachedCredentialKey)
            {
                // 
                // We create this key with an ACL allowing only the current user access to help prevent
                // anyone else from getting this information.
                //
                RegistrySecurity ACL = new RegistrySecurity();
                ACL.AddAccessRule(new RegistryAccessRule(WindowsIdentity.GetCurrent().User,
                                                         RegistryRights.FullControl,
                                                         AccessControlType.Allow));
                RegistryKey clusterKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\HPC\CachedCredentials\" + targetCluster,
                                                                           RegistryKeyPermissionCheck.ReadWriteSubTree,
                                                                           ACL);
                //
                // If we failed to create the key, that's ok, the credentials just won't get
                // cached.
                //
                if (clusterKey != null)
                {
                    clusterKey.SetValue(username, credential, RegistryValueKind.Binary);
                    clusterKey.Close();
                }
            }
        }

        internal static void PurgeCredential(string targetCluster, string userName)
        {
            if (String.IsNullOrEmpty(userName)) {
                //
                // This is an easy one, just delete the entire key.
                //
                try {
                    Registry.CurrentUser.DeleteSubKeyTree(@"SOFTWARE\Microsoft\HPC\CachedCredentials\" + targetCluster);
                } catch (ArgumentException) {
                    // the key doesn't exist, so nothing to worry about
                }
            } else {
                //
                // Delete the specified value under the target cluster
                //
                using (RegistryKey clusterKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\HPC\CachedCredentials\" + targetCluster,
                                                                           RegistryKeyPermissionCheck.ReadWriteSubTree)) {
                    //
                    // If we failed to open the key, that's ok, that just means there are no
                    // cached credentials to delete. (if the user does not have access to the key,
                    // OpenSubKey will raise a SecurityException)
                    //
                    if (clusterKey != null) {
                        clusterKey.DeleteValue(userName, false);
                    }
                }
                
            }
        }

        internal static string Name2Sid(string username)
        {
            if (string.IsNullOrEmpty(username))
                return string.Empty;

            try
            {
                NTAccount acct = new NTAccount(username);
                return (acct.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier).Value;
            }
            catch { return string.Empty; }
        }

        internal static string Sid2Name(string sid)
        {
            if (string.IsNullOrEmpty(sid))
                return string.Empty;

            try
            {
                SecurityIdentifier acct = new SecurityIdentifier(sid);
                return (acct.Translate(typeof(NTAccount)) as NTAccount).Value;
            }
            catch { return string.Empty; }
        }
    }
}

