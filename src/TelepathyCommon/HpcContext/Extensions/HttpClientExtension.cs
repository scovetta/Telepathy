namespace Microsoft.Hpc
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class HttpClientExtension
    {
        private const string ApiUriPattern = "https://{0}/{1}";

        private const int RetryManagerInitialWaitSecond = 1;
        private const int RetryManagerMaxWaitSecond = 4;
        private const int RetryManagerMaxRetryTimes = 3;
        private const int RetryManagerTotalTimeLimit = 10;

        // Please note this method swallows all exceptions by default
        public static async Task<HttpResponseMessage> GetHttpApiCallAsync(this HttpClient httpClient, string endpoint, string apiRoute, CancellationToken cancellationToken = default(CancellationToken), Func<Exception, bool> exPredicate = null, bool ensureSuccessStatusCode = true)
        {
            RetryManager retryManager = DefaultRetryManager;
            return await retryManager.InvokeWithRetryAsync(
                async () =>
                    {
                        var res = await httpClient.GetAsync(string.Format(ApiUriPattern, endpoint, apiRoute), cancellationToken).ConfigureAwait(false);
                        if(ensureSuccessStatusCode)
                        {
                            res.EnsureSuccessStatusCode();
                        }
                        return res;
                    },
                exPredicate ?? RetryAllExceptions, 
                cancellationToken).ConfigureAwait(false);
        }
        
        // Please note this method swallows all exceptions by default
        public static async Task<HttpResponseMessage> PostHttpApiCallAsync<T>(this HttpClient httpClient, string endpoint, string apiRoute, T value, CancellationToken cancellationToken = default(CancellationToken), Func<Exception, bool> exPredicate = null, bool ensureSuccessStatusCode = true)
        {
            RetryManager retryManager = DefaultRetryManager;
            return await retryManager.InvokeWithRetryAsync(
                async () =>
                    {
                        var res = await httpClient.PostAsJsonAsync(string.Format(ApiUriPattern, endpoint, apiRoute), value, cancellationToken).ConfigureAwait(false);
                        if (ensureSuccessStatusCode)
                        {
                            res.EnsureSuccessStatusCode();
                        }
                        return res;
                    },
                exPredicate ?? RetryAllExceptions, 
                cancellationToken).ConfigureAwait(false);
        }

        private static RetryManager DefaultRetryManager => new RetryManager(
            new ExponentialBackoffRetryTimer(RetryManagerInitialWaitSecond * 1000, RetryManagerMaxWaitSecond * 1000),
            RetryManagerMaxRetryTimes,
            RetryManagerTotalTimeLimit * 1000);

        private static bool RetryAllExceptions(Exception ex) => true; // Swallowed inner TaskCanceledException and stop retry
    }
}