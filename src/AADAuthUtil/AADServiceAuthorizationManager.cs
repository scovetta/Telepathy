namespace Microsoft.Hpc.AADAuthUtil
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Selectors;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Security.Claims;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;
    using IdentityModel.Protocols;

    public class AADServiceAuthorizationManager : ServiceAuthorizationManager
    {
        private const int SigningTokenRefreshInterval = 24;

        private string issuer = string.Empty;
        private List<SecurityToken> signingTokens = null;
        private DateTime stsMetadataRetrievalTime = DateTime.MinValue;
        private JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        private string audience;
        private Uri authority;
        private string clientAppId;
        private string clientRedirectUrl;
        private string stsDiscoveryEndpoint;
        private object refreshTokenLock = new object();
        private TokenValidationParameters validationParameters = new TokenValidationParameters() { CertificateValidator = X509CertificateValidator.None };
        private string targetPath = null;
        private IHpcContext context;

        private IHpcContext Context => this.context ?? HpcContext.Get();

        public AADServiceAuthorizationManager()
        {
        }

        public AADServiceAuthorizationManager(string targetPath) : this(targetPath, null)
        {
        }

        public AADServiceAuthorizationManager(IHpcContext context) : this(null, context)
        {
        }

        public AADServiceAuthorizationManager(string targetPath, IHpcContext context)
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                Debug.WriteLine($"[AADServiceAuthorizationManager] targetPath={targetPath}");
            }
            this.targetPath = targetPath;
            this.context = context;
        }

        private bool NeedRefreshSigningToken()
        {
            // The issuer and signingTokens are cached for 24 hours. They are updated if any of the conditions is true.
            return DateTime.UtcNow.Subtract(this.stsMetadataRetrievalTime).TotalHours > SigningTokenRefreshInterval
                || string.IsNullOrEmpty(this.issuer)
                || this.signingTokens == null;
        }

        private void RefreshSigningTokens()
        {
            if (!this.NeedRefreshSigningToken())
            {
                return;
            }

            lock (this.refreshTokenLock)
            {
                if (!this.NeedRefreshSigningToken())
                {
                    return;
                }

                this.UpdateSigningTokens().GetAwaiter().GetResult();
            }
        }

        private async Task UpdateSigningTokens()
        {
            this.clientAppId = await this.Context.GetAADClientAppIdAsync().ConfigureAwait(false);
            this.clientRedirectUrl = await this.Context.GetAADClientAppRedirectUriAsync().ConfigureAwait(false);
            string tenant = await this.Context.GetAADTenantAsync().ConfigureAwait(false);
            string appName = await this.Context.GetAADAppNameAsync().ConfigureAwait(false);
            string aadInstance = await this.Context.GetAADInstanceAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(this.clientAppId) ||
               string.IsNullOrEmpty(this.clientRedirectUrl) ||
               string.IsNullOrEmpty(tenant) ||
               string.IsNullOrEmpty(appName) ||
               string.IsNullOrEmpty(aadInstance))
            {
                throw new FaultException("AAD related information is missing or misconfigure in server side");
            }

            UriBuilder builder = new UriBuilder()
            {
                Scheme = Uri.UriSchemeHttps,
                Host = tenant,
                Path = appName
            };

            this.audience = builder.ToString();
            this.authority = new Uri(new Uri(aadInstance), tenant);
            this.stsDiscoveryEndpoint = new Uri(new Uri(aadInstance), $"{tenant}/.well-known/openid-configuration").ToString();
            this.validationParameters.ValidAudience = this.audience;

            // Get tenant information that's used to validate incoming jwt tokens
            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint);
            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync().ConfigureAwait(false);
            this.issuer = config.Issuer;
            this.signingTokens = config.SigningTokens.ToList();
            this.stsMetadataRetrievalTime = DateTime.UtcNow;
            this.validationParameters.ValidIssuer = this.issuer;
            this.validationParameters.IssuerSigningTokens = this.signingTokens;
        }

        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            if (!string.IsNullOrEmpty(this.targetPath))
            {
                string incomingPath = operationContext.IncomingMessageHeaders.To.PathAndQuery.ToString();
                Debug.WriteLine($"[AADServiceAuthorizationManager] IncomingMessageHeaders.To={incomingPath}");
                if (!incomingPath.Equals(this.targetPath, StringComparison.InvariantCultureIgnoreCase))
                {
                    Debug.WriteLine($"[AADServiceAuthorizationManager] IncomingMessageHeaders.To doesn't match.");

                    // This thread comes from thread pool, which may previously be assigned AAD user principal.
                    // We need to clear principal here.
                    Thread.CurrentPrincipal = null;
                    return base.CheckAccessCore(operationContext);
                }
                else
                {
                    Debug.WriteLine($"[AADServiceAuthorizationManager] IncomingMessageHeaders.To matches.");
                }
            }

            string customData = AADAuthMessageHeader.ReadHeader(operationContext.RequestContext.RequestMessage);
            return this.CheckHeaderAccess(customData);
        }

        public bool CheckHeaderAccess(string header)
        {
            ClaimsPrincipal principle = this.ValidateJwtToken(header);
            Claim role = principle.Claims.FirstOrDefault(c => c.Type == AADAuthMessageHeader.AADRoleClaimType);
            if (role == null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(role.Value))
            {
                ((ClaimsIdentity)principle.Identity).AddClaim(new Claim(ClaimTypes.Role, role.Value));
            }

            Thread.CurrentPrincipal = principle;
            return true;
        }

        public ClaimsPrincipal ValidateJwtToken(string jwtToken)
        {
            this.RefreshSigningTokens();

            try
            {
                // Validate token.
                SecurityToken validatedToken;
                return this.tokenHandler.ValidateToken(jwtToken, this.validationParameters, out validatedToken);
            }
            catch (Exception)
            {
                throw new FaultException(
                    MessageFault.CreateFault(
                        new FaultCode(HpcAADMessageFault.FaultCode),
                        new FaultReason("Need Jwt token from AAD"),
                        new HpcAADMessageFault(this.authority.ToString(), this.clientAppId, this.audience, this.clientRedirectUrl)
                    ));
            }
        }
    }
}
