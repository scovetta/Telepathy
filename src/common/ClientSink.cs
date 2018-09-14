//-------------------------------------------------------------------------------------------------
// <copyright file="ClientSink.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <securityReview name="geoffo" date="1-30-06"/>
// 
// <summary>
//    Simple client sink that forwards the serviceprincipalname
//    and groupconnectonname properties to the tcpclientsink. 
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using System.Collections;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Channels.Tcp;
    using System.Security.Principal;

    internal sealed class ClientSink : BaseChannelSinkWithProperties, IClientChannelSink
    {
        /// <summary>
        /// property collection
        /// </summary>
        private List<string> keys = new List<string>();

        /// <summary>
        /// Next sink.
        /// </summary>
        private IClientChannelSink nextSink;

        /// <summary>
        /// the propery interface of the next sink in the chain.
        /// Should be tcpclientsink
        /// </summary>
        private IDictionary nextSinkProperties;

        /// <summary>
        /// Creates a new client sink.
        /// </summary>
        /// <param name="sink">The next sink in the chain.</param>
        public ClientSink(IClientChannelSink sink)
        {
            if (sink == null) throw new ArgumentNullException("sink");
            nextSink = sink;
            nextSinkProperties = sink as IDictionary;
            this.keys.Add("serviceprincipalname");
            this.keys.Add("connectiongroupname");
        }

        /// <summary>
        /// The set of properties exposed by this sink.
        /// </summary>
        public override System.Collections.ICollection Keys
        {
            get { return this.keys; }
        }

        /// <summary>
        /// Setting the property simply forwards the value 
        /// to the next sink.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public override object this[object key]
        {
            get
            {
                if (nextSink != null)
                {
                    return nextSinkProperties[key];
                }
                return null;
            }
            set
            {
                if (nextSink != null)
                {
                    nextSinkProperties[key] = value;
                }
            }
        }

        /// <summary>
        /// Gets the next sink in the chain.
        /// </summary>
        public IClientChannelSink NextChannelSink
        {
            get
            {
                return (nextSink);
            }
        }

        /// <summary>
        /// Dummy implementation of get request stream.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="requestHeaders"></param>
        /// <returns></returns>
        public Stream GetRequestStream(IMessage message, ITransportHeaders requestHeaders)
        {
            return nextSink.GetRequestStream(message, requestHeaders);
        }

        /// <summary>
        /// Dummy implementation of process message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="requestStream"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responseStream"></param>
        public void ProcessMessage(IMessage message,
                                    ITransportHeaders requestHeaders,
                                    Stream requestStream,
                                    out ITransportHeaders responseHeaders,
                                    out Stream responseStream)
        {
            using (HPCIdentity hpcIdentity = new HPCIdentity())
            {
                hpcIdentity.Impersonate();
                nextSink.ProcessMessage(message, requestHeaders, requestStream, out responseHeaders, out responseStream);
            }
        }

        /// <summary>
        /// Dummy implementation of AsyncProcessRequest
        /// </summary>
        /// <param name="sinkStack"></param>
        /// <param name="message"></param>
        /// <param name="requestHeaders"></param>
        /// <param name="requestStream"></param>
        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack,
                                         IMessage message,
                                         ITransportHeaders requestHeaders,
                                         Stream requestStream)
        {
            using (HPCIdentity hpcIdentity = new HPCIdentity())
            {
                hpcIdentity.Impersonate();
                nextSink.AsyncProcessRequest(sinkStack, message, requestHeaders, requestStream);
            }
        }

        /// <summary>
        /// Dummy implementation of AsyncProcessResponse
        /// </summary>
        /// <param name="sinkStack"></param>
        /// <param name="state"></param>
        /// <param name="responseHeaders"></param>
        /// <param name="responseStream"></param>
        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack,
                                          Object state,
                                          ITransportHeaders responseHeaders,
                                          Stream responseStream)
        {
            nextSink.AsyncProcessResponse(sinkStack, state, responseHeaders, responseStream);
        }
    }
}
