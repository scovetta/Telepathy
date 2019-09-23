// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CcpWSLB.UnitTest")]
namespace Microsoft.Telepathy.ServiceBroker.BrokerQueue
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Xml;

    using Microsoft.Telepathy.ServiceBroker.Common;
    using Microsoft.Telepathy.ServiceBroker.FrontEnd;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    /// the storage item that include the request context and the persist item
    /// </summary>
    [Serializable]
    public class BrokerQueueItem : ISerializable, IDisposable, ICloneable
    {
        #region private fields

        /// <summary> the message tag for serializationInfo.</summary>
        private const string MessageTag = "MSG";

        /// <summary>
        /// message version tag for serilization info
        /// </summary>
        private const string MessageVersionTag = "VER";

        /// <summary> the async state tag for serializationInfo.</summary>
        private const string AsyncStateTag = "ATK";

        /// <summary> the persist id tag for serializationInfo.</summary>
        private const string PersistIdTag = "PID";

        /// <summary>dummy request context.</summary>
        private static DummyRequestContext DummyRequestContextField = DummyRequestContext.GetInstance(MessageVersion.Default);

        /// <summary> the persist version. </summary>
        private static int PersistVersionField;

        /// <summary> the messasge body.</summary>
        private Message message;

        /// <summary> the persist guid.</summary>
        private Guid persistGuid;

        /// <summary> the async state object for the message.</summary>
        private object asyncState;

        /// <summary> the corresponding request item for response item.</summary>
        private BrokerQueueItem peerItem;

        ///<summary> the request message headers</summary>
        //Note: [perf] bug9106 - based on WCF team feedback, message copy is more expensive if a message is modified.  Here we
        //store request message headers for response message, so that we can complete response message headers after creating
        //a buffered copy of the response message.
        private MessageHeaders requestHeaders;

        /// <summary> the async token for the specific persistence, 
        /// which will be used to track the internal setting by the physical persistence, 
        /// for example, the looup id for MSMQ.
        /// </summary>
        private BrokerQueueAsyncToken asyncToken;

        /// <summary>the request context.</summary>
        private RequestContextBase context;

        /// <summary>
        /// Stores message buffer
        /// </summary>
        private MessageBuffer buffer;

        #endregion

        /// <summary>
        /// Initializes a new instance of the BrokerQueueItem class.
        /// </summary>
        /// <param name="context">the request context.</param>
        /// <param name="msg">the message.</param>
        /// <param name="asyncState">the async state for the message.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods.", Justification = "the request context can be null by design.")]
        internal BrokerQueueItem(RequestContextBase context, Message msg, object asyncState)
        {
            this.Intialize(context, msg, Guid.NewGuid(), asyncState);
        }

        /// <summary>
        /// Initializes a new instance of the PersistMessage class for the broker queue.
        /// </summary>
        /// <param name="context">the request context.</param>
        /// <param name="msg">the message(request/response) in the persist item</param>
        /// <param name="persistId">the persist id.</param>
        /// <param name="asyncState">the token for write back the response to the client, serializable object</param>
        internal BrokerQueueItem(RequestContextBase context, Message msg, Guid persistId, object asyncState)
        {
            this.Intialize(context, msg, persistId, asyncState);
        }

        /// <summary>
        /// Initializes a new instance of the PersistMessage class for the broker queue.
        /// </summary>
        /// <param name="msg">the message(request/response) in the persist item</param>
        /// <param name="persistId">the persist id.</param>
        /// <param name="asyncState">the token for write back the response to the client, serializable object</param>
        internal BrokerQueueItem(Message msg, Guid persistId, object asyncState)
            : this(BrokerQueueItem.DummyRequestContextField, msg, persistId, asyncState)
        {
        }

        /// <summary>
        /// Initializes a new instance of the PersistMessage class for deserialization.
        /// </summary>
        /// <param name="info">the serializaion info.</param>
        /// <param name="context">the serialization context.</param>
        protected BrokerQueueItem(SerializationInfo info, StreamingContext context)
        {
            ParamCheckUtility.ThrowIfNull(info, "info");

            MessageVersion version = DeserializeMessageVersion(info.GetInt32(MessageVersionTag));
            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader((byte[])info.GetValue(BrokerQueueItem.MessageTag, typeof(byte[])), BrokerEntry.ReaderQuotas))
            {
                using (Message messageTmp = Message.CreateMessage(reader, BrokerEntry.MaxMessageSize, version))
                {
                    this.buffer = messageTmp.CreateBufferedCopy(BrokerEntry.MaxMessageSize);
                }
            }

            int deserializeFlags = 0;
            foreach (SerializationEntry entry in info)
            {
                switch (entry.Name)
                {
                    case BrokerQueueItem.AsyncStateTag:
                        this.asyncState = BrokerQueueItem.DeserializeObject(entry);
                        deserializeFlags |= 1;
                        break;
                    case BrokerQueueItem.PersistIdTag:
                        this.persistGuid = (Guid)BrokerQueueItem.DeserializeObject(entry);
                        deserializeFlags |= 2;
                        break;
                }

                if (deserializeFlags == 3)
                {
                    break;
                }
            }

            this.context = BrokerQueueItem.DummyRequestContextField;
            this.asyncToken = new BrokerQueueAsyncToken(this.persistGuid, this.asyncState, null, 0);
        }

        #region public properties
        /// <summary>
        /// Gets the request context
        /// </summary>
        internal RequestContextBase Context
        {
            get
            {
                return this.context;
            }
        }

        /// <summary>
        /// Gets or sets the message in this persist item
        /// </summary>
        public Message Message
        {
            get
            {
                if (this.message == null)
                {
                    this.message = this.buffer.CreateMessage();

                    // Complete response message headers
                    // Note: [perf]bug9106 - add stored request message headers to response message
                    if (this.requestHeaders != null)
                    {
                        // add request message action to response message header
                        this.message.Headers.Add(MessageHeader.CreateHeader(Constant.ActionHeaderName, Constant.HpcHeaderNS, this.requestHeaders.Action));
                        Utility.CopyMessageHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS, this.requestHeaders, this.message.Headers);

                        // add the request message Id header to the response message
                        Utility.CopyMessageHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, this.requestHeaders, this.message.Headers);

                    }
                }

                return this.message;
            }
        }

        /// <summary>
        /// Gets the persist id.
        /// </summary>
        public Guid PersistId
        {
            get
            {
                return this.persistGuid;
            }
        }

        /// <summary>
        /// Gets the async state relate to messasge
        /// </summary>
        public object AsyncState
        {
            get
            {
                return this.asyncState;
            }
        }

        /// <summary>
        /// Gets the async token for persistence implementation
        /// </summary>
        internal BrokerQueueAsyncToken PersistAsyncToken
        {
            get
            {
                return this.asyncToken;
            }
        }

        /// <summary>
        /// Gets or sets peer request item for response item.
        /// </summary>
        public BrokerQueueItem PeerItem
        {
            get
            {
                return this.peerItem;
            }
            set
            {
                this.peerItem = value;
                if (this.peerItem != null)
                {
                    // Store request message headers.
                    // Note: [perf] bug9106 - based on WCF team feedback, message copy is more expensive if a message is modified. Here 
                    // we store request message headers, which will later be added to response message after it's copied.
                    this.requestHeaders = new MessageHeaders(this.peerItem.Message.Headers.MessageVersion);

                    // only cache necessary headers
                    this.requestHeaders.Action = this.peerItem.Message.Headers.Action;
                    Utility.CopyMessageHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS, this.peerItem.Message.Headers, this.requestHeaders);
                    Utility.CopyMessageHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, this.peerItem.Message.Headers, this.requestHeaders);
                }
            }
        }

        /// <summary>
        /// Gets or sets the dispatch number that indicating how many times this message is dispatched.
        /// </summary>
        public int DispatchNumber
        {
            get
            {
                return this.asyncToken.DispatchNumber;
            }

            set
            {
                this.asyncToken.DispatchNumber = value;
            }
        }

        /// <summary>
        /// Gets or sets the total try count of this broker queue item
        /// </summary>
        public int TryCount
        {
            get { return this.asyncToken.TryCount; }
            set { this.asyncToken.TryCount = value; }
        }

        public ReemitToken ReemitToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the message buffer.
        /// </summary>
        internal MessageBuffer MessageBuffer
        {
            get
            {
                return this.buffer;
            }
        }

        /// <summary>
        /// Gets or sets the persist version
        /// </summary>
        public static int PersistVersion
        {
            get { return PersistVersionField; }
            set { PersistVersionField = value; }
        }

        private Timer ReemitTimer { get; set; }

        public void RegisterReemit(TimeSpan dueTime, int resendLimit, Action<BrokerQueueItem> reemit)
        {
            if (this.ReemitToken == null)
            {
                this.ReemitToken = ReemitToken.GetToken();
            }

            BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .RegisterReemit: Message {0}", Utility.GetMessageIdFromMessage(this.Message));
            this.ReemitTimer = new Timer(t =>
            {
                if (resendLimit + 1 > this.TryCount && this.ReemitToken.Available)
                {
                    var clonedRequest = (BrokerQueueItem)this.Clone();
                    BrokerTracing.TraceInfo("[BrokerQueueDispatcher] .RegisterReemit: Message emit callback old messageid {0}, new messageid {1}, TryCount {2}, ResendLimit {3}",
                        Utility.GetMessageIdFromMessage(this.Message),
                        Utility.GetMessageIdFromMessage(clonedRequest.Message),
                        this.TryCount,
                        resendLimit);

                    if (clonedRequest.ReemitToken.Available)
                    {
                        reemit(clonedRequest);
                    }
                }
            }, null, dueTime, TimeSpan.FromMilliseconds(-1));
        }

        public object Clone()
        {
            var info = new SerializationInfo(typeof(BrokerQueueItem), new FormatterConverter());
            var context = new StreamingContext();
            info.AddValue(BrokerQueueItem.MessageVersionTag, SerializeMessageVersion(this.Message.Version));
            MemoryStream msgStream = null;
            try
            {
                msgStream = new MemoryStream();
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(msgStream))
                {
                    var msgStreamTemp = msgStream;
                    msgStream = null;
                    var msg = this.MessageBuffer.CreateMessage();
                    msg.WriteMessage(writer);
                    writer.Flush();
                    info.AddValue(BrokerQueueItem.MessageTag, msgStreamTemp.ToArray());
                }
            }
            finally
            {
                if (msgStream != null)
                    msgStream.Dispose();
            }

            BrokerQueueItem.SerializeObject(info, this.asyncState, BrokerQueueItem.AsyncStateTag);
            BrokerQueueItem.SerializeObject(info, this.persistGuid, BrokerQueueItem.PersistIdTag);

            var item = new BrokerQueueItem(info, context);

            // Give it a new id.
            var newId = new UniqueId();
            item.Message.Headers.RemoveAll(Constant.MessageIdHeaderName, Constant.HpcHeaderNS);
            item.Message.Headers.Add(MessageHeader.CreateHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, newId));
            item.Message.Headers.MessageId = newId;
            item.CleanMessageBuffer();
            item.buffer = item.Message.CreateBufferedCopy(BrokerEntry.MaxMessageSize);

            item.asyncToken.AsyncToken = this.asyncToken.AsyncToken;
            item.asyncToken.Queue = this.asyncToken.Queue;
            item.TryCount = this.TryCount;
            item.DispatchNumber = this.DispatchNumber;
            item.ReemitToken = this.ReemitToken;
            return item;
        }

        #endregion

        /// <summary>
        /// Populates a System.Runtime.Serialization.SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
        /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ParamCheckUtility.ThrowIfNull(info, "info");

            info.AddValue(BrokerQueueItem.MessageVersionTag, SerializeMessageVersion(this.Message.Version));

            MemoryStream msgStream = null;
            try
            {
                msgStream = new MemoryStream();
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(msgStream))
                {
                    var msgStreamTemp = msgStream;
                    msgStream = null;
                    this.Message.WriteMessage(writer);
                    writer.Flush();
                    info.AddValue(BrokerQueueItem.MessageTag, msgStreamTemp.ToArray());
                }
            }
            finally
            {
                if (msgStream != null)
                    msgStream.Dispose();
            }

            BrokerQueueItem.SerializeObject(info, this.asyncState, BrokerQueueItem.AsyncStateTag);
            BrokerQueueItem.SerializeObject(info, this.persistGuid, BrokerQueueItem.PersistIdTag);
        }

        /// <summary>
        /// Prepare message to send
        /// </summary>
        public void PrepareMessage()
        {
            if (this.buffer != null)
            {
                if (this.message != null)
                {
                    this.message.Close();
                    this.message = null;
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or
        /// resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// clean the message buffer
        /// </summary>
        internal void CleanMessageBuffer()
        {
            if (this.buffer != null)
            {
                this.buffer.Close();
                this.buffer = null;
            }
        }

        /// <summary>
        /// dispose the resource
        /// </summary>
        /// <param name="dispose">indicate whether remove the resources.</param>
        protected void Dispose(bool dispose)
        {
            if (dispose)
            {
                if (this.ReemitTimer != null)
                {
                    this.ReemitTimer.Dispose();
                    this.ReemitTimer = null;
                }

                this.CleanMessageBuffer();
                if (this.message != null)
                {
                    this.message.Close();
                    this.message = null;
                }
            }
        }

        /// <summary>
        /// helper function to serialize the object.
        /// </summary>
        /// <param name="info">the serialization info.</param>
        /// <param name="obj">the object that will be serialized.</param>
        /// <param name="serializationTag">the tag in the serialization info.</param>
        private static void SerializeObject(SerializationInfo info, object obj, string serializationTag)
        {
            if (obj != null)
            {
                using (MemoryStream memorySteam = new MemoryStream())
                {
                    IFormatter formatter = (IFormatter)new BinaryFormatter();
                    formatter.Serialize(memorySteam, obj);
                    info.AddValue(serializationTag, memorySteam.ToArray());
                }
            }
        }

        /// <summary>
        /// helper function to deserialize the object from the serialization info.
        /// </summary>
        /// <param name="serializationEntry">the serialization entry.</param>
        /// <returns>the deserialized object.</returns>
        private static object DeserializeObject(SerializationEntry serializationEntry)
        {
            object deserializedObject = null;
            byte[] deserializeData = null;
            try
            {
                deserializeData = (byte[])serializationEntry.Value;
            }
            catch (Exception e)
            {
                BrokerTracing.TraceWarning("[PersistMessage] .DeserializeObject: the serialization entry.Value data type is, {0}, can not cast to byte[], the exception: {1}", serializationEntry.Value.GetType(), e);
            }

            if (deserializeData != null)
            {
                using (MemoryStream memoryStream = new MemoryStream(deserializeData))
                {
                    IFormatter formatter = (IFormatter)new BinaryFormatter();
                    deserializedObject = formatter.Deserialize(memoryStream);
                }
            }

            return deserializedObject;
        }

        /// <summary>
        /// Serialize message version into an int
        /// </summary>
        /// <param name="version">indicating the message version</param>
        /// <returns>returns the int represents this message version</returns>
        private static int SerializeMessageVersion(MessageVersion version)
        {
            if (version == MessageVersion.Soap12WSAddressing10)
            {
                return 0;
            }
            else if (version == MessageVersion.Soap11)
            {
                return 1;
            }
            else if (version == MessageVersion.Soap11WSAddressing10)
            {
                return 2;
            }
            else
            {
                BrokerTracing.TraceError("[BrokerQueueItem] Message version {0} is not supported.", version);
                throw new NotSupportedException(String.Format("Message version {0} is not supported.", version));
            }
        }

        /// <summary>
        /// Deserialize message version from an int
        /// </summary>
        /// <param name="value">indicating the int value</param>
        /// <returns>returns the message version</returns>
        private static MessageVersion DeserializeMessageVersion(int value)
        {
            switch (value)
            {
                case 0:
                    return MessageVersion.Soap12WSAddressing10;
                case 1:
                    return MessageVersion.Soap11;
                case 2:
                    return MessageVersion.Soap11WSAddressing10;
                default:
                    BrokerTracing.TraceError("[BrokerQueueItem] Message version ID = {0} is not supported.", value);
                    throw new NotSupportedException(String.Format("Message version ID = {0} is not supported.", value));
            }
        }

        /// <summary>
        /// the helper method to initialize the BrokerQueueItem instance.
        /// </summary>
        /// <param name="context">the request context.</param>
        /// <param name="msg">the message.</param>
        /// <param name="persistId">the persist id.</param>
        /// <param name="asyncState">the async state object.</param>
        private void Intialize(RequestContextBase context, Message msg, Guid persistId, object asyncState)
        {
            this.context = context;
            if (msg != null)
            {
                this.buffer = msg.CreateBufferedCopy(BrokerEntry.MaxMessageSize);
                msg.Close();
            }

            this.persistGuid = persistId;
            this.asyncState = asyncState;
            this.asyncToken = new BrokerQueueAsyncToken(this.persistGuid, asyncState, null, 0);
        }
    }
}
