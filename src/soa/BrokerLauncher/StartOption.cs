// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System.Collections.Generic;

    using CommandLine;

    internal class StartOption
    {
        [Option("AzureStorageConnectionString", HelpText = "Azure Storage connection string used in broker launcher")]
        public string AzureStorageConnectionString { get; set; }

        [Option("EnableAzureStorageQueueEndpoint", HelpText = "Weather to enable Azure Storage Queue endpoint")]
        public string EnableAzureStorageQueueEndpoint { get; set; }

        [Option("SvcHostList", HelpText = "Specify the list of service host addresses", Min = 1, Separator = ',')]
        public IEnumerable<string> SvcHostList { get; set; }

        [Option("ReadSvcHostFromEnv", HelpText = "Read service host from Azure Batch environment variable. Disables --SvcHostList")]
        public bool ReadSvcHostFromEnv { get; set; }

        [Option('d', HelpText = "Start as console application")]
        public bool AsConsole { get; set; }

        [Option("SessionAddress", HelpText = "The address of session launcher instance. Broker Launcher will start in standalone mode if SessionAddress is not set.")]
        public string SessionAddress { get; set; }
    }
}