
using System;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using IdentityModel.Client;
using WcfService;

namespace WcfClient
{
    class Program
    {
        static void Main(string[] args)
        {
            string jwt;
            if (args.Length > 1)
            {
                jwt = GetJwtByRO(args[0], args[1]).Result;
            }
            else
            {
                jwt = GetJwt().Result;
            }

            var xmlToken = WrapJwt(jwt);

            var binding = new WS2007FederationHttpBinding(WSFederationHttpSecurityMode.TransportWithMessageCredential);
            binding.HostNameComparisonMode = HostNameComparisonMode.Exact;
            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Message.IssuedKeyType = SecurityKeyType.BearerKey;
            
            var factory = new ChannelFactory<IService>(
                binding,
                new EndpointAddress("https://localhost:44335/token"));

            var channel = factory.CreateChannelWithIssuedToken(xmlToken);

            while (true)
            {
                string str = Console.ReadLine();
                string[] ops = str.Split(' ');

                if (ops[0].Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(channel.Add(ops[1])? "Add success" : "Add Error");
                }
                else if (ops[0].Equals("echo", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(channel.Echo());
                }
                else if (ops[0].Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(channel.Update(ops[1], ops[2]) ? "Update success" : "Update Error");
                }
                else if (ops[0].Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(channel.GetInfo());
                }
                else if (ops[0].Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Input operate error!");
                }
            }

        }

        static GenericXmlSecurityToken WrapJwt(string jwt)
        {
            var subject = new ClaimsIdentity("saml");
            subject.AddClaim(new Claim("jwt", jwt));

            var descriptor = new SecurityTokenDescriptor
            {
                //TokenType = TokenTypes.Saml2TokenProfile11,
                TokenIssuerName = "urn:wrappedjwt",
                Subject = subject
            };

            var handler = new Saml2SecurityTokenHandler();
            var token = handler.CreateToken(descriptor);

            var sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb))
            {
                handler.WriteToken(writer, token);
            }

            var doc = new XmlDocument();
            doc.Load(XElement.Parse(sb.ToString()).CreateReader());

            var xmlToken = new GenericXmlSecurityToken(
                doc.DocumentElement,
                null,
                DateTime.Now,
                DateTime.Now.AddHours(1),
                null,
                null,
                null);

            return xmlToken;
        }

        static async Task<string> GetJwt()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return string.Empty;
            }


            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "TestService",
            });



            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return string.Empty;
            }

            return tokenResponse.AccessToken;
        }

        static async Task<string> GetJwtByRO(string userName, string password)
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return string.Empty;
            }


            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest()
            {
                Address = disco.TokenEndpoint,

                ClientId = "ro.client",
                ClientSecret = "secret",
                Scope = "TestService",

                UserName = userName,
                Password = password
            });



            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return string.Empty;
            }

            return tokenResponse.AccessToken;
        }
    }
}