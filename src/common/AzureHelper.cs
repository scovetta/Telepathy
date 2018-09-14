namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.IO;
    using System.Security.Cryptography.Pkcs;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    using Microsoft.WindowsAzure.ServiceRuntime;

    internal static class AzureHelper
    {
        private const string VcRedistPath64 = @"vcredist\64";
        private const string VcRedistPath32 = @"vcredist\32";
        private static string[] VcRedist = new string[] { "concrt140.dll", "msvcp140.dll", "vccorlib140.dll", "vcruntime140.dll", "ucrtbased.dll" };
#if DEBUG
        private static string[] VcRedistDebug = new string[] { "concrt140d.dll", "msvcp140d.dll", "vccorlib140d.dll", "vcruntime140d.dll" };
#endif
        private static string AppRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        static bool? _isNettcpOver443 = null;

        /// <summary>
        /// Check whether the scheduler is in Azure
        /// </summary>
        public static bool IsNettcpOver443
        {
            get
            {
                if (_isNettcpOver443 == null)
                {
                    _isNettcpOver443 = true;
                    try
                    {
                        _isNettcpOver443 = Boolean.Parse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.NettcpOver443));
                    }
                    catch
                    {
                        // Swallow, ignore any error here
                    }
                }
                return (bool)_isNettcpOver443;
            }
        }

        static bool? hpcSyncFailureEnable = null;

        /// <summary>
        /// Check whether need reinit the role when HpcSync failed
        /// </summary>
        public static bool HpcSyncFailureEnable
        {
            get
            {
                if (hpcSyncFailureEnable == null)
                {
                    bool value = false;
                    Boolean.TryParse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.AzureHpcSyncFailureEnable), out value);
                    hpcSyncFailureEnable = value;
                }

                return hpcSyncFailureEnable.Value;
            }
        }

        static bool? azureStartupTaskFailureEnable = null;

        /// <summary>
        /// Check whether need handle startup task failure
        /// </summary>
        public static bool AzureStartupTaskFailureEnable
        {
            get
            {
                if (azureStartupTaskFailureEnable == null)
                {
                    bool value = false;
                    Boolean.TryParse(RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.AzureStartupTaskFailureEnable), out value);
                    azureStartupTaskFailureEnable = value;
                }

                return azureStartupTaskFailureEnable.Value;
            }
        }

        static bool? _isSchedulerOnAzure = null;

        /// <summary>
        /// Check whether the scheduler is in Azure
        /// </summary>
        public static bool IsSchedulerOnAzure
        {
            get
            {
                if (_isSchedulerOnAzure == null)
                {
                    _isSchedulerOnAzure = false;
                    string schedulerRole = null;
                    try
                    {
                        schedulerRole = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.SchedulerRole);

                    }
                    catch
                    {
                        // Swallow, expected
                    }

                    if (schedulerRole != null)
                    {
                        _isSchedulerOnAzure = true;
                    }
                }
                return (bool)_isSchedulerOnAzure;
            }
        }

        static bool? _schedulerNodeCoExist = null;
        public static bool SchedulerNodeCoExist
        {
            get
            {
                if (!IsSchedulerOnAzure)
                {
                    return false;
                }

                if (_schedulerNodeCoExist == null)
                {
                    _schedulerNodeCoExist = false;

                    string schedulerRole = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.SchedulerRole);
                    foreach (string nodeRoleStr in RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.NodeRoles).Split(';'))
                    {
                        if (string.Compare(schedulerRole, nodeRoleStr.Split('=')[0], StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _schedulerNodeCoExist = true;
                            break;
                        }
                    }
                }

                return (bool)_schedulerNodeCoExist;
            }
        }

        /// <summary>
        /// Find a certificate in localmachine/My with the given name, and use its private key
        /// to decrypt a encrypted text, presumably a password
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <param name="certName"></param>
        /// <returns></returns>
        public static string DecryptWithCertificate(string encryptedText, string thumbprint)
        {
            X509Certificate2 cert = FindCert(thumbprint);

            return DecryptWithCertificate(encryptedText, cert);
        }

        public static string DecryptWithCertificate(string encryptedText, X509Certificate2 cert)
        {
            EnvelopedCms env = new EnvelopedCms();
            byte[] decodedBytes = Convert.FromBase64String(encryptedText);
            env.Decode(decodedBytes);
            env.Decrypt(new X509Certificate2Collection(cert));
            byte[] decryptedBytes = env.ContentInfo.Content;
            string decryptedString = Encoding.UTF8.GetString(decryptedBytes);

            return decryptedString;
        }

        public static string EncryptWithCertificate(string clearText, string thumbprint)
        {
            X509Certificate2 cert = FindCert(thumbprint);

            return EncryptWithCertificate(clearText, cert);
        }

        public static string EncryptWithCertificate(string clearText, X509Certificate2 cert)
        {
            byte[] passbytes = Encoding.UTF8.GetBytes(clearText);

            ContentInfo content = new ContentInfo(passbytes);
            EnvelopedCms env = new EnvelopedCms(content);
            CmsRecipient recip = new CmsRecipient(cert);
            env.Encrypt(recip);

            string encryptedText = Convert.ToBase64String(env.Encode());
            return encryptedText;
        }

        internal static X509Certificate2 FindCert(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);

            X509Certificate2 cert = null;
            foreach (var c in store.Certificates)
            {
                //Console.WriteLine(c.SubjectName.Name);
                if (string.Compare(c.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    cert = c;
                    break;
                }
            }

            if (cert == null)
            {
                throw new InvalidProgramException("Certificate with thumbprint " + thumbprint + " isn't found.");
            }

            return cert;
        }

        /// <summary>
        /// Get Azure node name from environment variable
        /// </summary>
        /// <returns>Name of node</returns>
        public static string GetLogicalNodeName()
        {
            // First try to find logical name - only on Azure
            string nodeEnv = Environment.GetEnvironmentVariable("CCP_LOGICALNAME");
            if (!String.IsNullOrEmpty(nodeEnv))
            {
                return nodeEnv;
            }

            // If we can't find the environment variable or something is wrong, just return the current machine name
            return Environment.MachineName;
        }

        public static void LoadClusterAdminUserPass(out string username, out string password)
        {
            username = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.AdminAccount);

            password = AzureHelper.DecryptWithCertificate(
                        RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.AdminEncryptedPassword),
                        RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.PasswordCertThumbprint));
        }

        public static void ConfigureVcRuntime()
        {
            string sys32Path = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "System32");
            string wowPath = Path.Combine(Environment.GetEnvironmentVariable("WINDIR"), "SysWow64");

            //
            // Copy the appropriate vc++ runtime binaries into the system and syswow64 directories.
            //

            // 64bit goes into system32
            foreach (string dllName in VcRedist)
            {
                CopyFileIntoSystemFolder(sys32Path, dllName, VcRedistPath64);
            }

            // 32bit into syswow64
            foreach (string dllName in VcRedist)
            {
                CopyFileIntoSystemFolder(wowPath, dllName, VcRedistPath32);
            }
#if DEBUG
            foreach (string dllName in VcRedistDebug)
            {
                CopyFileIntoSystemFolder(sys32Path, dllName, VcRedistPath64);
            }

            foreach (string dllName in VcRedistDebug)
            {
                CopyFileIntoSystemFolder(wowPath, dllName, VcRedistPath32);
            }
#endif
        }

        private static void CopyFileIntoSystemFolder(string destRoot, string fileName, string path)
        {
            var dest = Path.Combine(destRoot, fileName);
            if (File.Exists(dest)) return;
            var srcFile = Path.Combine(AppRoot, path, fileName);
            File.Copy(srcFile, dest);
        }
    }
}
