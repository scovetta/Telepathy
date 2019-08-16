//-----------------------------------------------------------------------
// <copyright file="AzureQueueItem.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//     The queue item stored in request and response Azure storage queues.
// </summary>
//-----------------------------------------------------------------------

using TelepathyCommon;

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security;
    using System.Security.Permissions;
    using System.ServiceModel.Channels;
    using System.Xml;

    /// <summary>
    /// The queue item stored in request and response queues.
    /// </summary>
    [Serializable]
    public class AzureQueueMessageItem : ISerializable, IDisposable
    {
        /// <summary>
        /// the message tag for serializationInfo.
        /// </summary>
        private const string MessageTag = "MSG";

        /// <summary>
        /// message version tag for serilization info
        /// </summary>
        private const string MessageVersionTag = "VER";

        /// <summary>
        /// the messasge body.
        /// </summary>
        private Message message;

        /// <summary>
        /// Stores message buffer
        /// </summary>
        private MessageBuffer messageBuffer;

        /// <summary>
        /// It indicates if the object is disposed.
        /// </summary>
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the AzureQueueItem class.
        /// </summary>
        /// <param name="msg">the message stored in the queue item</param>
        public AzureQueueMessageItem(Message msg)
        {
            this.messageBuffer = msg.CreateBufferedCopy(int.MaxValue);
        }

        /// <summary>
        /// Initializes a new instance of the AzureQueueItem class.
        /// </summary>
        /// <param name="info">the serializaion info.</param>
        /// <param name="context">the serialization context.</param>
        protected AzureQueueMessageItem(SerializationInfo info, StreamingContext context)
        {
            MessageVersion version = DeserializeMessageVersion(info.GetInt32(MessageVersionTag));

            var buffer = (byte[])info.GetValue(MessageTag, typeof(byte[]));

            using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(buffer, XmlDictionaryReaderQuotas.Max))
            {
                using (Message messageTmp = Message.CreateMessage(reader, int.MaxValue, version))
                {
                    this.messageBuffer = messageTmp.CreateBufferedCopy(int.MaxValue);
                }
            }
        }

        /// <summary>
        /// Finalizes an instance of the AzureQueueItem class.
        /// </summary>
        ~AzureQueueMessageItem()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Gets the message buffer deserialized
        /// </summary>
        public MessageBuffer Buffer
        {
            get
            {
                return this.messageBuffer;
            }
        }

        /// <summary>
        /// Gets the message stored in the queue item
        /// </summary>
        public Message Message
        {
            get
            {
                if (this.message == null)
                {
                    this.message = this.messageBuffer.CreateMessage();
                }
                return this.message;
            }
        }

        /// <summary>
        /// Serialize the message.
        /// </summary>
        /// <param name="message">the message</param>
        /// <returns>the byte array which is serialized from the message</returns>
        public static byte[] Serialize(Message message)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();

                using (AzureQueueMessageItem item = new AzureQueueMessageItem(message))
                {
                    formatter.Serialize(stream, item);
                }

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Deserialize the message.
        /// </summary>
        /// <param name="byteArray">the source byte array</param>
        /// <returns>the message deserialized</returns>
        public static Message Deserialize(byte[] byteArray)
        {
            using (MemoryStream stream = new MemoryStream(byteArray))
            {
                var formatter = new BinaryFormatter();
                formatter.UseInAppDomainSerializationBinder();

                var item = formatter.Deserialize(stream) as AzureQueueMessageItem;

                Debug.Assert(item != null, "The item must be an AzureQueueItem");

                return item.Message;
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Populates a System.Runtime.Serialization.SerializationInfo with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
        /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(MessageVersionTag, SerializeMessageVersion(this.Message.Version));

            MemoryStream stream = null;
            try
            {
                stream = new MemoryStream();
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    var streamTemp = stream;
                    stream = null;
                    this.Message.WriteMessage(writer);
                    writer.Flush();
                    info.AddValue(MessageTag, streamTemp.ToArray());
                }
            }
            finally
            {
                if (stream != null)
                    stream.Dispose();
            }
        }

        /// <summary>
        /// Dispose the object.
        /// </summary>
        /// <param name="disposing">disposing flag</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.message != null)
                {
                    try
                    {
                        this.message.Close();
                        this.message = null;
                    }
                    catch
                    {
                    }
                }

                if (this.messageBuffer != null)
                {
                    try
                    {
                        this.messageBuffer.Close();
                        this.messageBuffer = null;
                    }
                    catch
                    {
                    }
                }
            }

            this.disposed = true;
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
                throw new NotSupportedException(string.Format("Message version {0} is not supported.", version));
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
                    throw new NotSupportedException(string.Format("Message version ID = {0} is not supported.", value));
            }
        }
    }
}
