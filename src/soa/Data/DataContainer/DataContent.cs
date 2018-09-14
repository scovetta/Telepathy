//------------------------------------------------------------------------------
// <copyright file="DataContent.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data content
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Data Content
    /// </summary>
    internal abstract class DataContent
    {
        /// <summary>
        /// Data content header length
        /// </summary>
        public const int HeaderLength = 32;

        /// <summary>
        /// GZip compression flag.
        /// NOTE: this 32-char magic string is obtained from a GUID. It is used to identify compressed data.
        /// </summary>
        private const string GZipCompressionFlag = "E7FE6B887495BE4F93CD243D8C1E06AC";

        /// <summary>
        /// Initializes a new instance of the DataContent class
        /// </summary>
        /// <param name="compress">a flag indicating whether the data content can be compressed or not</param>
        protected DataContent(bool compress)
        {
            this.Compressed = compress;
        }

        /// <summary>
        /// Gets a value indicating whether the content is compressed or not
        /// </summary>
        public bool Compressed
        {
            get;
            private set;
        }

        /// <summary>
        /// Check if a data content is compressed or not
        /// </summary>
        /// <param name="dataBytes">data content in bytes</param>
        /// <returns>true if the data content is compressed, false otherwise</returns>
        public static bool IsCompressed(byte[] dataBytes)
        {
            if (dataBytes.Length < GZipCompressionFlag.Length)
            {
                return false;
            }

            string compressionFlag = Encoding.ASCII.GetString(dataBytes, 0, GZipCompressionFlag.Length);
            return string.Equals(compressionFlag, GZipCompressionFlag, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Dumps data content to a stream
        /// </summary>
        /// <param name="stream">out stream</param>
        public void Dump(Stream stream)
        {
            if (this.Compressed)
            {
                // dump header
                byte[] header = Encoding.ASCII.GetBytes(GZipCompressionFlag);
                stream.Write(header, 0, GZipCompressionFlag.Length);
            }

            // dump body
            this.DumpData(stream);
        } 

        /// <summary>
        /// Dumps data body to a stream
        /// </summary>
        /// <param name="stream">out stream</param>
        protected abstract void DumpData(Stream stream);
    }
}
