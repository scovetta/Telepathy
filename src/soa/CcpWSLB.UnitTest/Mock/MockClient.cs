namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System;
    using System.ServiceModel.Channels;

    internal class MockClient : BackEnd.IService
    {
        public Action Action
        {
            get;
            set;
        }

        public Message Response
        {
            get;
            set;
        }

        public Message ProcessMessage(Message request)
        {
            return null;
        }

        public IAsyncResult BeginProcessMessage(Message request, AsyncCallback callback, object asyncState)
        {
            return null;
        }

        public Message EndProcessMessage(IAsyncResult ar)
        {
            if (this.Action != null)
            {
                this.Action();
            }

            return this.Response;
        }
    }
}
