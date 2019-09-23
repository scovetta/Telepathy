// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    using Microsoft.Telepathy.Test.E2E.Bvt.Helper.Trace;

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
            TraceLogger.LogResponseRecived(this.sessionId, id.ToString());
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            Guid id = Guid.NewGuid();
            request.Headers.MessageId = new UniqueId(id);
            TraceLogger.LogSendRequest(this.sessionId, id.ToString());
            return null;
        }
    }
}
