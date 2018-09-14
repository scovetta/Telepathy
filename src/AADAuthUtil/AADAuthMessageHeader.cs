namespace Microsoft.Hpc.AADAuthUtil
{
    using System;
    using System.ServiceModel.Channels;
    using System.Xml;

    public class AADAuthMessageHeader : MessageHeader
    {
        public static readonly string AADRoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        public static readonly string AADOIDClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public static readonly string CustomHeaderName = "Authentication";
        public static readonly string CustomHeaderNamespace = "hpccluster";

        public string CustomData { get; private set; }

        public AADAuthMessageHeader(string customData)
        {
            this.CustomData = customData;
        }

        public override string Name
        {
            get { return CustomHeaderName; }
        }

        public override string Namespace
        {
            get { return CustomHeaderNamespace; }
        }

        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            writer.WriteElementString(CustomHeaderName, CustomHeaderNamespace, this.CustomData);
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
    }
}
