namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Telepathy.ServiceBroker.BackEnd;
    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;
    using Microsoft.WindowsAzure.Storage;

    internal class MockDispatcher : IDispatcher
    {
        public DispatcherInfo Info
        {
            get
            {
                return null;
            }
        }

        private bool[] passBindingFlags;

        public bool[] PassBindingFlags
        {
            get
            {
                return this.passBindingFlags;
            }
        }

        public string TaskId
        {
            get
            {
                return 0.ToString();
            }
        }

        private Func<IAsyncResult, Task> processMessageCallback;

        public Func<IAsyncResult, Task> ProcessMessageCallback
        {
            get
            {
                return this.processMessageCallback;
            }

            set
            {
                this.processMessageCallback = value;
            }
        }

        public BrokerQueueItem[] items
        {
            get;

            private set;
        }

        private bool serviceInitializationCompleted;

        public bool ServiceInitializationCompleted
        {
            get
            {
                return this.serviceInitializationCompleted;
            }

            set
            {
                this.serviceInitializationCompleted = value;
            }
        }

        public MockDispatcher()
        {
            this.passBindingFlags = new bool[1];

            this.items = new BrokerQueueItem[1];
        }

        public void Close()
        {
        }

        public Task HandleException(DateTime dispatchTime, int clientIndex, BackEnd.IService client, BrokerQueueItem item, Guid messageId, Exception e)
        {
            return Task.CompletedTask;
        }

        public void HandleStorageException(DateTime dispatchTime, int clientIndex, BackEnd.IService client, BrokerQueueItem item, Guid messageId, StorageException e)
        {
        }

        public void HandleEndpointNotFoundException(int clientIndex, BackEnd.IService client, BrokerQueueItem item, Guid messageId, System.ServiceModel.EndpointNotFoundException e)
        {
        }

        public void GetNextRequest(int serviceClientIndex)
        {
        }

        public void DecreaseProcessingCount()
        {
        }

        public bool HandleFaultExceptionRetry(int clientIndex, BrokerQueueItem item, Message reply, DateTime dispatchTime, out bool preemption)
        {
            preemption = false;

            return true;
        }

        public void HandleServiceInstanceFailure(SessionFault sessionFault)
        {
        }

        public void OnServiceInstanceConnected(object state)
        {
        }

        public void CloseThis()
        {
        }

        private void ResponseReceived(IAsyncResult ar)
        {
        }

        public virtual bool CleanupClient(BackEnd.IService client)
        {
            return true;
        }
    }
}
