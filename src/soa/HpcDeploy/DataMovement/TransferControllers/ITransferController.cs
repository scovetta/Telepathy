//------------------------------------------------------------------------------
// <copyright file="ITransferController.cs" company="Microsoft">
//      Copyright 2012 Microsoft Corporation. All rights reserved.
// </copyright>
// <summary>
//      Transfer controller interface.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.DataMovement.TransferControllers
{
    using System;

    internal interface ITransferController
    {
        bool CanAddController
        {
            get;
        }

        bool CanAddMonitor
        {
            get;
        }

        bool HasWork
        {
            get;
        }

        bool IsFinished
        {
            get;
        }

        Action<Action<ITransferController, bool>> GetWork();
        
        void CancelWork();
    }
}
