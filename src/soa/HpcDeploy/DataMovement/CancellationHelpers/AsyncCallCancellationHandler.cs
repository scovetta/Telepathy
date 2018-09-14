//------------------------------------------------------------------------------
// <copyright file="AsyncCallCancellationHandler.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Deal with cancellation on cancellable async calls
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.CancellationHelpers
{
    using Microsoft.WindowsAzure.Storage;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    internal class AsyncCallCancellationHandler
    {
        private ConcurrentDictionary<ICancellableAsyncResult, object> asyncOperations
            = new ConcurrentDictionary<ICancellableAsyncResult, object>();

        private CancellationChecker cancellationChecker = new CancellationChecker();

        internal delegate ICancellableAsyncResult CancellableAsyncCall();

        public CancellationChecker CancellationChecker
        {
            get
            {
                return this.cancellationChecker;
            }
        }

        public void CheckCancellation()
        {
            this.cancellationChecker.CheckCancellation();
        }

        public void RegisterCancellableAsyncOper(CancellableAsyncCall asyncCall)
        {
            this.CheckCancellation();

            this.asyncOperations.TryAdd(asyncCall(), null);
        }

        public void DeregisterCancellableAsyncOper(ICancellableAsyncResult cancellableAsyncResult)
        { 
            object tempObject;
            this.asyncOperations.TryRemove(cancellableAsyncResult, out tempObject);
        }

        public void Cancel()
        {
            this.cancellationChecker.Cancel();

            foreach (KeyValuePair<ICancellableAsyncResult, object> keyValue in this.asyncOperations)
            {
                keyValue.Key.Cancel();
            }

            this.asyncOperations.Clear();            
        }
    }
}
