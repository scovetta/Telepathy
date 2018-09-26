//------------------------------------------------------------------------------
// <copyright file="IHPCExcelService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Generic Excel Runner Service Interface
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// Service Contract for Generic ExcelRunner Service
    /// </summary>
    [ServiceContract]
    public interface IExcelService
    { 
        /// <summary>
        /// Calculation Request Method
        /// </summary>
        /// <param name="workbookPath">Path to workbook</param>
        /// <param name="macroName">Name of Macro to invoke</param>
        /// <param name="inputs">Serialized Inputs</param>
        /// <param name="lastSaveDate">Date of last workbook save (or null)</param>
        /// <returns>Serialized output of Macro</returns>
        [OperationContract]
        [FaultContract(typeof(ExcelServiceError))]
        [FaultContract(typeof(RetryOperationError), Action = "http://hpc.microsoft.com/session/RetryOperationError", Name = "RetryOperationError", Namespace = "http://hpc.microsoft.com/session/")]
        byte[] Calculate(string macroName, byte[] inputs, DateTime? lastSaveDate);





    }   
}