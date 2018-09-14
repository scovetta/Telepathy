using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Microsoft.Hpc.Scheduler.Store
{
    internal class ServiceAsClientSinkProvider : IClientChannelSinkProvider
    {
        public ServiceAsClientSinkProvider(StoreServer server)
        {
            _server = server;
        }

        StoreServer _server;

        #region IClientChannelSinkProvider Members

        public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
        {
            IClientChannelSink nextSink = null;
            if (_next != null)
                nextSink = _next.CreateSink(channel, url, remoteChannelData);

            return new ServiceAsClientSink(nextSink, _server);
        }

        public IClientChannelSinkProvider Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;
            }
        }

        IClientChannelSinkProvider _next = null;

        #endregion
    }

    class ServiceAsClientSink : IClientChannelSink
    {
        IClientChannelSink _nextSink = null;
        StoreServer _server = null;

        public ServiceAsClientSink(IClientChannelSink nextSink, StoreServer server)
        {
            _nextSink = nextSink;
            _server = server;
        }

        #region IClientChannelSink Members

        public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, System.Runtime.Remoting.Messaging.IMessage msg, ITransportHeaders headers, System.IO.Stream stream)
        {
            _nextSink.AsyncProcessRequest(sinkStack, msg, headers, stream);
        }

        public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, System.IO.Stream stream)
        {
            _nextSink.AsyncProcessResponse(sinkStack, state, headers, stream);
        }

        public System.IO.Stream GetRequestStream(System.Runtime.Remoting.Messaging.IMessage msg, ITransportHeaders headers)
        {
            return _nextSink.GetRequestStream(msg, headers);
        }

        public IClientChannelSink NextChannelSink
        {
            get { return _nextSink; }
        }

        public void ProcessMessage(System.Runtime.Remoting.Messaging.IMessage msg, ITransportHeaders requestHeaders, System.IO.Stream requestStream, out ITransportHeaders responseHeaders, out System.IO.Stream responseStream)
        {
            string currentIdentity = _server.GetServiceAsClientIdentity();
            if (currentIdentity != null)
            {
                requestHeaders["ServiceAsClientToken"] = currentIdentity;
            }

            requestHeaders["ServiceAsClient"] = true;

            _nextSink.ProcessMessage(msg, requestHeaders, requestStream, out responseHeaders, out responseStream);

        }

        #endregion

        #region IChannelSinkBase Members

        public System.Collections.IDictionary Properties
        {
            get { return null; }
        }

        #endregion

    }
}
