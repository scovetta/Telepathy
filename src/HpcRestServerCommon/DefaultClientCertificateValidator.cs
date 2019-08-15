using TelepathyCommon.HpcContext;
using TelepathyCommon.HpcContext.Extensions.RegistryExtension;

namespace Microsoft.Hpc
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    public class DefaultClientCertificateValidator : IClientCertificateValidator
    {
        private static readonly Lazy<string> HeadNodeCertThumbprint =
            new Lazy<string>(() => TelepathyContext.GetOrAdd(CancellationToken.None).GetSSLThumbprint().GetAwaiter().GetResult());

        private static readonly ConcurrentDictionary<string, bool> ValidCertificateCache = new ConcurrentDictionary<string, bool>();

        // We don't cache certificate of failed requests as we need to know the detail exceptions occurred when building X509 Chain.
        private static void SetValidCertificateCache(X509Certificate2 cert)
        {
            if (cert == null || string.IsNullOrEmpty(cert.Thumbprint))
            {
                throw new ArgumentNullException(nameof(cert));
            }

            ValidCertificateCache.TryAdd(cert.Thumbprint, true);
        }

        private static bool CheckValidCertificateCache(X509Certificate2 cert)
        {
            if (cert == null || string.IsNullOrEmpty(cert.Thumbprint))
            {
                return false;
            }

            return ValidCertificateCache.TryGetValue(cert.Thumbprint, out bool _);
        }

        public ClientCertificateValidationResult Validate(X509Certificate2 certificate)
        {
            bool isValid = false;
            List<string> exceptions = new List<string>();
            
            if (CheckValidCertificateCache(certificate))
            {
                isValid = true;
            }
            else
            {
                try
                {
                    using (X509Chain chain = new X509Chain())
                    {
                        X509ChainPolicy chainPolicy = new X509ChainPolicy() { RevocationMode = X509RevocationMode.NoCheck, RevocationFlag = X509RevocationFlag.EntireChain };
                        chain.ChainPolicy = chainPolicy;
                        if (!chain.Build(certificate))
                        {
                            foreach (X509ChainElement chainElement in chain.ChainElements)
                            {
                                foreach (X509ChainStatus chainStatus in chainElement.ChainElementStatus)
                                {
                                    exceptions.Add(chainStatus.StatusInformation);
                                }
                            }
                        }
                        else
                        {
                            if (chain.ChainElements.Cast<X509ChainElement>().Any(e => e.Certificate.Thumbprint.Equals(HeadNodeCertThumbprint.Value, StringComparison.OrdinalIgnoreCase)))
                            {
                                isValid = true;
                                SetValidCertificateCache(certificate);
                            }
                            else
                            {
                                exceptions.Add("Client cert is not signed by Headnode cert.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex.Message);
                }
            }

            ClientCertificateValidationResult res = new ClientCertificateValidationResult(isValid);
            res.AddValidationExceptions(exceptions);
            return res;
        }
    }
}
