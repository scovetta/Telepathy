//-----------------------------------------------------------------------
// <copyright file="DisposableObject.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Provide base class for all disposable objects</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Common
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provide base class for all disposable objects
    /// Implemented IDisposable pattern
    /// </summary>
    public abstract class DisposableObject : IDisposable
    {
        /// <summary>
        /// Stores the dispose flag for not disposed
        /// </summary>
        private const int NotDisposed = 0;

        /// <summary>
        /// Stores the dispose flag for disposed by user
        /// </summary>
        private const int DisposedByUser = 1;

        /// <summary>
        /// Stores the dispose flag for disposed by GC
        /// </summary>
        private const int DisposedByGC = 2;

        /// <summary>
        /// Provides a debug only disposed flag
        /// 0: not dispsoed
        /// 1: disposed by calling "Close" or "IDisposable.Dispose()"
        /// 2: disposed by GC
        /// </summary>
        private int disposedFlag = NotDisposed;

        /// <summary>
        /// Finalizes an instance of the DisposableObject class
        /// </summary>
        ~DisposableObject()
        {
            // Only dispose once
            if (Interlocked.CompareExchange(ref this.disposedFlag, DisposedByGC, NotDisposed) == NotDisposed)
            {
                this.Dispose(false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed
        /// </summary>
        protected bool IsDisposed
        {
            get { return this.disposedFlag != NotDisposed; }
        }

        /// <summary>
        /// Close instance
        /// </summary>
        public void Close()
        {
            this.Dispose();
        }

        /// <summary>
        /// Dispose the instance of the DisposableObject class
        /// </summary>
        public void Dispose()
        {
            // Only dispose once
            if (Interlocked.CompareExchange(ref this.disposedFlag, DisposedByUser, NotDisposed) == NotDisposed)
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
