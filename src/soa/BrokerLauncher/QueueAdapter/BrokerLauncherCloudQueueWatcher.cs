// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher.QueueAdapter
{
    using System.Diagnostics;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.QueueAdapter;
    using Microsoft.Telepathy.Session.QueueAdapter.DTO;
    using Microsoft.Telepathy.Session.QueueAdapter.Interface;
    using Microsoft.Telepathy.Session.QueueAdapter.Server;

    public class BrokerLauncherCloudQueueWatcher : CloudQueueWatcherBase
    {
        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, string connectionString)
        {
            this.instance = instance;

            CloudQueueSerializer serializer = new CloudQueueSerializer(CloudQueueCmdTypeBinder.BrokerLauncherBinder);

            this.QueueListener = new CloudQueueListener<CloudQueueCmdDto>(connectionString, CloudQueueConstants.BrokerLauncherRequestQueueName, serializer, this.InvokeInstanceMethodFromCmdObj);
            this.QueueWriter = new CloudQueueWriter<CloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer);
            this.QueueListener.StartListen();

            this.RegisterCmdDelegates();
            Trace.TraceInformation("BrokerLauncherCloudQueueWatcher started.");
        }

        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, IQueueListener<CloudQueueCmdDto> listener, IQueueWriter<CloudQueueResponseDto> writer)
        {
            this.instance = instance;
            this.QueueListener = listener;
            this.QueueWriter = writer;
            this.QueueListener.MessageReceivedCallback = this.InvokeInstanceMethodFromCmdObj;
            this.RegisterCmdDelegates();
        }

        private readonly IBrokerLauncher instance;

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

        public Task Create(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().Unpack<SessionStartInfoContract>(out var info).UnpackString(out var id);
            var res = this.instance.Create(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task CreateDurable(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().Unpack<SessionStartInfoContract>(out var info).UnpackString(out var id);
            var res = this.instance.CreateDurable(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Attach(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var id);
            var res = this.instance.Attach(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Close(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var id);
            this.instance.Close(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, string.Empty);
        }

        public Task PingBroker(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var id);
            var res = this.instance.PingBroker(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task PingBroker2(CloudQueueCmdDto cmdObj)
        {
            cmdObj.GetUnpacker().UnpackString(out var id);
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