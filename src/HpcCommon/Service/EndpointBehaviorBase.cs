namespace Microsoft.Hpc
{
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Text;

    public abstract class EndpointBehaviorBase : IEndpointBehavior
    {
        protected ServiceAsClientIdentityProvider IdentityProvider { get; private set; }

        protected ServiceAsClientPrincipalProvider PrincipalProvider { get; private set; }
    
        protected EndpointBehaviorBase(ServiceAsClientIdentityProvider identityProvider, ServiceAsClientPrincipalProvider principalProvider = null)
        {
            this.IdentityProvider = identityProvider;
            this.PrincipalProvider = principalProvider;
        }

        public string Name
        {
            get
            {
                var build = new StringBuilder();
                if (this.IdentityProvider?.Method?.DeclaringType != null)
                {
                   build.Append($"{IdentityProvider.Method.DeclaringType.FullName}.{IdentityProvider.Method.Name}");
                    if (IdentityProvider.Target != null) build.Append($".{IdentityProvider?.Target?.GetHashCode()}");
                }

                if (this.PrincipalProvider?.Method?.DeclaringType != null)
                {
                    build.Append($"{PrincipalProvider.Method.DeclaringType.FullName}.{PrincipalProvider.Method.Name}");
                    if (PrincipalProvider.Target != null) build.Append($".{PrincipalProvider?.Target?.GetHashCode()}");
                }

                return build.ToString();
            }
        }
        
        public abstract void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters);
        public abstract void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime);
        public abstract void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher);
        public abstract void Validate(ServiceEndpoint endpoint);
    }
}
