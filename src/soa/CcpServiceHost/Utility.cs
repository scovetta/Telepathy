//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Utility for service host
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.CcpServiceHosting
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    using TelepathyCommon;

    using RuntimeTraceHelper = Microsoft.Hpc.RuntimeTrace.TraceHelper;
    /// <summary>
    /// Utility for service host
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Get service assembly file
        /// </summary>
        /// <param name="serviceConfigName">indicating service configuration file</param>
        /// <param name="onAzure">on azure or in on-premise cluster</param>
        /// <param name="registration">output service registration instance</param>
        /// <param name="serviceAssemblyFullPath">full path of the soa service assembly</param>
        /// <returns>returns return code</returns>
        public static int GetServiceRegistration(string serviceConfigName, bool onAzure, out ServiceRegistration registration, out string serviceAssemblyFullPath)
        {
            // Open the service registration configuration section
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = serviceConfigName;

            Configuration config = null;
            RetryManager.RetryOnceAsync(
                    () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                    TimeSpan.FromSeconds(1),
                    ex => ex is ConfigurationErrorsException)
                .GetAwaiter()
                .GetResult();
            Debug.Assert(config != null, "Configuration is not opened properly.");

            registration = ServiceRegistration.GetSectionGroup(config);

            serviceAssemblyFullPath = registration.Service.AssemblyPath;
            if (!string.IsNullOrEmpty(serviceAssemblyFullPath))
            {
                serviceAssemblyFullPath = Environment.ExpandEnvironmentVariables(serviceAssemblyFullPath);
            }

            if (onAzure)
            {
                bool exists = false;

                try
                {
                    exists = File.Exists(serviceAssemblyFullPath);
                }
                catch (Exception e)
                {
                    RuntimeTraceHelper.TraceEvent(
                        TraceEventType.Error,
                        SoaHelper.CreateTraceMessage(
                            "Utility",
                            "GetServiceLocalCacheFullPath",
                            string.Format("Exception happens when check the file existence. {0}", e)));
                }

                if (!exists)
                {
                    // If we find the service registration file under the specified path, use it.
                    // Otherwise, fall back to the package root folder. The service assembly is under the same folder as the registration file.
                    serviceAssemblyFullPath = Path.Combine(Path.GetDirectoryName(serviceConfigName), Path.GetFileName(serviceAssemblyFullPath));
                }
            }

            if (string.IsNullOrEmpty(serviceAssemblyFullPath))
            {
                string message = string.Format(CultureInfo.CurrentCulture, StringTable.AssemblyFileNotRegistered, serviceConfigName);
                Console.Error.WriteLine(message);
                RuntimeTraceHelper.TraceEvent(TraceEventType.Error, message);

                return ErrorCode.ServiceHost_AssemblyFileNameNullOrEmpty;
            }

            if (!File.Exists(serviceAssemblyFullPath))
            {
                // If the obtained service assembly path is not valid or not existing
                string message = string.Format(CultureInfo.CurrentCulture, StringTable.AssemblyFileCantFind, serviceAssemblyFullPath);
                Console.Error.WriteLine(message);
                RuntimeTraceHelper.TraceEvent(TraceEventType.Error, message);

                return ErrorCode.ServiceHost_AssemblyFileNotFound;
            }

            return ErrorCode.Success;
        }

        /// <summary>
        /// Get the full path of the local cache for the service config file,
        /// which is specified by Constant.ServiceConfigFileNameEnvVar.
        /// </summary>
        /// <returns>local cache folder path</returns>
        public static string GetServiceLocalCacheFullPath()
        {
            string path = SoaHelper.GetCcpPackageRoot();
            if (string.IsNullOrEmpty(path))
            {
                RuntimeTraceHelper.TraceEvent(
                    TraceEventType.Error,
                    SoaHelper.CreateTraceMessage(
                        "Utility",
                        "GetServiceLocalCacheFullPath",
                        "The env var CCP_PACKAGE_ROOT has no value"));
            }
            else
            {
                if (Directory.Exists(path))
                {
                    string configFileName = Environment.GetEnvironmentVariable(Constant.ServiceConfigFileNameEnvVar);
                    if (string.IsNullOrEmpty(configFileName))
                    {
                        RuntimeTraceHelper.TraceEvent(
                            TraceEventType.Error,
                            SoaHelper.CreateTraceMessage(
                                "Utility",
                                "GetServiceLocalCacheFullPath",
                                "The env var CCP_SERVICE_CONFIG_FILENAME has no value"));
                    }
                    else
                    {
                        string subFolderName = Path.GetFileNameWithoutExtension(configFileName);
                        string result = FindLatestSubDirectory(Path.Combine(path, subFolderName));
                        if (!string.IsNullOrEmpty(result) && Directory.Exists(result))
                        {
                            return result;
                        }
                    }
                }
            }

            // fall back to the home folder, which stores the build-in CcpEchoSvc and HpcSeviceHost.exe
            string home = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            RuntimeTraceHelper.TraceEvent(
                TraceEventType.Information,
                SoaHelper.CreateTraceMessage(
                    "Utility",
                    "GetServiceLocalCacheFullPath",
                    string.Format(CultureInfo.InvariantCulture, "Fall back to the home folder {0}.", home)));

            return home;
        }

        /// <summary>
        /// Set the trace level according to the env var
        /// </summary>
        public static void SetTraceSwitchLevel()
        {
            SourceLevels level = Utility.GetTraceSwitchLevel();

            ServiceContext.Logger.Switch.Level = level;

            if (level != SourceLevels.Off)
            {
                RuntimeTraceHelper.IsDiagTraceEnabled =  x => true;

                RuntimeTraceHelper.TraceEvent(
                    TraceEventType.Information,
                    SoaHelper.CreateTraceMessage(
                        "Utility",
                        "SetTraceSwitchLevel",
                        "Add the SoaDiagTraceListener."));

                ServiceContext.Logger.Listeners.Add(new SoaDiagTraceListener(Utility.GetJobId()));
            }
            else
            {
                RuntimeTraceHelper.TraceEvent(
                    TraceEventType.Information,
                    SoaHelper.CreateTraceMessage(
                        "Utility",
                        "SetTraceSwitchLevel",
                        "SoaDiagTrace is disabled."));
            }
        }

        /// <summary>
        /// Get switch level from job env var.
        /// </summary>
        /// <returns>trace level</returns>
        private static SourceLevels GetTraceSwitchLevel()
        {
            string values = Environment.GetEnvironmentVariable(Constant.TraceSwitchValue);

            RuntimeTraceHelper.TraceEvent(
                TraceEventType.Information,
                "{0}={1}",
                Constant.TraceSwitchValue,
                values);

            SourceLevels level = SourceLevels.Off;

            if (!string.IsNullOrEmpty(values))
            {
                string[] levelStrings = values.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string value in levelStrings)
                {
                    try
                    {
                        level |= (SourceLevels)Enum.Parse(typeof(SourceLevels), value, true);
                    }
                    catch (ArgumentException)
                    {
                        RuntimeTraceHelper.TraceEvent(
                            TraceEventType.Error,
                            SoaHelper.CreateTraceMessage(
                                "Utility",
                                "GetTraceSwitchLevel",
                                string.Format(CultureInfo.CurrentCulture, "{0} is not a correct value of SourceLevels.", value)));

                        return SourceLevels.Off;
                    }
                }

                RuntimeTraceHelper.TraceEvent(
                    TraceEventType.Information,
                    SoaHelper.CreateTraceMessage(
                        "Utility",
                        "GetTraceSwitchLevel",
                        string.Format(CultureInfo.InvariantCulture, "The trace switch level is {0}.", level)));
            }

            return level;
        }

        /// <summary>
        /// Get job Id from the env var.
        /// </summary>
        /// <returns>job Id of the session</returns>
        public static int GetJobId()
        {
            int jobId;

            string jobIdString = Environment.GetEnvironmentVariable(Constant.JobIDEnvVar);

            if (string.IsNullOrEmpty(jobIdString) || !int.TryParse(jobIdString, out jobId))
            {
                return 0;
            }
            else
            {
                return jobId;
            }
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
    }
}
