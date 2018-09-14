namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal class SchedulerClientEndpointBehavior : EndpointBehaviorBase
    {
        public SchedulerClientEndpointBehavior(ServiceAsClientIdentityProvider identityProvider, ServiceAsClientPrincipalProvider principalProvider = null)
            :base(identityProvider, principalProvider)
        {
        }
        public override void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public override void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            SchedulerClientMessageInspector inspector = new SchedulerClientMessageInspector(this.IdentityProvider, this.PrincipalProvider);
            clientRuntime.MessageInspectors.Add(inspector);
        }

        public override void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        public override void Validate(ServiceEndpoint endpoint)
        {
        }
    }

    internal class SchedulerClientMessageInspector : IClientMessageInspector
    {
        private ServiceAsClientIdentityProvider identityProvider;
        private ServiceAsClientPrincipalProvider principalProvider;
        
        public SchedulerClientMessageInspector(ServiceAsClientIdentityProvider identityProvider, ServiceAsClientPrincipalProvider principalProvider)
        {
            this.identityProvider = identityProvider;
            this.principalProvider = principalProvider;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            // Prepare the request message copy to be modified
            MessageBuffer buffer = request.CreateBufferedCopy(int.MaxValue);
            request = buffer.CreateMessage();
            var identityMessageHeader = new SchedulerAuthMessageHeader(this.identityProvider);

            // Add the custom header to the request.
            request.Headers.Add(identityMessageHeader);

            if (this.principalProvider != null)
            {
                var principalMessageHeader = new ThreadPrincipalMessageHeader(this.principalProvider);
                request.Headers.Add(principalMessageHeader);
            }

            return null;
        }
    }

    public class SchedulerAuthMessageHeader : MessageHeader
    {
        private const string CustomHeaderName = "ServiceAsClientToken";
        private const string CustomHeaderNamespace = "hpccluster";

        private ServiceAsClientIdentityProvider provider;

        public override string Name
        {
            get { return CustomHeaderName; }
        }

        public override string Namespace
        {
            get { return CustomHeaderNamespace; }
        }

        public SchedulerAuthMessageHeader(ServiceAsClientIdentityProvider provider)
        {
            this.provider = provider;
        }

        public static string ReadHeader(Message request)
        {
            Int32 headerPosition = request.Headers.FindHeader(CustomHeaderName, CustomHeaderNamespace);
            if (headerPosition == -1)
            {
                return null;
            }

            MessageHeaderInfo headerInfo = request.Headers[headerPosition];
            XmlNode[] content = request.Headers.GetHeader<XmlNode[]>(headerPosition);
            return content[0].InnerText;
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            string username = this.provider.Invoke();
            writer.WriteElementString(CustomHeaderName, CustomHeaderNamespace, username);
        }
    }

    public class ThreadPrincipalMessageHeader : MessageHeader
    {
        private const string CustomHeaderName = "ThreadPrincipalToken";
        private const string CustomHeaderNamespace = "hpccluster";

        private ServiceAsClientPrincipalProvider provider;
        private readonly static BinaryFormatter BinaryFormatter = new BinaryFormatter();

        public override string Name => CustomHeaderName;

        public override string Namespace => CustomHeaderNamespace;

        public ThreadPrincipalMessageHeader(ServiceAsClientPrincipalProvider provider)
        {
            this.provider = provider;
        }

        public static IPrincipal ReadHeader(Message request)
        {
            int headerPosition = request.Headers.FindHeader(CustomHeaderName, CustomHeaderNamespace);
            if (headerPosition == -1)
            {
                return null;
            }

            XmlNode[] content = request.Headers.GetHeader<XmlNode[]>(headerPosition);
            if (content.Length < 1)
            {
                return null;
            }

            var serialized = content[0].InnerText;
            if (!string.IsNullOrEmpty(serialized))
            {
                return DeserializePrincipal(serialized);
            }
            else
            {
                return null;
            }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            IPrincipal principal = this.provider.Invoke();
            if (principal != null)
            {
                writer.WriteElementString(CustomHeaderName, CustomHeaderNamespace, SerializePrincipal(principal));
            }
        }

        private static string SerializePrincipal(IPrincipal p)
        {
            Debug.Assert(p.GetType().IsSerializable);
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter.Serialize(stream, p);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        public static IPrincipal DeserializePrincipal(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return BinaryFormatter.Deserialize(stream) as IPrincipal;
            }
        }
    }
}