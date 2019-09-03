namespace TelepathyCommon.HpcContext.Extensions.RegistryExtension
{
    using System.Threading;
    using System.Threading.Tasks;

    using TelepathyCommon.Registry;

    public static class RegistryExtension
    {
        private static string thumbPrintCache;

        public static async Task<string> GetSSLThumbprint(this ITelepathyContext context)
        {
            return await context.Registry.GetSSLThumbprint(context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> GetSSLThumbprint(this IRegistry registry, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(thumbPrintCache))
            {
                thumbPrintCache = await registry.GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.SslThumbprint, cancellationToken, null).ConfigureAwait(false);
            }

            return thumbPrintCache;
        }

        public static async Task<CertificateValidationType> GetCertificateValidationTypeAsync(this NonHARegistry registry)
        {
            return (CertificateValidationType)await registry.GetValueAsync<int>(
                                                      HpcConstants.HpcFullKeyName,
                                                      HpcConstants.CertificateValidationType,
                                                      CancellationToken.None,
                                                      0) // Default value is CertificateValidationType.None
                                                  .ConfigureAwait(false);
        }
    }
}
