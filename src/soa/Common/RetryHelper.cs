//------------------------------------------------------------------------------
// <copyright file="RetryHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class to retry operation if exception occured
// </summary>
//------------------------------------------------------------------------------

using TelepathyCommon;

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Threading.Tasks;
    /// <summary>
    /// Helper class to retry operation if exception occured
    /// </summary>
    /// <typeparam name="T">indicating the return type</typeparam>
    internal static class RetryHelper<T>
    {
        /// <summary>
        /// Operation delegate that returns type T
        /// </summary>
        /// <returns>returns value as type T</returns>
        public delegate T OperationDelegate();

        /// <summary>
        /// Operation delegate that returns type Task<T>
        /// </summary>
        /// <returns></returns>
        public delegate Task<T> OperationDelegateAsync();

        /// <summary>
        /// Delegate for method to handle exception
        /// </summary>
        /// <param name="e">indicating the exception</param>
        /// <param name="retry">indicating the current retry count</param>
        public delegate void ExceptionThrownDelegate(Exception e, int retry);

        /// <summary>
        /// Delegate for method to handle exception with retry manager
        /// </summary>
        /// <param name="e">indicating the exception</param>
        /// <param name="retryManager">indicating the retry manager</param>
        public delegate Task HandleExceptionDelegateAsync(Exception e, RetryManager retryManager);

        /// <summary>
        /// Invoke the operation with default retry count
        /// </summary>
        /// <param name="operation">indicating the operation delegate</param>
        /// <param name="onException">indicating the delegate to handle exception</param>
        /// <returns>returns result</returns>
        public static T InvokeOperation(OperationDelegate operation, ExceptionThrownDelegate onException)
        {
            return InvokeOperation(operation, onException, new RetryPolicy());
        }

        /// <summary>
        /// Invoke the operation
        /// </summary>
        /// <param name="operation">indicating the operation delegate</param>
        /// <param name="onException">indicating the delegate to handle exception</param>
        /// <param name="retryCount">indicating the max retry count</param>
        /// <returns>returns result</returns>
        public static T InvokeOperation(OperationDelegate operation, ExceptionThrownDelegate onException, int retryCount)
        {
            return InvokeOperation(operation, onException, new RetryPolicy(retryCount));
        }

        /// <summary>
        /// Invoke the operation
        /// </summary>
        /// <param name="operation">indicating the operation delegate</param>
        /// <param name="onException">indicating the delegate to handle exception</param>
        /// <param name="retryCount">indicating the max retry count</param>
        /// <returns>returns result</returns>
        public static T InvokeOperation(OperationDelegate operation, ExceptionThrownDelegate onException, RetryPolicy policy)
        {
            int retry = policy.RetryCount;
            Exception lastException = null;
            while (retry > 0)
            {
                try
                {
                    return operation();
                }
                catch (Exception e)
                {
                    lastException = e;
                    onException(e, retry);

                    if (!policy.NoIncreaseRetryCountExceptionList.Contains(e.GetType()))
                    {
                        retry--;
                    }
                }
            }

            throw lastException;
        }

        /// <summary>
        /// Invoke operation async
        /// </summary>
        /// <param name="operation">the operation async</param>
        /// <param name="onException">the exception handler async</param>
        /// <param name="retryManager">the retry manager</param>
        /// <returns>the operation result</returns>
        public async static Task<T> InvokeOperationAsync(OperationDelegateAsync operation, HandleExceptionDelegateAsync onException, RetryManager retryManager)
        {
            Exception lastException = null;
            while (true)
            {
                try
                {
                    return await operation().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    lastException = e;
                }
                
                await onException(lastException, retryManager).ConfigureAwait(false);

                if (retryManager.HasAttemptsLeft)
                {
                    await retryManager.AwaitForNextAttempt().ConfigureAwait(false);
                }
                else
                {
                    break;
                }
            }

            throw lastException;
        }
    }
}
