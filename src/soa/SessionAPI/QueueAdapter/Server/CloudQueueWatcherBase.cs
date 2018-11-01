namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class CloudQueueWatcherBase
    {
        protected IQueueListener<CloudQueueCmdDto> QueueListener { get; set; }

        protected IQueueWriter<CloudQueueResponseDto> QueueWriter { get; set; }

        private readonly Dictionary<string, Func<CloudQueueCmdDto, Task>> cmdNameToDelegate = new Dictionary<string, Func<CloudQueueCmdDto, Task>>();

        protected async Task InvokeInstanceMethodFromCmdObj(CloudQueueCmdDto cmdObj)
        {
            if (cmdObj == null)
            {
                throw new ArgumentNullException(nameof(cmdObj));
            }

            if (string.IsNullOrEmpty(cmdObj.CmdName))
            {
                throw new InvalidOperationException($"{nameof(cmdObj.CmdName)} is null or empty string.");
            }

            if (this.cmdNameToDelegate.TryGetValue(cmdObj.CmdName, out var del))
            {
                await del(cmdObj);
            }
            else
            {
                throw new InvalidOperationException($"Unknown CmdName {cmdObj.CmdName}");
            }
        }

        protected void RegisterCmdDelegate(string cmdName, Func<CloudQueueCmdDto, Task> del)
        {
            this.cmdNameToDelegate[cmdName] = del;
        }

        protected Task CreateAndSendResponse(string requestId, string cmdName, object response)
        {
            var ans = new CloudQueueResponseDto(requestId, cmdName, response);
            return this.QueueWriter.WriteAsync(ans);
        }
    }
}