//------------------------------------------------------------------------------
// <copyright file="CallbackState.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Helper class for BlobTransfer to do callback.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferStatusHelpers
{
    using System;
    using Microsoft.Hpc.Azure.DataMovement.TransferControllers;

    /// <summary>
    /// Helper class for BlobTransfer to do callback.
    /// </summary>
    internal class CallbackState
    {
        public Action<ITransferController, bool> FinishDelegate
        {
            get;
            set;
        }

        public void CallFinish(ITransferController transferController, bool finished)
        {
            this.FinishDelegate(transferController, finished);
        }
    }
}