// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.TelepathyContext.Extensions.RegistryExtension
{
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Common.Registry;

    public static class RegistryExtension
    {
        private static string thumbPrintCache;

        public static async Task<string> GetSSLThumbprint(this ITelepathyContext context)
        {
            return await context.Registry.GetSSLThumbprint(context.CancellationToken).ConfigureAwait(false);
        }

        public static async Task<string> GetSSLThumbprint(this IRegistry registry, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(thumbPrintCache))
            {
                thumbPrintCache = await registry.GetValueAsync<string>(TelepathyConstants.HpcFullKeyName, TelepathyConstants.SslThumbprint, cancellationToken).ConfigureAwait(false);
            }

            return thumbPrintCache;
        }
    }
}