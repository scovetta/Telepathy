//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="SessionLauncher.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the session launcher serivce.</summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.AADAuthUtil;
    using Microsoft.Hpc.Scheduler.Session.HpcPack.DataMapping;

    using JobState = Microsoft.Hpc.Scheduler.Session.Data.JobState;

    /// <summary>
    /// the session launcher service.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple,
                     Name = "SessionLauncher",
                     Namespace = "http://hpc.microsoft.com/sessionlauncher/",
                     IncludeExceptionDetailInFaults = true)]
    internal abstract class SessionLauncher : DisposableObject, ISessionLauncher
    {
        #region private fields
        /// <summary>
        /// If the user doesn't set any min/max value to the session, we will give an initial value
        /// of 16 tasks to the service job.
        /// </summary>
        protected const int DefaultInitTaskNumber = 16;

        /// <summary>
        /// the timeout setting for retring to conect to the scheduler.
        /// </summary>
        protected static int ConnectToSchedulerTimeout => int.MaxValue; // 9 * 60 * 1000;

        /// <summary>
        /// service registration related environment variables
        /// </summary>
       // private const string RegistryPathEnv = "CCP_SERVICEREGISTRATION_PATH";

        /// <summary>
        /// the commandline for the service task.
        /// note: no %CCP_HOME% on azure
        /// </summary>
        protected const string TaskCommandLine64 = @"HpcServiceHost.exe";

        /// <summary>
        /// the commandline for the service task.
        /// note: no %CCP_HOME% on azure
        /// </summary>
        protected const string TaskCommandLine32 = "HpcServiceHost32.exe";

        /// <summary>
        /// cancel job if failed task count is more than this boundary
        /// </summary>
        protected const int MaxFailedTask = 10000;

        /// <summary>
        /// Stores the server version
        /// </summary>
        internal static readonly Version ServerVersion = new Version(FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileVersion);

        /// <summary>
        /// the broker node manager.
        /// </summary>
        protected BrokerNodesManager brokerNodesManager;

        /// <summary>
        /// a value indicating the current state of connection to scheduler.
        /// </summary>
        protected SchedulerConnectState schedulerConnectState = SchedulerConnectState.None;

        /// <summary>
        /// data service instance
        /// </summary>
        protected Microsoft.Hpc.Scheduler.Session.Data.Internal.DataService dataService;

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

        /// <summary>
        /// Gets data server information
        /// </summary>
        /// <returns>returns data server information</returns>
        internal Microsoft.Hpc.Scheduler.Session.Data.Internal.DataService GetDataService()
        {
            return this.dataService;
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

        public abstract Task<SessionAllocateInfoContract> AllocateV5Async(SessionStartInfoContract info, string endpointPrefix);

        public abstract string[] Allocate(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

        public abstract Task<SessionAllocateInfoContract> AllocateDurableV5Async(SessionStartInfoContract info, string endpointPrefix);

        public abstract string[] AllocateDurable(SessionStartInfoContract info, string endpointPrefix, out int sessionid, out string serviceVersion, out SessionInfoContract sessionInfo);

        /// <summary>
        /// Attach to an exisiting session (create a session info by the specified service job)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="endpointPrefix">the prefix of the endpoint epr.</param>
        /// <param name="sessionId">the session id</param>
        /// <returns>the session information.</returns>
        Task<SessionInfoContract> ISessionLauncher.GetInfoV5Async(string endpointPrefix, int sessionId) =>
            ((ISessionLauncher)this).GetInfoV5Sp1Async(endpointPrefix, sessionId, false);

        public abstract Task<SessionInfoContract> GetInfoV5Sp1Async(string endpointPrefix, int sessionId, bool useAad);

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

        public abstract Task TerminateV5Async(int sessionId);

        /// <summary>
        /// terminate a session (cancel the service job specified by the id)
        /// </summary>
        /// <param name="headnode">the headnode.</param>
        /// <param name="sessionId">the session id</param>

        void ISessionLauncher.Terminate(string headnode, int sessionId)
        {
            ((ISessionLauncher)this).TerminateV5Async(sessionId).GetAwaiter().GetResult();
        }

        public abstract Task<Version[]> GetServiceVersionsAsync(string serviceName);

        public abstract Task<string> GetSOAConfigurationAsync(string key);

        public abstract Task<Dictionary<string, string>> GetSOAConfigurationsAsync(List<string> keys);

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
            TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] GetVersionFromRegistration identity {0},serviceRegistrationDir:{1}:", Thread.CurrentPrincipal.Identity.Name, serviceRegistrationDir ?? "null");
#if HPCPACK
            if (string.IsNullOrEmpty(serviceRegistrationDir) || SoaRegistrationAuxModule.IsRegistrationStoreToken(serviceRegistrationDir))
            {
                TraceHelper.TraceEvent(TraceEventType.Information, "[SessionLauncher] GetVersionFromRegistration from reliable registry.");
                List<string> services = HpcContext.Get().GetServiceRegistrationStore().EnumerateAsync().GetAwaiter().GetResult();


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
#else
            if (false)
            {
            }
#endif
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
        /// Get specified service's versions in the Azure cluster.
        /// </summary>
        /// <param name="serviceName">specified service name</param>
        /// <param name="addUnversionedService">include un-versioned service or not</param>
        /// <returns>service versions</returns>
        protected Version[] GetServiceVersionsInternalAzure(string serviceName, bool addUnversionedService)
        {
            List<Version> versions = new List<Version>();
            try
            {
                // (1) Get the service version from ccp package root folder.
                List<string> names = new List<string>();

                string pattern = string.Format("{0}*", serviceName);

                foreach (string path in Directory.GetDirectories(SoaHelper.GetCcpPackageRoot(), pattern, SearchOption.TopDirectoryOnly))
                {
                    // Following method returns the deepest folder name of the specified directory.
                    names.Add(Path.GetFileName(path));
                }

                bool addedFlag = false;
                foreach (string name in names)
                {
                    if (string.Equals(name, serviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (addUnversionedService && !addedFlag)
                        {
                            AddSortedVersion(versions, Constant.VersionlessServiceVersion);
                            addedFlag = true;
                        }
                    }
                    else
                    {
                        try
                        {
                            Version version = ParseVersion(name, serviceName);
                            if (version != null)
                            {
                                AddSortedVersion(versions, version);
                            }
                        }
                        catch (Exception e)
                        {
                            TraceHelper.TraceEvent(TraceEventType.Error,
                                "[SessionLauncher] GetServiceVersionsInternalAzure: Failed to parse name {0}. Exception:{1}", name, e);

                            continue;
                        }
                    }
                }

                // (2) Get the service version from ccp home folder.
                string ccphome = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                this.GetVersionFromRegistrationDir(ccphome, serviceName, addUnversionedService, versions, ref addedFlag);
            }
            catch (Exception e)
            {
                TraceHelper.TraceEvent(TraceEventType.Error,
                    "[SessionLauncher] .GetServiceVersionsInternalAzure: Get service versions. exception = {0}", e);

                throw new SessionException(SR.SessionLauncher_FailToEnumerateServicVersions, e);
            }

            return versions.ToArray();
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
        /// the helper function to check whether the endpoint prefix is supported.
        /// </summary>
        /// <param name="endpointPrefix"></param>
        /// <returns>a value indicating if the endpoint is supported.</returns>
        protected static bool IsEndpointPrefixSupported(string endpointPrefix)
        {
            if (string.IsNullOrEmpty(endpointPrefix))
            {
                return false;
            }

            switch (endpointPrefix.ToLowerInvariant().Trim())
            {
                case BrokerNodesManager.NettcpPrefix:
                case BrokerNodesManager.HttpsPrefix:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Ensure the caller is a valid cluster user
        /// </summary>
        protected static void CheckAccess()
        {
            if (Thread.CurrentPrincipal.IsHpcAadPrincipal())
            {
                return;
            }

            if (SoaHelper.CheckX509Identity(OperationContext.Current))
            {
                return;
            }

            if (!SoaHelper.CheckWindowsIdentity(OperationContext.Current))
            {
                throw new SecurityException();
            }
        }

        /// <summary>
        /// Convert plain text string to SecureString
        /// </summary>
        protected static SecureString CreateSecureString(string textString)
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
        /// Returns the service assembly's directory
        /// </summary>
        /// <param name="serviceAssemblyPath">Path to service assembly</param>
        /// <returns></returns>
        protected static string GetServiceAssemblyDirectory(string serviceAssemblyPath)
        {
            if (String.IsNullOrEmpty(serviceAssemblyPath))
            {
                return String.Empty;
            }

            return Path.GetDirectoryName(serviceAssemblyPath);
        }

        /// <summary>
        /// Convert a SecureString password back to a plain string
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        protected static string UnsecureString(SecureString secureString)
        {
            if (secureString == null)
            {
                return null;
            }
            else
            {
                string unsecureString;
                IntPtr ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);

                try
                {
                    unsecureString = Marshal.PtrToStringUni(ptr);
                }
                finally
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(ptr);
                }

                return unsecureString;
            }
        }

        protected static string GenerateBrokerLauncherAddress(string endpointPrefix, SessionStartInfoContract startInfo)
        {
            if (startInfo.UseAad)
            {
                return BrokerNodesManager.GenerateBrokerLauncherAadEpr(endpointPrefix, startInfo.DiagnosticBrokerNode);
            }
            else if (startInfo.IsAadOrLocalUser)
            {
                // Local user goes here
                return BrokerNodesManager.GenerateBrokerLauncherInternalEpr(endpointPrefix, startInfo.DiagnosticBrokerNode);
            }
            else
            {
                return BrokerNodesManager.GenerateBrokerLauncherEpr(endpointPrefix, startInfo.DiagnosticBrokerNode, startInfo.TransportScheme);
            }
        }

        /// <summary>
        /// Adds pre/post tasks to the service job (if configured)
        /// </summary>
        /// <param name="schedulerJob">Service job</param>
        /// <param name="workingDir">Working dir prep/release task working dir</param>
        /// <param name="prepareNodeCommandLine">Prepare node task command line</param>
        /// <param name="releaseNodeCommandLine">Release node task command line</param>
        /// <remarks>
        ///     Always use 32 bit HpcServiceHost since the pre/post command line is always a separate process. 32 bit version runs on 32 and 64 bit OSes.
        ///     Set working dir to service assembly's directory if it has one. 
        ///         On Azure the pre\post task is relative to the service assembly's local dir on the Azure node. Pre/post tasks can be absolute on azure.
        ///         On premise the pre\post task is relative to service assembly path's dir if any. If no assembly path dir its relative to user's profile dir. It can be absolute on premise.
        /// </remarks>
        protected static void AddPrepReleaseTasks(ISchedulerJob schedulerJob, string workingDir, string prepareNodeCommandLine, string releaseNodeCommandLine)
        {
            if (!String.IsNullOrEmpty(prepareNodeCommandLine))
            {
                ISchedulerTask prepTask = schedulerJob.CreateTask();
                prepTask.Type = TaskType.NodePrep;
                prepTask.CommandLine = TaskCommandLine32;
                prepTask.SetEnvironmentVariable(Constant.PrePostTaskCommandLineEnvVar, prepareNodeCommandLine);
                prepTask.SetEnvironmentVariable(Constant.PrePostTaskOnPremiseWorkingDirEnvVar, workingDir);

                schedulerJob.AddTask(prepTask);
            }

            if (!String.IsNullOrEmpty(releaseNodeCommandLine))
            {
                ISchedulerTask releaseTask = schedulerJob.CreateTask();
                releaseTask.Type = TaskType.NodeRelease;
                releaseTask.CommandLine = TaskCommandLine32;
                releaseTask.SetEnvironmentVariable(Constant.PrePostTaskCommandLineEnvVar, releaseNodeCommandLine);
                releaseTask.SetEnvironmentVariable(Constant.PrePostTaskOnPremiseWorkingDirEnvVar, workingDir);

                schedulerJob.AddTask(releaseTask);
            }
        }


        private void ParseStorageConnectionString(string connStr, out string accountName, out string accountKey)
        {
            string[] items = connStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            accountName = null;
            accountKey = null;
            foreach (var item in items)
            {
                int equalIndex = item.IndexOf('=');
                string itemName = item.Substring(0, equalIndex);
                string itemValue = item.Substring(equalIndex + 1);
                if (itemName.Equals("AccountName"))
                {
                    accountName = itemValue;
                }
                else if (itemName.Equals("AccountKey"))
                {
                    accountKey = itemValue;
                }
            }
        }

        /// <summary>
        /// Convert SchedulerException's Params property to SOA fault code.
        /// </summary>
        /// <param name="schedulerParams">
        /// When the SchedulerException is an inner exception of InvalidAuthernticationException,
        /// it's Params is expected to be null or empty or int.
        /// </param>
        /// <returns>SOA fault code</returns>
        protected static int ConvertToFaultCode(string schedulerParams)
        {
            if (string.IsNullOrEmpty(schedulerParams))
            {
                // need password, keep back-compact with previous version client
                return SOAFaultCode.AuthenticationFailure;
            }

            int error;
            if (!int.TryParse(schedulerParams, out error))
            {
                return SOAFaultCode.UnknownError;
            }

            switch (error)
            {
                // need either password or cert when cluster param HpcSoftCard is Allowed
                case ErrorCode.AuthFailureAllowSoftCardDisableCredentialReuse:
                    return SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred_UnReusable;

                // need either password or cert when cluster param HpcSoftCard is Allowed
                case ErrorCode.AuthFailureAllowSoftCardAboutToExpireSaved:
                case ErrorCode.AuthFailureAllowSoftCardNoValidSaved:
                    return SOAFaultCode.AuthenticationFailure_NeedEitherTypeCred;

                // need cert when cluster param HpcSoftCard is Required
                case ErrorCode.AuthFailureRequireSoftCardDisableCredentialReuse:
                case ErrorCode.AuthFailureRequireSoftCardAboutToExpireSaved:
                case ErrorCode.AuthFailureRequireSoftCardNoValidSaved:
                    return SOAFaultCode.AuthenticationFailure_NeedCertOnly;

                // need password when cluster param HpcSoftCard is Disabled
                case ErrorCode.AuthFailureDisableCredentialReuse:
                    return SOAFaultCode.AuthenticationFailure_NeedPasswordOnly_UnReusable;

                default:
                    return SOAFaultCode.UnknownError;
            }
        }

        protected static WindowsIdentity GetCallerWindowsIdentity()
        {
            WindowsIdentity callerWindowsIdentity =
            ServiceSecurityContext.Current.WindowsIdentity;
            if (callerWindowsIdentity == null)
            {
                throw new InvalidOperationException("The caller cannot be mapped to a WindowsIdentity");
            }
            else
            {
                return callerWindowsIdentity;
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

            foreach (Dictionary<string, SessionPool> dictionary in
                new Dictionary<string, SessionPool>[] { this.nonDurableSessionPool, this.durableSessionPool })
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
        /// Add a session to the session pool.
        /// </summary>
        ///<param name="serviceNameWithVersion">ServiceName_Version</param>
        /// <param name="durable">durable session or not</param>
        /// <param name="sessionId">session id</param>
        /// <param name="poolSize">the session pool size</param>
        protected void AddSessionToPool(string serviceNameWithVersion, bool durable, int sessionId, int poolSize)
        {
            Dictionary<string, SessionPool> dictionary = durable ? this.durableSessionPool : this.nonDurableSessionPool;
            Debug.Assert(dictionary != null, "the session pool is null");

            lock (dictionary)
            {
                SessionPool sp;
                if (!dictionary.TryGetValue(serviceNameWithVersion, out sp))
                {
                    TraceHelper.TraceEvent(TraceEventType.Error,
                        "[SessionLauncher] .AddSessionToPool: cannot find the session pool for service {0}", serviceNameWithVersion);
                }

                if (sessionId > 0 && sp.Length < poolSize)
                {
                    sp.SessionIds.Add(sessionId);
                }

                sp.Preparing--;
                sp.PoolChangeEvent.Set();

                TraceHelper.TraceEvent(TraceEventType.Verbose,
                                "[SessionLauncher] .AddSessionToPool: Session Pool {0} statics Length/Size/Index/Preparing/Ids: {1}/{2}/{3}/{4}/{5}.", serviceNameWithVersion, sp.Length, poolSize, sp.Index, sp.Preparing, string.Join(",", sp.SessionIds));

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
                get { return SessionIds[i]; }
                set { SessionIds[i] = value; }
            }

            private int index = 0;

            public int Index
            {
                get { return index; }
                set { index = value; }
            }

            private int preparing = 0;

            public int Preparing
            {
                get { return preparing; }
                set { preparing = value; }
            }

            public int Length
            {
                get { return SessionIds.Count; }
            }

            private List<int> sessionIds = new List<int>();

            public List<int> SessionIds
            {
                get { return sessionIds; }
                set { sessionIds = value; }
            }

            private ManualResetEvent poolChangeEvent = new ManualResetEvent(true);

            public ManualResetEvent PoolChangeEvent
            {
                get { return poolChangeEvent; }
                set { poolChangeEvent = value; }
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
    }
}