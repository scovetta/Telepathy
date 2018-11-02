//------------------------------------------------------------------------------
// <copyright file="ControllerFrontendProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker controller instance provider
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Common;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker.FrontEnd.AzureQueue;

    /// <summary>
    /// Broker controller instance provider
    /// </summary>
    internal class ControllerFrontendProvider : DisposableObjectSlim, IInstanceProvider, IEndpointBehavior
    {
        /// <summary>
        /// Stores the singleton instance of the BrokerController class, this field is null if singleton is not required
        /// </summary>
        private BrokerController singletonInstance;

        /// <summary>
        /// Stores the client manager
        /// </summary>
        private BrokerClientManager clientManager;

        /// <summary>
        /// Stores the broker authorization
        /// </summary>
        private BrokerAuthorization brokerAuth;

        /// <summary>
        /// Stores broker observer
        /// </summary>
        private BrokerObserver observer;

        private AzureQueueProxy azureQueueProxy;

        private BrokerWorkerControllerQueueWatcher cloudQueueWatcher;

        /// <summary>
        /// Initializes a new instance of the ControllerFrontendProvider class
        /// </summary>
        /// <param name="isSingleton">indicating whether a singleton controller class is required</param>
        /// <param name="clientManager">indicating the client manager</param>
        /// <param name="brokerAuth">indicating the broker authorization</param>
        /// <param name="observer">indicating broker observer</param>
        /// <param name="azureQueueProxy">indicating the Azure storage proxy</param>
        public ControllerFrontendProvider(bool isSingleton, BrokerClientManager clientManager, BrokerAuthorization brokerAuth, BrokerObserver observer, AzureQueueProxy azureQueueProxy)
        {
            if (isSingleton)
            {
                this.singletonInstance = new BrokerController(true, clientManager, brokerAuth, observer, azureQueueProxy);
                this.cloudQueueWatcher = new BrokerWorkerControllerQueueWatcher(this.singletonInstance, azureQueueProxy.AzureStorageConnectionString);
            }

            this.clientManager = clientManager;
            this.brokerAuth = brokerAuth;
            this.observer = observer;
            this.azureQueueProxy = azureQueueProxy;
        }

        #region IEndpointBehavior Members

        /// <summary>
        /// Interface stub
        /// </summary>
        /// <param name="endpoint">The parameter is not used.</param>
        /// <param name="bindingParameters">The parameter is not used.</param>
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        /// <summary>
        /// Interface stub
        /// </summary>
        /// <param name="endpoint">The parameter is not used.</param>
        /// <param name="clientRuntime">The parameter is not used.</param>
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        /// <summary>
        /// Set the instance provider
        /// </summary>
        /// <param name="endpoint">indicating the endpoint</param>
        /// <param name="endpointDispatcher">indicating the endpoint dispatcher</param>
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.InstanceProvider = this;
        }

        /// <summary>
        /// Interface stub
        /// </summary>
        /// <param name="endpoint">The parameter is not used.</param>
        public void Validate(ServiceEndpoint endpoint)
        {
        }

        #endregion

        #region IInstanceProvider Members

        /// <summary>
        /// Gets the instance
        /// </summary>
        /// <param name="instanceContext">indicating the instance context</param>
        /// <param name="message">indicating the incoming message</param>
        /// <returns>returns the broker controller instance</returns>
        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.GetInstance();
        }

        /// <summary>
        /// Gets the instance
        /// </summary>
        /// <param name="instanceContext">indicating the instance context</param>
        /// <returns>returns the broker controller instance</returns>
        public object GetInstance(InstanceContext instanceContext)
        {
            return this.GetInstance();
        }

        /// <summary>
        /// Releases the instance
        /// </summary>
        /// <param name="instanceContext">indicating the instance context</param>
        /// <param name="instance">indicating the instance</param>
        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            // Only release the instance if it is not a singleton
            if (this.singletonInstance == null)
            {
                BrokerTracing.TraceInfo("[ControllerFrontendProvider] Release instance.");
                IDisposable disposableInstance = instance as IDisposable;
                if (disposableInstance != null)
                {
                    disposableInstance.Dispose();
                }
            }
        }

        #endregion

        /// <summary>
        /// Dispose the instance
        /// </summary>
        protected override void DisposeInternal()
        {
            base.DisposeInternal();

            if (this.singletonInstance != null)
            {
                try
                {
                    this.cloudQueueWatcher.StopWatch();
                    ((IDisposable)this.singletonInstance).Dispose();
                }
                catch (Exception ex)
                {
                    BrokerTracing.TraceWarning("[ControllerFrontendProvider].DisposeInternal: Exception {0}.", ex);
                }
            }
        }

        /// <summary>
        /// Gets the broker controller instance
        /// </summary>
        /// <returns>returns the broker controller instance</returns>
        private BrokerController GetInstance()
        {
            if (this.singletonInstance == null)
            {
                BrokerTracing.TraceInfo("[ControllerFrontendProvider] GetInstance.");
                return new BrokerController(false, this.clientManager, this.brokerAuth, this.observer, this.azureQueueProxy);
            }
            else
            {
                return this.singletonInstance;
            }
        }
    }
}
