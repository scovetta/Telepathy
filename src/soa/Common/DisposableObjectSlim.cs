//-----------------------------------------------------------------------
// <copyright file="DisposableObjectSlim.cs" company="Microsoft">
//     Copyright (C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Provides the common base class for Disposable objects which
//   don't need a finalizer
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Common
{
    using System;
    using System.Threading;

    /// <summary>
    /// Provides the common base class for Disposable objects which
    /// don't need a finalizer
    /// </summary>
    internal abstract class DisposableObjectSlim : IDisposable
    {
        /// <summary>
        /// Stores the disposed flag
        /// 0: not disposed
        /// 1: disposed
        /// </summary>
        private int disposed;

        /// <summary>
        /// Gets a value indicating whether the current instance
        /// has been disposed
        /// </summary>
        protected bool IsDisposed
        {
            get { return this.disposed == 1; }
        }

        /// <summary>
        /// Dispose the instance and release resources
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
            {
                this.DisposeInternal();
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Do real work to release resources
        /// </summary>
        protected virtual void DisposeInternal()
        {
        }
    }
}
