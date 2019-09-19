// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;

    /// <summary>
    /// Represents a reference counter for an object
    /// All objects need to have the referenced disposed feature needs to inherit this class
    /// </summary>
    internal abstract class ReferenceObject : IDisposable
    {
#if DEBUG
        /// <summary>
        /// Stores the total count
        /// </summary>
        private static int totalCount;

        /// <summary>
        /// Gets a value indicating whether all the reference object has been disposed
        /// </summary>
        /// <returns>returns a value indicating whether all the reference object has been disposed</returns>
        public static bool CheckDisposed()
        {
            return totalCount == 0;
        }
#endif

        /// <summary>
        /// Stores the ref count
        /// </summary>
        private int refCount = 1;

        /// <summary>
        /// Stores the dispose wait handle
        /// </summary>
        private ManualResetEvent disposeWaitHandle;

        /// <summary>
        /// Initializes a new instance of the ReferenceObject class
        /// </summary>
        public ReferenceObject()
        {
#if DEBUG
            // In debug build, maintain the total count
            Interlocked.Increment(ref totalCount);
#endif
            this.disposeWaitHandle = new ManualResetEvent(false);
        }

        /// <summary>
        /// Finalizes an instance of the ReferenceObject class
        /// </summary>
        ~ReferenceObject()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Increase the count
        /// </summary>
        /// <returns>Return false means the count is already 0 and needs to return immediately</returns>
        public bool IncreaseCount()
        {
            if (this.refCount <= 0)
            {
                BrokerTracing.TraceVerbose("[ReferenceObject] Ref count is 0 when increasing, return false.");
                return false;
            }

            Interlocked.Increment(ref this.refCount);
            return true;
        }

        /// <summary>
        /// Decrease the count
        /// This method will call dispose if count is 0
        /// </summary>
        public void DecreaseCount()
        {
            if (Interlocked.Decrement(ref this.refCount) == 0)
            {
#if DEBUG
                // In debug build, decrease the total count when disposing
                Interlocked.Decrement(ref totalCount);
#endif
                this.Dispose(true);
            }
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        void IDisposable.Dispose()
        {
            int count = Interlocked.Decrement(ref this.refCount);
            if (count < 0)
            {
                BrokerTracing.TraceError("[ReferenceObject] Ref count is {0} in dispose method of {1} class", count, this.GetType());
                return;
            }
            else if (count == 0)
            {
#if DEBUG
                // In debug build, decrease the total count when disposing
                Interlocked.Decrement(ref totalCount);
#endif

                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
            else
            {
                // If the real dispose method is not done by the call, wait until the real procedure is done
                // This wait handle makes sure that the real dispose procedure is done after the Dispose method is called, which guaranteed the dispose order
                this.disposeWaitHandle.WaitOne();
            }

            // Dispose the wait handle at last
            this.disposeWaitHandle.Close();
            this.disposeWaitHandle = null;
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing">Indicating whether it is disposing</param>
        [SuppressMessage("Microsoft.Usage", "CA2213", MessageId = "disposeWaitHandle", Justification = "disposeWaitHandle will be disposed in the IDisposable.Dispose method.")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.disposeWaitHandle.Set();
            }
        }
    }
}
