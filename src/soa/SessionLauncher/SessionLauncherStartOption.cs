namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using CommandLine;

    internal class SessionLauncherStartOption
    {
        [Option('s', "AzureBatchServiceUrl", SetName = "AzureBatch")]
        public string AzureBatchServiceUrl { get; set; }

        [Option('n', "AzureBatchAccountName", SetName = "AzureBatch")]
        public string AzureBatchAccountName { get; set; }

        [Option('k', "AzureBatchAccountKey", SetName = "AzureBatch")]
        public string AzureBatchAccountKey { get; set; }

        [Option('j', "AzureBatchJobId", SetName = "AzureBatch")]
        public string AzureBatchJobId { get; set; }

        [Option('p', "AzureBatchPoolName", SetName = "AzureBatch")]
        public string AzureBatchPoolName { get; set; }

        [Option('c', "AzureBatchBrokerStorageConnectionString", SetName = "AzureBatch")]
        public string AzureBatchBrokerStorageConnectionString { get; set; }

        [Option('h', "HpcPackSchedulerAddress", SetName = "HpcPack")]
        public string HpcPackSchedulerAddress { get; set; }

        [Option('d', HelpText = "Start as console application")]
        public bool AsConsole { get; set; }
    }
}