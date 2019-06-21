//------------------------------------------------------------------------------
// <copyright file="ServiceRegistration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Common implementation to find the service Registration file
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.IO;

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
    }
}
