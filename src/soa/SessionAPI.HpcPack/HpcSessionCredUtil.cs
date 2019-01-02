namespace Microsoft.Hpc.Scheduler.Session.HpcPack
{
    using System;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    public static class HpcSessionCredUtil
    {
        public static async Task<bool> RetrieveCredentialOnPremise(SessionStartInfo info, CredType expectedCredType, Binding binding, bool bConsole, IntPtr hwnd)
        {
            bool popupDialog = false;

            // Make sure that we have a password and credentials for the user.
            if (string.IsNullOrEmpty(info.Username) || string.IsNullOrEmpty(info.InternalPassword))
            {
                string username = null;

                // First try to get something from the cache.
                if (string.IsNullOrEmpty(info.Username))
                {
                    username = WindowsIdentity.GetCurrent().Name;
                }
                else
                {
                    username = info.Username;
                }

                // Use local machine name for session without service job
                string headnode = info.Headnode;
                if (string.IsNullOrEmpty(headnode))
                {
                    headnode = Environment.MachineName;
                }

                //TODO: SF: headnode is a gateway string now
                // For back compact, get the cached password if it exists.
                byte[] cached = CredentialHelper.FetchPassword(headnode, username);
                if (cached != null)
                {
                    info.Username = username;
                    info.InternalPassword = Encoding.Unicode.GetString(ProtectedData.Unprotect(cached, null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    if (expectedCredType != CredType.None)
                    {
                        if (expectedCredType == CredType.Either || expectedCredType == CredType.Either_CredUnreusable)
                        {
                            // Pops up dialog asking users to specify the type of the credetial (password or certificate).
                            // The behavior here aligns with the job submission.
                            expectedCredType = CredUtil.PromptForCredentialType(bConsole, hwnd, expectedCredType);
                        }

                        Debug.Assert(expectedCredType == CredType.Password
                            || expectedCredType == CredType.Password_CredUnreusable
                            || expectedCredType == CredType.Certificate);

                        if (expectedCredType == CredType.Password)
                        {
                            bool fSave = false;
                            SecureString password = null;
                            Credentials.PromptForCredentials(headnode, ref username, ref password, ref fSave, bConsole, hwnd);
                            popupDialog = true;

                            info.Username = username;
                            info.SavePassword = fSave;
                            info.InternalPassword = Credentials.UnsecureString(password);
                        }
                        else if (expectedCredType == CredType.Password_CredUnreusable)
                        {
                            SecureString password = null;
                            Credentials.PromptForCredentials(headnode, ref username, ref password, bConsole, hwnd);
                            popupDialog = true;

                            info.Username = username;
                            info.SavePassword = false;
                            info.InternalPassword = Credentials.UnsecureString(password);
                        }
                        else
                        {
                            // Get the value of cluster parameter HpcSoftCardTemplate.
                            SessionLauncherClient client = new SessionLauncherClient(await Utility.GetSessionLauncherAsync(info, binding).ConfigureAwait(false), binding, info.IsAadOrLocalUser);
                            string softCardTemplate = string.Empty;
                            try
                            {
                                softCardTemplate = await client.GetSOAConfigurationAsync(Constant.HpcSoftCardTemplateParam).ConfigureAwait(false);
                            }
                            finally
                            {
                                Utility.SafeCloseCommunicateObject(client);
                            }

                            // Query certificate from local store, and pops up CertSelectionDialog.
                            SecureString pfxPwd;
                            info.Certificate = CredUtil.GetCertFromStore(null, softCardTemplate, bConsole, hwnd, out pfxPwd);
                            info.PfxPassword = Credentials.UnsecureString(pfxPwd);
                        }
                    }
                    else
                    {
                        // Expect to use the cached credential at scheuler side.
                        // Exception may happen later if no cached redential or it is invalid.
                        info.ClearCredential();
                        info.Username = username;
                    }
                }
            }

            return popupDialog;
        }

        public static async Task<bool> RetrieveCredentialOnPremise(SessionAttachInfo info, CredType expectedCredType, Binding binding, bool bConsole, IntPtr hwnd)
        {
            bool popupDialog = false;

            // Make sure that we have a password and credentials for the user.
            if (string.IsNullOrEmpty(info.Username) || string.IsNullOrEmpty(info.InternalPassword))
            {
                string username = null;

                // First try to get something from the cache.
                if (string.IsNullOrEmpty(info.Username))
                {
                    username = WindowsIdentity.GetCurrent().Name;
                }
                else
                {
                    username = info.Username;
                }

                // Use local machine name for session without service job
                string headnode = info.Headnode;
                if (string.IsNullOrEmpty(headnode))
                {
                    headnode = Environment.MachineName;
                }

                // For back compact, get the cached password if it exists.
                byte[] cached = CredentialHelper.FetchPassword(headnode, username);
                if (cached != null)
                {
                    info.Username = username;
                    info.InternalPassword = Encoding.Unicode.GetString(ProtectedData.Unprotect(cached, null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    if (expectedCredType != CredType.None)
                    {
                        if (expectedCredType == CredType.Either || expectedCredType == CredType.Either_CredUnreusable)
                        {
                            // Pops up dialog asking users to specify the type of the credetial (password or certificate).
                            // The behavior here aligns with the job submission.
                            expectedCredType = CredUtil.PromptForCredentialType(bConsole, hwnd, expectedCredType);
                        }

                        Debug.Assert(expectedCredType == CredType.Password
                            || expectedCredType == CredType.Password_CredUnreusable
                            || expectedCredType == CredType.Certificate);

                        if (expectedCredType == CredType.Password)
                        {
                            bool fSave = false;
                            SecureString password = null;
                            Credentials.PromptForCredentials(headnode, ref username, ref password, ref fSave, bConsole, hwnd);
                            popupDialog = true;

                            info.Username = username;
                            info.SavePassword = fSave;
                            info.InternalPassword = Credentials.UnsecureString(password);
                        }
                        else if (expectedCredType == CredType.Password_CredUnreusable)
                        {
                            SecureString password = null;
                            Credentials.PromptForCredentials(headnode, ref username, ref password, bConsole, hwnd);
                            popupDialog = true;

                            info.Username = username;
                            info.SavePassword = false;
                            info.InternalPassword = Credentials.UnsecureString(password);
                        }
                        else
                        {
                            // Get the value of cluster parameter HpcSoftCardTemplate.
                            SessionLauncherClient client = new SessionLauncherClient(await Utility.GetSessionLauncherAsync(info, binding).ConfigureAwait(false), binding, info.IsAadOrLocalUser);
                            string softCardTemplate = string.Empty;
                            try
                            {
                                softCardTemplate = await client.GetSOAConfigurationAsync(Constant.HpcSoftCardTemplateParam).ConfigureAwait(false);
                            }
                            finally
                            {
                                Utility.SafeCloseCommunicateObject(client);
                            }

                            // Query certificate from local store, and pops up CertSelectionDialog.
                            SecureString pfxPwd;
                            info.Certificate = CredUtil.GetCertFromStore(null, softCardTemplate, bConsole, hwnd, out pfxPwd);
                            info.PfxPassword = Credentials.UnsecureString(pfxPwd);
                        }
                    }
                    else
                    {
                        // Expect to use the cached credential at scheuler side.
                        // Exception may happen later if no cached redential or it is invalid.
                        info.ClearCredential();
                        info.Username = username;
                    }
                }
            }

            return popupDialog;
        }
    }
}
