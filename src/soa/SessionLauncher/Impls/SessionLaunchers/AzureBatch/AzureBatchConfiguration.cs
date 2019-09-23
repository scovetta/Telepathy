// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch
{
    using System;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Auth;

    internal static class AzureBatchConfiguration
    {
        public static string BatchServiceUrl { get; set; }

        public static string BatchAccountName { get; set; }

        public static string BatchAccountKey { get; set; }

        public static string BatchPoolName { get; set; }

        public static string BatchJobId { get; set; }

        public static string SoaBrokerStorageConnectionString { get; set; }

        public static string BrokerLauncherPath { get; set; }

        public static string GetBatchJobId()
        {
            if (!string.IsNullOrEmpty(BatchJobId))
            {
                return BatchJobId;
            }

            return AzureBatchEnvVarReader.GetJobId();
        }

        public static BatchClient GetBatchClient()
        {
            if (string.IsNullOrEmpty(BatchServiceUrl))
            {
                throw new InvalidOperationException($"{nameof(BatchServiceUrl)} is not properly set.");
            }

            if (string.IsNullOrEmpty(BatchAccountName))
            {
                throw new InvalidOperationException($"{nameof(BatchAccountName)} is not properly set.");
            }

            if (string.IsNullOrEmpty(BatchAccountKey))
            {
                throw new InvalidOperationException($"{nameof(BatchAccountKey)} is not properly set.");
            }

            return BatchClient.Open(new BatchSharedKeyCredentials(BatchServiceUrl, BatchAccountName, BatchAccountKey));
        }
    }
}