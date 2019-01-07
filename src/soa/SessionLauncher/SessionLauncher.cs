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
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

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