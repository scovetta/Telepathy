//-------------------------------------------------------------------------------------------------
// <copyright file="ClientSinkProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <securityReview name="geoffo" date="1-30-06"/>
// 
// <summary>
//    Simple client sink provider that creates the simple client sink that forwards serviceprincipalname property
//    to the tcpclientsink. 
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;

    /// <summary>
    /// Simple provider class that is used to insert a simple sink
    /// into the client sink chain.
    /// </summary>
    internal sealed class ClientSinkProvider : IClientChannelSinkProvider
    {
        /// <summary>
        /// The binary sink provide that we use to create the first 
        /// sink in the chain.
        /// </summary>
        private BinaryClientFormatterSinkProvider binaryProvider = new BinaryClientFormatterSinkProvider();
        
        /// <summary>
        /// The dummy sink that allows us to insert the client sink into the chain while
        /// using the binary client formatter sink provide to create the first sink.
        /// </summary>
        private SinkFacade facadeSink = new SinkFacade();

        /// <summary>
        /// This constructor is required in order to use the provider in file-based configuration.
        /// It need not do anything unless you want to use the information in the parameters. 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="providerData"></param>
        public ClientSinkProvider(IDictionary properties, ICollection providerData)
        {
            this.binaryProvider.Next = this.facadeSink;
        }

        /// <summary>
        /// Gets and sets the next sink provider.
        /// </summary>
        public IClientChannelSinkProvider Next
        {
            get
            {
                return this.facadeSink.Next;
            }

            set
            {
                this.facadeSink.Next = value;
            }
        }

        /// <summary>
        /// Creates a sink.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="url"></param>
        /// <param name="remoteChannelData"></param>
        /// <returns></returns>
        public IClientChannelSink CreateSink(IChannelSender channel, String url, Object remoteChannelData)
        {
            //strip the query from the url, we use it to ensure that the
            //identity of the channel is unique when we make multiple connections
            //to the same object on the server.
            if (url != null)
            {
                url = url.Split('?')[0];
            }
            return this.binaryProvider.CreateSink(channel, url, remoteChannelData);
        }

        /// <summary>
        /// Simple facade class that allows us to insert a sink into the chain
        /// without owning the first or last parts of the chain.
        /// </summary>
        private class SinkFacade : IClientChannelSinkProvider
        {
            /// <summary>
            /// The next provider in the chain.
            /// </summary>
            private IClientChannelSinkProvider nextProvider;

            /// <summary>
            /// Sets the next provider in the chain.
            /// </summary>
            public IClientChannelSinkProvider Next
            {
                get
                {
                    return this.nextProvider;
                }

                set
                {
                    this.nextProvider = value;
                }
            }

            /// <summary>
            /// Creates a new sink.
            /// </summary>
            /// <param name="channel"></param>
            /// <param name="url"></param>
            /// <param name="remoteChannelData"></param>
            /// <returns></returns>
            public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
            {
                // Create the sink after us in the chain.
                IClientChannelSink nextSink = this.nextProvider.CreateSink(channel, url, remoteChannelData);

                //create our sink
                return new ClientSink(nextSink);
            }
        }
    }
}
