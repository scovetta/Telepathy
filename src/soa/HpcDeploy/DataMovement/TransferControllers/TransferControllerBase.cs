//------------------------------------------------------------------------------
// <copyright file="TransferControllerBase.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Defines common function of different transfer controllers.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;
    using System.Threading;
    using Microsoft.Hpc.Azure.DataMovement.CancellationHelpers;
    using Microsoft.Hpc.Azure.DataMovement.Exceptions;
    using Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers;

    internal abstract class TransferControllerBase : ITransferController
    {
        /// <summary>
        /// Count of active tasks in this controller.
        /// </summary>
        private int activeTasks;

        private volatile bool isFinished = false;

        private object lockOnFinished = new object();

        /// <summary>
        /// Handler object to deal with aync call cancellation.
        /// </summary>
        private AsyncCallCancellationHandler cancellationHandler = new AsyncCallCancellationHandler();
        
        /// <summary>
        /// Action declare for prompt callback.
        /// </summary>
        /// <returns>
        /// Returns what prompt returned.
        /// </returns>
        protected delegate bool PromptCallbackAction();

        /// <summary>
        /// Gets a value indicating whether this transfer item may leads
        /// additional controllers to be added to the queue.
        /// </summary>
        public abstract bool CanAddController
        {
            get;
        }

        /// <summary>
        /// Gets a value indicating whether this transfer item may leads
        /// additional monitors to be added to the queue.
        /// </summary>
        public abstract bool CanAddMonitor
        {
            get;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the controller has work available
        /// or not for the calling code. If HasWork is false, while IsFinished
        /// is also false this indicates that there are currently still active
        /// async tasks running. The caller should continue checking if this
        /// controller HasWork available later; once the currently active 
        /// async tasks are done HasWork will change to True, or IsFinished
        /// will be set to True.
        /// </summary>
        public bool HasWork
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a value indicating whether this controller is finished with
        /// its uploading task.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return this.isFinished;
            }
        }

        /// <summary>
        /// Gets or sets opaque user data object.
        /// </summary>
        public object UserData
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets handler object to deal with aync call cancellation.
        /// </summary>
        protected AsyncCallCancellationHandler CancellationHandler
        {
            get
            {
                return this.cancellationHandler;
            }
        }

        /// <summary>
        /// Gets or sets manager object which creates this object.
        /// </summary>
        protected BlobTransferManager Manager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a work item delegate. Each work item represents a single 
        /// asynchronous operation or a single asynchronous continuation.
        /// If no work is currently available returns null.
        /// </summary>
        /// <returns>Work item delegate.</returns>
        public abstract Action<Action<ITransferController, bool>> GetWork();
        
        /// <summary>
        /// Cancels all work in the controller.
        /// </summary>
        public void CancelWork()
        {
            this.cancellationHandler.Cancel();
        }

        /// <summary>
        /// Pre work action.
        /// </summary>
        protected void PreWork()
        {
            Interlocked.Increment(ref this.activeTasks);
        }

        /// <summary>
        /// Post work action.
        /// </summary>
        /// <returns>
        /// Count of current active task in the controller.
        /// A Controller can only be destroyed after this count of active tasks is 0.
        /// </returns>
        protected bool PostWork()
        {
            lock (this.lockOnFinished)
            {
                return 0 == Interlocked.Decrement(ref this.activeTasks) && this.isFinished;
            }
        }

        protected bool SetFinishedAndPostWork()
        {
            lock (this.lockOnFinished)
            {
                this.isFinished = true;
                return 0 == Interlocked.Decrement(ref this.activeTasks) && this.isFinished;
            }
        }
        
        /// <summary>
        /// Sets the state of the controller to Error, while recording
        /// the last occured exception and setting the HasWork and 
        /// IsFinished fields.
        /// </summary>
        /// <param name="ex">Exception to record.</param>
        /// <param name="callbackState">Callback state to finish after 
        /// setting error state.</param>
        protected abstract void SetErrorState(Exception ex, CallbackState callbackState);

        /// <summary>
        /// Catches and handles exception thrown from prompt callback.
        /// </summary>
        /// <param name="promptcallbackAction">Delegate to call prompt callback.</param>
        /// <param name="callbackState">CallbackState object used to finish current work if any exception caught. </param>
        /// <returns>Prompt return value or false if any exception thrown in prompt callback.</returns>
        protected bool PromptCallBackExceptionHandler(PromptCallbackAction promptcallbackAction, CallbackState callbackState)
        {
            try
            {
                return promptcallbackAction();
            }
            catch (Exception ex)
            {
                this.SetErrorState(ex, callbackState);
                return false;
            }
        }

        /// <summary>
        /// Catches and hanles exception thrown from callback.
        /// </summary>
        /// <param name="callbackAction">Delegate to call callback.</param>
        protected void CallbackExceptionHandler(Action callbackAction)
        {
            try
            {
                callbackAction();
            }
            catch (Exception ex)
            {
                throw new BlobTransferCallbackException(Resources.DataMovement_ExceptionFromCallback, ex);
            }
        }

        /// <summary>
        /// Handles prompt callback for whether to overwrite an exist destination.
        /// </summary>
        /// <param name="sourceFileName">Source file Name.</param>
        /// <param name="desFileName">Destination file Name.</param>
        /// <param name="callbackState">CallbackState object to SetErrorState.</param>
        /// <returns>
        /// Whether to overwrite the destination.
        /// If the indicated overwrite prompt callback is null or it returns, this function returns true. 
        /// Otherwise, returns false.
        /// </returns>
        protected bool OverwritePromptCallbackHandler(string sourceFileName, string desFileName, CallbackState callbackState)
        {
            return this.PromptCallBackExceptionHandler(
                delegate
                {
                    if (null != this.Manager.TransferOptions.OverwritePromptCallback)
                    {
                        if (!this.Manager.TransferOptions.OverwritePromptCallback(sourceFileName, desFileName))
                        {
                            throw new OperationCanceledException(Resources.OverwriteCallbackCancelTransferException);
                        }
                    }

                    return true;
                },
                callbackState);
        }

        /// <summary>
        /// Handles prompt callback for whether to retransfer a modified source from very beginning.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="callbackState">CallbackState object to SetErrorState.</param>
        /// <returns>
        /// If the retransfer modified callback returns true, returns true.
        /// Otherwise, if the callback is null or threw any exception, return false.
        /// </returns>
        protected bool RetransferModifiedCallbackHandler(string sourceFileName, CallbackState callbackState)
        {
            return this.PromptCallBackExceptionHandler(
                delegate
                {
                    if (null != this.Manager.TransferOptions.RetransferModifiedCallback)
                    {
                        if (this.Manager.TransferOptions.RetransferModifiedCallback(sourceFileName))
                        {
                            return true;
                        }
                    }

                    throw new InvalidOperationException(
                            Resources.SourceFileHasBeenChangedException);
                },
                callbackState);
        }
    }
}
