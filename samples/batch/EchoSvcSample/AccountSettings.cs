// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace EchoSvcSample
{
    using System.Text;

    public class AccountSettings
    {
        public string BatchServiceUrl { get; set; }

        public string BatchAccountName { get; set; }

        public string BatchAccountKey { get; set; }

        public string StorageServiceUrl { get; set; }

        public string StorageAccountName { get; set; }

        public string StorageAccountKey { get; set; }

        public string BrokerStorageConnectionString { get; set; }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            AddSetting(stringBuilder, "BatchAccountName", this.BatchAccountName);
            AddSetting(stringBuilder, "BatchAccountKey", this.BatchAccountKey);
            AddSetting(stringBuilder, "BatchServiceUrl", this.BatchServiceUrl);

            AddSetting(stringBuilder, "StorageAccountName", this.StorageAccountName);
            AddSetting(stringBuilder, "StorageAccountKey", this.StorageAccountKey);
            AddSetting(stringBuilder, "StorageServiceUrl", this.StorageServiceUrl);

            AddSetting(stringBuilder, nameof(this.BrokerStorageConnectionString), this.BrokerStorageConnectionString);

            return stringBuilder.ToString();
        }

        private static void AddSetting(StringBuilder stringBuilder, string settingName, object settingValue)
        {
            stringBuilder.AppendFormat("{0} = {1}", settingName, settingValue).AppendLine();
        }
    }
}