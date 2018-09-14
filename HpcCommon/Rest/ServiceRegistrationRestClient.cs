namespace Microsoft.Hpc.Rest
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using static SoaRegistrationAuxModule;

    public class ServiceRegistrationRestClient : HpcRestClient, IServiceRegistrationStore
    {
        private const string EmptyThumbPrint = "Empty";

        public ServiceRegistrationRestClient(IHpcContext context) : base(context)
        {
            this.thumbprint = this.Context.GetSSLThumbprint().GetAwaiter().GetResult();

            if (string.IsNullOrEmpty(this.thumbprint))
            {
                Trace.TraceInformation($"[{this.RestClientName}] Cert thumbprint is null or empty. No client cert will be used");
                this.thumbprint = EmptyThumbPrint;
            }

            if (!RestClientCache.ContainsKey(this.thumbprint))
            {
                RestClientCache.TryAdd(this.thumbprint, new Lazy<HttpClient>(() => this.HttpClientFactory(this.thumbprint)));
            }
        }

        private HttpClient HttpClientFactory(string clientThumbprint)
        {
            if (string.IsNullOrEmpty(clientThumbprint))
            {
                throw new ArgumentException(nameof(clientThumbprint));
            }

            if (clientThumbprint == EmptyThumbPrint)
            {
                return HpcRestClient.DefaultHttpClientFactory();
            }

            WebRequestHandler handler = new WebRequestHandler() { Proxy = null, UseProxy = false };
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                var cert = store.Certificates.Find(X509FindType.FindByThumbprint, clientThumbprint, true);
                if (cert.Count > 0)
                {
                    if (cert.Count > 1)
                    {
                        Trace.TraceWarning(
                            $"[{this.RestClientName}] More than one certs with thumbprint {clientThumbprint} are found. Use first match.");
                    }

                    handler.ClientCertificates.Add(cert[0]);
                }
                else
                {
                    Trace.TraceWarning($"[{this.RestClientName}] Cannot find cert with thumbprint {clientThumbprint}. No client cert will be used.");
                }
            }
            finally
            {
                store.Close();
            }

            return new HttpClient(handler);
        }

        private static readonly ConcurrentDictionary<string, Lazy<HttpClient>> RestClientCache = new ConcurrentDictionary<string, Lazy<HttpClient>>();

        private readonly string thumbprint;

        protected override sealed HttpClient RestClient => RestClientCache[this.thumbprint].Value;

        protected override sealed string RestClientName => nameof(ServiceRegistrationRestClient);

        protected override string ApiRoot => "api/soa/service-registration";

        private CancellationToken CancellationToken => this.Context.CancellationToken;

        public async Task<string> GetAsync(
            string serviceName,
            string serviceVersion = "",
            CancellationToken token = default(CancellationToken)) =>
            await this.GetHttpApiCallAsync<string>(token, "{0}?serviceVersion={1}", serviceName, serviceVersion).ConfigureAwait(false);

        public async Task<string> GetAsync(string serviceName, Version serviceVersion, CancellationToken token) =>
            await this.GetAsync(serviceName, serviceVersion == null ? string.Empty : serviceVersion.ToString(), token)
                .ConfigureAwait(false);

        public async Task<string> GetMd5Async(
            string serviceName,
            string serviceVersion = "",
            CancellationToken token = default(CancellationToken)) =>
            await this.GetHttpApiCallAsync<string>(token, "{0}/md5?serviceVersion={1}", serviceName, serviceVersion).ConfigureAwait(false);

        public async Task<string> GetMd5Async(string serviceName, Version serviceVersion, CancellationToken token) =>
            await this.GetMd5Async(serviceName, serviceVersion == null ? string.Empty : serviceVersion.ToString(), token)
                .ConfigureAwait(false);

        public async Task SetAsync(
            string serviceName,
            string serviceRegistration,
            string serviceVersion = "",
            CancellationToken token = default(CancellationToken)) =>
            await this.PostHttpApiCallAsync<object, string>(token, "{0}?serviceVersion={1}", serviceRegistration, serviceName, serviceVersion)
                .ConfigureAwait(false);

        public async Task
            SetAsync(string serviceName, string serviceRegistration, Version serviceVersion, CancellationToken token) =>
            await this.SetAsync(
                    serviceName,
                    serviceRegistration,
                    serviceVersion == null ? string.Empty : serviceVersion.ToString(),
                    token)
                .ConfigureAwait(false);

        public async Task DeleteAsync(
            string serviceName,
            string serviceVersion = "",
            CancellationToken token = default(CancellationToken)) =>
            await this.PostHttpApiCallAsync<object, string>(token, "{0}/delete?serviceVersion={1}", null, serviceName, serviceVersion)
                .ConfigureAwait(false);

        public Task DeleteAsync(string serviceName, Version serviceVersion, CancellationToken token) =>
            this.DeleteAsync(serviceName, serviceVersion == null ? string.Empty : serviceVersion.ToString(), token);

        public async Task<List<string>> EnumerateAsync(CancellationToken token) =>
            await this.GetHttpApiCallAsync<List<string>>(token, string.Empty).ConfigureAwait(false);

        public async Task ImportFromFileAsync(string filePath, string serviceName, CancellationToken token) =>
            await ImportServiceRegistrationFromFileAuxAsync(
                    (name, ver, content) => this.SetAsync(name, content, ver, token),
                    filePath,
                    serviceName)
                .ConfigureAwait(false);

        public async Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion, CancellationToken token)
        {
            string md5 = await this.GetMd5Async(serviceName, serviceVersion, token).ConfigureAwait(false);
            string localCacheFilePath = GetServiceRegistrationTempFilePath(md5);
            if (File.Exists(localCacheFilePath))
            {
                return localCacheFilePath;
            }
            else
            {
                return await ExportServiceRegistrationToTempFileAuxAsync(
                               (name, ver) => this.GetAsync(name, ver, token),
                               serviceName,
                               serviceVersion,
                               md5)
                           .ConfigureAwait(false);
            }
        }

        public async Task ExportToFileAsync(
            string serviceName,
            Version serviceVersion,
            string fileName,
            CancellationToken token = default(CancellationToken)) => await ExportServiceRegistrationToFileAuxAsync(
                                                                             (name, ver) => this.GetAsync(name, ver, token),
                                                                             serviceName,
                                                                             serviceVersion,
                                                                             fileName)
                                                                         .ConfigureAwait(false);

        public Task<string> GetMd5Async(string serviceName, Version serviceVersion) =>
            this.GetMd5Async(serviceName, serviceVersion, this.CancellationToken);

        public Task<string> GetAsync(string serviceName, Version serviceVersion) =>
            this.GetAsync(serviceName, serviceVersion, this.CancellationToken);

        public Task SetAsync(string serviceName, Version serviceVersion, string serviceRegistration) =>
            this.SetAsync(serviceName, serviceRegistration, serviceVersion, this.CancellationToken);

        public Task DeleteAsync(string serviceName, Version serviceVersion) =>
            this.DeleteAsync(serviceName, serviceVersion, this.CancellationToken);

        public Task<List<string>> EnumerateAsync() => this.EnumerateAsync(this.CancellationToken);

        public Task ImportFromFileAsync(string filePath, string serviceName) =>
            this.ImportFromFileAsync(filePath, serviceName, this.CancellationToken);

        public Task<string> ExportToTempFileAsync(string serviceName, Version serviceVersion) =>
            this.ExportToTempFileAsync(serviceName, serviceVersion, this.CancellationToken);
    }

    public static class ServiceRegistrationRestClientHpcContextExtension
    {
        public static ServiceRegistrationRestClient GetServiceRegistrationRestClient(this IHpcContext context) => new ServiceRegistrationRestClient(context);
    }
}
