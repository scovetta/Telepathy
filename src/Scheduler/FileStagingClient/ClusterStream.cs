//--------------------------------------------------------------------------
// <copyright file="ClusterStream.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     A wrapper of stream passed back by file staging client.
// </summary>
//--------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.FileStaging.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using Microsoft.Hpc.Azure.FileStaging;

    /// <summary>
    /// This class wraps stream object passed back by file staging client.
    /// It forwards calls to internal stream, and overrides Dispose method
    /// to dispose related file staging client instance.
    /// </summary>
    internal class ClusterStream : Stream
    {
        /// <summary>
        /// FileStagingClient instances associated with streams
        /// </summary>
        private static Dictionary<Stream, FileStagingClientWithHeaders> allFileStagingClients = new Dictionary<Stream, FileStagingClientWithHeaders>();

        /// <summary>
        /// Lock object for allFileStagingClients
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// Internal stream
        /// </summary>
        private Stream internalStream;

        /// <summary>
        /// Initializes a new instance of the ClusterStream class
        /// </summary>
        /// <param name="stream">internal stream</param>
        /// <param name="client">associated FileStagingClient instance</param>
        public ClusterStream(Stream stream, FileStagingClientWithHeaders client)
        {
            this.internalStream = stream;
            lock (lockObj)
            {
                allFileStagingClients.Add(stream, client);
            }
        }
      
        #region implementation of Stream abstract class

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead { get { return this.internalStream.CanRead; } }
        
        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek { get { return this.internalStream.CanSeek; } }

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        [ComVisible(false)]
        public override bool CanTimeout { get { return this.internalStream.CanTimeout; } }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite { get { return this.internalStream.CanWrite; } }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length { get { return this.internalStream.Length; } }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position { get { return this.internalStream.Position; } set { this.internalStream.Position = value; } }

        /// <summary>
        ///  Gets or sets a value, in miliseconds, that determines how long the stream
        ///  will attempt to read before timing out.
        /// </summary>
        [ComVisible(false)]
        public override int ReadTimeout { get { return this.internalStream.ReadTimeout; } set { this.internalStream.ReadTimeout = value; } }

        /// <summary>
        ///  Gets or sets a value, in miliseconds, that determines how long the stream
        ///  will attempt to write before timing out.
        /// </summary>
        [ComVisible(false)]
        public override int WriteTimeout { get { return this.internalStream.WriteTimeout; } set { this.internalStream.WriteTimeout = value; } }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.internalStream.BeginRead(buffer, offset, count, callback, state);
        }
         
        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.internalStream.BeginWrite(buffer, offset, count, callback, state);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the stream and optionally
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                FileStagingClientWithHeaders fileStagingClient;
                lock(lockObj)
                {
                    if (allFileStagingClients.TryGetValue(this.internalStream, out fileStagingClient))
                    {
                        allFileStagingClients.Remove(this.internalStream);
                    }
                }

                if (fileStagingClient != null)
                {
                    try
                    {
                        fileStagingClient.Dispose();
                    }
                    catch
                    {
                    }
                }

                try
                {
                    this.internalStream.Dispose();
                }
                catch
                {
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.internalStream.EndRead(asyncResult);
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult"></param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.internalStream.EndWrite(asyncResult);
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be
        /// written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            this.internalStream.Flush();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position
        /// within the stream by the number of bytes read.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.internalStream.Read(buffer, offset, count);
        }

        /// <summary>
        ///  Reads a byte from the stream and advances the position within the stream
        ///  by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        public override int ReadByte()
        {
            return this.internalStream.ReadByte();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.internalStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            this.internalStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current
        /// position within this stream by the number of bytes written.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.internalStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position
        /// within the stream by one byte.
        /// </summary>
        /// <param name="value"></param>
        public override void WriteByte(byte value)
        {
            this.internalStream.WriteByte(value);
        }

        #endregion
    }
}
