//------------------------------------------------------------------------------
// <copyright file="ExcelClientCOM.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      COM interface for generic ExcelClient engine
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel.Com
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Office.Interop.Excel;

    using SoaService.DataClient;

    /// <summary>
    ///   <para>Enumeration used as intermediate for Job Scheduler type</para>
    /// </summary>
    [GuidAttribute("F88683D5-A26B-4768-A100-D79B11833729")]
    [ComVisible(true)]
    public enum SessionUnitType
    {
        /// <summary>
        ///   <para>Resource type is core</para>
        /// </summary>
        Core = 0,

        /// <summary>
        ///   <para>Resource type is socket</para>
        /// </summary>
        Socket = 1,

        /// <summary>
        ///   <para>Resource type is node</para>
        /// </summary>
        Node = 2,
    }

    /// <summary>
    ///   <para>COM interface for generic ExcelClient engine</para>
    /// </summary>
    [ProgId("Microsoft.Hpc.Excel.ExcelClient")]
    [GuidAttribute("9A716A75-166F-4EFF-B5B5-5E8CDBEFB375")]
    [ComVisible(true)]
    public class ExcelClient : IExcelClient, IExcelClientV1, IDisposable
    {
        /// <summary>
        /// Excel client to interact with
        /// </summary>
        private Microsoft.Hpc.Excel.ExcelClient client;

        /// <summary>
        /// Set when Initialize is called
        /// </summary>
        private bool initialized = false;

        /// <summary>
        ///   <para>Initializes a new instance of the ExcelClient class.</para>
        /// </summary>
        public ExcelClient()
        {
            this.client = new Microsoft.Hpc.Excel.ExcelClient();
        }

        /// <summary>
        ///   <para>Gets the current client version.</para>
        /// </summary>
        /// <value>
        ///   <para>A string that indicates the current client version.</para>
        /// </value>
        public string Version
        {
            get
            {
                return Microsoft.Hpc.Excel.ExcelClient.Version.ToString(2);
            }
        }

        /// <summary>
        ///   <para>Gets or sets the name of the resource which implements the hpc macros.</para>
        /// </summary>
        /// <value>
        ///   <para>A string that indicates the name of the resource.</para>
        /// </value>
        public string MacroResource
        {
            get
            {
                return this.client.MacroResource;
            }

            set
            {
                this.client.MacroResource = value;
            }
        }

        /// <summary>
        /// Register macro names and Excel application
        /// </summary>
        /// <param name="excelWorkbook">Excel Workbook</param>
        public void Initialize(Workbook excelWorkbook)
        {
            this.Initialize(excelWorkbook, null);
        }

        /// <summary>
        /// Register macro names and Excel application
        /// </summary>
        /// <param name="excelWorkbook">Excel Workbook</param>
        /// <param name="dependFiles">The depending files in format of "localFilePath1=remoteFilePath1;localFilePath2=remoteFilePath2;..."</param>
        public void Initialize(Workbook excelWorkbook, [Optional] string dependFiles)
        {
            this.client.Initialize(excelWorkbook);
            if (!string.IsNullOrEmpty(dependFiles))
            {
                foreach (string file in dependFiles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] keyValuePair = file.Split(new char[] { '=' });
                    if (keyValuePair.Length != 2)
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidDependFiles, file), "dependFiles");
                    }

                    if (Path.IsPathRooted(keyValuePair[1]))
                    {
                        throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidDependFiles, file), "dependFiles");
                    }

                    this.client.DependFiles.Add(keyValuePair[0], keyValuePair[1]);
                }
            }

            this.initialized = true;
        }

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
		/// <returns>
		///   <para> ID of opened session </para>
		/// </returns>
        public int OpenSession(string headNode, string remoteWorkbookPath, [Optional] object minResources, [Optional] object maxResources, [Optional] object resourceType, [Optional] string jobTemplate, [Optional] string serviceName)
        {
            return this.OpenSession(headNode, remoteWorkbookPath, minResources, maxResources, jobTemplate, serviceName, null, null, null, null, null, null, null, null);
        }

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
        public int OpenSession(string headNode, string remoteWorkbookPath, [Optional] object minResources, [Optional] object maxResources, [Optional] object resourceType, [Optional] string jobTemplate, [Optional] string serviceName, [Optional] string jobName, [Optional] string projectName, [Optional] string transportScheme, [Optional] object useAzureQueue, [Optional] string username, [Optional] string password, [Optional] object jobPriority)
        {
            // Check if workbook path has been provided (ExcelClient initialize called)
            if (!this.initialized)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientOpenSessionBeforeInit);
                throw new InvalidOperationException(Resources.ExcelClientOpenSessionBeforeInit);
            }
            SessionStartInfo info;
            bool useWebApi = false;

            if (headNode.StartsWith("d:", StringComparison.OrdinalIgnoreCase))
            {
                // HACK: using head node string to pass actual parameters
                var param = headNode.Split('?');
                string serviceRegDirectory = null;
                string[] computeNodeIpList = null;
                string storageCredential = null;
                string[] dependFiles = null;

                bool CheckParameterSwitch(string parameter, string switchStr)
                {
                    return parameter.StartsWith(switchStr, StringComparison.OrdinalIgnoreCase);
                }

                foreach (var p in param)
                {
                    if (CheckParameterSwitch(p, "d:"))
                    {
                        serviceRegDirectory = p.Substring(p.IndexOf(":") + 1);
                    }
                    else if (CheckParameterSwitch(p, "c:"))
                    {
                        computeNodeIpList = p.Substring(p.IndexOf(":") + 1).Split(',');
                    }
                    else if (CheckParameterSwitch(p, "s:"))
                    {
                        storageCredential = p.Substring(p.IndexOf(":") + 1);
                    }
                    else if (CheckParameterSwitch(p, "f:"))
                    {
                        dependFiles = p.Substring(p.IndexOf(":") + 1).Split(',');
                    }
                }

                if (serviceRegDirectory == null || computeNodeIpList == null)
                {
                    throw new ArgumentNullException();
                }


                // Standalone mode
                if (string.IsNullOrEmpty(serviceName))
                {
                    info = new SessionStartInfo(Microsoft.Hpc.Excel.ExcelClient.SERVICE, serviceRegDirectory, null, computeNodeIpList);
                }
                else
                {
                    info = new SessionStartInfo(serviceName, serviceRegDirectory, null, computeNodeIpList);
                }

                if (dependFiles != null && dependFiles.Any())
                {
                    // Start data client related logic
                    // TODO: move logic into ExcelClient
                    try
                    {
                        if (string.IsNullOrEmpty(storageCredential))
                        {
                            throw new ArgumentNullException(nameof(storageCredential));
                        }

                        StandaloneDataClient dataClient = new StandaloneDataClient(storageCredential);
                        var sasTokens = dataClient.UploadFilesAsync(dependFiles).GetAwaiter().GetResult();

                        if (sasTokens.Length != dependFiles.Length)
                        {
                            throw new InvalidOperationException($"Number of sas token ({sasTokens.Length}) does not equal to depend files ({dependFiles.Length})");
                        }

                        var depFileInfo = new Dictionary<string, string>();
                        for (int i = 0; i != sasTokens.Length; ++i)
                        {
                            depFileInfo[Path.GetFileName(dependFiles[i])] = sasTokens[i];
                        }

                        info.DependFilesStorageInfo = depFileInfo;
                        remoteWorkbookPath = Path.GetFileName(remoteWorkbookPath);
                    }
                    catch (Exception ex)
                    {
                        Tracing.SoaTrace(
                            XlTraceLevel.Error, "error when uploading files {0}", ex.ToString());
                        throw;
                    }

                    // End data client related logic
                }

                info.UseInprocessBroker = true;
                info.IsNoSession = true;
            }
            else
            {
                // If https prefix from head node name
                useWebApi = Microsoft.Hpc.Excel.ExcelClient.TryRemoveWebApiPrefix(ref headNode);

                // If the service name is provided, use it rather than the default in ExcelClient
                if (string.IsNullOrEmpty(serviceName))
                {
                    info = new SessionStartInfo(headNode, Microsoft.Hpc.Excel.ExcelClient.SERVICE);
                }
                else
                {
                    info = new SessionStartInfo(headNode, serviceName);
                }
            }

            if (useWebApi)
            {
                info.TransportScheme = TransportScheme.WebAPI;
            }

            if (!string.IsNullOrEmpty(transportScheme))
            {
                if (transportScheme.Equals("Http", StringComparison.InvariantCultureIgnoreCase))
                {
                    info.TransportScheme = TransportScheme.Http;
                }
                else if (transportScheme.Equals("NetTcp", StringComparison.InvariantCultureIgnoreCase))
                {
                    info.TransportScheme = TransportScheme.NetTcp;
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidTransportScheme, transportScheme), "transportScheme");
                }
            }

            if (!(useAzureQueue == null || System.Reflection.Missing.Value.Equals(useAzureQueue) || DBNull.Value.Equals(useAzureQueue)))
            {
                bool useAQ;
                if (bool.TryParse(useAzureQueue.ToString(), out useAQ))
                {
                    info.UseAzureQueue = useAQ;
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidBoolParamValue, "useAzureQueue", useAzureQueue), "useAzureQueue");
                }
            }

            if (!string.IsNullOrEmpty(username))
            {
                info.Username = username;
            }

            if (!string.IsNullOrEmpty(password))
            {
                info.Password = password;
            }

            if (!string.IsNullOrEmpty(jobName))
            {
                info.ServiceJobName = jobName;
            }

            if (!string.IsNullOrEmpty(projectName))
            {
                info.Project = projectName;
            }

            // If jobPriority is specified, try to parse it into an integer. 
            if (!(jobPriority == null || System.Reflection.Missing.Value.Equals(jobPriority) || DBNull.Value.Equals(jobPriority)))
            {
                int priority;
                if (int.TryParse(jobPriority.ToString(), out priority))
                {
                    info.SessionPriority = priority;
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidMinCores, jobPriority), "minResources");
                }
            }

            // If minResources is specified, try to parse it into an integer. 
            if (!(minResources == null || System.Reflection.Missing.Value.Equals(minResources) || DBNull.Value.Equals(minResources)))
            {
                int minUnits;
                if (int.TryParse(minResources.ToString(), out minUnits))
                {
                    info.MinimumUnits = minUnits;
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidMinCores, minResources), "minResources");
                }
            }

            // If maxResources is specified, try to parse it into an integer
            if (!(maxResources == null || System.Reflection.Missing.Value.Equals(maxResources) || DBNull.Value.Equals(maxResources)))
            {
                int maxUnits;
                if (int.TryParse(maxResources.ToString(), out maxUnits))
                {
                    info.MaximumUnits = maxUnits;
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidMaxCores, maxResources), "maxResources");
                }
            }

            // If job template is specified, set it
            if (!string.IsNullOrEmpty(jobTemplate))
            {
                info.JobTemplate = jobTemplate;
            }

            // Set resource type (defaults to core)
            if (!(resourceType == null || System.Reflection.Missing.Value.Equals(resourceType) || DBNull.Value.Equals(resourceType)))
            {
                int resIndex;
                if (int.TryParse(resourceType.ToString(), out resIndex))
                {
                    info.SessionResourceUnitType = ResourceToSessionUnitType((Microsoft.Hpc.Excel.Com.SessionUnitType)resIndex);
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidResType, resourceType), "resourceType");
                }
            }

            // Set interface mode to UI to ensure that Excel asks for credentials in the UI when not cached
            SessionBase.SetInterfaceMode(false, new IntPtr(this.client.Driver.App.Hwnd));

            return this.client.OpenSession(info, remoteWorkbookPath);
        }

        /// <summary>
        ///   <para>Perform calculations using partition/execute/merge asynchronously.</para>
        /// </summary>
        /// <param name="executeLocally">
        ///   <para>A bool indicates the calculation executes on local (true) or on cluster (false).</para>
        /// </param>
        public void Run(bool executeLocally)
        {
            // Get separate thread to execute the run command and monitor it's progress.
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object stateInfo)
            {
                // client.Run takes care of all error handling and workbook callbacks. 
                try
                {
                    this.client.Run(executeLocally);
                }
                catch
                {
                    // Fail silently, as workbook has already be notified of any failures
                }
            }));
        }

        /// <summary>
        ///   <para>Cancel running calculation.</para>
        /// </summary>
        public void Cancel()
        {
            this.client.Cancel();
        }

        /// <summary>
        ///   <para>Close down cluster session.</para>
        /// </summary>
        /// <param name="timeoutMilliseconds">
        ///   <para>Optional parameter containing the number of milliseconds to wait before timing out.</para>
        /// </param>
        public void CloseSession([Optional] object timeoutMilliseconds)
        {
            // Check if timeoutMilliseconds is supplied, use overloaded timeout operation if so, otherwise use default
            if (!(timeoutMilliseconds == null || System.Reflection.Missing.Value.Equals(timeoutMilliseconds) || DBNull.Value.Equals(timeoutMilliseconds)))
            {
                int timeout = int.Parse(timeoutMilliseconds.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                this.client.CloseSession(timeout);
            }
            else
            {
                this.client.CloseSession();
            }
        }

        /// <summary>
        ///   <para>Dispose of .NET references to Excel. Leave Excel open.</para>
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   <para>Actual performs the release of allocated resources.</para>
        /// </summary>
        /// <param name="disposing">
        ///   <para>True if disposing explicitly, which should only be done once.</para>
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // prevent multiple dispose calls from throwing a nullreference exception
                if (this.client != null)
                {
                    this.client.Dispose();
                    this.client = null;
                }
            }
        }

        /// <summary>
        /// Helper method to convert between Excel-defined resource type and SOA-defined resource type
        /// </summary>
        /// <param name="type">Excel-defined enum specifying resource type</param>
        /// <returns>SOA-defined resource type</returns>
        private static Microsoft.Hpc.Scheduler.Session.SessionUnitType? ResourceToSessionUnitType(SessionUnitType type)
        {
            Microsoft.Hpc.Scheduler.Session.SessionUnitType? unitType = null;

            // Assign return value based on input
            switch (type)
            {
                case SessionUnitType.Core:
                    unitType = Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core;
                    break;
                case SessionUnitType.Node:
                    unitType = Microsoft.Hpc.Scheduler.Session.SessionUnitType.Node;
                    break;
                case SessionUnitType.Socket:
                    unitType = Microsoft.Hpc.Scheduler.Session.SessionUnitType.Socket;
                    break;
                default:
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidResType, type), "type");
            }

            return unitType;
        }
    }
}
