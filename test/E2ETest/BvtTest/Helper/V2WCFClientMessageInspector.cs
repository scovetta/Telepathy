// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel;
using System.Xml;
using AITestLib.Helper.Trace;

namespace AITestLib.Helper
{
    public class V2WCFClientMessageInspector : IClientMessageInspector
    {
        private string sessionId;

        public V2WCFClientMessageInspector(string sessionId)
        {
            this.sessionId = sessionId;
        }

        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            Guid id;
            reply.Headers.RelatesTo.TryGetGuid(out id);
            TraceLogger.LogResponseRecived(sessionId, id.ToString());
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            Guid id = Guid.NewGuid();
            request.Headers.MessageId = new UniqueId(id);
            TraceLogger.LogSendRequest(sessionId, id.ToString());
            return null;
        }
    }
}
