namespace Microsoft.Hpc.AADAuthUtil
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Linq;
    using System.Security;
    using System.Security.Claims;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    using TelepathyCommon.HpcContext;
    using TelepathyCommon.HpcContext.Extensions.RegistryExtension;
    using TelepathyCommon.Service;

    public static class AADAuthUtil
    {
        private const string TokenCachedFile = "HpcTokenCache.dat";
        
        private static string DefaultTokenCachePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), TokenCachedFile);

        public static void RemoveTokenCache()
        {
            RemoveTokenCache(DefaultTokenCachePath);
        }

        public static void RemoveTokenCache(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }

        public static async Task<AuthenticationResult> AcquireAADTokenAsync(string resourceId, string authority, string clientId, string rediretUri, string cacheFile, PromptBehavior promptBehavior, UserIdentifier userIdentifier)
        {
            var authContext = new AuthenticationContext(authority, new FileCache(cacheFile ?? DefaultTokenCachePath));
            try
            {
                if (userIdentifier == null)
                {
                    return await authContext.AcquireTokenAsync(resourceId, clientId, new Uri(rediretUri), new PlatformParameters(promptBehavior)).ConfigureAwait(false);
                }
                else
                {
                    return await authContext.AcquireTokenAsync(resourceId, clientId, new Uri(rediretUri), new PlatformParameters(promptBehavior), userIdentifier).ConfigureAwait(false);
                }
            }
            catch (AdalException e)
            {
                Trace.TraceError("Failed to acquire token from AAD: {0}", e);
                throw;
            }
        }

        public static async Task<string> AcquireAADJwtTokenAsync(string resourceId, string authority, string clientId, string rediretUri)
        {
            return (await AcquireAADTokenAsync(resourceId, authority, clientId, rediretUri, DefaultTokenCachePath, PromptBehavior.Auto, null).ConfigureAwait(false)).AccessToken;
        }

        public static async Task<string> AcquireAADJwtTokenAsync(string resourceId, string authority, string clientId, string userName, SecureString password)
        {
            var authContext = new AuthenticationContext(authority, new FileCache(DefaultTokenCachePath));
            try
            {
                return (await authContext.AcquireTokenAsync(resourceId, clientId, new UserPasswordCredential(userName, password)).ConfigureAwait(false)).AccessToken;
            }
            catch (AdalException e)
            {
                Trace.TraceError("Failed to acquire token from AAD: {0}", e);
                throw;
            }
        }

        public static async Task<string> AcquireAADJwtTokenAsync(string appId, string authrority, string resource, X509Certificate2 cert, string cacheFile = null)
        {
            var context = new AuthenticationContext(authrority, new FileCache(cacheFile ?? DefaultTokenCachePath));
            var assertioncert = new ClientAssertionCertificate(appId, cert);

            try
            {
                return (await context.AcquireTokenAsync(resource, assertioncert).ConfigureAwait(false)).AccessToken;
            }
            catch (AdalException e)
            {
                Trace.TraceError("Failed to acquire token from AAD: {0}", e);
                throw;
            }
        }

        public static string GenerateSidByOid(string oid)
        {
            string sid = string.Concat("S-1-1-", oid.ToLower());
            sid = sid.Replace("a", "10");
            sid = sid.Replace("b", "11");
            sid = sid.Replace("c", "12");
            sid = sid.Replace("d", "13");
            sid = sid.Replace("e", "14");
            sid = sid.Replace("f", "15");

            return sid;
        }

        /// <summary>
        /// Generate <see cref="SecurityIdentifier"/> from AAD identity.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns><see langword="null"/> if <paramref name="principal"/> is not HPC AAD principal</returns>
        public static SecurityIdentifier GenerateSecurityIdentifierFromAadPrincipal(this IPrincipal principal, ITelepathyContext telepathyContext = null, string aadInfoNode = null)
        {
            if (!principal.IsHpcAadPrincipal(telepathyContext, aadInfoNode))
            {
                return null;
            }
            else
            {
                Claim identifier = ((ClaimsIdentity)principal.Identity).Claims.FirstOrDefault(c => c.Type == AADAuthMessageHeader.AADOIDClaimType);
                Debug.Assert(identifier != null, "Hpc AAD Identity should have AADOIDClaimType");
                return new SecurityIdentifier(AADAuthUtil.GenerateSidByOid(identifier.Value));
            }
        }

        [Pure]
        public static bool IsAADAuthenticationException(this FaultException ex) => ex.CreateMessageFault().Code.Name.Equals(HpcAADMessageFault.FaultCode, StringComparison.OrdinalIgnoreCase);

        [Pure]
        public static async Task<string> GetAADJwtTokenFromExAsync(FaultException ex, string userName, string password) 
        {
            Debug.Assert(ex.IsAADAuthenticationException());
            HpcAADMessageFault fault = ex.CreateMessageFault().GetDetail<HpcAADMessageFault>();
            return await GetAADJwtTokenAsync(fault.ServiceResourceId, fault.Authority, fault.ClientId, fault.RedirectUri, userName, password).ConfigureAwait(false);
        }

        [Pure]
        public static async Task<string> GetAADJwtTokenAsync(string serviceResourceId, string authority, string clientId, string redirectUri, string userName, string password) 
        {
            // try get JWT token
            if (userName != null && password != null)
            {
                var securePass = new SecureString();
                foreach (var c in password)
                {
                    securePass.AppendChar(c);
                }

                securePass.MakeReadOnly();
                return await AcquireAADJwtTokenAsync(serviceResourceId, authority, clientId, userName, securePass).ConfigureAwait(false);
            }

                return await AcquireAADJwtTokenAsync(serviceResourceId, authority, clientId, redirectUri).ConfigureAwait(false);
        }

        [Pure]
        public static async Task<string> GetAADJwtTokenAsync(this ITelepathyContext context, string userName, string password, string aadInfoNode = null) 
        {
            string clientId = await context.GetAADClientAppIdAsync(aadInfoNode).ConfigureAwait(false);
            string redirectUrl = await context.GetAADClientAppRedirectUriAsync(aadInfoNode).ConfigureAwait(false);
            string tenant = await context.GetAADTenantAsync(aadInfoNode).ConfigureAwait(false);
            string appName = await context.GetAADAppNameAsync(aadInfoNode).ConfigureAwait(false);
            string aadInstance = await context.GetAADInstanceAsync(aadInfoNode).ConfigureAwait(false);

            string authority = new Uri(new Uri(aadInstance), tenant).ToString();
            UriBuilder builder = new UriBuilder()
            {
                Scheme = Uri.UriSchemeHttps,
                Host = tenant,
                Path = appName
            };

            string serviceResourceId = builder.ToString();
            return await GetAADJwtTokenAsync(serviceResourceId, authority, clientId, redirectUrl, userName, password).ConfigureAwait(false);
        }
    }
}
