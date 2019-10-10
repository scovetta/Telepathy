// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.ServiceRegistrationStore
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Caching;
    using System.Threading.Tasks;

    public abstract class ServiceRegistrationStore : IServiceRegistrationStore
    {
        private static readonly MemoryCache CacheInstance = new MemoryCache("ServiceRegistration");

        private static DateTimeOffset ExpireIn5Secs => DateTimeOffset.Now.AddSeconds(5);

        public abstract Task DeleteAsync(string serviceName, Version serviceVersion);

        /// <inheritdoc />
        /// <summary>
        ///     Enumerate serivce registration files
        /// </summary>
        /// <returns>
        ///     Service registration file name list. Because a Reliable Registry restrict, return value should be in
        ///     lowercase.
        /// </returns>
        public abstract Task<List<string>> EnumerateAsync();

        public async Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion)
        {
            return await SoaRegistrationAuxModule.ExportServiceRegistrationToTempFileAuxAsync(this.GetAsync, serviceName, serviceVersion).ConfigureAwait(false);
        }

        public async Task<string> GetAsync(string serviceName, Version serviceVersion)
        {
            return (await this.GetServiceRegistrationInfo(serviceName, serviceVersion).ConfigureAwait(false)).ServiceRegistration;
        }

        public async Task<string> GetMd5Async(string serviceName, Version serviceVersion)
        {
            return (await this.GetServiceRegistrationInfo(serviceName, serviceVersion).ConfigureAwait(false)).Md5;
        }

        /// <inheritdoc />
        public async Task ImportFromFileAsync(string filePath, string serviceName)
        {
            await SoaRegistrationAuxModule.ImportServiceRegistrationFromFileAuxAsync(this.SetAsync, filePath, serviceName).ConfigureAwait(false);
        }

        public abstract Task SetAsync(string serviceName, Version serviceVersion, string serviceRegistration);

        public string CalculateMd5Hash(byte[] blobData)
        {
            return SoaRegistrationAuxModule.CalculateMd5Hash(System.Text.Encoding.UTF8.GetString(blobData, 0, blobData.Length));
        }

        /// <summary>
        ///     Find specific service registration file and return the content.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceVersion">The service version.</param>
        /// <returns>Content of service registration file. Return <see langword="null" /> if file not found.</returns>
        protected abstract Task<string> GetCoreAsync(string serviceName, Version serviceVersion);

        private async Task<ServiceRegistrationInfo> GetServiceRegistrationInfo(string serviceName, Version serviceVersion)
        {
            var key = SoaRegistrationAuxModule.GetRegistrationName(serviceName, serviceVersion);
            var res = CacheInstance.Get(key);
            if (res == null)
            {
                var svrReg = await this.GetCoreAsync(serviceName, serviceVersion).ConfigureAwait(false);
                if (string.IsNullOrEmpty(svrReg))
                {
                    return ServiceRegistrationInfo.Empty;
                }

                var svcRegInfo = new ServiceRegistrationInfo(svrReg);
                var newVal = CacheInstance.AddOrGetExisting(key, svcRegInfo, ExpireIn5Secs) as ServiceRegistrationInfo;
                return newVal ?? svcRegInfo;
            }

            return (ServiceRegistrationInfo)res;
        }
    }
}