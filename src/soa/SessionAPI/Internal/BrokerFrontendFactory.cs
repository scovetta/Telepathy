//------------------------------------------------------------------------------
// <copyright file="BrokerFrontendFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Factory to build broker frontend
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session.Common;

    /// <summary>
    /// Factory to build broker frontend
    /// </summary>
    internal abstract class BrokerFrontendFactory : DisposableObject
    {
        /// <summary>
        /// Stores the client id
        /// </summary>
        private string clientId;

        /// <summary>
        /// Stores the callback
        /// </summary>
        private IResponseServiceCallback responseCallback;

        /// <summary>
        /// Initializes a new instance of the BrokerFrontendFactory class
        /// </summary>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="callback">indicating the response callback</param>
        protected BrokerFrontendFactory(string clientId, IResponseServiceCallback callback)
        {
            this.clientId = clientId;
            this.responseCallback = callback;
        }

        /// <summary>
        /// Gets the client id
        /// </summary>
        protected string ClientId
        {
            get { return this.clientId; }
        }

        /// <summary>
        /// Gets the response callback
        /// </summary>
        protected IResponseServiceCallback ResponseCallback
        {
            get { return this.responseCallback; }
        }

        /// <summary>
        /// Get broker client proxy for send request
        /// </summary>
        /// <returns>returns broker client proxy for send request as IOutputChannel</returns>
        public abstract IChannel GetBrokerClient();

        /// <summary>
        /// Get the Azue queue proxy
        /// </summary>
        /// <returns>Azure queue proxy</returns>
        public abstract AzureQueueProxy GetBrokerClientAQ();

        /// <summary>
        /// Gets the controller client
        /// </summary>
        /// <returns>returns the controller client</returns>
        public abstract IController GetControllerClient();

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="operationTimeout">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public abstract IController GetControllerClient(TimeSpan operationTimeout);

        /// <summary>
        /// Gets controller client and set operation timeout if it is a WCF proxy
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating the operation timeout</param>
        /// <returns>returns IController instance</returns>
        public abstract IController GetControllerClient(int timeoutMilliseconds);

        /// <summary>
        /// Gets the response service client
        /// </summary>
        /// <returns>returns the response service client</returns>
        public abstract IResponseService GetResponseServiceClient();

        /// <summary>
        /// Set close timeout
        /// </summary>
        /// <param name="timeoutMilliseconds">indicating timeout</param>
        public abstract void SetCloseTimeout(int timeoutMilliseconds);

        /// <summary>
        /// Close broker client proxy
        /// </summary>
        /// <param name="setToNull">Whether to set client object to null after closing. THis should only be true if caller is within BrokerClient's objectLock to ensure
        /// another thread isnt using or about to use it</param>
        /// <param name="timeoutInMS">How long to wait for close. -1 means use binding's close timeout</param>
        public abstract void CloseBrokerClient(bool setToNull, int timeoutInMS);
    }
}
