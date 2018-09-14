namespace Microsoft.Hpc.AADAuthUtil
{
    using System.ServiceModel.Description;

    public class AADClientEndpointBehavior : IEndpointBehavior
    {
        public string JwtToken { get; set; }

        public AADClientEndpointBehavior(string authorization)
        {
            this.JwtToken = authorization;
        }

        #region IEndpointBehavior Members
        public void AddBindingParameters(ServiceEndpoint endpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            AADClientMessageInspector inspector = new AADClientMessageInspector(this.JwtToken);
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher)
        {
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }
        #endregion
    }
}
