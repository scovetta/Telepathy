// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Test.E2E.Bvt.Helper
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    class V2WCFClientEndpointBehavior: IEndpointBehavior
    {
        private string sessionId;

        public V2WCFClientEndpointBehavior(string sessionId)
        {
            this.sessionId = sessionId;
        }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            V2WCFClientMessageInspector inspector = new V2WCFClientMessageInspector(this.sessionId);
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
            
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            
        }
    }
}
