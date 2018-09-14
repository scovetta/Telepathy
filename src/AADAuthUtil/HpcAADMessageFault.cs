namespace Microsoft.Hpc.AADAuthUtil
{
    using System;

    [Serializable]
    public class HpcAADMessageFault
    {
        public const string FaultCode = "AADAuthentication";

        public string Authority { get; private set; }
        public string ClientId { get; private set; }
        public string ServiceResourceId { get; private set; }
        public string RedirectUri { get; private set; }

        public HpcAADMessageFault(string authority, string clientId, string serviceResourceId, string redirectUri)
        {
            this.Authority = authority;
            this.ClientId = clientId;
            this.ServiceResourceId = serviceResourceId;
            this.RedirectUri = redirectUri;
        }
    }
}
