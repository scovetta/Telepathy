// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker.FrontEnd
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    /// <summary>
    /// Stores frontend information
    /// </summary>
    internal sealed class FrontendInfo
    {
        /// <summary>
        /// Stores the frontend
        /// </summary>
        private FrontEndBase frontEnd;

        /// <summary>
        /// Stores the controller frontend
        /// </summary>
        private ServiceHost controllerFrontend;

        /// <summary>
        /// Stores the controller uri
        /// </summary>
        private string controllerUri;

        /// <summary>
        /// Stores the get response uri
        /// </summary>
        private string getResponseUri;

        /// <summary>
        /// Stores the max message size
        /// </summary>
        private long maxMessageSize;

        /// <summary>
        /// Stores the reader quotas
        /// </summary>
        private XmlDictionaryReaderQuotas readerQuotas;

        /// <summary>
        /// Gets or sets the reader quotas
        /// </summary>
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.readerQuotas; }
            set { this.readerQuotas = value; }
        }

        /// <summary>
        /// Gets or sets the max message size
        /// </summary>
        public long MaxMessageSize
        {
            get { return this.maxMessageSize; }
            set { this.maxMessageSize = value; }
        }

        /// <summary>
        /// Gets or sets the frontend
        /// </summary>
        public FrontEndBase FrontEnd
        {
            get { return this.frontEnd; }
            set { this.frontEnd = value; }
        }

        /// <summary>
        /// Gets or sets the controller frontend
        /// </summary>
        public ServiceHost ControllerFrontend
        {
            get { return this.controllerFrontend; }
            set { this.controllerFrontend = value; }
        }

        /// <summary>
        /// Gets or sets the controller uri
        /// </summary>
        public string ControllerUri
        {
            get { return this.controllerUri; }
            set { this.controllerUri = value; }
        }

        /// <summary>
        /// Gets or sets the get response uri
        /// </summary>
        public string GetResponseUri
        {
            get { return this.getResponseUri; }
            set { this.getResponseUri = value; }
        }

        /// <summary>
        /// Gets or sets the binding
        /// </summary>
        public Binding Binding { get; set; }
    }
}
