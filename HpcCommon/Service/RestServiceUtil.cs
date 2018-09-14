namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    public static class RestServiceUtil
    {
        public const int InternalRestHttpPort = 80;
        public const int InternalRestHttpsPort = 443;

        private static readonly object SetCallbackLock = new object();
        private static bool isCertificateValidationCallbackSet = false;

        public static readonly RemoteCertificateValidationCallback CertificateValidationCallback =
            (s, cert, chain, sslPolicyErrors) =>
            {
                Debug.Assert(cert != null, "Cert passed in validation callback is null.");
                Trace.TraceInformation("cert issuer {0}, cert subject {1}, sslPolicyErrors {2}", cert.Issuer, cert.Subject, sslPolicyErrors);

                foreach (var error in chain.ChainStatus)
                {
                    Trace.TraceInformation("detailed error {0}, info {1}", error.Status, error.StatusInformation);
                }

                sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch;
                return sslPolicyErrors == SslPolicyErrors.None;
            };

        public static readonly RemoteCertificateValidationCallback IgnoreValidationCallback = (sender, certificate, chain, errors) =>
            {
                Trace.TraceInformation(
                    "Ignoring all cert validation error. Cert issuer {0}, cert subject {1}, sslPolicyErrors {2}.",
                    certificate.Issuer,
                    certificate.Subject,
                    errors);
                return true;
            };

#if !NETCORE
        public static void IgnoreCertNameMismatchValidation() => IgnoreCertNameMismatchValidation(null);

        public static void IgnoreCertNameMismatchValidation(this IHpcContext context)
        {
            if (isCertificateValidationCallbackSet)
            {
                return;
            }

            Thread.MemoryBarrier();
            lock (SetCallbackLock)
            {
                if (isCertificateValidationCallbackSet)
                {
                    return;
                }

                var callback = context.GetCertificateValidationCallback();
                if (callback != null)
                {
                    ServicePointManager.ServerCertificateValidationCallback += context.GetCertificateValidationCallback();
                }

                isCertificateValidationCallbackSet = true;
            }
        }

        public static RemoteCertificateValidationCallback GetCertificateValidationCallback(this IHpcContext context)
        {
            var validationType = new NonHARegistry().GetCertificateValidationTypeAsync().GetAwaiter().GetResult();

            if ((context != null && context.FabricContext.IsHpcService()) || validationType == CertificateValidationType.BypassCnValidation)
            {
                Trace.TraceInformation($"[{nameof(GetCertificateValidationCallback)}] Bypass certificate CN validation.");
                return CertificateValidationCallback;
            }
            else if (validationType == CertificateValidationType.None)
            {
                Trace.TraceInformation($"[{nameof(GetCertificateValidationCallback)}] Ignore all certificate validation error.");
                return IgnoreValidationCallback;
            }
            else if (validationType == CertificateValidationType.ValidateAll)
            {
                Trace.TraceInformation($"[{nameof(GetCertificateValidationCallback)}] Do regular certificate validation.");
                return null;
            }
            else
            {
                Trace.TraceWarning($"[{nameof(GetCertificateValidationCallback)}] Unknown {nameof(CertificateValidationType)}: {validationType}. Fallback as {CertificateValidationType.ValidateAll}.");
                return null;
            }
        }

#endif

#if NETCORE
        public static bool ServerCertificateCustomValidationCallback(HttpRequestMessage httpRequestMessage, X509Certificate2 cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Trace.TraceInformation("cert issuer {0}, cert subject {1}, sslPolicyErrors {2}", cert.Issuer, cert.Subject, sslPolicyErrors);
            
            foreach (var error in chain.ChainStatus)
            {
                Trace.TraceInformation("detailed error {0}, info {1}", error.Status, error.StatusInformation);
            }
            
            sslPolicyErrors &= ~SslPolicyErrors.RemoteCertificateNameMismatch;
            return sslPolicyErrors == SslPolicyErrors.None;
        }
#endif
    }
}
