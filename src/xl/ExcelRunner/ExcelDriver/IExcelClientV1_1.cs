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
    [Guid("D6EA25E4-DC17-450D-91D8-A5746E98CD54")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IExcelClient
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
        ///   <para>Gets or sets the resource (workbook or addin) that contains the P/E/M macros.</para>
        /// </summary>
        /// <value>
        ///   <para>A string indicates the macro resource.</para>
        /// </value>
        string MacroResource
        {
            get;
            set;
        }

        /// <summary>
        /// Register macro names and Excel application
        /// </summary>
        /// <param name="excelWorkbook">Excel Workbook</param>
        /// <param name="dependFiles">The depending files in format of "localFilePath1=remoteFilePath1;localFilePath2=remoteFilePath2;..."</param>
        void Initialize(Workbook excelWorkbook, [Optional] string dependFiles);

		/// <summary>
		///   <para>
		/// Initializes session parameters and calls open session. Only for cluster computation.
		/// </para>
		/// </summary>
		/// <param name="headNode">
		///   <para>Name of cluster head node</para>
		/// </param>
		/// <param name="remoteWorkbookPath">
		///   <para>Workbook location relative to compute node</para>
		/// </param>
		/// <param name="minResources">
		///   <para>minimum number of resources requested</para>
		/// </param>
		/// <param name="maxResources">
		///   <para>Maximum number of resources required</para>
		/// </param>
		/// <param name="resourceType">
		///   <para>Name of resource requested (core, node, or socket)</para>
		/// </param>
		/// <param name="jobTemplate">
		///   <para> Name of the job template to be used </para>
		/// </param>
		/// <param name="serviceName">
		///   <para> Name of the service to use </para>
		/// </param>
		/// <param name="jobName">
		///   <para>Specify the job name</para>
		/// </param>
		/// <param name="projectName">
		///   <para>Specify the project name</para>
		/// </param>
		/// <param name="transportScheme">
		///   <para>The transport scheme (Http or NetTcp)</para>
		/// </param>
		/// <param name="useAzureQueue">
		///   <para>Specify if Azure storage queue is used (True or False)</para>
		/// </param>
		/// <param name="username">
		///   <para>Specify the user name</para>
		/// </param>
		/// <param name="password">
		///   <para>Specify the password</para>
		/// </param>
		/// <param name="jobPriority">
		///   <para>Specify the job priority</para>
		/// </param>
		/// <returns>
		///   <para> ID of opened session </para>
		/// </returns>
        int OpenSession(string headNode, string remoteWorkbookPath, [Optional] object minResources, [Optional] object maxResources, [Optional] object resourceType, [Optional] string jobTemplate, [Optional] string serviceName, [Optional] string jobName, [Optional] string projectName, [Optional] string transportScheme, [Optional] object useAzureQueue, [Optional] string username, [Optional] string password, [Optional] object jobPriority);

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
