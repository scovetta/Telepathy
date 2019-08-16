using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace TelepathyCommon.Telepathy
{
    public abstract class ServiceRegistrationStore : IServiceRegistrationStore
    {
        private static readonly MemoryCache CacheInstance = new MemoryCache("ServiceRegistration");

        private static DateTimeOffset ExpireIn5Secs => DateTimeOffset.Now.AddSeconds(5);
        
        /// <summary>
        /// Find specific service registration file and return the content.
        /// </summary>
        /// <param name="serviceName">The service name.</param>
        /// <param name="serviceVersion">The service version.</param>
        /// <returns>Content of service registration file. Return <see langword="null"/> if file not found.</returns>
        protected abstract Task<string> GetCoreAsync(string serviceName, Version serviceVersion);

        public async Task<string> GetMd5Async(string serviceName, Version serviceVersion) => (await this.GetServiceRegistrationInfo(serviceName, serviceVersion).ConfigureAwait(false)).Md5;

        public async Task<string> GetAsync(string serviceName, Version serviceVersion) => (await this.GetServiceRegistrationInfo(serviceName, serviceVersion).ConfigureAwait(false)).ServiceRegistration;

        public abstract Task SetAsync(string serviceName, Version serviceVersion, string serviceRegistration);

        public abstract Task DeleteAsync(string serviceName, Version serviceVersion);
        
        /// <inheritdoc />
        /// <summary>
        /// Enumerate serivce registration files
        /// </summary>
        /// <returns>Service registration file name list. Because a Reliable Registry restrict, return value should be in lowercase.</returns>
        public abstract Task<List<string>> EnumerateAsync();

        /// <inheritdoc />
        public async Task ImportFromFileAsync(string filePath, string serviceName)
            => await SoaRegistrationAuxModule.ImportServiceRegistrationFromFileAuxAsync(this.SetAsync, filePath, serviceName).ConfigureAwait(false);

        public async Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion)
            => await SoaRegistrationAuxModule.ExportServiceRegistrationToTempFileAuxAsync(this.GetAsync, serviceName, serviceVersion).ConfigureAwait(false);

        private async Task<ServiceRegistrationInfo> GetServiceRegistrationInfo(string serviceName, Version serviceVersion)
        {
            string key = SoaRegistrationAuxModule.GetRegistrationName(serviceName, serviceVersion);
            var res = CacheInstance.Get(key);
            if (res == null)
            {
                var svrReg = await this.GetCoreAsync(serviceName, serviceVersion).ConfigureAwait(false);
                if (string.IsNullOrEmpty(svrReg))
                {
                    return ServiceRegistrationInfo.Empty;
                }

                var svcRegInfo = new ServiceRegistrationInfo(svrReg);
                ServiceRegistrationInfo newVal = CacheInstance.AddOrGetExisting(key, svcRegInfo, ExpireIn5Secs) as ServiceRegistrationInfo;
                return newVal ?? svcRegInfo;
            }
            else
            {
                return (ServiceRegistrationInfo)res;
            }
        }
    }
}