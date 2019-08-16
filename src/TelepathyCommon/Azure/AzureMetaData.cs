//-------------------------------------------------------------------------------------------------
// <copyright file="AzureMetaData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Get the Azure Meta Data.
// </summary>
//-------------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TelepathyCommon.Azure
{
    public class AzureMetaData
    {
        private const string IMDSServer = "169.254.169.254";
        private const string DefaultApiVersion = "2017-08-01";
        private const int RetryManagerInitialWaitSecond = 1;
        private const int RetryManagerMaxWaitSecond = 2;
        private const int RetryManagerMaxRetryTimes = 2;
        private const int RetryManagerTotalTimeLimit = 5;
        private static string _instanceMetaData = null;

        private static async Task<string> QueryMetaData(string path)
        {
            var apiVersion = string.IsNullOrEmpty(ApiVersion) ? DefaultApiVersion : ApiVersion;
            string imdsUri = $"http://{IMDSServer}/metadata/{path}?api-version={apiVersion}";
            var retryManager = new RetryManager(
                new ExponentialBackoffRetryTimer(RetryManagerInitialWaitSecond * 1000, RetryManagerMaxWaitSecond * 1000),
                RetryManagerMaxRetryTimes);
            using (var httpClient = new HttpClient())
            {
                try
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(1);
                    httpClient.DefaultRequestHeaders.Add("Metadata", "True");
                    var resp = await retryManager.InvokeWithRetryAsync(
                        async () =>
                        {
                            var res = await httpClient.GetAsync(imdsUri).ConfigureAwait(false);
                            res.EnsureSuccessStatusCode();
                            return res;
                        },
                        (e) => e is HttpRequestException).ConfigureAwait(false);
                    return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is RetryCountExhaustException || ex is TaskCanceledException)
                {
                    return string.Empty;
                }
            }
        }

        public static string ApiVersion
        {
            get;
            set;
        }

        public static string InstanceMetaData
        {
            get
            {
                if (_instanceMetaData == null)
                    _instanceMetaData = QueryInstanceMetaData().GetAwaiter().GetResult();
                return _instanceMetaData;
            }
        }

        public static async Task<string> QueryInstanceMetaData()
        {
            return await QueryMetaData("instance").ConfigureAwait(false);
        }
    }
}
