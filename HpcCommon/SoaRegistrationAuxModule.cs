namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public static class SoaRegistrationAuxModule
    {
        private const int MutexTimeOut = 5000;

        /// <summary>
        /// The interval to do the retry
        /// </summary>
        private const int RetryInterval = 3 * 1000;

        /// <summary>
        /// The handle that identifies a directory.
        /// </summary>
        private const int FILE_ATTRIBUTE_DIRECTORY = 0x10;

        /// <summary>
        /// The network path was not found.
        /// </summary>
        private const int ERROR_BAD_NETPATH = 0x35;

        /// <summary>
        /// Environment Variable to pass the localtion of the process.
        /// </summary>
        private const string OnAzureEnvVar = "CCP_ONAZURE";

        /// <summary>
        /// It is only used at service side. It indicates if current process is running on Azure. Check Env Var "CCP_ONAZURE".
        /// </summary>
        /// <returns>on azure or not</returns>
        public static bool IsOnAzure => Environment.GetEnvironmentVariable(OnAzureEnvVar) == "1";

        public static string ConfigExtensionName => @".config";

        public static async Task ImportServiceRegistrationFromFileAuxAsync(Func<string, Version, string, Task> setServiceRegistrationAsync, string filePath, string serviceName)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists)
            {
                var keyName = string.IsNullOrEmpty(serviceName) ? fileInfo.Name.Substring(0, fileInfo.Name.LastIndexOf('.')) : serviceName;
                await setServiceRegistrationAsync(keyName.ToLowerInvariant(), null, File.ReadAllText(fileInfo.FullName)).ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException($"FilePath not found: {filePath}.");
            }
        }

        public static async Task ExportServiceRegistrationToFileAuxAsync(Func<string, Version, Task<string>> getServiceRegistrationAsync, string serviceName, Version serviceVersion, string fileName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("Service name is null or empty.");
            }

            string serviceNameWithNoEx = serviceName.RemoveConfigExtensionName();

            string serviceConfig = await getServiceRegistrationAsync(serviceNameWithNoEx, serviceVersion).ConfigureAwait(false);
            if (string.IsNullOrEmpty(serviceConfig))
            {
                throw new InvalidOperationException($"Service Config {serviceNameWithNoEx} does not exist.");
            }

            File.WriteAllText(fileName, serviceConfig);
        }

        public static async Task<string> ExportServiceRegistrationToTempFileAuxAsync(Func<string, Version, Task<string>> getServiceRegistrationAsync, string serviceName, Version serviceVersion, string expectedMd5 = "")
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                throw new ArgumentException("Service name is null or empty.");
            }

            string serviceNameWithNoEx = serviceName.RemoveConfigExtensionName();

            // Check if local cache exists once more right before remote call
            if (!string.IsNullOrEmpty(expectedMd5))
            {
                string localCacheFilePath = GetServiceRegistrationTempFilePath(expectedMd5);
                if (File.Exists(localCacheFilePath))
                {
                    return localCacheFilePath;
                }
            }

            string serviceConfig = await getServiceRegistrationAsync(serviceNameWithNoEx, serviceVersion).ConfigureAwait(false);
            if (string.IsNullOrEmpty(serviceConfig))
            {
                return string.Empty;
            }

            string md5 = CalculateMd5Hash(serviceConfig);
            if (!string.IsNullOrEmpty(expectedMd5) && (md5 != expectedMd5))
            {
                Trace.TraceError("[{0}]File MD5 mismatch. Expected {1}, got {2}", nameof(ExportServiceRegistrationToTempFileAuxAsync), expectedMd5, md5);
                throw new InvalidOperationException($"File MD5 mismatch. Expected {expectedMd5}, got {md5}");
            }

            string tempFilePath = GetServiceRegistrationTempFilePath(md5);
            if (File.Exists(tempFilePath))
            {
                return tempFilePath;
            }
            else
            {
                using (new GlobalMutex(md5, MutexTimeOut))
                {
                    if (!File.Exists(tempFilePath))
                    {
                        using (var fs = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            using (var writer = new StreamWriter(fs))
                            {
                                writer.WriteAsync(serviceConfig).GetAwaiter().GetResult();
                            }
                        }
                    }
                }

                return tempFilePath;
            }
        }

        private static string RemoveConfigExtensionName(this string serviceName)
        {
            if (serviceName.EndsWith(ConfigExtensionName, StringComparison.InvariantCultureIgnoreCase))
            {
                return RemoveConfigExtensionName(serviceName.Substring(0, serviceName.Length - ConfigExtensionName.Length));
            }
            else
            {
                return serviceName;
            }
        }

        public static string GetRegistrationName(string serviceName, Version serviceVersion)
        {
            string valueName = serviceName.RemoveConfigExtensionName();
            if (serviceVersion != null)
            {
                valueName = valueName + "_" + serviceVersion.ToString();
            }

            return valueName;
        }

        public static string GetRegistrationFileName(string serviceName, Version serviceVersion) => GetRegistrationName(serviceName, serviceVersion) + ConfigExtensionName;

        public static bool IsRegistrationStoreToken(string str) => string.Equals(str, HpcConstants.RegistrationStoreToken, StringComparison.OrdinalIgnoreCase);

        public static string CalculateMd5Hash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string GetServiceRegistrationTempFilePath(string fileName) => Path.GetTempPath() + fileName + ConfigExtensionName;

        /// <summary>
        /// Win32 API GetFileAttributes
        /// </summary>
        /// <param name="lpFileName"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetFileAttributes(string lpFileName);

        /// <summary>
        /// Check if the file exits by win32 API so that we can retry in certain cases
        /// </summary>
        /// <param name="path">the file path</param>
        /// <returns>true if file exits, otherwise, false</returns>
        public static bool InternalFileExits(string path)
        {
            for (int retry = 0; retry < 3; retry++)
            {
                int value = GetFileAttributes(path);
                if (value == -1)
                {
                    // If API calls failed
                    if (Marshal.GetLastWin32Error() == ERROR_BAD_NETPATH)
                    {
                        Thread.Sleep(RetryInterval);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                //
                // Check if we finds a folder instead of a file
                //
                if ((value & FILE_ATTRIBUTE_DIRECTORY) != 0)
                {
                    break;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the lastest sub-directory under the specified directory.
        /// The sub-directory name is a timestamp in yyyy-mm-ddThh:mm:ss.Z format,
        /// but ':' is removed, which is an invalid char for folder name. So can't
        /// call DateTime.Parse to convert it to a DateTime.
        /// </summary>
        /// <param name="directoryPath">the full path of the specified directory</param>
        /// <returns>the full path of the sub-directory</returns>
        private static string FindLatestSubDirectory(string directoryPath)
        {
            string latestDirectory = string.Empty;
            if (Directory.Exists(directoryPath))
            {
                string latestTimestamp = string.Empty;
                foreach (string path in Directory.GetDirectories(directoryPath))
                {
                    string directoryName = (new DirectoryInfo(path)).Name;
                    if (string.Compare(directoryName, latestTimestamp, StringComparison.OrdinalIgnoreCase) > 0)
                    {
                        latestTimestamp = directoryName;
                        latestDirectory = path;
                    }
                }
            }

            return latestDirectory;
        }

        /// <summary>
        /// Get the full path of the service registration path.
        /// </summary>
        /// <param name="centralPath">
        /// for azure, it is ccp_package_root.
        /// for on-premise cluster, it is service_registration_path.
        /// </param>
        /// <param name="configFileName">service registration file name</param>
        /// <returns>service registration file path</returns>
        public static string GetServiceRegistrationPath(string centralPath, string configFileName)
        {
            string path = null;
            if (IsOnAzure)
            {
                if (!string.IsNullOrEmpty(centralPath) && Directory.Exists(centralPath) && !string.IsNullOrEmpty(configFileName))
                {
                    string subFolderName = Path.GetFileNameWithoutExtension(configFileName);
                    string result = FindLatestSubDirectory(Path.Combine(centralPath, subFolderName));
                    if (!string.IsNullOrEmpty(result) && Directory.Exists(result))
                    {
                        path = result;
                    }
                }

                // fall back to the home folder, which stores the build-in CcpEchoSvc and HpcServiceHost.exe
                if (string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }
            }
            else
            {
                path = centralPath;
            }

            return Path.Combine(path, configFileName);
        }
    }
}
