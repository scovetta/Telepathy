//------------------------------------------------------------------------------
// <copyright file="ServiceRegistration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Common implementation to find the service Registration file
// </summary>
//------------------------------------------------------------------------------

using TelepathyCommon;

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Microsoft.Hpc.Scheduler.Session;

    using static SoaRegistrationAuxModule;

    internal class ServiceRegistrationRepo
    {
        private string[] centralPaths;

        private string centrialPathList;

        internal IServiceRegistrationStore ServiceRegistrationStore { get; private set; }

        internal string ServiceRegistrationStoreFacadeFolder { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ServiceRegistration class
        /// </summary>
        public ServiceRegistrationRepo(string centrialPathList, IServiceRegistrationStore store, string facade)
        {
            if (!string.IsNullOrEmpty(centrialPathList))
            {
                this.centrialPathList = centrialPathList;

                this.centralPaths = centrialPathList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < this.centralPaths.Length; i++)
                {
                    this.centralPaths[i] = Environment.ExpandEnvironmentVariables(this.centralPaths[i]);
                }
            }

            this.ServiceRegistrationStore = store;
            this.ServiceRegistrationStoreFacadeFolder = facade;
        }

        public ServiceRegistrationRepo(string centrialPathList, IServiceRegistrationStore store) : this(centrialPathList, store, null) { }


        public ServiceRegistrationRepo(string centrialPathList) : this(centrialPathList, null)
        {
        }

        /// <summary>
        /// Returns the service registration directories
        /// </summary>
        /// <returns></returns>
        public string[] GetServiceRegistrationDirectories()
        {
            return this.centralPaths;
        }

        /// <summary>
        /// Sets or gets the user setting which is in env 
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared source file")]
        public string CentrialPath
        {
            get
            {
                return this.centrialPathList;
            }
        }

        /// <summary>
        /// Get the name of the service registration file
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="serviceVersion">Version of the service</param>
        /// <returns>file name</returns>
        internal static string GetServiceRegistrationFileName(string serviceName, Version serviceVersion) =>
            GetRegistrationFileName(serviceName, serviceVersion);

        /// <summary>
        /// Get the xml file which config the service.
        /// </summary>
        /// <param name="serviceName">Name of the service</param>
        /// <param name="serviceVersion">Version of the service</param>
        /// <returns>The file path. If no file found, a FileNotFoundException will raise</returns>
        public string GetServiceRegistrationPath(string serviceName, Version serviceVersion)
        {
            return this.GetServiceRegistrationPath(GetServiceRegistrationFileName(serviceName, serviceVersion));
        }

        /// <summary>
        /// Get the xml file which config the service.
        /// </summary>
        /// <param name="fileName">Service config file name</param>
        /// <returns>Return file path, or return null if the file doesn't exist.</returns>
        public string GetServiceRegistrationPath(string filename)
        {
            Trace.TraceInformation($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Try get file {filename}");
            if (this.centralPaths != null)
            {
                foreach (string centralPath in this.centralPaths)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(centralPath) || IsRegistrationStoreToken(centralPath))
                        {
                            Trace.TraceInformation($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Get from reliable registry");
                            string path;
                            if (this.ServiceRegistrationStore != null)
                            {
                                path = this.ServiceRegistrationStore.ExportToTempFileAsync(filename, null).GetAwaiter()
                                    .GetResult();
                            }
                            else
                            {
                                Trace.TraceWarning($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Trying to get service registration from reliable registry while no ServiceRegistrationStore instance available");
                                continue;
                            }

                            if (!string.IsNullOrEmpty(path))
                            {
                                Trace.TraceInformation($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Found file {path}");
                                path = this.MoveFileToFacadeFolder(path);
                                return path;
                            }
                        }
                        else
                        {
                            string path = SoaRegistrationAuxModule.GetServiceRegistrationPath(centralPath, filename);
                            if (InternalFileExits(path))
                            {
                                Trace.TraceInformation($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Found file {path}");
                                return path;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError($"[{nameof(ServiceRegistrationRepo)}] {nameof(GetServiceRegistrationPath)}: Exception happened when find file {filename} in path {centralPath}:{Environment.NewLine}{ex.ToString()}");
                    }
                }
            }

            return null;
        }

        private string MoveFileToFacadeFolder(string filePath)
        {
            Trace.TraceInformation(
                $"[{nameof(ServiceRegistrationRepo)}] {nameof(this.MoveFileToFacadeFolder)}: Copy file {filePath} to {this.ServiceRegistrationStoreFacadeFolder}");

            try
            {
                if (!string.IsNullOrWhiteSpace(this.ServiceRegistrationStoreFacadeFolder))
                {
                    Directory.CreateDirectory(this.ServiceRegistrationStoreFacadeFolder);
                }

                var destination = Path.Combine(this.ServiceRegistrationStoreFacadeFolder, Path.GetFileName(filePath));
                File.Copy(filePath, destination, true);
                return destination;
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    $"[{nameof(ServiceRegistrationRepo)}] {nameof(this.MoveFileToFacadeFolder)}: Exception happened when copy file {filePath} to {this.ServiceRegistrationStoreFacadeFolder}:{Environment.NewLine}{ex.ToString()}");

                throw;
            }
        }

        /// <summary>
        /// Returns the versions for a specific service
        /// </summary>
        /// <param name="serviceName">name of service whose versions are to be returned</param>
        /// <param name="addUnversionedService">add the un-versioned service or not</param>
        /// <returns>Available service versions</returns>
        public Version[] GetServiceVersionsInternal(string serviceName, bool addUnversionedService)
        {
            string callId = Guid.NewGuid().ToString();

            // Ensure the caller only supplies alpha-numeric characters
            for (int i = 0; i < serviceName.Length; i++)
            {
                if (!char.IsLetterOrDigit(serviceName[i]) && !char.IsPunctuation(serviceName[i]))
                {
                    throw new ArgumentException(SR.ArgumentMustBeAlphaNumeric, "serviceName");
                }
            }

            // TODO: What if there a huge number of files? Unlikely for the same service
            try
            {
                List<Version> versions = new List<Version>();
                bool unversionedServiceAdded = false;
                string[] directories = this.GetServiceRegistrationDirectories();
                if (directories != null)
                {
                    foreach (string serviceRegistrationDir in directories)
                    { 
                        try
                        {
                            this.GetVersionFromRegistrationDir(serviceRegistrationDir, serviceName, addUnversionedService, versions, ref unversionedServiceAdded);
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError($"[SessionLauncher] .GetServiceVersionsInternalOnPremise: Get service versions. exception = {e}");
                        }
                    }
                }

                return versions.ToArray();
            }
            catch (Exception e)
            {
                Trace.TraceError($"[SessionLauncher] .GetServiceVersionsInternalOnPremise: Get service versions. exception = {e}");
                throw new SessionException(SR.FailToEnumerateServicVersions, e);
            }
        }

        public Version GetServiceVersionInternal(string serviceName, bool addUnversionedService)
        {
            Version[] versions = this.GetServiceVersionsInternal(serviceName, addUnversionedService);
            if (versions != null && versions.Length != 0)
            {
                Version dynamicServiceVersion = versions[0];

                if (dynamicServiceVersion != null)
                {
                    return dynamicServiceVersion;
                }
            }
            return null;
        }


        /// <summary>
        /// Get service version from specified service registration folder.
        /// </summary>
        /// <param name="serviceRegistrationDir">service registration folder</param>
        /// <param name="serviceName">service name</param>
        /// <param name="addUnversionedService">add un-versioned service or not</param>
        /// <param name="versions">service versions</param>
        /// <param name="unversionedServiceAdded">is un-versioned service added or not</param>
        private void GetVersionFromRegistrationDir(string serviceRegistrationDir, string serviceName, bool addUnversionedService, List<Version> versions, ref bool unversionedServiceAdded)
        {
            if (string.IsNullOrEmpty(serviceRegistrationDir) || SoaRegistrationAuxModule.IsRegistrationStoreToken(serviceRegistrationDir))
            {
                List<string> services = ServiceRegistrationStore.EnumerateAsync().GetAwaiter().GetResult();
                Trace.TraceInformation("[SessionLauncher] GetVersionFromRegistration from reliable registry.");

                // If caller asked for unversioned service and it hasn't been found yet, check for it now
                if (addUnversionedService && !unversionedServiceAdded)
                {
                    if (services.Contains(serviceName))
                    {
                        this.AddSortedVersion(versions, Constant.VersionlessServiceVersion);
                        unversionedServiceAdded = true;
                    }
                }

                foreach (string service in services)
                {
                    try
                    {
                        Version version = ParseVersion(service, serviceName);
                        if (version != null)
                        {
                            this.AddSortedVersion(versions, version);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"[SessionLauncher] GetVersionFromRegistrationDir: Failed to parse service name {service}. Exception:{e}");
                        continue;
                    }
                }
            }
            else
            {
                // If caller asked for unversioned service and it hasnt been found yet, check for it now
                if (addUnversionedService && !unversionedServiceAdded)
                {
                    string configFilePath = Path.Combine(serviceRegistrationDir, Path.ChangeExtension(serviceName, ".config"));

                    if (File.Exists(configFilePath))
                    {
                        this.AddSortedVersion(versions, Constant.VersionlessServiceVersion);
                        unversionedServiceAdded = true;
                    }
                }

                string[] files = Directory.GetFiles(serviceRegistrationDir, string.Format(Constant.ServiceConfigFileNameFormat, serviceName, '*'));

                foreach (string file in files)
                {
                    try
                    {
                        Version version = ParseVersion(Path.GetFileNameWithoutExtension(file), serviceName);
                        if (version != null)
                        {
                            this.AddSortedVersion(versions, version);
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError($"[SessionLauncher] GetVersionFromRegistrationDir: Failed to parse file name {file}. Exception:{e}");
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Addes a Version to a sorted list of Versions (decending)
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="version"></param>
        private void AddSortedVersion(IList<Version> versions, Version version)
        {
            bool added = false;

            for (int i = 0; i < versions.Count; i++)
            {
                if (0 < version.CompareTo(versions[i]))
                {
                    versions.Insert(i, version);
                    added = true;
                    break;
                }
            }

            if (!added)
            {
                versions.Add(version);
            }
        }

        /// <summary>
        /// Get the version from specified name
        /// </summary>
        /// <param name="name">it can be a file name (without extension) or folder name</param>
        /// <param name="serviceName">service name</param>
        /// <returns>service version</returns>
        private static Version ParseVersion(string name, string serviceName)
        {
            string[] fileParts = name.Split('_');

            // Validate there are 2 parts {filename_version}
            if (fileParts.Length != 2)
            {
                return null;
            }

            // Validate the servicename
            if (!string.Equals(fileParts[0], serviceName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            try
            {
                // TODO: In .Net 4 move to Parse
                Version version = new Version(fileParts[1]);

                // Validate version, ensure Major and Minor are set and Revision and Build are not
                if (!(version.Major == 0 && version.Minor == 0))
                {
                    return version;
                }
            }
            catch (Exception ex)
            {
               
            }

            return null;
        }

    }
}
