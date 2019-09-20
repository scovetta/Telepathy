// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.Threading;

    using Microsoft.Hpc.ServiceBroker;

    /// <summary>
    /// It encapsulates the node mapping logic.
    /// </summary>
    internal class NodeMappingData : IDisposable
    {
        private const string NodeMappingCacheEpr = @"net.pipe://localhost/NodeMapping";

        /// <summary>
        /// It indicates if the node mapping retrieving completes.
        /// </summary>
        private ManualResetEvent complete = new ManualResetEvent(false);

        /// <summary>
        /// LogicalNameToIP mapping
        /// </summary>
        private Dictionary<string, string> dictionary;

        /// <summary>
        /// Get the dictionary of LogicalNameToIP mapping.
        /// </summary>
        public Dictionary<string, string> Dictionary
        {
            get { return this.dictionary; }
        }

        /// <summary>
        /// Use a worker thread to retrieve node mapping (costs several seconds) avoid blocking current thread.
        /// </summary>
        public void GetNodeMapping()
        {
            ThreadPool.QueueUserWorkItem(new ThreadHelper<object>(new WaitCallback(this.GetNodeMappingWorker)).CallbackRoot);
        }

        /// <summary>
        /// Wait for node mapping retrieving completes.
        /// </summary>
        public void Wait()
        {
            this.complete.WaitOne();
        }

        /// <summary>
        /// Get the node mapping for the Azure nodes.
        /// </summary>
        /// <param name="state">object used by the callback method.</param>
        private void GetNodeMappingWorker(object state)
        {
            try
            {
                EndpointAddress endpoint = new EndpointAddress(NodeMappingCacheEpr);
                using (ChannelFactory<INodeMappingCache> channelFactory = new ChannelFactory<INodeMappingCache>(BindingHelper.HardCodedNamedPipeBinding, endpoint))
                {
                    INodeMappingCache cache = null;
                    try
                    {
                        cache = channelFactory.CreateChannel();
                        this.dictionary = cache.EndGetNodeMapping(cache.BeginGetNodeMapping(true, null, null));
                    }
                    finally
                    {
                        if (cache != null)
                        {
                            ((IClientChannel)cache).Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                BrokerTracing.TraceError("[NodeMappingData] .GetNodeMapping: Failed to get node mapping. {0}", e);
            }
            finally
            {
                // If error occurs, this.dictionary is null. Set following waithandle avoid blocking the waiter.
                this.complete.Set();
            }
        }

        #region IDisposable Members

        private bool disposed;

        ~NodeMappingData()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.complete != null)
                {
                    this.complete.Close();
                    this.complete = null;
                }
            }

            this.disposed = true;
        }

        #endregion
    }
}
