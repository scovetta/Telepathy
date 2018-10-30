namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter.DTO;

    internal class BrokerLauncherCloudQueueWatcher
    {
        public BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance, string connectionString)
        {
            this.Instance = instance;
            this.queueListener = new BrokerLauncherCloudQueueListener<BrokerLauncherCloudQueueCmdDto>(
                connectionString,
                BrokerLauncherRequestQueueName,
                TypeBinder,
                this.InvokeInstanceMethodFromCmdObj);
            this.queueWriter = new BrokerLauncherCloudQueueWriter<BrokerLauncherCloudQueueResponseDto>(connectionString, BrokerLauncherResponseQueueName, TypeBinder);
            this.queueListener.StartListen();
        }

        public static string BrokerLauncherRequestQueueName => "brokerlaunchreq";

        public static string BrokerLauncherResponseQueueName => "brokerlaunchres";

        private readonly IBrokerLauncher Instance;

        private readonly BrokerLauncherCloudQueueListener<BrokerLauncherCloudQueueCmdDto> queueListener;

        private readonly BrokerLauncherCloudQueueWriter<BrokerLauncherCloudQueueResponseDto> queueWriter;

        private static readonly BrokerLauncherCloudQueueCmdTypeBinder TypeBinder =
            new BrokerLauncherCloudQueueCmdTypeBinder() { ParameterTypes = { typeof(SessionStartInfoContract), typeof(BrokerInitializationResult), typeof(object[]), typeof(int[]) } };

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

        private async Task InvokeInstanceMethodFromCmdObj(BrokerLauncherCloudQueueCmdDto cmdObj)
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
                    this.Close(cmdObj);
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

        internal BrokerLauncherCloudQueueWatcher(IBrokerLauncher instance)
        {
            this.Instance = instance;
        }

        private Task CreateAndSendResponse(string requestId, string cmdName, object response)
        {
            var ans = new BrokerLauncherCloudQueueResponseDto(requestId, cmdName, response);
            return this.queueWriter.WriteAsync(ans);
        }

        public Task Create(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var (info, id) = UnpackParameter<SessionStartInfoContract, int>(cmdObj.Parameters);
            var res = this.Instance.Create(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task CreateDurable(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var (info, id) = UnpackParameter<SessionStartInfoContract, int>(cmdObj.Parameters);
            var res = this.Instance.CreateDurable(info, id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task Attach(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<int>(cmdObj.Parameters);
            var res = this.Instance.Attach(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public void Close(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<int>(cmdObj.Parameters);
            this.Instance.Close(id);
        }

        public Task PingBroker(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<int>(cmdObj.Parameters);
            var res = this.Instance.PingBroker(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task PingBroker2(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var id = UnpackParameter<int>(cmdObj.Parameters);
            var res = this.Instance.PingBroker2(id);
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }

        public Task GetActiveBrokerIdList(BrokerLauncherCloudQueueCmdDto cmdObj)
        {
            var res = this.Instance.GetActiveBrokerIdList();
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, res);
        }
    }
}