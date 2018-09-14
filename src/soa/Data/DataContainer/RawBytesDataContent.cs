//------------------------------------------------------------------------------
// <copyright file="RawBytesDataContent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Raw bytes data content
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Raw bytes data content
    /// </summary>
    internal class RawBytesDataContent : DataContent
    {
        /// <summary>
        /// Initializes a new instance of the RawBytesDataContent class
        /// </summary>
        /// <param name="data">raw bytes data</param>
        /// <param name="compress">a flag indicating if the data content should be compressed or not</param>
        public RawBytesDataContent(byte[] data, bool compress) :
            base(compress)
        {
            this.RawBytes = data;
        }

        /// <summary>
        /// Gets the raw bytes data
        /// </summary>
        public byte[] RawBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a RawBytesDataContent instance from byte array
        /// </summary>
        /// <param name="contentBytes">data content in byte array</param>
        /// <returns>new created RawBytesDataContent instance</returns>
        public static RawBytesDataContent Parse(byte[] contentBytes)
        {
            if (DataContent.IsCompressed(contentBytes))
            {
                TraceHelper.TraceSource.TraceEvent(TraceEventType.Verbose, 0, "[DataClient] .RawBytesDataContent.Parse: data is compressed");

                MemoryStream inStream = null;
                try
                {
                    inStream = new MemoryStream(contentBytes);
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        inStream.Seek(DataContent.HeaderLength, SeekOrigin.Begin);
                        using (GZipStream gzs = new GZipStream(inStream, CompressionMode.Decompress))
                        {
                            inStream = null;
                            const int BufferSize = 4096;
                            byte[] buffer = new byte[BufferSize];

                            while (true)
                            {
                                int bytesRead = gzs.Read(buffer, 0, BufferSize);
                                if (bytesRead <= 0)
                                {
                                    break;
                                }

                                outStream.Write(buffer, 0, bytesRead);
                            }

                            return new RawBytesDataContent(outStream.ToArray(), true);
                        }
                    }
                }
                finally
                {
                    if (inStream != null)
                        inStream.Dispose();
                }
            }
            else
            {
                return new RawBytesDataContent(contentBytes, false);
            }
        }

        /// <summary>
        /// Dumps data body to a stream
        /// </summary>
        /// <param name="stream">out stream</param>
        protected override void DumpData(Stream stream)
        {
            if (!this.Compressed)
            {
                stream.Write(this.RawBytes, 0, this.RawBytes.Length);
            }
            else
            {
                using (GZipStream gzs = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    gzs.Write(this.RawBytes, 0, this.RawBytes.Length);
                }
            }
        }
    }
}
