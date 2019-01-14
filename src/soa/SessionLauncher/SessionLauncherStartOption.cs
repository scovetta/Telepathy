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
        [Option("AzureBatchServiceUrl", SetName = "AzureBatch")]
        public string AzureBatchServiceUrl { get; set; }

        [Option("AzureBatchAccountName", SetName = "AzureBatch")]
        public string AzureBatchAccountName { get; set; }

        [Option("AzureBatchAccountKey", SetName = "AzureBatch")]
        public string AzureBatchAccountKey { get; set; }

        [Option("AzureBatchJobId", SetName = "AzureBatch")]
        public string AzureBatchJobId { get; set; }

        [Option("AzureBatchPoolName", SetName = "AzureBatch")]
        public string AzureBatchPoolName { get; set; }

        [Option("HpcPackSchedulerAddress", SetName = "HpcPack")]
        public string HpcPackSchedulerAddress { get; set; }

        [Option('d', HelpText = "Start as console application")]
        public bool AsConsole { get; set; }
    }
}
