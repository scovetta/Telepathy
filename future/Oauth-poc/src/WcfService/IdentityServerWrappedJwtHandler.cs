using IdentityModel.Client;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace WcfService
{
    class IdentityServerWrappedJwtHandler : Saml2SecurityTokenHandler
    {
        X509Certificate2 _signingCert;
        string _issuerName;
        private readonly string[] _requiredScopes;

        public IdentityServerWrappedJwtHandler(string authority, params string[] requiredScopes)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap = new Dictionary<string, string>();

            var client = new HttpClient();
            var disco = client.GetDiscoveryDocumentAsync(authority).Result;
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }
            IdentityModelEventSource.ShowPII = true;
            _signingCert = new X509Certificate2(Convert.FromBase64String(disco.KeySet.Keys.First().X5c.First()));
            _issuerName = disco.Issuer;
            _requiredScopes = requiredScopes;
        }

        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            var saml = token as Saml2SecurityToken;
            var samlAttributeStatement = saml.Assertion.Statements.OfType<Saml2AttributeStatement>().FirstOrDefault();
            var jwt = samlAttributeStatement.Attributes.Where(sa => sa.Name.Equals("jwt", StringComparison.OrdinalIgnoreCase)).SingleOrDefault().Values.Single();
            
            var parameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidAudiences = _requiredScopes,// _issuerName.EnsureTrailingSlash() + "resources",
                ValidIssuer = _issuerName,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.X509SecurityKey(_signingCert)
            };

            Microsoft.IdentityModel.Tokens.SecurityToken validatedToken;
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(jwt, parameters, out validatedToken);
                var ci = new ReadOnlyCollection<ClaimsIdentity>(new List<ClaimsIdentity> { principal.Identities.First() });

                if (_requiredScopes.Any())
                {
                    bool found = false;

                    foreach (var scope in _requiredScopes)
                    {
                        if (principal.HasClaim("scope", scope))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found == false)
                    {
                        throw new SecurityTokenValidationException("Insufficient Scope");
                    }
                }

                return ci;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            
        }
    }

    internal static class StringExtensions
    {
        public static string EnsureTrailingSlash(this string input)
        {
            if (!input.EndsWith("/"))
            {
                return input + "/";
            }

            return input;
        }
    }
}
