//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionLauncher.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the session launcher serivce.</summary>
//-----------------------------------------------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Security;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    /// <summary>
    /// the session launcher service.
    /// </summary>
    [ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        Name = "SessionLauncher",
        Namespace = "http://hpc.microsoft.com/sessionlauncher/",
        IncludeExceptionDetailInFaults = true)]
    internal abstract class SessionLauncher : DisposableObject, ISessionLauncher
    {
        #region private fields

        /// <summary>
        /// service registration related environment variables
        /// </summary>
        // private const string RegistryPathEnv = "CCP_SERVICEREGISTRATION_PATH";

        /// <summary>
        /// the commandline for the service task.
        /// note: no %CCP_HOME% on azure
        /// </summary>
        protected static string TaskCommandLine64 => @"HpcServiceHost.exe";

        /// <summary>
        /// the commandline for the service task.
        /// note: no %CCP_HOME% on azure
        /// </summary>
        protected static string TaskCommandLine32 => "HpcServiceHost32.exe";

        /// <summary>
        /// Stores the server version
        /// </summary>
        internal static readonly Version ServerVersion = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion);

        /// <summary>
        /// session pool for durable session
        /// Key: ServiceName_Version
        /// Value: Session Id list
        /// </summary>
        protected Dictionary<string, SessionPool> durableSessionPool = new Dictionary<string, SessionPool>();

        /// <summary>
        /// session pool for non-durable session
        /// Key: ServiceName_Version
        /// Value: Session Id list
        /// </summary>
        protected Dictionary<string, SessionPool> nonDurableSessionPool = new Dictionary<string, SessionPool>();

        protected ClusterInfo clusterInfo;

        #endregion

        /// <summary>
        /// Initializes a new instance of the SessionLauncher class with the specified head node. 
        /// </summary>
        /// <param name="headNode">the specified head node.</param>
        protected SessionLauncher()
        {
        }

        #region SessionLauncher operations

        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        async Task<Version> ISessionLauncher.GetServerVersionAsync()
        {
            return await Task.FromResult<Version>(SessionLauncher.ServerVersion); // (new Func<Version>(() => SessionLauncher.ServerVersion).Invoke());
        }

        public virtual async Task<SessionAllocateInfoContract> AllocateV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.AllocateInternalAsync(info, endpointPrefix, false);
        }

        public virtual string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            var contract = this.AllocateV5Async(info, endpointPrefix).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        public virtual async Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix)
        {
            return await this.AllocateInternalAsync(info, endpointPrefix, true);
        }

        public virtual string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo)
        {
            SessionAllocateInfoContract contract = this.AllocateDurableV5Async(info, endpointPrefix).GetAwaiter().GetResult();
            sessionid = contract.Id;
            serviceVersion = contract.ServiceVersion?.ToString();
            sessionInfo = contract.SessionInfo;
            return contract.BrokerLauncherEpr;
        }

        /// <summary>
        /// Attach to an exisiting session (create a session info by the specified service job)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="endpointPrefix">the prefix of the endpoint epr.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the session information.</returns>
        Task<SessionInfoContract> ISessionLauncher.GetInfoV5Async(string endpointPrefix, int sessionId) => ((ISessionLauncher)this).GetInfoV5Sp1Async(endpointPrefix, sessionId, false);

        public abstract Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad);

        public abstract Task TerminateV5Async(int sessionId);

        public abstract Task<Version[]> GetServiceVersionsAsync(string serviceName);

        public abstract Task<string> GetSOAConfigurationAsync(string key);

        public abstract Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys);

        #region sync interface

        /// <summary>
        /// Gets server version
        /// </summary>
        /// <returns>returns server version</returns>
        //Version ISessionLauncher.GetServerVersion()
        //{
        //    return SessionLauncher.ServerVersion;
        //}

        /// <summary>
        /// Attach to an exisiting session (create a session info by the specified service job)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="endpointPrefix">the prefix of the endpoint epr.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the session information.</returns>
        SessionInfoContract ISessionLauncher.GetInfo(string headnode, string endpointPrefix, int sessionId)
        {
            return ((ISessionLauncher)this).GetInfoV5Async(endpointPrefix, sessionId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// terminate a session (cancel the service job specified by the id)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>
        void ISessionLauncher.Terminate(string headnode, int sessionId)
        {
            ((ISessionLauncher)this).TerminateV5Async(sessionId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets SOA configuration
        /// </summary>
        /// <param name="key">indicating the key</param>
        /// <returns>returns the value</returns>
        /// <remarks>
        /// This operation could be called by both client (with user's credential) and broker/broker worker
        /// (with broker node's machine account).
        /// If it is called by client (user), we need to impersonate the caller and let scheduler API to
        /// authenticate it.
        /// If it is called by broker/broker worker (broker node's machine account), we do not impersonate
        /// the caller since broker node's machine account is not HpcUser or HpcAdmin.
        /// </remarks>
        //string ISessionLauncher.GetSOAConfiguration(string key)
        //{
        //    return ((ISessionLauncher)this).GetSOAConfigurationAsync(key).Result;
        //}

        #endregion

        #endregion

        /// <summary>
        /// Get service version from specified service registration folder.
        /// </summary>
        /// <param name="serviceRegistrationDir">service registration folder</param>
        /// <param name="serviceName">service name</param>
        /// <param name="addUnversionedService">add un-versioned service or not</param>
        /// <param name="versions">service versions</param>
        /// <param name="unversionedServiceAdded">is un-versioned service added or not</param>
        protected void GetVersionFromRegistrationDir(string serviceRegistrationDir, string serviceName, bool addUnversionedService, List<Version> versions, ref bool unversionedServiceAdded)
        {
            TraceHelper.TraceEvent(
                TraceEventType.Information,
                "[SessionLauncher] GetVersionFromRegistration identity {0},serviceRegistrationDir:{1}:",
                Thread.CurrentPrincipal.Identity.Name,
                serviceRegistrationDir ?? "null");

            if (string.IsNullOrEmpty(serviceRegistrationDir) || SoaRegistrationAuxModule.IsRegistrationStoreToken(serviceRegistrationDir))
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] GetVersionFromRegistration from reliable registry.");
                List<string> services = this.CreateServiceRegistrationRepo(string.Empty).ServiceRegistrationStore.EnumerateAsync().GetAwaiter().GetResult();


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
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] GetVersionFromRegistrationDir: Failed to parse service name {0}. Exception:{1}", service, e);
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
                        TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] GetVersionFromRegistrationDir: Failed to parse file name {0}. Exception:{1}", file, e);

                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Get the version from specified name
        /// </summary>
        /// <param name="name">it can be a file name (without extension) or folder name</param>
        /// <param name="serviceName">service name</param>
        /// <returns>service version</returns>
        protected static Version ParseVersion(string name, string serviceName)
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
                if (ParamCheckUtility.IsServiceVersionValid(version))
                {
                    return version;
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Warning, "[SessionLauncher].ParseVersion: Exception {0}", ex);
            }

            return null;
        }

        /// <summary>
        /// Addes a Version to a sorted list of Versions (decending)
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="version"></param>
        protected void AddSortedVersion(IList<Version> versions, Version version)
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

        #region Session Pool

        /// <summary>
        /// Update the session pool by checking an active job list
        /// </summary>
        /// <param name="jobInfoList">active job list</param>
        internal void UpdateSessionPool(List<JobInfo> jobInfoList)
        {
            SortedList<int, int> jobIds = new SortedList<int, int>();
            foreach (JobInfo info in jobInfoList)
            {
                if (!jobIds.ContainsKey(info.Id))
                {
                    jobIds.Add(info.Id, 0);
                }
            }

            foreach (Dictionary<string, SessionPool> dictionary in new Dictionary<string, SessionPool>[] { this.nonDurableSessionPool, this.durableSessionPool })
            {
                Debug.Assert(dictionary != null, "the session pool is null");
                lock (dictionary)
                {
                    foreach (SessionPool sp in dictionary.Values)
                    {
                        for (int i = sp.Length - 1; i >= 0; i--)
                        {
                            // remove the session from the pool if it is not in the running and prerunning job list
                            if (!jobIds.ContainsKey(sp[i]))
                            {
                                sp.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get cluster info from the reliable registry
        /// </summary>
        /// <returns></returns>
        Task<ClusterInfoContract> ISessionLauncher.GetClusterInfoAsync()
        {
            return Task.FromResult(this.clusterInfo.Contract);
        }

        #endregion

        /// <summary>
        /// the class holds session pool info of a service
        /// </summary>
        protected class SessionPool
        {
            public int this[int i]
            {
                get
                {
                    return SessionIds[i];
                }
                set
                {
                    SessionIds[i] = value;
                }
            }

            private int index = 0;

            public int Index
            {
                get
                {
                    return index;
                }
                set
                {
                    index = value;
                }
            }

            private int preparing = 0;

            public int Preparing
            {
                get
                {
                    return preparing;
                }
                set
                {
                    preparing = value;
                }
            }

            public int Length
            {
                get
                {
                    return SessionIds.Count;
                }
            }

            private List<int> sessionIds = new List<int>();

            public List<int> SessionIds
            {
                get
                {
                    return sessionIds;
                }
                set
                {
                    sessionIds = value;
                }
            }

            private ManualResetEvent poolChangeEvent = new ManualResetEvent(true);

            public ManualResetEvent PoolChangeEvent
            {
                get
                {
                    return poolChangeEvent;
                }
                set
                {
                    poolChangeEvent = value;
                }
            }

            public void RemoveAt(int i)
            {
                SessionIds.RemoveAt(i);
                if (Index > i)
                {
                    Index--;
                }
            }
        }

        /// <summary>
        /// Allocate a new durable or non-durable session
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="durable">whether session should be durable</param>
        /// <param name="endpointPrefix">the endpoint prefix, net.tcp:// or https:// </param>
        /// <returns>the Broker Launcher EPRs, sorted by the preference.</returns>
        protected virtual async Task<SessionAllocateInfoContract> AllocateInternalAsync(SessionStartInfoContract startInfo, string endpointPrefix, bool durable)
        {
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] Begin: AllocateInternalAsync");
            SessionAllocateInfoContract sessionAllocateInfo = new SessionAllocateInfoContract();

            ParamCheckUtility.ThrowIfNull(startInfo, "startInfo");
            ParamCheckUtility.ThrowIfNullOrEmpty(startInfo.ServiceName, "startInfo.ServiceName");
            ParamCheckUtility.ThrowIfNullOrEmpty(endpointPrefix, "endpointPrefix");

#if HPCPACK
            // check client api version, 4.3 or older client is not supported by 4.4 server for the broken changes
            if (startInfo.ClientVersion == null || startInfo.ClientVersion < new Version(4, 4))
            {
                TraceHelper.TraceEvent(TraceEventType.Error,
                    "[SessionLauncher] .AllocateInternalAsync: ClientVersion {0} does not match ServerVersion {1}.", startInfo.ClientVersion, ServerVersion);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.ClientServerVersionMismatch,
                                              SR.SessionLauncher_ClientServerVersionMismatch,
                                              startInfo.ClientVersion == null ? "NULL" : startInfo.ClientVersion.ToString(),
                                              ServerVersion.ToString());
            }
#endif

            // Init service version to the service version passed in
            if (startInfo.ServiceVersion != null)
            {
                sessionAllocateInfo.ServiceVersion = startInfo.ServiceVersion;
                TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: Original service version is {0}", sessionAllocateInfo.ServiceVersion);
            }
            else
            {
                sessionAllocateInfo.ServiceVersion = null;
                TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: Original service version is null.");
            }

            string callId = Guid.NewGuid().ToString();
            this.CheckAccess();

            SecureString securePassword = CreateSecureString(startInfo.Password);
            startInfo.Password = null;

            // BUG 4522 : Use CCP_SCHEDULER when referencing service registration file share so HA HN virtual name is used when needed
            //var reliableRegistry = new ReliableRegistry(this.fabricClient.PropertyManager);
            //string defaultServiceRegistrationServerName = await reliableRegistry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.FileShareServerRegVal, this.token);

            //if (String.IsNullOrEmpty(defaultServiceRegistrationServerName))
            //{
            //    defaultServiceRegistrationServerName = "localhost";
            //}

            // the reg repo path is from scheduler environments, defaultServiceRegistrationServerName is actually not used

            string serviceConfigFile;
            ServiceRegistrationRepo serviceRegistration = this.GetRegistrationRepo(callId);
            serviceConfigFile = serviceRegistration.GetServiceRegistrationPath(startInfo.ServiceName, startInfo.ServiceVersion);

            // If the serviceConfigFile wasnt found and serviceversion isnt specified, try getitng the service config based on the service's latest version
            if (string.IsNullOrEmpty(serviceConfigFile) && (startInfo.ServiceVersion == null))
            {
                TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: Try to find out versioned service.");

                Version[] versions = this.GetServiceVersionsInternal(startInfo.ServiceName, false);

                if (versions != null && versions.Length != 0)
                {
                    TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: {0} versioned services are found.", versions.Length);

                    Version dynamicServiceVersion = versions[0];

                    if (dynamicServiceVersion != null)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: Selected dynamicServiceVersion is {0}.", dynamicServiceVersion.ToString());
                    }

                    serviceConfigFile = serviceRegistration.GetServiceRegistrationPath(startInfo.ServiceName, dynamicServiceVersion);

                    // If a config file is found, update the serviceVersion that is returned to client and stored in recovery info
                    if (!string.IsNullOrEmpty(serviceConfigFile))
                    {
                        TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: serviceConfigFile is {0}.", serviceConfigFile);

                        startInfo.ServiceVersion = dynamicServiceVersion;

                        if (dynamicServiceVersion != null)
                        {
                            sessionAllocateInfo.ServiceVersion = dynamicServiceVersion;
                        }
                    }
                }
            }

            string serviceName = ServiceRegistrationRepo.GetServiceRegistrationFileName(startInfo.ServiceName, startInfo.ServiceVersion);
            TraceHelper.TraceEvent(TraceEventType.Verbose, "[SessionLauncher] .AllocateInternalAsync: Service name = {0}, Configuration file = {1}", serviceName, serviceConfigFile);

            // If the service is not found and user code doesn't specify
            // version, we will use the latest version. 
            if (string.IsNullOrEmpty(serviceConfigFile))
            {
                if (startInfo.ServiceVersion != null)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.ServiceVersion_NotFound, SR.SessionLauncher_ServiceVersionNotFound, startInfo.ServiceName, startInfo.ServiceVersion.ToString());
                }
                else
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Service_NotFound, SR.SessionLauncher_ServiceNotFound, startInfo.ServiceName);
                }
            }

            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = serviceConfigFile;

            ServiceRegistration registration = null;
            BrokerConfigurations brokerConfigurations = null;
            string hostpath = null;
            string traceSwitchValue = null;

            try
            {
                Configuration config = null;

                RetryManager.RetryOnceAsync(
                    () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                    TimeSpan.FromSeconds(1),
                    ex => ex is ConfigurationErrorsException).GetAwaiter().GetResult();

                Debug.Assert(config != null, "Configuration is not opened properly.");
                registration = ServiceRegistration.GetSectionGroup(config);
                brokerConfigurations = BrokerConfigurations.GetSectionGroup(config);

                if (registration != null && registration.Host != null && registration.Host.Path != null)
                {
                    hostpath = registration.Host.Path;
                }
                else
                {
                    // x86 or x64
                    hostpath = registration.Service.Architecture == ServiceArch.X86 ? TaskCommandLine32 : TaskCommandLine64;
                }

                traceSwitchValue = registration.Service.SoaDiagTraceLevel;

                // TODO: should deprecate the previous settings
                if (string.IsNullOrEmpty(traceSwitchValue))
                {
                    traceSwitchValue = ConfigurationHelper.GetTraceSwitchValue(config);
                }
            }
            catch (ConfigurationErrorsException e)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ConfigFile_Invalid, SR.SessionLauncher_ConfigFileInvalid, e.ToString());
            }
            catch (Exception ex)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, ex.ToString());
                throw;
            }

            // after figuring out the service and version, and the session pool size, we check if the service pool already has the instance.
            sessionAllocateInfo.Id = 0;
            sessionAllocateInfo.SessionInfo = null;
            if (startInfo.UseSessionPool)
            {
                if (this.TryGetSessionAllocateInfoFromPooled(endpointPrefix, durable, sessionAllocateInfo, serviceConfigFile, registration, out var allocateInternal))
                {
                    return allocateInternal;
                }
            }

            // for sessions to add in session pool
            try
            {
                var sessionAllocateInfoContract = await this.CreateAndSubmitSessionJob(
                                                      startInfo,
                                                      endpointPrefix,
                                                      durable,
                                                      callId,
                                                      securePassword,
                                                      registration,
                                                      sessionAllocateInfo,
                                                      traceSwitchValue,
                                                      serviceName,
                                                      brokerConfigurations,
                                                      hostpath);
                if (sessionAllocateInfoContract != null)
                {
                    return sessionAllocateInfoContract;
                }
            }
            finally
            {
                // Add the submitted job to the session pool.
                if (startInfo.UseSessionPool)
                {
                    this.AddSessionToPool(Path.GetFileNameWithoutExtension(serviceConfigFile), durable, sessionAllocateInfo.Id, registration.Service.MaxSessionPoolSize);
                }
            }

            return null;
        }

        protected abstract Task<SessionAllocateInfoContract> CreateAndSubmitSessionJob(
            SessionStartInfoContract startInfo,
            string endpointPrefix,
            bool durable,
            string callId,
            SecureString securePassword,
            ServiceRegistration registration,
            SessionAllocateInfoContract sessionAllocateInfo,
            string traceSwitchValue,
            string serviceName,
            BrokerConfigurations brokerConfigurations,
            string hostpath);

        protected abstract void AddSessionToPool(string serviceNameWithVersion, bool durable, int sessionId, int poolSize);

        protected abstract bool TryGetSessionAllocateInfoFromPooled(
            string endpointPrefix,
            bool durable,
            SessionAllocateInfoContract sessionAllocateInfo,
            string serviceConfigFile,
            ServiceRegistration registration,
            out SessionAllocateInfoContract allocateInternal);

        /// <summary>
        /// Implement authorization logic here
        /// </summary>
        protected abstract void CheckAccess();

        protected virtual ServiceRegistrationRepo GetRegistrationRepo(string callId)
        {
            ServiceRegistrationRepo repo = null;
            string regPath = string.Empty;

            try
            {
                if (!string.IsNullOrEmpty(SessionLauncherSettings.Default.ServiceRegistrationPath))
                {
                    regPath = SessionLauncherSettings.Default.ServiceRegistrationPath + ";" + regPath;
                }
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: callId={0}, Get the scheduler environment failed. exception = {1}", callId, e);

                ThrowHelper.ThrowSessionFault(SOAFaultCode.GetClusterPropertyFailure, SR.SessionLauncher_FailToGetClusterProperty, e.ToString());
            }

            if (!string.IsNullOrEmpty(regPath))
            {

                repo = this.CreateServiceRegistrationRepo(regPath);
            }
            else
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: callId={0}, Get the scheduler environment for RegistryPathEnv is empty or null.", callId);
            }

            if (repo != null && repo.GetServiceRegistrationDirectories() != null)
            {
                return repo;
            }
            else
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetRegistrationRepo: No service registration directories are configured");

                ThrowHelper.ThrowSessionFault(SOAFaultCode.Service_RegistrationDirsMissing, SR.SessionLauncher_NoServiceRegistrationDirs);

                return null;
            }
        }

        protected internal virtual ServiceRegistrationRepo CreateServiceRegistrationRepo(string regPath) => new ServiceRegistrationRepo(regPath);

        /// <summary>
        /// Convert plain text string to SecureString
        /// </summary>
        private static SecureString CreateSecureString(string textString)
        {
            if (textString == null)
            {
                return null;
            }
            else
            {
                SecureString secureString = new SecureString();
                foreach (char c in textString)
                {
                    secureString.AppendChar(c);
                }

                return secureString;
            }
        }

        /// <summary>
        /// Get specified service's versions in the on-premise cluster.
        /// </summary>
        /// <param name="serviceName">specified service name</param>
        /// <param name="addUnversionedService">include un-versioned service or not</param>
        /// <returns>service versions</returns>
        protected virtual Version[] GetServiceVersionsInternal(string serviceName, bool addUnversionedService)
        {
            string callId = Guid.NewGuid().ToString();

            // Ensure the caller only supplies alpha-numeric characters
            for (int i = 0; i < serviceName.Length; i++)
            {
                if (!char.IsLetterOrDigit(serviceName[i]) && !char.IsPunctuation(serviceName[i]))
                {
                    throw new ArgumentException(SR.SessionLauncher_ArgumentMustBeAlphaNumeric, "serviceName");
                }
            }

            ServiceRegistrationRepo serviceRegistration = this.GetRegistrationRepo(callId);

            // TODO: What if there a huge number of files? Unlikely for the same service
            try
            {
                List<Version> versions = new List<Version>();
                bool unversionedServiceAdded = false;

                string[] directories = serviceRegistration.GetServiceRegistrationDirectories();
                if (directories != null)
                {
                    foreach (string serviceRegistrationDir in directories)
                    {
                        this.GetVersionFromRegistrationDir(serviceRegistrationDir, serviceName, addUnversionedService, versions, ref unversionedServiceAdded);
                    }
                }

                return versions.ToArray();
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error, "[SessionLauncher] .GetServiceVersionsInternalOnPremise: Get service versions. exception = {0}", e);

                throw new SessionException(SR.SessionLauncher_FailToEnumerateServicVersions, e);
            }
        }
    }
}