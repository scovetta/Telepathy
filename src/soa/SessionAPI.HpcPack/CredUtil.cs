//------------------------------------------------------------------------------
// <copyright file="CredUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      It is a utility class for the credential.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using Properties;
    using Internal;
    using Store;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

#if !net40
    using AADAuthUtil;
#endif

    /// <summary>
    /// It is a utility class for the credential
    /// </summary>
    public class CredUtil
    {
        /// <summary>
        /// Pops up dialog asking users which type of credential they expect to provide. The behavior aligns with job submission.
        /// </summary>
        internal static CredType PromptForCredentialType(bool console, IntPtr hwndParent, CredType originalType)
        {
            string[] dialogTexts = new string[]
                {
                    SR.CredentialTypeDialog_LabelString,
                    SR.CredentialTypeDialog_PwdCheckBoxString,
                    SR.CredentialTypeDialog_CertCheckBoxString,
                    SR.CredentialTypeDialog_OkButtonString,
                    SR.CredentialTypeDialog_CancelButtonString
                };

            int choice = CredentialType.NoChoice;
            CredentialType.PromptForCredentialType(console, hwndParent, ref choice, dialogTexts);

            if (choice == CredentialType.CertChoice)
            {
                return CredType.Certificate;
            }
            else
            {
                Debug.Assert(originalType == CredType.Either || originalType == CredType.Either_CredUnreusable);
                if (originalType == CredType.Either)
                {
                    return CredType.Password;
                }
                else
                {
                    return CredType.Password_CredUnreusable;
                }
            }
        }

        /// <summary>
        /// Get certificate from the store at local machine.
        /// </summary>
        internal static byte[] GetCertFromStore(string thumbprint, string templateName, bool _fConsole, IntPtr _hWnd, out SecureString pfxPassword)
        {
            string[] certChooserStrings = new string[]
                {
                    SR.CertificateChooser_Title,
                    SR.CertificateChooser_Text
                };

            return CertificateHelper.GetCertFromStore(thumbprint, templateName, _fConsole, _hWnd, out pfxPassword, certChooserStrings);
        }

        /// <summary>
        /// Get CredType from the specified fault code.
        /// </summary>
        /// <param name="faultCode">fault code</param>
        /// <returns>credential type</returns>
        internal static CredType GetCredTypeFromFaultCode(int faultCode)
        {
            switch (faultCode)
            {
                // back-compact: the previous version server only returns SOAFaultCode.AuthenticationFailure,
                // which means it needs users to input the password.
                case SOAFaultCode.AuthenticationFailure:
                    return CredType.Password;

                case SOAFaultCode.AuthenticationFailure_NeedPasswordOnly_UnReusable:
                    return CredType.Password_CredUnreusable;

                case SOAFaultCode.AuthenticationFailure_NeedCertOnly:
                    return CredType.Certificate;

                case SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred:
                    return CredType.Either;

                case SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred_UnReusable:
                    return CredType.Either_CredUnreusable;

                default:
                    return CredType.None;
            }
        }


        internal static async Task<CredType> GetCredTypeFromClusterAsync(SessionInitInfoBase info, Binding binding) => await GetCredTypeFromClusterAsync(await info.ResolveHeadnodeMachineAsync().ConfigureAwait(false), binding, info.IsAadOrLocalUser).ConfigureAwait(false);

        /// <summary>
        /// Get the expected credential type according to the cluster parameters
        /// "HpcSoftCard" and "DisableCredentialReuse"
        /// </summary>
        /// <param name="headnode">scheduler name</param>
        /// <param name="binding">indicating the binding</param>
        /// <returns>credential type</returns>
        internal static async Task<CredType> GetCredTypeFromClusterAsync(string headnode, Binding binding, bool isAadUser)
        {
            HpcSoftCardPolicy policy = HpcSoftCardPolicy.Disabled;
            bool disableCredentialReuse = false;
            //TODO: SF: retry
            SessionLauncherClient client = new SessionLauncherClient(headnode, binding, isAadUser);
            try
            {
                List<string> keys = new List<string>() { Constant.HpcSoftCard, Constant.DisableCredentialReuse };
                Dictionary<string, string> result = await client.GetSOAConfigurationsAsync(keys).ConfigureAwait(false);

                string hpcSoftCardString = result[Constant.HpcSoftCard];
                if (!String.IsNullOrEmpty(hpcSoftCardString))
                {
                    try
                    {
                        policy = (HpcSoftCardPolicy)Enum.Parse(typeof(HpcSoftCardPolicy), hpcSoftCardString);
                    }
                    catch (ArgumentException)
                    {
                        // use the default value if the value is not valid
                    }
                }

                string disableCredentialReuseString = result[Constant.DisableCredentialReuse];
                if (!String.IsNullOrEmpty(disableCredentialReuseString))
                {
                    try
                    {
                        disableCredentialReuse = Boolean.Parse(disableCredentialReuseString);
                    }
                    catch (FormatException)
                    {
                        // use the default value if the value is not valid
                    }
                }
            }
            catch (Exception)
            {
                // the previous version server doesn't provide such info, so use the default values
            }
            finally
            {
                Utility.SafeCloseCommunicateObject(client);
            }

            switch (policy)
            {
                case HpcSoftCardPolicy.Disabled:
                    if (disableCredentialReuse)
                    {
                        return CredType.Password_CredUnreusable;
                    }
                    else
                    {
                        return CredType.Password;
                    }

                case HpcSoftCardPolicy.Allowed:
                    if (disableCredentialReuse)
                    {
                        return CredType.Either_CredUnreusable;
                    }
                    else
                    {
                        return CredType.Either;
                    };

                case HpcSoftCardPolicy.Required:
                    return CredType.Certificate;

                default:
                    return CredType.None;
            }
        }

        /// <summary>
        /// Popup dialog or command prompt for cert selection.
        /// It is a little from the scheduler cert selection. It lists the certs in LocalMachine->TrustedRoot
        /// for selection, and returns the thumbprint rather than cert bytes.
        /// </summary>
        /// <param name="isConsole">is console or not</param>
        /// <returns>cert thumbprint</returns>
        internal static string GetCertForSoaClient(bool isConsole)
        {
            X509Store store = null;

            try
            {
                store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection collection = store.Certificates;
                if (collection == null || collection.Count == 0)
                {
                    return null;
                }

                collection = collection.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

                X509Certificate2Collection validCollection = new X509Certificate2Collection();
                foreach (X509Certificate2 cert in collection)
                {
                    if (cert.HasPrivateKey)
                    {
                        validCollection.Add(cert);
                    }
                }

                if (validCollection.Count == 0)
                {
                    return null;
                }

                if (validCollection.Count == 1)
                {
                    return validCollection[0].Thumbprint;
                }
                else
                {
                    X509Certificate2 chosenCert = null;
                    if (isConsole)
                    {
                        Console.WriteLine(SR.CertificateChooser_Title);
                        for (int i = 0; i < validCollection.Count; i++)
                        {
                            Console.WriteLine("{0})\tThumbprint: {1}", i + 1, validCollection[i].Thumbprint);
                            Console.WriteLine("\tSubject Name: {0}", validCollection[i].SubjectName.Name);
                            Console.WriteLine("\tValid from: {0}", validCollection[i].NotBefore);
                            Console.WriteLine("\tValid to: {0}", validCollection[i].NotAfter);
                        }

                        Console.WriteLine("Enter the index of certificate to use:");
                        string input = Console.ReadLine();

                        int choice = -1;
                        if (Int32.TryParse(input, out choice))
                        {
                            if (choice > 0 && choice <= validCollection.Count)
                            {
                                chosenCert = validCollection[choice - 1];
                            }
                        }
                    }
                    else
                    {
                        X509Certificate2Collection chosenCertCol = X509Certificate2UI.SelectFromCollection(
                            validCollection,
                            SR.CertificateChooser_Title,
                            SR.CertificateChooser_Text_Azure,
                            X509SelectionFlag.SingleSelection);

                        if (chosenCertCol.Count > 0)
                        {
                            chosenCert = chosenCertCol[0];
                        }
                    }

                    if (chosenCert != null)
                    {
                        return chosenCert.Thumbprint;
                    }
                }
            }
            catch
            { }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }

            return null;
        }

#if !net40
        /// <summary>
        /// This method will query head node for cluster AAD info if cluster is an Azure IaaS cluster. 
        /// </summary>
        /// <param name="headnode">Headnode address of cluster headnode</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<string> GetSoaAadJwtToken(string headnode, string username, string password) => await ClientCredExtension.GetSoaAadJwtToken(headnode, username, password);
#endif
    }
}
