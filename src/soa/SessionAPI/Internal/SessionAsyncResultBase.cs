// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides base class for async result of session creation
    /// </summary>
    internal abstract class SessionAsyncResultBase : DisposableObject, IAsyncResult
    {
        /// <summary>
        /// The async callback
        /// </summary>
        private AsyncCallback callback;

        /// <summary>
        /// The async state
        /// </summary>
        private object asyncState;

        /// <summary>
        /// The operation is finished.
        /// </summary>
        private bool finished;

        /// <summary>
        /// The operation is canceled
        /// </summary>
        private bool canceled;

        /// <summary>
        /// the event to wait for the operation finish
        /// </summary>
        private ManualResetEvent evt = new ManualResetEvent(false);

        /// <summary>
        /// weather this is disposed
        /// </summary>
        private bool disposed;

        /// <summary>
        /// The result, the exception
        /// </summary>
        private Exception exception;

        /// <summary>
        /// Stores the session instance
        /// </summary>
        private SessionBase session;

        /// <summary>
        /// the session start info
        /// </summary>
        private SessionStartInfo startInfo;

        /// <summary>
        /// Initializes a new instance of the SessionAsyncResultBase class
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="callback">the async callback</param>
        /// <param name="asyncState">the async state</param>
        public SessionAsyncResultBase(
            SessionStartInfo startInfo,
            AsyncCallback callback,
            object asyncState)
        {
            this.callback = callback;
            this.startInfo = startInfo;
            this.asyncState = asyncState;
        }

        /// <summary>
        /// Gets a value indicating whether the operation has been canceled
        /// </summary>
        public bool Canceled
        {
            get { return this.canceled; }
        }

        /// <summary>
        /// Gets a value indicating whether this object is disposed
        /// </summary>
        public bool Disposed
        {
            get { return this.disposed; }
        }

        /// <summary>
        /// Gets the async state
        /// </summary>
        public object AsyncState
        {
            get { return this.asyncState; }
        }

        /// <summary>
        /// Gets the wait handle
        /// </summary>
        public System.Threading.WaitHandle AsyncWaitHandle
        {
            get { return this.evt; }
        }

        /// <summary>
        /// Gets a value indicating whether this operation is finished synchronously
        /// </summary>
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the operation is finished
        /// </summary>
        public bool IsCompleted
        {
            get { return this.finished; }
        }

        /// <summary>
        /// Gets the exception if there is any
        /// </summary>
        public Exception ExceptionResult
        {
            get { return this.exception; }
        }

        /// <summary>
        /// Gets the session instance as the result
        /// </summary>
        public SessionBase Session
        {
            get { return this.session; }
        }

        /// <summary>
        /// Gets the session start info
        /// </summary>
        protected SessionStartInfo StartInfo
        {
            get { return this.startInfo; }
        }

        /// <summary>
        /// Cancel the current operation
        /// </summary>
        public virtual void Cancel()
        {
            if (this.finished)
            {
                throw new InvalidOperationException();
            }

            // we will not callback if we are in cancel
            this.callback = null;
            this.canceled = true;
            this.MarkFinish(null, null);
        }

        /// <summary>
        /// Mark the operation finished, signal the event and call the callback.
        /// </summary>
        /// <param name="ex">the exception occured</param>
        protected void MarkFinish(Exception ex)
        {
            this.MarkFinish(ex, null);
        }

        /// <summary>
        /// Mark the operation finished, signal the event and call the callback.
        /// </summary>
        /// <param name="ex">the exception occured</param>
        /// <param name="session">indicating the session instance</param>
        protected void MarkFinish(Exception ex, SessionBase session)
        {
            if (this.disposed || this.finished)
            {
                return;
            }

            if (ex != null)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Error, 0, "[Session:Unknown] Create session failed: {0}", ex);
            }
            else if (session != null)
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "[Session:{0}] Create session succeeded.", session.Id);
            }
            else
            {
                SessionBase.TraceSource.TraceEvent(TraceEventType.Information, 0, "[Session:Unknown] Create session canceled.");
            }

            this.session = session;
            this.exception = ex;
            this.finished = true;
            this.evt.Set();

            if (this.callback != null && !this.canceled)
            {
                this.callback.BeginInvoke(this, null, this);

                // make sure we will not call callback twice
                this.callback = null;
            }

            if (ex != null || this.canceled)
            {
                this.CleanupOnFailure().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Override this operation to do clean up if failed to create session
        /// </summary>
        protected abstract Task CleanupOnFailure();

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">indicating whether it is disposing</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.disposed = true;
            if (disposing)
            {
                try
                {
                    this.evt.Close();
                }
                catch
                {
                }
            }
        }
    }
}
