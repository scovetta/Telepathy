//------------------------------------------------------------------------------
// <copyright file="ObjectDataContent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Object data content
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Object data content
    /// </summary>
    internal class ObjectDataContent : DataContent
    {
        /// <summary>
        /// Initializes a new instance of the ObjectDataContent class
        /// </summary>
        /// <param name="data">object data</param>
        /// <param name="compress">a flag indicating if the data content should be compressed or not</param>
        public ObjectDataContent(object data, bool compress) :
            base(compress)
        {
            this.Object = data;
        }

        /// <summary>
        /// Gets data content object
        /// </summary>
        public object Object
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new ObjectDataContent instance from byte array
        /// </summary>
        /// <param name="contentBytes">data content in byte array</param>
        /// <returns>new created ObjectDataContent instance</returns>
        public static ObjectDataContent Parse(byte[] contentBytes)
        {
            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream inStream = null;
            try
            {
                inStream = new MemoryStream(contentBytes);
                if (IsCompressed(contentBytes))
                {
                    // decompress data
                    inStream.Seek(DataContent.HeaderLength, SeekOrigin.Begin);

                    using (GZipStream gzs = new GZipStream(inStream, CompressionMode.Decompress))
                    {
                        inStream = null;
                        object o = bf.Deserialize(gzs);
                        return new ObjectDataContent(o, true);
                    }
                }
                else
                {
                    object o = bf.Deserialize(inStream);
                    return new ObjectDataContent(o, false);
                }
            }
            finally
            {
                if (inStream != null)
                    inStream.Dispose();
            }
        }

        /// <summary>
        /// Dumps data body to a stream
        /// </summary>
        /// <param name="stream">out stream</param>
        protected override void DumpData(Stream stream)
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (!this.Compressed)
            {
                bf.Serialize(stream, this.Object);
            }
            else
            {
                // compress data (optioinally)
                using (GZipStream gzs = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    bf.Serialize(gzs, this.Object);
                }
            }
        }
    }
}
