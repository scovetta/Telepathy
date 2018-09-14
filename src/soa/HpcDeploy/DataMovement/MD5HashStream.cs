//------------------------------------------------------------------------------
// <copyright file="MD5HashStream.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Define class to make thread safe stream access and calculate MD5 hash.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;

    /// <summary>
    /// Class to make thread safe stream access and calculate MD5 hash.
    /// </summary>
    internal class MD5HashStream : IDisposable
    {
        /// <summary>
        /// Max retry count when try to allocate buffer to read from stream.
        /// </summary>
        private static int maxRetryCount = 10;

        /// <summary>
        /// Stream  object.
        /// </summary>
        private Stream stream;

        /// <summary>
        /// Semaphore object. In our case, we can only have one operation at the same time.
        /// </summary>
        private SemaphoreSlim semaphore;
        
        /// <summary>
        /// In restart mode, we start a separate thread to calculate MD5hash of transferred part.
        /// This variable indicates whether finished to calculate this part of MD5hash.
        /// </summary>
        private volatile bool finishedSeparateMd5Calculator = false;

        /// <summary>
        /// Indicates whether succeeded in calculating MD5hash of the transferred bytes.
        /// </summary>
        private bool succeededSeparateMd5Calculator = false;
        
        /// <summary>
        /// Running md5 hash of the blob being downloaded.
        /// </summary>
        private MD5CryptoServiceProvider md5hash;

        /// <summary>
        /// Offset of the transferred bytes. We should calculate MD5hash on all bytes before this offset.
        /// </summary>
        private long md5hashOffset;

        /// <summary>
        /// Initializes a new instance of the <see cref="MD5HashStream"/> class.
        /// </summary>
        /// <param name="stream">Stream object.</param>
        /// <param name="lastTransferOffset">Offset of the transferred bytes.</param>
        /// <param name="md5hashCheck">Whether need to calculate MD5Hash.</param>
        public MD5HashStream(
            Stream stream, 
            long lastTransferOffset,
            bool md5hashCheck)
        {
            this.stream = stream;
            this.md5hashOffset = lastTransferOffset;

            if ((0 == this.md5hashOffset)
                || (!md5hashCheck))
            {
                this.finishedSeparateMd5Calculator = true;
                this.succeededSeparateMd5Calculator = true;
            }
            else
            {
                this.semaphore = new SemaphoreSlim(1, 1);
            }

            if (md5hashCheck)
            {
                this.md5hash = new MD5CryptoServiceProvider();
            }

            if ((!this.finishedSeparateMd5Calculator)
                && (!this.stream.CanRead))
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.StreamMustSupportReadException,
                    "Stream"));
            }

            if (!this.stream.CanSeek)
            {
                throw new NotSupportedException(string.Format(
                    CultureInfo.InvariantCulture,
                    Resources.StreamMustSupportSeekException,
                    "Stream"));
            }
        }

        /// <summary>
        /// Gets a value indicating whether need to calculate MD5 hash.
        /// </summary>
        public bool CheckMd5Hash
        {
            get
            {
                return null != this.md5hash;
            }
        }

        /// <summary>
        /// Gets MD5 hash bytes.
        /// </summary>
        public byte[] Hash
        {
            get
            {
                return null == this.md5hash ? null : this.md5hash.Hash;
            }
        }

        /// <summary>
        /// Gets a value indicating whether already finished to calculate MD5 hash of transferred bytes.
        /// </summary>
        public bool FinishedSeparateMd5Calculator
        {
            get
            {
                return this.finishedSeparateMd5Calculator;
            }
        }

        /// <summary>
        /// Gets a value indicating whether already succeeded in calculating MD5 hash of transferred bytes.
        /// </summary>
        public bool SucceededSeparateMd5Calculator
        {
            get
            {
                this.WaitMD5CalculationToFinish();
                return this.succeededSeparateMd5Calculator;
            }
        }

        /// <summary>
        /// Calculate MD5 hash of transferred bytes.
        /// </summary>
        /// <param name="memoryManager">Reference to MemoryManager object to require buffer from.</param>
        /// <param name="cancellationChecker">Reference to CancellationChecker object to check whether to cancel this calculating.</param>
        public void CalculateMd5(MemoryManager memoryManager, CancellationChecker cancellationChecker)
        {
            if (null == this.md5hash)
            {
                return;
            }

            byte[] buffer;
            buffer = memoryManager.RequireBuffer();

            if (null == buffer)
            {
                int retryCount = 0;
                while ((retryCount < maxRetryCount)
                    && (null == buffer))
                {
                    cancellationChecker.CheckCancellation();
                    Thread.Sleep(200);
                    buffer = memoryManager.RequireBuffer();
                    ++retryCount;
                }

                if (null == buffer)
                { 
                    lock (this.md5hash)
                    {
                        this.finishedSeparateMd5Calculator = true;
                    }

                    throw new BlobTransferException(
                        BlobTransferErrorCode.FailToAllocateMemory, 
                        Resources.FailedToAllocateMemoryException);
                }
            }
            
            long offset = 0;
            int readLength = 0;

            while (true)
            {
                lock (this.md5hash)
                {
                    if (offset >= this.md5hashOffset)
                    {
                        Debug.Assert(
                            offset == this.md5hashOffset,
                            "We should stop the separate calculator thread just at the transferred offset");

                        this.succeededSeparateMd5Calculator = true;
                        this.finishedSeparateMd5Calculator = true;
                        break;
                    }

                    readLength = (int)Math.Min((this.md5hashOffset - offset), buffer.Length);
                }

                try
                {
                    cancellationChecker.CheckCancellation();
                    readLength = this.Read(offset, buffer, 0, readLength);

                    cancellationChecker.CheckCancellation();

                    lock (this.md5hash)
                    {
                        this.md5hash.TransformBlock(buffer, 0, readLength, null, 0);
                    }
                }
                catch (Exception)
                {
                    lock (this.md5hash)
                    {
                        this.finishedSeparateMd5Calculator = true;
                    }

                    memoryManager.ReleaseBuffer(buffer);

                    throw;
                }

                offset += readLength;
            }

            memoryManager.ReleaseBuffer(buffer);
        }

        /// <summary>
        /// Begin async read from stream.
        /// </summary>
        /// <param name="readOffset">Offset in stream to read from.</param>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in buffer at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read
        ///  request from other requests.</param>
        /// <returns> An System.IAsyncResult that represents the asynchronous read.</returns>
        public IAsyncResult BeginRead(long readOffset, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.AsyncBegin(
                delegate
                {
                    this.stream.Position = readOffset;
                    return this.stream.BeginRead(
                        buffer,
                        offset,
                        count,
                        delegate(IAsyncResult asyncResult)
                        {
                            callback(asyncResult);
                        },
                        state);
                });
        }

        /// <summary>
        /// End the async read from stream.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>The number of bytes read from the stream.</returns>
        public int EndRead(IAsyncResult asyncResult)
        {
            try
            {
                int readBytes = this.stream.EndRead(asyncResult);
                return readBytes;
            }
            finally
            {
                this.ReleaseSemaphore();
            }
        }

        /// <summary>
        /// Begin async write to stream.
        /// </summary>
        /// <param name="writeOffset">Offset in stream to write to.</param>
        /// <param name="buffer">The buffer to write the data from.</param>
        /// <param name="offset">The byte offset in buffer from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write
        ///  request from other requests.</param>
        /// <returns>An System.IAsyncResult that represents the asynchronous write.</returns>
        public IAsyncResult BeginWrite(long writeOffset, byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.AsyncBegin(
                delegate
                {
                    this.stream.Position = writeOffset;
                    return this.stream.BeginWrite(
                        buffer,
                        offset,
                        count,
                        delegate(IAsyncResult asyncResult)
                        {
                            callback(asyncResult);
                        },
                        state);
                });
        }

        /// <summary>
        /// End the async write to stream.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        public void EndWrite(IAsyncResult asyncResult)
        {
            try
            {
                this.stream.EndWrite(asyncResult);
            }
            finally
            {
                this.ReleaseSemaphore();
            }
        }

        /// <summary>
        /// Computes the hash value for the specified region of the input byte array
        /// and copies the specified region of the input byte array to the specified
        /// region of the output byte array.
        /// </summary>
        /// <param name="streamOffset">Offset in stream of the block on which to calculate MD5 hash.</param>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">A copy of the part of the input array used to compute the hash code.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>Whether succeeded in calculating MD5 hash 
        /// or not finished the separate thread to calculate MD5 hash at the time. </returns>
        public bool MD5HashTransformBlock(long streamOffset, byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (null == this.md5hash)
            {
                return true;
            }

            bool shouldCalculateMd5 = this.finishedSeparateMd5Calculator;

            if (!this.finishedSeparateMd5Calculator)
            {
                lock (this.md5hash)
                {
                    shouldCalculateMd5 = this.finishedSeparateMd5Calculator;

                    if (!this.finishedSeparateMd5Calculator)
                    {
                        if (streamOffset == this.md5hashOffset)
                        {
                            this.md5hashOffset += inputCount;
                        }

                        return true;
                    }
                    else
                    {
                        if (!this.succeededSeparateMd5Calculator)
                        {
                            return false;
                        }
                    }
                }
            }

            if (streamOffset >= this.md5hashOffset)
            {
                Debug.Assert(
                    this.finishedSeparateMd5Calculator,
                    "The separate thread to calculate MD5 hash should have finished or md5hashOffset should get updated.");

                this.md5hash.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            return true;
        }

        /// <summary>
        /// Computes the hash value for the specified region of the specified byte array.
        /// </summary>
        /// <param name="inputBuffer">The input to compute the hash code for.</param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data.</param>
        /// <returns>An array that is a copy of the part of the input that is hashed.</returns>
        public byte[] MD5HashTransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            this.WaitMD5CalculationToFinish();

            if (!this.succeededSeparateMd5Calculator)
            {
                return null;
            }

            return null == this.md5hash ? null : this.md5hash.TransformFinalBlock(inputBuffer, inputOffset, inputCount);
        }

        /// <summary>
        /// Releases or resets unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary>
        /// Private dispose method to release managed/unmanaged objects.
        /// If disposing = true clean up managed resources as well as unmanaged resources.
        /// If disposing = false only clean up unmanaged resources.
        /// </summary>
        /// <param name="disposing">Indicates whether or not to dispose managed resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != this.md5hash)
                {
                    this.md5hash.Clear();
                    this.md5hash = null;
                }

                this.stream = null;

                // should not dispose this.semaphore becasue task/thread may hang in wait.
            }
        }

        /// <summary>
        /// Read from stream.
        /// </summary>
        /// <param name="readOffset">Offset in stream to read from.</param>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified
        ///  byte array with the values between offset and (offset + count - 1) replaced
        ///  by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the buffer.</returns>
        private int Read(long readOffset, byte[] buffer, int offset, int count)
        {
            this.WaitOnSemaphore();

            try
            {
                this.stream.Position = readOffset;
                int readBytes = this.stream.Read(buffer, offset, count);

                return readBytes;
            }
            finally
            {
                this.ReleaseSemaphore();
            }
        }

        /// <summary>
        /// Begin an async stream access operation.
        /// </summary>
        /// <param name="asyncCall">Async operation to begin.</param>
        /// <returns>An System.IAsyncResult that represents the asynchronous operation.</returns>
        private IAsyncResult AsyncBegin(Func<IAsyncResult> asyncCall)
        {
            this.WaitOnSemaphore();

            try
            {
                return asyncCall();
            }
            catch (Exception)
            {
                this.ReleaseSemaphore();
                throw;
            }
        }

        /// <summary>
        /// Wait for one semahpore.
        /// </summary>
        private void WaitOnSemaphore()
        {
            if (!this.finishedSeparateMd5Calculator)
            {
                this.semaphore.Wait();
            }
        }

        /// <summary>
        /// Release semahpore.
        /// </summary>
        private void ReleaseSemaphore()
        {
            if (!this.finishedSeparateMd5Calculator)
            {
                this.semaphore.Release();
            }
        }

        /// <summary>
        /// Wait for MD5 calculation to be finished.
        /// In our test, MD5 calculation is really fast, 
        /// and SpinOnce has sleep mechanism, so use Spin instead of sleep here.
        /// </summary>
        private void WaitMD5CalculationToFinish()
        {
            if (this.finishedSeparateMd5Calculator)
            {
                return;
            }

            SpinWait sw = new SpinWait();

            while (!this.finishedSeparateMd5Calculator)
            {
                sw.SpinOnce();
            }

            sw.Reset();
        }
    }
}
