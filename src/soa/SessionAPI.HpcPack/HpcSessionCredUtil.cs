namespace Microsoft.Hpc.Scheduler.Session.HpcPack
{
    using System;
    using System.ComponentModel;
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
        static private IntPtr hwnd = IntPtr.Zero;

        public static IntPtr Hwnd => hwnd;

        /// <summary>
        /// Set default value "true"
        /// Keep consistent with v2 soa client
        /// </summary>
        static private bool bConsole = true;

        public static bool BConsole
        {
            get
            {
                return bConsole;
            }
        }

        /// <summary>
        ///   <para>Specifies whether the client is a console or Windows application.</para>
        /// </summary>
        /// <param name="console">
        ///   <para>Set to True if the client is a console application; otherwise, set to False.</para>
        /// </param>
        /// <param name="wnd">
        ///   <para>The handle to the parent window if the client is a Windows application.</para>
        /// </param>
        /// <remarks>
        ///   <para>This information is used to determine how to prompt the user for the credentials if the credentials are 
        /// not specified in the job. If you do not call this method, the client is assumed to be a console application.</para>
        /// </remarks>
        public static void SetInterfaceMode(bool console, IntPtr wnd)
        {
            bConsole = console;
            hwnd = wnd;
        }

        public static async Task<bool> RetrieveCredentialOnPremise(SessionStartInfo info, CredType expectedCredType, Binding binding)
        {
            bool popupDialog = false;

            // Make sure that we have a password and credentials for the user.
            if (String.IsNullOrEmpty(info.Username) || String.IsNullOrEmpty(info.InternalPassword))
            {
                string username = null;

                // First try to get something from the cache.
                if (String.IsNullOrEmpty(info.Username))
                {
                    username = WindowsIdentity.GetCurrent().Name;
                }
                else
                {
                    username = info.Username;
                }

                // Use local machine name for session without service job
                string headnode = info.Headnode;
                if (String.IsNullOrEmpty(headnode))
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
                            string softCardTemplate = String.Empty;
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

        public static async Task<bool> RetrieveCredentialOnPremise(SessionAttachInfo info, CredType expectedCredType, Binding binding)
        {
            bool popupDialog = false;

            // Make sure that we have a password and credentials for the user.
            if (String.IsNullOrEmpty(info.Username) || String.IsNullOrEmpty(info.InternalPassword))
            {
                string username = null;

                // First try to get something from the cache.
                if (String.IsNullOrEmpty(info.Username))
                {
                    username = WindowsIdentity.GetCurrent().Name;
                }
                else
                {
                    username = info.Username;
                }

                // Use local machine name for session without service job
                string headnode = info.Headnode;
                if (String.IsNullOrEmpty(headnode))
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
                            string softCardTemplate = String.Empty;
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

        /// <summary>
        /// Validate the credential.
        /// Throw AuthenticationException if validation fails.
        /// </summary>
        /// <param name="info">session start info contains credential</param>
        internal static void CheckCredential(SessionStartInfo info)
        {
            if (info.InternalPassword != null)
            {
                // Verify the username password if we can.
                // Verify the cached credential in case it is expired.
                Credentials.ValidateCredentials(info.Username, info.InternalPassword, true);
            }
            else
            {
                // For back-compact, don't transmit null password to session service, which can causes exception there.
                // It is fine to replace null by empty string even user's password is empty string.
                info.InternalPassword = String.Empty;
            }
        }

        internal static void CheckCredential(SessionAttachInfo info)
        {
            if (info.InternalPassword != null)
            {
                // Verify the username password if we can.
                // Verify the cached credential in case it is expired.
                Credentials.ValidateCredentials(info.Username, info.InternalPassword, true);
            }
            else
            {
                // For back-compact, don't transmit null password to session service, which can causes exception there.
                // It is fine to replace null by empty string even user's password is empty string.
                info.InternalPassword = String.Empty;
            }
        }

        /// <summary>
        /// Get user's credential for the Azure cluster.
        /// We can't validate such credential at on-premise client.
        /// This method doesn't save the credential.
        /// </summary>
        /// <returns>pops up credential dialog or not</returns>
        internal static bool RetrieveCredentialOnAzure(SessionStartInfo info)
        {
            string username = info.Username;
            string internalPassword = info.InternalPassword;
            bool savePassword = info.SavePassword;

            bool result = RetrieveCredentialOnAzure(info.Headnode, ref username, ref internalPassword, ref savePassword);

            info.Username = username;
            info.InternalPassword = internalPassword;
            info.SavePassword = savePassword;
            return result;
        }

        /// <summary>
        /// Get user's credential for Azure cluster when attaching session.
        /// </summary>
        /// <param name="info">Session attach info</param>
        /// <returns>pops up credential dialog or not</returns>
        internal static bool RetrieveCredentialOnAzure(SessionAttachInfo info)
        {
            string username = info.Username;
            string internalPassword = info.InternalPassword;
            bool savePassword = info.SavePassword;

            bool result = RetrieveCredentialOnAzure(info.Headnode, ref username, ref internalPassword, ref savePassword);

            info.Username = username;
            info.Password = internalPassword;
            info.SavePassword = savePassword;
            return result;
        }

        /// <summary>
        /// Get user's credential for the Azure cluster.
        /// We can't validate such credential at on-premise client.
        /// This method doesn't save the credential.
        /// </summary>
        ///<param name="headnode">head node name</param>
        ///<param name="username">user name</param>
        ///<param name="internalPassword">user password</param>
        ///<param name="savePassword">save password or not</param>
        internal static bool RetrieveCredentialOnAzure(string headnode, ref string username, ref string internalPassword, ref bool savePassword)
        {
            if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(internalPassword))
            {
                return false;
            }

            // Try to get the default username if it is not specified.
            if (String.IsNullOrEmpty(username))
            {
                username = CredentialHelper.FetchDefaultUsername(headnode);
            }

            // If the username is specified, try to get password from Windows Vault.
            if (!String.IsNullOrEmpty(username))
            {
                byte[] cached = CredentialHelper.FetchPassword(headnode, username);
                if (cached != null)
                {
                    internalPassword = Encoding.Unicode.GetString(ProtectedData.Unprotect(cached, null, DataProtectionScope.CurrentUser));

                    return false;
                }
            }

            // If username or password is not specified, popup credential dialog.
            SecureString password = null;
            Credentials.PromptForCredentials(headnode, ref username, ref password, ref savePassword, bConsole, hwnd);
            internalPassword = Credentials.UnsecureString(password);
            return true;
        }

        /// <summary>
        /// Save the credential at local Windows Vault, if
        /// (1) scheduler is on Azure (credential is needed by session service to run as that user)
        /// (2) debug mode (no scheduler)
        /// (3) scheduler version is before 3.1 (scheduler side credential cache is not supported before 3.1)
        /// </summary>
        /// <param name="info">it contians credential and targeted scheduler</param>
        /// <param name="binding">indicating the binding</param>
        public static void SaveCrendential(SessionStartInfo info, Binding binding)
        {
            if (info.SavePassword && info.InternalPassword != null)
            {
                Debug.Assert(!String.IsNullOrEmpty(info.Headnode), "The headnode can't be null or empty.");

                bool saveToLocal = false;

                if (SoaHelper.IsSchedulerOnAzure(info.Headnode) || SoaHelper.IsSchedulerOnIaaS(info.Headnode))
                {
                    saveToLocal = true;
                }
                else if (info.DebugModeEnabled)
                {
                    saveToLocal = true;
                }

                if (saveToLocal)
                {
                    SaveCrendential(info.Headnode, info.Username, info.InternalPassword);
                    SessionBase.TraceSource.TraceInformation("Cached credential is saved to local Windows Vault.");
                }
                else
                {
                    // the password is already sent to session service, which saves it to the scheduler.
                    SessionBase.TraceSource.TraceInformation("Cached credential is expected to be saved to the scheduler by session service.");
                }
            }
        }

        /// <summary>
        /// Save the credential at local Windows Vault.
        /// </summary>
        /// <param name="info">
        /// attach info
        /// it specifies credential and targeted scheduler
        /// </param>
        internal static void SaveCrendential(SessionAttachInfo info)
        {
            if (info.SavePassword)
            {
                SaveCrendential(info.Headnode, info.Username, info.InternalPassword);
            }
        }

        /// <summary>
        /// Save the user credential to the local windows vault.
        /// </summary>
        /// <param name="headnode">head node name</param>
        /// <param name="username">user name</param>
        /// <param name="password">user password</param>
        internal static void SaveCrendential(string headnode, string username, string password)
        {
            try
            {
                if (password != null)
                {
                    Debug.Assert(!string.IsNullOrEmpty(headnode), "The headnode can't be null or empty.");

                    CredentialHelper.PersistPassword(headnode, username, ProtectedData.Protect(Encoding.Unicode.GetBytes(password), null, DataProtectionScope.CurrentUser));

                    SessionBase.TraceSource.TraceInformation("Cached credential is saved to local Windows Vault.");
                }
            }
            catch (Win32Exception)
            {
                SessionBase.TraceSource.TraceInformation("Cached credential can't be saved to local Windows Vault.");
            }
        }
    }


}
