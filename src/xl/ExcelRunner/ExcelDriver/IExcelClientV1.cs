//------------------------------------------------------------------------------
// <copyright file="IExcelClientV1.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Version 1 Interface for ExcelClientCOM 
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel.Com
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Office.Interop.Excel;

	/// <summary>
	///   <para />
	/// </summary>
    [Guid("0391C6BF-5891-43c9-A440-D365610B575C")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IExcelClientV1
    {
        /// <summary>
        ///   <para>Gets the current client version.</para>
        /// </summary>
        /// <value>
        ///   <para>A string indicates the current client version.</para>
        /// </value>
        string Version
        {
            get;
        }

        /// <summary>
        /// Register macro names and Excel application
        /// </summary>
        /// <param name="excelWorkbook">Excel Workbook</param>
        void Initialize(Workbook excelWorkbook);

        /// <summary>
        ///   <para>Initializes session parameters and calls open session. Only for cluster computation.</para>
        /// </summary>
        /// <param name="headNode">
        ///   <para />
        /// </param>
        /// <param name="remoteWorkbookPath">
        ///   <para />
        /// </param>
        /// <param name="minResources">
        ///   <para />
        /// </param>
        /// <param name="maxResources">
        ///   <para />
        /// </param>
        /// <param name="resourceType">
        ///   <para />
        /// </param>
        /// <param name="jobTemplate">
        ///   <para />
        /// </param>
        /// <param name="serviceName">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        int OpenSession(string headNode, string remoteWorkbookPath, [Optional] object minResources, [Optional] object maxResources, [Optional] object resourceType, [Optional] string jobTemplate, [Optional] string serviceName);

        /// <summary>
        ///   <para>Perform calculations using partition/execute/merge.</para>
        /// </summary>
        /// <param name="executeLocally">
        ///   <para>Execute calculations locally or on cluster.</para>
        /// </param>
        void Run(bool executeLocally);

        /// <summary>
        ///   <para>Cancel running calculation.</para>
        /// </summary>
        void Cancel();

        /// <summary>
        ///   <para>Close down cluster session.</para>
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///   <para>Optional parameter containing the number of milliseconds to wait before timing out.</para>
        /// </param>
        void CloseSession([Optional] object timeoutMilliseconds);

        /// <summary>
        ///   <para>Dispose of .NET references to Excel. Leave Excel open.</para>
        /// </summary>
        void Dispose();
    }
}
