//------------------------------------------------------------------------------
// <copyright file="BrokerEntry.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Entry for broker
// </summary>
//------------------------------------------------------------------------------


using TelepathyCommon.HpcContext;

namespace Microsoft.Hpc.ServiceBroker
{
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.BackEnd;
    using Microsoft.Hpc.ServiceBroker.BrokerStorage;
    using Microsoft.Hpc.ServiceBroker.FrontEnd;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Principal;
    using System.ServiceModel.Configuration;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Module;
    using SoaAmbientConfig;
    /// <summary>
    /// Remoting entry for broker
    /// Called by the broker launcher
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "The BrokerEntry is garaunteed to be closed properly.")]
    public class BrokerEntry : IBrokerEntry
    {
        /// <summary>
        /// Stores the default close timeout
        /// </summary>
        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Stores the close flag
        /// </summary>
        private int closeFlag;

        /// <summary>
        /// Stores the broker queue
        /// </summary>
        private BrokerQueueFactory brokerQueueFactory;

        /// <summary>
        /// Stores the broker state manager
        /// </summary>
        private BrokerStateManager stateManager;

        /// <summary>
        /// Store the service job monitor
        /// </summary>
        private ServiceJobMonitorBase monitor;

        /// <summary>
        /// Stores the frontend result
        /// </summary>
        private FrontendResult frontendResult;

        /// <summary>
        /// Stores the broker client manager
        /// </summary>
        private BrokerClientManager clientManager;

        /// <summary>
        /// Stores the shared data
        /// </summary>
        private SharedData sharedData;

        /// <summary>
        /// Stores broker observer
        /// </summary>
        private BrokerObserver observer;

        /// <summary>
        /// Stores the broker authorization
        /// </summary>
        private BrokerAuthorization brokerAuth;

        /// <summary>
        /// Stores a flag indicating whether the broker is closed with data cleaned up
        /// </summary>
        private bool cleanData;

        /// <summary>
        /// Stores node mapping
        /// </summary>
        private NodeMappingData nodeMappingData;

        /// <summary>
        /// Stores the Azure storage proxy
        /// </summary>
        private AzureQueueProxy azureQueueProxy;

        /// <summary>
        /// Initializes a new instance of the BrokerEntry class
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        public BrokerEntry(string sessionId)
        {
            BrokerTracing.Initialize(sessionId);

            if (SoaHelper.IsSchedulerOnAzure())
            {
                this.nodeMappingData = new NodeMappingData();

                // Start to get node mapping in a ThreadPool thread.
                this.nodeMappingData.GetNodeMapping();
            }

            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[BrokerEntry] Broker core service launched.");
        }

        /// <summary>
        /// Gets the broker finished event handler
        /// </summary>
        public event EventHandler BrokerFinished;

        /// <summary>
        /// Gets the broker authorization
        /// </summary>
        public BrokerAuthorization Auth
        {
            get
            {
                return this.brokerAuth;
            }
        }

        /// <summary>
        /// Gets the session id
        /// </summary>
        public string SessionId
        {
            get { return this.sharedData.BrokerInfo.SessionId; }
        }

        /// <summary>
        /// Gets the current session max message size
        /// </summary>
        internal static int MaxMessageSize
        {
            get { return int.MaxValue; }
        }

        /// <summary>
        /// Gets the current session reader quotas
        /// </summary>
        internal static XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return XmlDictionaryReaderQuotas.Max; }
        }

        /// <summary>
        /// cleanup the stake session data.
        /// </summary>
        /// <param name="isStaleSessionCallback">the callback to judge whether </param>
        public static async Task CleanupStaleSessionData(IsStaleSessionCallback isStaleSessionCallback, string connectString)
        {
            if (!SoaHelper.IsOnAzure())
            {
                // TODO: on azure, about MSMQ
                await BrokerQueueFactory.CleanupStalePersistedData("azurequeue", isStaleSessionCallback, connectString);
            }
        }

        /// <summary>
        /// Gets the broker frontend
        /// </summary>
        /// <param name="callbackInstance">indicating response service callback</param>
        /// <returns>returns the broker frontend instance</returns>
        public IBrokerFrontend GetFrontendForInprocessBroker(IResponseServiceCallback callbackInstance)
        {
            BrokerTracing.TraceInfo("[BrokerEntry] GetFrontendForInprocessBroker...");
            try
            {
                if (this.closeFlag != 0)
                {
                    if (this.cleanData)
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerUnavailable, Microsoft.Hpc.SvcBroker.SR.BrokerIsUnavailable);
                    }
                    else
                    {
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerSuspending, Microsoft.Hpc.SvcBroker.SR.BrokerSuspending);
                    }
                }

                this.sharedData.WaitForInitializationComplete();
                BrokerTracing.TraceInfo("[BrokerEntry] GetFrontendForInprocessBroker successfully.");
                return new BrokerController(this.clientManager, callbackInstance, this.observer);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerEntry] GetFrontendForInprocessBroker failed: {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Run the broker
        /// </summary>
        /// <param name="startInfo">session start info</param>
        /// <param name="brokerInfo">indicate the broker start info</param>
        /// <returns>initialization result</returns>
        public BrokerInitializationResult Run(SessionStartInfoContract startInfo, BrokerStartInfo brokerInfo)
        {
            BrokerTracing.TraceEvent(System.Diagnostics.TraceEventType.Information, 0, "[BrokerEntry] Broker is starting initialization, ID = {0}", brokerInfo.SessionId);

            try
            {
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: ClusterTopology is {0}", brokerInfo.NetworkTopology);

                // Step 1: Initialize configuration and shared data
                ServiceConfiguration serviceConfig;
                BrokerConfigurations brokerConfig;
                BindingsSection bindings;
                SoaCommonConfig.WithoutSessionLayer = startInfo.IsNoSession; // TODO: this is a hack. Working mode should be decided by something like a *SchedulerType* filed.

                ConfigurationHelper.LoadConfiguration(startInfo, brokerInfo, out brokerConfig, out serviceConfig, out bindings);
                this.sharedData = new SharedData(brokerInfo, startInfo, brokerConfig, serviceConfig);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 1: Loading configuration and shared data succeeded.");
                Debug.WriteLine($"[BrokerEntry](Debug) UseAad:{startInfo.UseAad}");

                // Step 2: Initialize broker queue
                ClientInfo[] clientInfo;
                this.brokerQueueFactory = BrokerEntry.InitBrokerQueue(this.sharedData, out clientInfo);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 2: Initialize broker queue succeeded.");

                // Step 3: Initialize observer
                this.observer = new BrokerObserver(this.sharedData, clientInfo);
                this.sharedData.Observer = this.observer;
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 3: Initialize broker observer succeeded.");

                // Step 4: Initialize state manager
                this.stateManager = new BrokerStateManager(this.sharedData, clientInfo.Length != 0);
                this.stateManager.UnloadBroker += this.UnloadBroker;
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 4: Initialize broker state manager succeeded.");

                // Step 5: Initialize service job monitor
                var context = TelepathyContext.GetOrAdd(this.sharedData.BrokerInfo.Headnode);

                if (SoaCommonConfig.WithoutSessionLayer)
                { 
                    this.monitor = new DummyServiceJobMonitor(this.sharedData, this.stateManager, this.nodeMappingData, context);
                }
                else
                { 
                    this.monitor = new ServiceJobMonitor(this.sharedData, this.stateManager, this.nodeMappingData, context);
                }
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 5: Initialize service job monitor succeeded.");

                // Step 6: Initalize broker authorization
                this.brokerAuth = BrokerEntry.BuildBrokerAuthorization(this.sharedData);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 6: Initialize broker authorization succeeded.");

                // Step 7: Initialize dispatcher manager
                DispatcherManager dispatcherManager = new DispatcherManager(bindings, this.sharedData, this.observer, this.monitor, this.brokerQueueFactory, context);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 7: Initialize dispatcher manager succeeded.");

                // Step 8: Start service job monitor
                this.monitor.Start(startInfo, dispatcherManager, this.observer).GetAwaiter().GetResult();
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 8: Start service job monitor succeeded.");
                
                // Step 9: Initailize client manager
                this.clientManager = new BrokerClientManager(clientInfo, this.brokerQueueFactory, this.observer, this.stateManager, this.monitor, this.sharedData);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 9: Initialize client manager succeeded.");

                // if using AzureQueue, retrieve the connection string and build the request and response message queues if not exist
                string[] requestQueueUris = { };
                string requestBlobUri = string.Empty;
                string controllerRequestQueueUri = string.Empty;
                string controllerResponseQueueUri = string.Empty;
                if (startInfo.UseAzureStorage)
                {
                    int clusterHash = 0;
                    if (!string.IsNullOrEmpty(brokerInfo.ClusterId))
                    {
                        string clusterIdString = brokerInfo.ClusterId.ToLowerInvariant();
                        clusterHash = clusterIdString.GetHashCode();
                    }
                    else if (!string.IsNullOrEmpty(brokerInfo.ClusterName))
                    {
                        string clusterNameString = brokerInfo.ClusterName.ToLowerInvariant();
                        clusterHash = clusterNameString.GetHashCode();
                    }
                    else
                    {
                        throw new InvalidOperationException($"Both {nameof(brokerInfo.ClusterId)} and {nameof(brokerInfo.ClusterName)} are null or empty. No {nameof(clusterHash)} can be determined.");
                    }

                    if (!string.IsNullOrEmpty(brokerInfo.AzureStorageConnectionString))
                    {
                        this.azureQueueProxy = new AzureQueueProxy(brokerInfo.ClusterName, clusterHash, this.SessionId, brokerInfo.AzureStorageConnectionString);
                        requestQueueUris = this.azureQueueProxy.RequestQueueUris;
                        requestBlobUri = this.azureQueueProxy.RequestBlobUri;
                        var requestQName = CloudQueueConstants.GetBrokerWorkerControllerRequestQueueName(this.SessionId);
                        var responseQName = CloudQueueConstants.GetBrokerWorkerControllerResponseQueueName(this.SessionId);
                        controllerRequestQueueUri = CloudQueueCreationModule.CreateCloudQueueAndGetSas(
                            brokerInfo.AzureStorageConnectionString,
                            requestQName,
                            CloudQueueCreationModule.AddMessageSasPolicy).GetAwaiter().GetResult();
                        controllerResponseQueueUri = CloudQueueCreationModule.CreateCloudQueueAndGetSas(
                            brokerInfo.AzureStorageConnectionString,
                            responseQName,
                            CloudQueueCreationModule.ProcessMessageSasPolicy).GetAwaiter().GetResult();
                        if (this.SessionId == SessionStartInfo.StandaloneSessionId)
                        {
                            CloudQueueCreationModule.ClearCloudQueuesAsync(brokerInfo.AzureStorageConnectionString, new[] { requestQName, responseQName });
                        }
                    }
                    else
                    {
                        BrokerTracing.TraceError("[BrokerEntry] Initialization: Use Azure Queue is specified, however the Azure connection string is not set.");
                        ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_AzureConnectionStringNotAvailable, Microsoft.Hpc.SvcBroker.SR.Broker_AzureConnectionStringNotAvailable);
                    }
                }

                // Step 10: Initialize frontend
                this.frontendResult = FrontEndBuilder.BuildFrontEnd(this.sharedData, this.observer, this.clientManager, this.brokerAuth, bindings, this.azureQueueProxy);
                ////this.maxMessageSize = (int)this.frontendResult.MaxMessageSize;
                ////this.readerQuotas = this.frontendResult.ReaderQuotas;
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 10: Initialize frontend succeeded.");

                // Step 11: Start frontend, Initialization finished after this step
                this.OpenFrontend();
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 11: Open frontend succeeded.");

                // Step 12: Build initialization result and retrun to client
                BrokerInitializationResult result = BrokerEntry.BuildInitializationResult(
                    this.frontendResult,
                    dispatcherManager,
                    this.sharedData.Config.LoadBalancing.ServiceOperationTimeout,
                    this.sharedData.Config.Monitor.ClientBrokerHeartbeatInterval,
                    this.sharedData.Config.Monitor.ClientBrokerHeartbeatRetryCount,
                    requestQueueUris,
                    requestBlobUri,
                    controllerRequestQueueUri,
                    controllerResponseQueueUri,
                    startInfo.UseAzureStorage);
                BrokerTracing.TraceVerbose("[BrokerEntry] Initialization: Step 12: Build initialization result suceeded.");
                BrokerTracing.TraceInfo("[BrokerEntry] Initialization succeeded.");
                return result;
            }
            catch (Exception ex)
            {
                BrokerTracing.TraceError(ex.ToString());
                throw;
            }
            finally
            {
                if (this.sharedData != null)
                {
                    this.sharedData.InitializationFinished();
                }
            }
        }

        /// <summary>
        /// Informs that a client is attaching to this broker
        /// </summary>
        public void Attach()
        {
            BrokerTracing.TraceInfo("[BrokerEntry] Client attached.");
            try
            {
                // Bug 8379: If closing is under going, returns broker suspending exception and let broker manager try again latter.
                if (this.closeFlag != 0)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerSuspending, Microsoft.Hpc.SvcBroker.SR.BrokerSuspending);
                }

                this.stateManager.Attach();
                this.monitor.Attach();

                BrokerTracing.TraceInfo("[BrokerEntry] Client attached successfully.");
            }
            catch (NullReferenceException)
            {
                // Bug 8379: NullReferenceException caught because closing procedure is on going, returns broker suspending exception instead so that broker manager could properly handle
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BrokerSuspending, Microsoft.Hpc.SvcBroker.SR.BrokerSuspending);
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[BrokerEntry] Client attached failed: {0}", e);
                throw;
            }
        }

        /// <summary>
        /// Close the broker
        /// </summary>
        /// <param name="cleanData">indicate whether the broker should clean up the data</param>
        public async Task Close(bool cleanData)
        {
            BrokerTracing.TraceVerbose("[BrokerEntry] Close: Start closing: cleanData = {0}", cleanData);
            this.sharedData.WaitForInitializationComplete();

            if (Interlocked.Increment(ref this.closeFlag) != 1)
            {
                BrokerTracing.TraceInfo("[BrokerEntry] Close race condition detected, quit.");
                return;
            }

            this.cleanData = cleanData;
            int step = 0;

            // Step 1: Close Frontend
            if (this.frontendResult != null)
            {
                for (int i = 0; i < this.frontendResult.ServiceHostList.Length; i++)
                {
                    try
                    {
                        if (this.frontendResult.ServiceHostList[i] != null)
                        {
                            this.frontendResult.ServiceHostList[i].Close(CloseTimeout);
                            BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close {1} controller frontend succeeded.", ++step, FrontendResult.GetTransportSchemeNameByIndex(i));
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {1}: Close {2} controller frontend failed: {0}", e, ++step, FrontendResult.GetTransportSchemeNameByIndex(i));
                    }
                }

                for (int i = 0; i < this.frontendResult.FrontendList.Length; i++)
                {
                    try
                    {
                        if (this.frontendResult.FrontendList[i] != null)
                        {
                            this.frontendResult.FrontendList[i].Close();
                            BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close {1} frontend succeeded.", ++step, FrontendResult.GetTransportSchemeNameByIndex(i));
                        }
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {1}: Close {2} frontend failed: {0}", e, ++step, FrontendResult.GetTransportSchemeNameByIndex(i));
                    }
                }
            }

            // Step 2: Close client manager
            List<string> activeClientIdList;
            if (this.clientManager != null)
            {
                activeClientIdList = this.clientManager.GetAllActiveClientIds();
                try
                {
                    if (cleanData)
                    {
                        this.clientManager.DeleteAllQueues();
                    }

                    this.clientManager.Dispose();
                    this.clientManager = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close client manager succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close client manager failed: {1}", ++step, e);
                }
            }
            else
            {
                activeClientIdList = new List<string>();
            }

            //Check the StrategyConfig.WithoutSessionLayer for the close progress.
            //Step 3: Finish the service job if it is needed.
            // We only finish the service job if clean data is required, in other cases, the service job monitor will finish the service job according to the service job life cycle before we enter this stage
            if (this.monitor != null && !SoaCommonConfig.WithoutSessionLayer)
            {
                try
                {
                    if (cleanData)
                    {
                        await this.monitor.FinishServiceJob("Close Session");
                        BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Finish service job succeeded.", ++step);
                    }
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {0}: Finish service job failed: {1}", ++step, e);
                }
            }

            // Step 4: Close monitor
            if (this.monitor != null)
            {
                try
                {
                    // Update suspended state
                    if (!SoaCommonConfig.WithoutSessionLayer)
                    { 
                        await this.monitor.UpdateSuspended(!cleanData);
                    }
                    this.monitor.Close();
                    this.monitor = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close monitor succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {1}: Close monitor failed: {0}", e, ++step);
                }
            }

            // Step 5: Close state manager
            if (this.stateManager != null)
            {
                try
                {
                    this.stateManager.Close();
                    this.stateManager = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close state manager succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {1}: Close state manager failed: {0}", e, ++step);
                }
            }

            // Step 7: Close broker queue
            if (this.brokerQueueFactory != null)
            {
                foreach (ClientInfo clientInfo in this.brokerQueueFactory.AllClientInfos)
                {
                    if (activeClientIdList.Contains(clientInfo.ClientId))
                    {
                        continue;
                    }

                    try
                    {
                        bool isNewCreated;
                        BrokerQueue queue = this.brokerQueueFactory.GetPersistQueueByClient(clientInfo.ClientId, clientInfo.UserName, out isNewCreated);
                        Debug.Assert(!isNewCreated, "[BrokerEntry] Close: Should only get exsiting persist queue");
                        if (cleanData)
                        {
                            queue.Close();
                        }
                        else
                        {
                            queue.Dispose();
                        }

                        BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close broker queue {1} succeeded.", ++step, clientInfo.ClientId);
                    }
                    catch (Exception e)
                    {
                        BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close broker queue {1} failed: {2}", ++step, clientInfo.ClientId, e);
                    }
                }

                try
                {
                    this.brokerQueueFactory.Dispose();
                    this.brokerQueueFactory = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close broker queue factory succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {1}: Close broker queue factory failed: {0}", e, ++step);
                }
            }

            // Step 8: Clean up shared data
            if (this.sharedData != null)
            {
                try
                {
                    this.sharedData.Dispose();
                    this.sharedData = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Close shared data succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {0}: Close shared data failed: {1}", ++step, e);
                }
            }

            // Step 9: Dispose node mapping
            if (this.nodeMappingData != null)
            {
                try
                {
                    this.nodeMappingData.Dispose();
                    this.nodeMappingData = null;
                    BrokerTracing.TraceVerbose("[BrokerEntry] Close: Step {0}: Disposing node mapping succeeded.", ++step);
                }
                catch (Exception e)
                {
                    BrokerTracing.TraceWarning("[BrokerEntry] Close: Step {0}: Disposing node mapping failed: {1}", ++step, e);
                }
            }

#if DEBUG
            if (!ReferenceObject.CheckDisposed())
            {
                BrokerTracing.TraceEvent(TraceEventType.Warning, 0, "[BrokerEntry] Reference object not disposed after closing proceduer");
            }
#endif

            BrokerTracing.TraceVerbose("[BrokerEntry] Close finished.");

            if (this.BrokerFinished != null)
            {
                this.BrokerFinished(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Ask the broker manager to close this broker
        /// </summary>
        /// <param name="suspend">indicating whether to suspend</param>
        internal void UnloadBroker(bool suspend)
        {
            BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[BrokerEntry] Unload broker domain, ID = {0}", this.sharedData.BrokerInfo.SessionId);

            try
            {
                this.Close(!suspend).GetAwaiter().GetResult();
                BrokerTracing.TraceInfo("[BrokerEntry] Self Cleanup: Close succeeded");
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[BrokerEntry] Self Cleanup: Close failed: {0}", e);
            }
        }


        /// <summary>
        /// Build initialization result
        /// </summary>
        /// <param name="frontendResult">indicating the frontend result</param>
        /// <param name="dispatcherManager">indicating the dispatcher manager</param>
        /// <param name="serviceOperationTimeout">indicating service operation timeout</param>
        /// <param name="clientBrokerHeartbeatInterval">indicating client broker heartbeat interval</param>
        /// <param name="clientBrokerHeartbeatRetryCount">indicating client broker heartbeat retry count</param>
        /// <param name="azureRequestQueueUris">the Azure storage queue SAS Uri</param>
        /// <param name="azureRequestBlobUri">the Azure storage blob container SAS Uri</param>
        /// <param name="useAzureQueue">if the azure storage queue(blob) is used</param>
        /// <returns>returns the initialization result</returns>
        private static BrokerInitializationResult BuildInitializationResult(
            FrontendResult frontendResult,
            DispatcherManager dispatcherManager,
            int serviceOperationTimeout,
            int clientBrokerHeartbeatInterval,
            int clientBrokerHeartbeatRetryCount,
            string[] azureRequestQueueUris,
            string azureRequestBlobUri,
            bool? useAzureQueue)
        {
            BrokerInitializationResult info = new BrokerInitializationResult();
            info.BrokerEpr = frontendResult.FrontendUriList;
            info.ControllerEpr = frontendResult.ControllerUriList;
            info.ResponseEpr = frontendResult.GetResponseUriList;
            info.ServiceOperationTimeout = serviceOperationTimeout;
            info.ClientBrokerHeartbeatInterval = clientBrokerHeartbeatInterval;
            info.ClientBrokerHeartbeatRetryCount = clientBrokerHeartbeatRetryCount;
            info.MaxMessageSize = frontendResult.MaxMessageSize;
            info.SupportsMessageDetails = frontendResult.FrontendSupportsMessageDetails && dispatcherManager.BackendSupportsMessageDetails;
            info.AzureRequestQueueUris = azureRequestQueueUris;
            info.AzureRequestBlobUri = azureRequestBlobUri;
            info.UseAzureQueue = (useAzureQueue == true);
            return info;
        }

        private static BrokerInitializationResult BuildInitializationResult(
            FrontendResult frontendResult,
            DispatcherManager dispatcherManager,
            int serviceOperationTimeout,
            int clientBrokerHeartbeatInterval,
            int clientBrokerHeartbeatRetryCount,
            string[] azureRequestQueueUris,
            string azureRequestBlobUri,
            string controllerRequestQueueUri,
            string controllerResponseQueueUri,
            bool? useAzureQueue
            )
        {
            var info = BuildInitializationResult(
                frontendResult,
                dispatcherManager,
                serviceOperationTimeout,
                clientBrokerHeartbeatInterval,
                clientBrokerHeartbeatRetryCount,
                azureRequestQueueUris,
                azureRequestBlobUri,
                useAzureQueue);
            info.AzureControllerRequestQueueUri = controllerRequestQueueUri;
            info.AzureControllerResponseQueueUri = controllerResponseQueueUri;
            return info;
        }

        /// <summary>
        /// Build broker authorization
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="monitor">indicating the service job monitor</param>
        /// <returns>returns the broker autorization instance</returns>
        private static BrokerAuthorization BuildBrokerAuthorization(SharedData sharedData)
        {
            // No authorization for inprocess session
            if (sharedData.StartInfo.UseInprocessBroker)
            {
                return null;
            }

            if (sharedData.StartInfo.Secure)
            {
                if (SoaHelper.IsOnAzure())
                {
                    return null;
                }

                if (sharedData.StartInfo.ShareSession)
                {
                    // TODO: Feature: share session
                    throw new NotImplementedException();
                    // return new BrokerAuthorization(sharedData.BrokerInfo.JobTemplateACL, (int)JobTemplateRights.SubmitJob, (int)JobTemplateRights.Generic_Read, (int)JobTemplateRights.Generic_Write, (int)JobTemplateRights.Generic_Execute, (int)JobTemplateRights.Generic_All);
                }
                else
                {
                    return new BrokerAuthorization(new SecurityIdentifier(sharedData.BrokerInfo.JobOwnerSID));
                }
            }
            else
            {
                // No authorization for nonsecure session
                return null;
            }
        }

        /// <summary>
        /// Init the broker queue
        /// </summary>
        /// <param name="sharedData">indicating the shared data</param>
        /// <param name="clientInfo">output the client info</param>
        /// <returns>returns the broker queue factory</returns>
        private static BrokerQueueFactory InitBrokerQueue(SharedData sharedData, out ClientInfo[] clientInfo)
        {
            BrokerQueueFactory result;

            // Create a durable broker queue if durable is required
            if (sharedData.BrokerInfo.Durable)
            {
                result = new BrokerQueueFactory("AzureQueue", sharedData);
            }
            else
            {
                result = new BrokerQueueFactory(null, sharedData);
            }

            clientInfo = result.AllClientInfos;
            BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[BrokerEntry] Get client id list count: {0}", clientInfo.Length);

            if (clientInfo.Length != 0 && !sharedData.BrokerInfo.Attached)
            {
                throw new BrokerQueueException((int)BrokerQueueErrorCode.E_BQ_CREATE_BROKER_FAIL_EXISTING_BROKER_QUEUE, "Create a broker with an existing broker queue is not allowed.");
            }

            // set persist version
            BrokerQueueItem.PersistVersion = sharedData.BrokerInfo.PersistVersion;

            return result;
        }

        /// <summary>
        /// Open the frontend
        /// </summary>
        private void OpenFrontend()
        {
            for (int i = 0; i < this.frontendResult.ServiceHostList.Length; i++)
            {
                if (this.frontendResult.ServiceHostList[i] != null)
                {
                    this.frontendResult.ServiceHostList[i].Open();
                    BrokerTracing.TraceVerbose("[BrokerEntry] {0} controller frontend opened.", FrontendResult.GetTransportSchemeNameByIndex(i));
                }
            }

            for (int i = 0; i < this.frontendResult.FrontendList.Length; i++)
            {
                if (this.frontendResult.FrontendList[i] != null)
                {
                    this.frontendResult.FrontendList[i].Open();
                    BrokerTracing.TraceVerbose("[BrokerEntry] {0} frontend opened.", FrontendResult.GetTransportSchemeNameByIndex(i));
                }
            }
        }
    }
}
