namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System;
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

            CloudQueueSerializer serializer = new CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder.Default);

            this.queueListener = new CloudQueueListener<CloudQueueCmdDto>(
                connectionString,
                CloudQueueConstants.BrokerLauncherRequestQueueName,
                serializer,
                this.InvokeInstanceMethodFromCmdObj);
            this.queueWriter = new CloudQueueWriter<CloudQueueResponseDto>(connectionString, CloudQueueConstants.BrokerLauncherResponseQueueName, serializer);
            this.queueListener.StartListen();

            Trace.TraceInformation("BrokerLauncherCloudQueueWatcher started.");
        }

        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, IQueueListener<CloudQueueCmdDto> listener, IQueueWriter<CloudQueueResponseDto> writer)
        {
            this.instance = instance;
            this.queueListener = listener;
            this.queueWriter = writer;
            this.queueListener.MessageReceivedCallback = this.InvokeInstanceMethodFromCmdObj;
        }

        private readonly IBrokerLauncher instance;

        private readonly IQueueListener<CloudQueueCmdDto> queueListener;

        private readonly IQueueWriter<CloudQueueResponseDto> queueWriter;

        private static (T1, T2) UnpackParameter<T1, T2>(object[] objectArr)
        {
            if (objectArr.Length != 2)
            {
                throw new ArgumentException("Argument length mismatch", nameof(objectArr));
            }

            if (objectArr[0] is T1 arg1 && objectArr[1] is T2 arg2)
            {
                return (arg1, arg2);
            }
            else
            {
                throw new ArgumentException("Argument type mismatch", nameof(objectArr));
            }
        }

        private static T UnpackParameter<T>(object[] objectArr)
        {
            if (objectArr.Length != 1)
            {
                throw new ArgumentException("Argument length mismatch", nameof(objectArr));
            }

            if (objectArr[0] is T arg)
            {
                return arg;
            }
            else
            {
                throw new ArgumentException("Argument type mismatch", nameof(objectArr));
            }
        }

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

            switch (cmdObj.CmdName)
            {
                case nameof(this.Create):
                    await this.Create(cmdObj);
                    break;
                case nameof(this.CreateDurable):
                    await this.CreateDurable(cmdObj);
                    break;
                case nameof(this.Attach):
                    await this.Attach(cmdObj);
                    break;
                case nameof(this.Close):
                    await this.Close(cmdObj);
                    break;
                case nameof(this.PingBroker):
                    await this.PingBroker(cmdObj);
                    break;
                case nameof(this.PingBroker2):
                    await this.PingBroker2(cmdObj);
                    break;
                case nameof(this.GetActiveBrokerIdList):
                    await this.GetActiveBrokerIdList(cmdObj);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown CmdName {cmdObj.CmdName}");
            }
        }

        private Task CreateAndSendResponse(string requestId, string cmdName, object response)
        {
            var ans = new CloudQueueResponseDto(requestId, cmdName, response);
            return this.queueWriter.WriteAsync(ans);
        }

        public Task Create(CloudQueueCmdDto cmdObj)
        {
            var (info, id) = UnpackParameter<SessionStartInfoContract, long>(cmdObj.Parameters);
            var res = this.instance.Create(info, (int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task CreateDurable(CloudQueueCmdDto cmdObj)
        {
            var (info, id) = UnpackParameter<SessionStartInfoContract, long>(cmdObj.Parameters);
            var res = this.instance.CreateDurable(info, (int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Attach(CloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<long>(cmdObj.Parameters);
            var res = this.instance.Attach((int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Close(CloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<long>(cmdObj.Parameters);
            this.instance.Close((int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, string.Empty);
        }

        public Task PingBroker(CloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<long>(cmdObj.Parameters);
            var res = this.instance.PingBroker((int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task PingBroker2(CloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<long>(cmdObj.Parameters);
            var res = this.instance.PingBroker2((int)id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task GetActiveBrokerIdList(CloudQueueCmdDto cmdObj)
        {
            var res = this.instance.GetActiveBrokerIdList();
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }
    }
}