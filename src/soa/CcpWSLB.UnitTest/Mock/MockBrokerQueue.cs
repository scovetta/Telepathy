// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.UnitTest.Mock
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.ServiceBroker.BrokerQueue;

    /// <summary>
    /// Mock object for broker queue
    /// </summary>
    internal class MockBrokerQueue : BrokerQueue
    {
        /// <summary>
        /// Stores the broker queue
        /// </summary>
        private Queue<BrokerQueueItem> queue;

        /// <summary>
        /// Store the callback queue
        /// </summary>
        private Queue<KeyValuePair<BrokerQueueCallback, object>> callbackQueue;

        /// <summary>
        /// Store the reply message queue
        /// </summary>
        private Queue<Message> replyMessageQueue;

        /// <summary>
        /// Stores a flag informing the broker queue should directly reply the request or not
        /// </summary>
        private bool directReply;

        /// <summary>
        /// Initializes a new instance of the MockBrokerQueue class
        /// </summary>
        public MockBrokerQueue()
        {
            this.queue = new Queue<BrokerQueueItem>();
            this.callbackQueue = new Queue<KeyValuePair<BrokerQueueCallback, object>>();
            this.replyMessageQueue = new Queue<Message>();
        }

        /// <summary>
        /// Gets the reply message queue
        /// </summary>
        public Queue<Message> ReplyMessageQueue
        {
            get { return this.replyMessageQueue; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the broker should directly reply the request or not
        /// </summary>
        public bool DirectReply
        {
            get { return this.directReply; }
            set { this.directReply = value; }
        }

        /// <summary>
        /// Gets the broker queue
        /// </summary>
        public Queue<BrokerQueueItem> Queue
        {
            get { return this.queue; }
        }

        public override bool EOMReceived { get; }

        /// <summary>
        /// Gets a value indicating whether all the requests in the storage are processed.
        /// </summary>
        public override bool IsAllRequestsProcessed
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the total number of the requests that be persisted to the broker queue
        /// </summary>
        public override long AllRequestsCount
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of the requests that get the corresponding responses.
        /// </summary>
        public override long ProcessedRequestsCount
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the number of the requests that are dispatched but still not get corresponding responses.
        /// </summary>
        public override long ProcessingRequestsCount
        {
            get { throw new NotImplementedException(); }
        }

        public override long FlushedRequestsCount { get; }

        public override long FailedRequestsCount { get; }

        public override string SessionId { get; }

        public override string ClientId { get; }

        public override string PersistName { get; }

        public override string UserName { get; }

        /// <summary>
        /// Trigger the callback when called GetRequestAsync
        /// For UnitTesting purpose
        /// </summary>
        /// <param name="item">indicate the broker queue item</param>
        public void TriggerGetRequestCallback(BrokerQueueItem item)
        {
            KeyValuePair<BrokerQueueCallback, object> pair = this.callbackQueue.Dequeue();
            pair.Key(item, pair.Value);
        }

        /// <summary>
        /// Put the request item into the storage. and the storage will cache the requests in the memory 
        /// until the front end call the flush method. the async result will return the BrokerQueueItem.
        /// </summary>
        /// <param name="context">the request context relate to the message</param>
        /// <param name="msg">the request message</param>
        /// <param name="asyncState">the asyncState relate to the message</param>
        public override async Task PutRequestAsync(Telepathy.ServiceBroker.FrontEnd.RequestContextBase context, Message msg, object asyncState)
        {
            this.queue.Enqueue(new BrokerQueueItem(context, msg, asyncState));
            if (this.directReply)
            {
                MessageFault fault = MessageFault.CreateFault(FaultCode.CreateReceiverFaultCode("DummyReply", "http://hpc.microsoft.com"), "DummyReply");
                Message reply = Message.CreateMessage(context.MessageVersion, fault, msg.Headers.Action + "Response");
                if (context.MessageVersion == MessageVersion.Default)
                {
                    reply.Headers.RelatesTo = msg.Headers.MessageId;
                }

                context.Reply(reply, TimeSpan.MaxValue);
            }
        }

        /// <summary>
        /// Fetch the requests one by one from the storage but not remove the original message in the storage.
        /// if reach the end of the storage, empty exception raised.this is async call by BrokerQueueCallback.
        /// the async result will return the request message
        /// </summary>
        /// <param name="requestCallback">the call back to retrieve the request message</param>
        /// <param name="state">the async state object</param>
        public override void GetRequestAsync(BrokerQueueCallback requestCallback, object state)
        {
            this.callbackQueue.Enqueue(new KeyValuePair<BrokerQueueCallback, object>(requestCallback, state));
        }

        public override async Task PutResponseAsync(Message responseMsg, BrokerQueueItem requestItem)
        {
            this.replyMessageQueue.Enqueue(responseMsg);

        }

        public override bool RegisterResponsesCallback(BrokerQueueCallback responseCallback, MessageVersion messageVersion, ResponseActionFilter filter, int responseCount, object state)
        {
            throw new NotImplementedException();
        }

        public override void ResetResponsesCallback()
        {
            throw new NotImplementedException();
        }

        public override void AckResponse(BrokerQueueItem response, bool success)
        {
            throw new NotImplementedException();
        }

        public override void AckResponses(List<BrokerQueueItem> responses, bool success)
        {
            throw new NotImplementedException();
        }

        public override void Flush(long msgCount, int timeoutMs, bool endOfMessage)
        {
            throw new NotImplementedException();
        }

        public override void FlushCount()
        {
            throw new NotImplementedException();
        }

        public override void DiscardUnflushedRequests()
        {
            throw new NotImplementedException();
        }

        public override SessionPersistCounter Close()
        {
            throw new NotImplementedException();
        }
    }
}
