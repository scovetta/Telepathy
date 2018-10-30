namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter.DTO
{
    using System;

    public class BrokerLauncherCloudQueueCmdDto
    {
        public BrokerLauncherCloudQueueCmdDto(string cmdName, object[] parameters)
        {
            this.RequestId = Guid.NewGuid().ToString();
            this.CmdName = cmdName;
            this.Parameters = parameters;
            this.Version = 1;
        }

        public string RequestId { get; set; }

        public string CmdName { get; set; }

        public object[] Parameters { get; set; }

        public int Version { get; set; }
    }
}