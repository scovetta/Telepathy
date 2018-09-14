//------------------------------------------------------------------------------
// <copyright file="CertificateHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      It is an utility class for certificate.
//      This source file is shared by StoreAPI and Soa SessionAPI.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Cryptography.X509Certificates;
    using System.Web.Security;

    internal static class CertificateHelper
    {
        /// <summary>
        /// Method used to generate a random password that is currently set to 35 chars length
        /// with 10 special characters
        /// </summary>
        /// <returns>Securestring containing the certificate</returns>
        internal static SecureString GetRandomPassword()
        {
            SecureString pfxPassword = new SecureString();

            foreach (char c in Membership.GeneratePassword(35, 10).ToCharArray())
            {
                pfxPassword.AppendChar(c);
            }

            return pfxPassword;
        }

        internal static byte[] GetCertFromStore(string thumbprint, string templateName, bool _fConsole, IntPtr _hWnd, out SecureString pfxPassword)
        {
            string[] certChooserStrings = {
                                              "Certificate Chooser",
                                              "Choose a certificate to upload to the scheduler"
                                          };
            return GetCertFromStore(thumbprint, templateName, _fConsole, _hWnd, out pfxPassword, certChooserStrings);
        }

        /// <summary>
        /// Pickup single certificate from certificate list based on templateName
        /// User cancel treat as not found certificate
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="templateName"></param>
        /// <param name="_fConsole"></param>
        /// <param name="_hWnd"></param>
        /// <param name="pfxPassword"></param>
        /// <param name="certChooserStrings"></param>
        /// <returns></returns>
        internal static byte[] GetCertFromStore(string thumbprint, string templateName, bool _fConsole, IntPtr _hWnd, out SecureString pfxPassword, string[] certChooserStrings)
        {
            bool cancelled;
            return GetCertFromStore(thumbprint, templateName, _fConsole, _hWnd, out pfxPassword, certChooserStrings, out cancelled);
        }

        /// <summary>
        /// Pickup single certificate from certificate list based on templateName
        /// </summary>
        /// <param name="thumbprint"></param>
        /// <param name="templateName"></param>
        /// <param name="_fConsole"></param>
        /// <param name="_hWnd"></param>
        /// <param name="pfxPassword"></param>
        /// <param name="certChooserStrings"></param>
        /// <param name="cancelled">cancelled will be true if user cancel selection</param>
        /// <returns></returns>
        internal static byte[] GetCertFromStore(string thumbprint, string templateName, bool _fConsole, IntPtr _hWnd, out SecureString pfxPassword, string [] certChooserStrings, out bool cancelled)
        {
            cancelled = false;
            pfxPassword = GetRandomPassword();

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCol;

                //if a template is specified, look at certificates from just that template
                if (string.IsNullOrEmpty(templateName))
                {
                    certCol = store.Certificates;
                }
                else
                {
                    certCol = store.Certificates.Find(X509FindType.FindByTemplateName, templateName, true);
                }

                //if a thumbprint is specified narrow the search by thumbprint
                if (!string.IsNullOrEmpty(thumbprint))
                {
                    certCol = certCol.Find(X509FindType.FindByThumbprint, thumbprint, true);
                }

                if (certCol.Count > 0)
                {
                    certCol = certCol.Find(X509FindType.FindByTimeValid, DateTime.Now, true);

                    X509Certificate2Collection validCertsCol = new X509Certificate2Collection();

                    foreach (X509Certificate2 cert in certCol)
                    {
                        //check if the certificate is valid
                        if (cert.HasPrivateKey)
                        {
                            validCertsCol.Add(cert);
                        }
                    }

                    if (validCertsCol.Count == 0)
                    {
                        //No valid cert found
                        return null;
                    }

                    if (validCertsCol.Count == 1)
                    {
                        //if just one cert was found return it
                        byte[] certBytes = validCertsCol[0].Export(X509ContentType.Pfx, pfxPassword);
                        return certBytes;
                    }
                    else
                    {
                        X509Certificate2 chosenCert = null;

                        if (_fConsole)
                        {
                            Console.WriteLine("Hpc SoftCards available: ");
                            for (int i = 0; i < validCertsCol.Count; i++)
                            {
                                Console.WriteLine("{0})\tThumbprint: {1}", i + 1, validCertsCol[i].Thumbprint);
                                Console.WriteLine("\tValid from: {0}", validCertsCol[i].NotBefore);
                                Console.WriteLine("\tValid to: {0}", validCertsCol[i].NotAfter);
                            }

                            int choice = -1;
                            while (choice == -1)
                            {
                                Console.WriteLine("Enter the number of HPC SoftCard credential to use:");
                                string input = Console.ReadLine();
                                if (Int32.TryParse(input, out choice))
                                {
                                    if (choice > 0 && choice <= validCertsCol.Count)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        choice = -1;
                                    }
                                }
                            }

                            if (choice != -1)
                            {
                                //the choice is base 1, whereas index in certcollection is base 0
                                chosenCert = validCertsCol[choice - 1];
                            }
                            else
                            {
                                cancelled = true;
                            }
                        }
                        else
                        {
                            if (_hWnd == new IntPtr(-1))
                            {
                                throw new InvalidCredentialException();
                            }

                            X509Certificate2Collection chosenCertCol = X509Certificate2UI.SelectFromCollection(validCertsCol,
                                certChooserStrings[0], certChooserStrings[1], X509SelectionFlag.SingleSelection);

                            if (chosenCertCol.Count > 0)
                            {
                                chosenCert = chosenCertCol[0];
                            }
                            else
                            {
                                cancelled = true;
                            }
                        }

                        if (chosenCert != null)
                        {
                            byte[] certBytes = chosenCert.Export(X509ContentType.Pfx, pfxPassword);
                            return certBytes;
                        }
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                if (store != null)
                {
                    store.Close();
                }
            }

            return null;
        }
    }
}
