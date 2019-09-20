// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.FrontEnd
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Xml;

    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// Wrapped result for building frontend
    /// </summary>
    internal sealed class FrontendResult
    {
        /// <summary>
        /// Stores the TransportScheme element array
        /// </summary>
        private static readonly string[] TransportSchemeElementArray = Enum.GetNames(typeof(TransportScheme));

        /// <summary>
        /// Stores the frontend uri list
        /// </summary>
        private string[] frontendUriList = new string[TransportSchemeElementArray.Length];

        /// <summary>
        /// Stores the controller uri list
        /// </summary>
        private string[] controllerUriList = new string[TransportSchemeElementArray.Length];

        /// <summary>
        /// Stores the get response uri list
        /// </summary>
        private string[] getResponseUriList = new string[TransportSchemeElementArray.Length];

        /// <summary>
        /// Stores the frontend list
        /// </summary>
        private FrontEndBase[] frontendList = new FrontEndBase[TransportSchemeElementArray.Length];

        /// <summary>
        /// Stores the service host list
        /// </summary>
        private ServiceHost[] serviceHostList = new ServiceHost[TransportSchemeElementArray.Length];

        /// <summary>
        /// Stores a value indicating whether "Message Details" is available for this session
        /// </summary>
        private bool supportsMessageDetails = true;

        /// <summary>
        /// Stores the max message size
        /// </summary>
        private long maxMessageSize;

        /// <summary>
        /// Stores the reader quotas
        /// </summary>
        private XmlDictionaryReaderQuotas readerQuotas;

        /// <summary>
        /// Gets the max message size
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public long MaxMessageSize
        {
            get { return this.maxMessageSize; }
        }

        /// <summary>
        /// Gets the ReaderQuotas
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "May need in the future")]
        public XmlDictionaryReaderQuotas ReaderQuotas
        {
            get { return this.readerQuotas; }
        }

        /// <summary>
        /// Gets the frontend uri list
        /// </summary>
        public string[] FrontendUriList
        {
            get { return this.frontendUriList; }
        }

        /// <summary>
        /// Gets the service host list
        /// </summary>
        public ServiceHost[] ServiceHostList
        {
            get { return this.serviceHostList; }
        }

        /// <summary>
        /// Gets the frontend list
        /// </summary>
        public FrontEndBase[] FrontendList
        {
            get { return this.frontendList; }
        }

        /// <summary>
        /// Gets the controller uri list
        /// </summary>
        public string[] ControllerUriList
        {
            get { return this.controllerUriList; }
        }

        /// <summary>
        /// Gets the get response uri list
        /// </summary>
        public string[] GetResponseUriList
        {
            get { return this.getResponseUriList; }
        }

        /// <summary>
        /// Gets a value indicating whether "Message Details" is available for the frontend of this session
        /// </summary>
        public bool FrontendSupportsMessageDetails
        {
            get
            {
                return this.supportsMessageDetails;
            }
        }

        /// <summary>
        /// Gets the transport scheme name by index
        /// </summary>
        /// <param name="index">indicating the index</param>
        /// <returns>transport shceme name</returns>
        public static string GetTransportSchemeNameByIndex(int index)
        {
            return TransportSchemeElementArray[index + 1];
        }

        /// <summary>
        /// Set the frontend info for a certain transport scheme
        /// </summary>
        /// <param name="info">indicating the frontend info</param>
        /// <param name="scheme">indicating the transport scheme</param>
        public void SetFrontendInfo(FrontendInfo info, TransportScheme scheme)
        {
            int index = (int)Math.Log((int)scheme, 2);

            this.frontendUriList[index] = info.FrontEnd.ListenUri;
            this.controllerUriList[index] = info.ControllerUri;
            this.getResponseUriList[index] = info.GetResponseUri;
            this.frontendList[index] = info.FrontEnd;
            this.serviceHostList[index] = info.ControllerFrontend;

            // If any frontend does not support addressing or not http binding,
            // the session would be treated as not available for Message Details View.
            if (info.Binding.MessageVersion.Addressing == AddressingVersion.None && !(info.Binding is BasicHttpBinding))
            {
                this.supportsMessageDetails = false;
            }

            if (info.MaxMessageSize > this.maxMessageSize)
            {
                this.maxMessageSize = info.MaxMessageSize;
            }

            if (this.readerQuotas == null)
            {
                this.readerQuotas = info.ReaderQuotas;
            }
            else
            {
                if (info.ReaderQuotas.MaxArrayLength > this.readerQuotas.MaxArrayLength)
                {
                    this.readerQuotas.MaxArrayLength = info.ReaderQuotas.MaxArrayLength;
                }

                if (info.ReaderQuotas.MaxBytesPerRead > this.readerQuotas.MaxBytesPerRead)
                {
                    this.readerQuotas.MaxBytesPerRead = info.ReaderQuotas.MaxBytesPerRead;
                }

                if (info.ReaderQuotas.MaxDepth > this.readerQuotas.MaxDepth)
                {
                    this.readerQuotas.MaxDepth = info.ReaderQuotas.MaxDepth;
                }

                if (info.ReaderQuotas.MaxNameTableCharCount > this.readerQuotas.MaxNameTableCharCount)
                {
                    this.readerQuotas.MaxNameTableCharCount = info.ReaderQuotas.MaxNameTableCharCount;
                }

                if (info.ReaderQuotas.MaxStringContentLength > this.readerQuotas.MaxStringContentLength)
                {
                    this.readerQuotas.MaxStringContentLength = info.ReaderQuotas.MaxStringContentLength;
                }
            }
        }
    }
}
