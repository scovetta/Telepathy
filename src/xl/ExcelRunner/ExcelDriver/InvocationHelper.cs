//------------------------------------------------------------------------------
// <copyright file="InvocationHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class for calling into Excel from UI thread.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.EnterpriseServices;
    using System.Runtime.InteropServices;

    /// <summary>
    ///   <para>Helper class for calling into Excel from UI thread.</para>
    /// </summary>
    [ComVisible(true)]
    [GuidAttribute("D3822ECB-9F1B-4ece-9A3F-DF22A1E73257")]
    public class InvocationHelper : ServicedComponent
    {
        /// <summary>
        /// Initialize macro name with workbook name prepended
        /// </summary>
        private string initializeMacro;

        /// <summary>
        /// Partition macro name with workbook name prepended
        /// </summary>
        private string partitionMacro;

        /// <summary>
        /// Execute macro name with workbook name prepended
        /// </summary>
        private string executeMacro;

        /// <summary>
        /// Finalize macro name with workbook name prepended
        /// </summary>
        private string finalizeMacro;

        /// <summary>
        /// Merge macro name with workbook name prepended
        /// </summary>
        private string mergeMacro;

        /// <summary>
        /// Version macro name with workbook name prepended
        /// </summary>
        private string versionMacro;

        /// <summary>
        /// Error macro name with workbook name prepended
        /// </summary>
        private string errorMacro;

        /// <summary>
        /// ExcelClient which owns this InvocationHelper
        /// </summary>
        private ExcelClient excelClient;

        /// <summary>
        ///   <para>Initializes a new instance of the InvocationHelper class.</para>
        /// </summary>
        public InvocationHelper() : base()
        {
        }

        /// <summary>
        /// Invoke the hpc_initialize macro.
        /// </summary>
        internal void InvokeInitialize()
        {
            this.excelClient.Driver.RunMacro(this.initializeMacro);
        }

        /// <summary>
        /// Invoke the hpc_Partition macro.
        /// </summary>
        internal void InvokePartition()
        {
            this.excelClient.MacroOutput = this.excelClient.Driver.RunMacro(this.partitionMacro);
        }

        /// <summary>
        /// Invoke the hpc_execute macro.
        /// </summary>
        internal void InvokeExecute()
        {
            this.excelClient.MacroOutput = this.excelClient.Driver.RunMacro(this.executeMacro, this.excelClient.MacroInput);
        }

        /// <summary>
        /// Invoke the hpc_merge macro.
        /// </summary>
        internal void InvokeMerge()
        {
            this.excelClient.Driver.RunMacro(this.mergeMacro, this.excelClient.MacroInput);
        }

        /// <summary>
        /// Invoke the hpc_finalize macro.
        /// </summary>
        internal void InvokeFinalize()
        {
            this.excelClient.Driver.RunMacro(this.finalizeMacro);
        }

        /// <summary>
        /// Invoke the hpc_executionerror macro.
        /// </summary>
        internal void InvokeError()
        {
            Exception ex = (Exception)this.excelClient.MacroInput;
            this.excelClient.Driver.RunMacro(this.errorMacro, ex.Message, ex.ToString());
        }

        /// <summary>
        /// Invoke the hpc_getversion macro.
        /// </summary>
        /// <returns> Response from hpc_getversion macro</returns>
        internal object InvokeGetVersion()
        {
            return this.excelClient.Driver.RunMacro(this.versionMacro);
        }

        /// <summary>
        /// Register the ExcelClient instance with the helper.
        /// </summary>
        /// <param name="client">ExcelClient instance to use to interact with Excel</param>
        internal void Initialize(ExcelClient client)
        {
            this.excelClient = client;
           
            // Capture the macro names locally to avoid cross-thread calls to excelclient at invocation time
            this.initializeMacro = this.excelClient.InitializeMacro;
            this.partitionMacro = this.excelClient.PartitionMacro;
            this.executeMacro = this.excelClient.ExecuteMacro;
            this.mergeMacro = this.excelClient.MergeMacro;
            this.finalizeMacro = this.excelClient.FinalizeMacro;
            this.versionMacro = this.excelClient.VersionMacro;
            this.errorMacro = this.excelClient.ErrorMacro;
        }
    }
}
