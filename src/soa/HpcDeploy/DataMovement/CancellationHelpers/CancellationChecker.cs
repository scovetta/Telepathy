//------------------------------------------------------------------------------
// <copyright file="CancellationChecker.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Check whether the call has been cancelled, if yes, throw an exception
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.DataMovement.CancellationHelpers
{
    using System;

    internal class CancellationChecker
    {
        private volatile bool cancelled = false;

        public void CheckCancellation()
        {
            if (this.cancelled)
            {
                throw new OperationCanceledException(Resources.BlobTransferCancelledException);
            }
        }

        public void Cancel()
        {
            this.cancelled = true;
        }
    }
}
