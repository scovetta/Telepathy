namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    public class BrokerLauncherCloudQueueWatcher
    {
        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, string connectionString)
        {
            this.instance = instance;

            CloudQueueSerializer serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);

            this.queueListener = new CloudQueueListener<CloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerLauncherRequestQueueName, serializer, this.InvokeInstanceMethodFromCmdObj);
            this.queueWriter = new CloudQueueWriter<CloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer);
            this.queueListener.StartListen();

            this.RegisterCmdDelegates();
            Trace.TraceInformation("BrokerLauncherCloudQueueWatcher started.");
        }

        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, IQueueListener<CloudQueueCmdDto> listener, IQueueWriter<CloudQueueResponseDto> writer)
        {
            this.instance = instance;
            this.queueListener = listener;
            this.queueWriter = writer;
            this.queueListener.MessageReceivedCallback = this.InvokeInstanceMethodFromCmdObj;
            this.RegisterCmdDelegates();
        }

        private readonly IBrokerLauncher instance;

        private readonly IQueueListener<CloudQueueCmdDto> queueListener;

        private readonly IQueueWriter<CloudQueueResponseDto> queueWriter;

        private async Task InvokeInstanceMethodFromCmdObj(CloudQueueCmdDto cmdObj)
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

        private void RegisterCmdDelegates()
        {
            this.RegisterCmdDelegate(nameof(this.Create), this.Create);
            this.RegisterCmdDelegate(nameof(this.CreateDurable), this.CreateDurable);
            this.RegisterCmdDelegate(nameof(this.Attach), this.Attach);
            this.RegisterCmdDelegate(nameof(this.Close), this.Close);
            this.RegisterCmdDelegate(nameof(this.PingBroker), this.PingBroker);
            this.RegisterCmdDelegate(nameof(this.PingBroker2), this.PingBroker2);
            this.RegisterCmdDelegate(nameof(this.GetActiveBrokerIdList), this.GetActiveBrokerIdList);
        }

        private void RegisterCmdDelegate(string cmdName, Func<CloudQueueCmdDto, Task> del)
        {
            this.cmdNameToDelegate[cmdName] = del;
        }

        private Dictionary<string, Func<CloudQueueCmdDto, Task>> cmdNameToDelegate = new Dictionary<string, Func<CloudQueueCmdDto, Task>>();

        private Task CreateAndSendResponse(string requestId, string cmdName, object response)
        {
            var ans = new CloudQueueResponseDto(requestId, cmdName, response);
            return this.queueWriter.WriteAsync(ans);
        }

        public Task Create(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().Unpack<SessionStartInfoContract>(out var info).UnpackInt(out var id);
            var res = this.instance.Create(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task CreateDurable(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().Unpack<SessionStartInfoContract>(out var info).UnpackInt(out var id);
            var res = this.instance.CreateDurable(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Attach(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var id);
            var res = this.instance.Attach(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Close(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var id);
            this.instance.Close(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, string.Empty);
        }

        public Task PingBroker(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var id);
            var res = this.instance.PingBroker(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task PingBroker2(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackInt(out var id);
            var res = this.instance.PingBroker2(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task GetActiveBrokerIdList(CloudQueueCmdDto cmdObj)
        {
            var res = this.instance.GetActiveBrokerIdList();
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }
    }
}