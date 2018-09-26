//------------------------------------------------------------------------------
// <copyright file="ExcelClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Generic Engine for interacting with ExcelService and ExcelDriver
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    using ExcelService;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Excel.Win32;
    using Microsoft.Hpc.Scheduler;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Office.Interop.Excel;
    using System.Threading.Tasks;
	/// <summary>
	///   <para>Represents an engine that implements the partition, calculate, and merge model that Microsoft HPC Pack Services for 
	/// Excel uses to run calculations in a workbook in parallel on an HPC cluster. The workbook must implement the callback macro framework.</para>
	/// </summary>
	/// <remarks>
	///   <para>This class acts as a service-oriented architecture (SOA) client for the built-in HPC Excel service that the 
	/// <see cref="Microsoft.Hpc.Excel.ExcelService.IExcelService" /> interface represents, and interacts with Excel through the 
	/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> class.</para>
	/// </remarks>
	/// <seealso cref="Microsoft.Hpc.Excel.ExcelService.IExcelService" />
	/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver" />
    [ComVisible(false)]
    public class ExcelClient : IDisposable
    {
		/// <summary>
		///   <para>Represents the version of the Excel client.</para>
		/// </summary>
		/// <returns>
		///   <para />
		/// </returns>
		/// <remarks>
		///   <para>This field is read-only. For Windows HPC Server 2008 R2, this value is always 1.0.</para>
		/// </remarks>
        public static readonly Version Version = new Version(1, 1);

        /// <summary>
        /// Name of environment variable holding workbook path for ExcelService
        /// </summary>
        internal const string SERVICE = "Microsoft.Hpc.Excel.ExcelService";

        /// <summary>
        /// Partition Macro Name
        /// </summary>
        internal const string PARTITIONMACRO = "HPC_Partition";

        /// <summary>
        /// Execute Macro Name
        /// </summary>
        internal const string EXECUTEMACRO = "HPC_Execute";

        /// <summary>
        /// Merge Macro Name
        /// </summary>
        internal const string MERGEMACRO = "HPC_Merge";

        /// <summary>
        /// Initialize Macro Name
        /// </summary>
        internal const string INITIALIZEMACRO = "HPC_Initialize";

        /// <summary>
        /// Finalize Macro Name
        /// </summary>
        internal const string FINALIZEMACRO = "HPC_Finalize";

        /// <summary>
        /// Execution Error Macro Name
        /// </summary>
        internal const string ERRORMACRO = "HPC_ExecutionError";

        /// <summary>
        /// Get Version Macro Name
        /// </summary>
        internal const string VERSIONMACRO = "HPC_GetVersion";

        /// <summary>
        /// Name of environment variable holding workbook path for ExcelService
        /// </summary>
        private const string WORKBOOKENVVAR = "Microsoft.Hpc.Excel.WorkbookPath";

        /// <summary>
        /// Name of the environment variable of the session job data share folder
        /// </summary>
        private const string SoaDataJobDirEnvVar = "HPC_SOADATAJOBDIR";

        /// <summary>
        /// Default timeout for close operation. Using 2 minutes or 120 seconds.
        /// </summary>
        private const int DEFAULTCLOSETIMEOUT = 120000;

        /// <summary>
        /// Version of the macro specification supported by this binary. Checked against
        /// value returned from hpc_getversion
        /// </summary>
        private const string MACROSPECVERSION = "1.0";

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
        /// Helper object which invokes macros from the UI thread.
        /// </summary>
        private InvocationHelper invocationHelper = null;

        /// <summary>
        /// Object containing the input for the invoked macro
        /// </summary>
        private object macroInput;

        /// <summary>
        /// Object containing the output for the invoked macro
        /// </summary>
        private object macroOutput;

        /// <summary>
        /// ExcelDriver to interact with local Excel workbook
        /// </summary>
        private ExcelDriver excelDriver;

        /// <summary>
        /// Lockable object for synchronizing access to the InvocationHelper
        /// </summary>
        private object invocationLock = new object();

        /// <summary>
        /// Path to local workbook
        /// </summary>
        private string localWorkbookPath;

        /// <summary>
        /// Thread coordination lock
        /// </summary>
        private object initializeLock = new object();

        /// <summary>
        /// Lock to coordinate response handling
        /// </summary>
        private object responseLock = new object();

        /// <summary>
        /// Session with Broker
        /// </summary>
        private Session session;

        /// <summary>
        /// Client interacting with broker
        /// </summary>
        private BrokerClient<IExcelService> client;

        /// <summary>
        /// Thread coordination lock
        /// </summary>
        private AutoResetEvent done = new AutoResetEvent(false);

        /// <summary>
        /// Remote location of workbook
        /// </summary>
        private string remoteWorkbookPath;

        /// <summary>
        /// Name of cluster headNode
        /// </summary>
        private string headNode;

        /// <summary>
        /// Exception caught during responses.
        /// </summary>
        private bool responseException;

        /// <summary>
        /// Job cancelled flag
        /// </summary>
        private bool cancelled = false;

        /// <summary>
        /// Job cancelled flag
        /// </summary>
        private bool alreadyRunning = false;

        /// <summary>
        /// Lock for client modification/usage
        /// </summary>
        private object clientLock = new object();

        /// <summary>
        /// Job cancelled flag
        /// </summary>
        private bool initialized = false;

        /// <summary>
        /// Asynchronous action for BeginRun/EndRun 
        /// </summary>
        private Action<bool> runAction;

        /// <summary>
        /// Flag to keep track if the session is open
        /// </summary>
        private bool sessionOpen = false;

        /// <summary>
        /// Lockable object to synchronize access to the session object
        /// and critical sections that rely on a consistent session.
        /// </summary>
        private object sessionLock = new object();

        /// <summary>
        /// Lockable object to synchronize starting and finishing a run
        /// </summary>
        private object runningLock = new object();

        /// <summary>
        /// Flag to remember whether the user initialized the ExcelClient with an
        /// alternative resource to use for macros
        /// </summary>
        private string macroResourceName = null;

        /// <summary>
        /// The transport scheme
        /// </summary>
        private TransportScheme scheme = TransportScheme.NetTcp;

        /// <summary>
        /// Static constructor to resolve possible loading problem
        /// </summary>
        static ExcelClient()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
        }

		/// <summary>
		///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Excel.ExcelClient" /> class.</para>
		/// </summary>
		/// <remarks>
		///   <para>This default constructor initializes any fields to their default values. </para>
		/// </remarks>
        public ExcelClient()
        {
        }

		/// <summary>
		///   <para>An event that is raised when the response handler receives errors from the HPC cluster.</para>
		/// </summary>
		/// <remarks>
		///   <para>To be notified of errors in responses to calculation requests, add a delegate to 
		/// this event. This delegate must include parameters for the object that sent the event and for a  
		/// <see cref="Microsoft.Hpc.Excel.ResponseErrorEventArgs" /> object. The following code example shows the signature for such a delegate.</para>
		///   <code>private void ErrorHandler(object sender, ResponseErrorEventArgs e)</code>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ResponseErrorEventArgs" />
        public event EventHandler<ResponseErrorEventArgs> ErrorHandler;

		/// <summary>
		///   <para>Gets the <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object that interacts with the currently open Excel workbook.</para>
		/// </summary>
		/// <value>
		///   <para>The <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object that interacts with the currently open Excel workbook.</para>
		/// </value>
		/// <remarks>
		///   <para>You can use the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> and its properties to control the running instance of Excel more finely.</para>
		///   <para>The 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object is created when you call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(System.String)" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(System.String)" />
        public ExcelDriver Driver
        {
            get
            {
                return this.excelDriver;
            }
        }

		/// <summary>
		///   <para>Gets or sets the name of the file that contains the implementation of 
		/// the macros in the HPC Services for Excel macro framework that the Excel client should use.</para>
		/// </summary>
		/// <value>
		///   <para>A 
		/// 
		/// <see cref="System.String" /> that specifies the name of the file that contains the implementation of the macros that the Excel client should use.</para> 
		/// </value>
		/// <remarks>
		///   <para>When you use this property, you can include implementations of the macros in the HPC 
		/// Services for Excel macro framework in an Excel Add-In (.xlam) file or in a supporting workbook rather  
		/// than including them directly in the workbook that your users work with. You must implement these macros 
		/// in a module. Macros implemented for a worksheet, a class module, or the ThisWorkbook object are not supported.</para> 
		///   <para>You must set this property before you call the <see cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(System.String)" /> method. </para>
		///   <para>When you set this property, the client can use the macros in the specified file both on the computer on which the client runs and on the compute 
		/// nodes. The file in which you implement the macros must be open in the same instance of Excel as the workbook that runs the client when that workbook calls the  
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.Run(System.Boolean)" /> method. When you run calculations on the HPC cluster rather than on the local computer, the workbook must open the file that the  
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.MacroResource" /> property specifies on the compute nodes that perform the calculation. You can specify that the workbook that runs the client should open that file by implementing the  
		/// <see href="http://go.microsoft.com/fwlink/?LinkId=218969">Workbook_Open</see> macro for the ThisWorkbook object in that workbook, and calling the 
		/// <see href="http://go.microsoft.com/fwlink/?LinkId=218970">Workbooks.Open</see> method with the path to the file that implements the 
		/// macros in the HPC Services for Excel macro framework within the Workbook_Open macro.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(System.String)" />
        public string MacroResource
        {
            get
            {
                return this.macroResourceName;
            }

            set
            {
                // Only allow the macro resource name to be specified prior to initialization to ensure a single resource is used throughout.
                if (this.initialized)
                {
                    throw new InvalidOperationException(Resources.ExcelClient_MacroResourcePostInit);
                }

                this.macroResourceName = value;
            }
        }

        private Dictionary<string, string> azureFiles;

        /// <summary>
        ///   <para>Get the depending Azure files.</para>
        /// </summary>
        /// <value>
        ///   <para>A dictionary that specifies the local depending file path and remote relative path.</para>
        /// </value>
        public Dictionary<string, string> DependFiles
        {
            get
            {
                if (this.azureFiles == null)
                {
                    this.azureFiles = new Dictionary<string, string>();
                }

                return this.azureFiles;
            }
        }


        /// <summary>
        /// Gets the input to use for macro invocation.
        /// </summary>
        internal object MacroInput
        {
            get
            {
                return this.macroInput;
            }
        }

        /// <summary>
        /// Sets the output from a macro invocation
        /// </summary>
        internal object MacroOutput
        {
            set
            {
                this.macroOutput = value;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_initialize
        /// </summary>
        internal string InitializeMacro
        {
            get
            {
                return this.initializeMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_partition
        /// </summary>
        internal string PartitionMacro
        {
            get
            {
                return this.partitionMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_execute
        /// </summary>
        internal string ExecuteMacro
        {
            get
            {
                return this.executeMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_merge
        /// </summary>
        internal string MergeMacro
        {
            get
            {
                return this.mergeMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_finalize
        /// </summary>
        internal string FinalizeMacro
        {
            get
            {
                return this.finalizeMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_executionerror
        /// </summary>
        internal string ErrorMacro
        {
            get
            {
                return this.errorMacro;
            }
        }

        /// <summary>
        /// Gets the macro to use for hpc_getversion
        /// </summary>
        internal string VersionMacro
        {
            get
            {
                return this.versionMacro;
            }
        }

        /// <summary>
        /// Gets or sets the InvocationHelper to use when running within Excel.
        /// </summary>
        internal InvocationHelper MacroHelper
        {
            get
            {
                return this.invocationHelper;
            }

            set
            {
                this.invocationHelper = value;
            }
        }

        /// <summary>
        /// Initialize the client with a workbook.
        /// </summary>
        /// <param name="excelWorkbook">Excel Workbook to attach to</param>
        public void Initialize(Workbook excelWorkbook)
        {
            // Prevent calling Initialize twice
            lock (this.initializeLock)
            {
                if (this.initialized)
                {
                    Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientInitializeOnce);
                    throw new InvalidOperationException(Resources.ExcelClientInitializeOnce);
                }

                // Assign an ExcelDriver instance
                this.excelDriver = new ExcelDriver(excelWorkbook);
                this.localWorkbookPath = excelWorkbook.FullName;
                Tracing.WriteDebugTextVerbose(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_Initialization, this.localWorkbookPath);

                // Build macro names
                this.BuildMacroNames();

                // Check workbook version
                this.VerifyWorkbookVersion();

                // Get thread ID for UI thread and current thread
                IntPtr hwndPtr = (IntPtr)this.Driver.App.Hwnd;
                uint uiThreadID = NativeMethods.GetWindowThreadProcessId(hwndPtr, IntPtr.Zero);
                uint currentThreadID = NativeMethods.GetCurrentThreadId();

                // Use invocation helper if running from within Excel UI thread
                if (uiThreadID == currentThreadID)
                {
                    this.invocationHelper = new InvocationHelper();
                    this.invocationHelper.Initialize(this);
                }

                this.initialized = true;
            }
        }

		/// <summary>
		///   <para>Initializes the Excel SOA client by creating a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> class to interact with the Excel workbook located at the specified path.</para>
		/// </summary>
		/// <param name="localWorkbookPath">
		///   <para>String that specifies the path on the local computer of the 
		/// Excel workbook for which the Excel SOA client should submit calculations to the HPC cluster.</para>
		/// </param>
		/// <remarks>
		///   <para>To use an 
		/// <see cref="Microsoft.Office.Interop.Excel.Workbook" /> interface to specify the Excel workbook that the Excel SOA client should use, call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(Microsoft.Office.Interop.Excel.Workbook)" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(Microsoft.Office.Interop.Excel.Workbook)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver" />
        public void Initialize(string localWorkbookPath)
        {
            // Prevent calling Initialize twice
            lock (this.initializeLock)
            {
                if (this.initialized)
                {
                    Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientInitializeOnce);
                    throw new InvalidOperationException(Resources.ExcelClientInitializeOnce);
                }

                // Create an ExcelDriver instance
                this.excelDriver = new ExcelDriver();
                this.excelDriver.OpenWorkbook(localWorkbookPath);
                this.excelDriver.App.EnableCancelKey = XlEnableCancelKey.xlErrorHandler;
                this.localWorkbookPath = localWorkbookPath;

                Tracing.WriteDebugTextVerbose(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_Initialization, this.localWorkbookPath);

                // Build macro names
                this.BuildMacroNames();

                // Check workbook version
                this.VerifyWorkbookVersion();

                this.initialized = true;
            }
        }

        /// <summary>
        /// Initializes session parameters and opens session. Only required for cluster computation.
        /// Closes previously opened sessions if not already closed.
        /// </summary>
        /// <param name="headNode">name of cluster head node</param>
        /// <param name="minResources">minimum number of resources</param>
        /// <param name="maxResources">maximum number of resources</param>
        /// <param name="resourceType">Type of resource(core, node, or socket)</param>
        /// <param name="remoteWorkbookPath">Compute node relative path to workbook</param>
        /// <returns> ID of session that was opened </returns>
        public int OpenSession(string headNode, int minResources, int maxResources, SessionUnitType resourceType, string remoteWorkbookPath)
        {
            return this.OpenSession(InitSession(resourceType, minResources, maxResources, headNode), remoteWorkbookPath);
        }

        /// <summary>
        /// Initializes session parameters and opens session. Only required for cluster computation. 
        /// Closes previously opened sessions if not already closed.
        /// </summary>
        /// <param name="startInfo">Cluster start information</param>
        /// <param name="remoteWorkbookPath">Compute node relative path to workbook</param>
        /// <returns> ID of session that was opened </returns>
        public int OpenSession(SessionStartInfo startInfo, string remoteWorkbookPath)
        {
            // Check if workbook path has been provided (ExcelClient initialize called)
            if (this.initialized != true)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientOpenSessionBeforeInit);
                throw new InvalidOperationException(Resources.ExcelClientOpenSessionBeforeInit);
            }

            // Ensure that OpenSession cannot be called while workbook is running
            lock (this.runningLock)
            {
                if (this.alreadyRunning)
                {
                    string message = string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_OpenSessionDuringRun, FINALIZEMACRO);
                    Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, message);
                    throw new InvalidOperationException(message);
                }

                // Ensure thread safety between opensession and closesession
                lock (this.sessionLock)
                {
                    if (this.session != null)
                    {
                        Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_OpenSessionTwice);
                        throw new InvalidOperationException(Resources.ExcelClient_OpenSessionTwice);
                    }

                    // Assign remoteWorkbook path
                    if (remoteWorkbookPath != null && string.IsNullOrEmpty(this.remoteWorkbookPath))
                    {
                        this.remoteWorkbookPath = remoteWorkbookPath;
                    }
                    else
                    {
                        this.remoteWorkbookPath = this.localWorkbookPath;
                    }

                    // Assign remoteWorkbook path
                    if (this.DependFiles != null && this.DependFiles.Count > 0)
                    {
                        startInfo.DependFiles = this.DependFiles;
                        foreach (string srcfile in this.DependFiles.Keys)
                        {
                            if (this.remoteWorkbookPath.Equals(this.DependFiles[srcfile], StringComparison.InvariantCultureIgnoreCase))
                            {
                                this.remoteWorkbookPath = string.Format("%{0}%\\{1}", SoaDataJobDirEnvVar, this.remoteWorkbookPath);
                                break;
                            }
                        }
                    }

                    // Set environment variable for session
                    startInfo.Environments.Add(WORKBOOKENVVAR, this.remoteWorkbookPath);

                    this.headNode = startInfo.Headnode;

                    try
                    {
                        // Session configuration and creation
                        this.session =
#if net40
                            TaskEx.Run(
#else
                            Task.Run(
#endif
                                () => Session.CreateSession(startInfo)).GetAwaiter().GetResult();
                        this.scheme = startInfo.TransportScheme;
                        // In future versions, we need to check the returned service version to ensure that we use the correct APIs.
                        // For example, if we add a service operation in Vnext, and the service version is still 1.0, we should only
                        // use the 1.0 service operations. Check 'this.session.ServerVersion' for this.
                        this.sessionOpen = true;

                        Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_OpenSession, this.session.Id, this.headNode);
                    }
                    catch (Exception ex)
                    {
                        Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelClient, ex.ToString(), delegate { Tracing.EventProvider.LogExcelClient_OpenSessionError(SERVICE, this.headNode, ex.ToString()); });
                        throw;
                    }
                }
            }

            return this.session.Id;
        }

		/// <summary>
		///   <para>Closes the session that you opened by calling the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method subject to a default timeout period, and closes all instances of Excel that are running on the compute nodes.</para> 
		/// </summary>
		/// <remarks>
		///   <para>The default timeout period is 120,000 milliseconds (2 minutes). You may need to specify a longer timeout period if you are 
		/// running the session on a cluster with high network congestion. To specify the length of the timeout period when closing a session, call the  
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.CloseSession(System.Int32)" /> method instead.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.CloseSession(System.Int32)" />
        public void CloseSession()
        {
            // Timeout not specified, so set timeoutSpecified to false. Time doesn't matter.
            this.CloseSessionInternal(false, 0);
        }

		/// <summary>
		///   <para>Closes the session that you opened by calling the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method subject to the specified timeout period, and closes all instances of Excel that are running on the compute nodes.</para> 
		/// </summary>
		/// <param name="timeoutMilliseconds">
		///   <para>An integer that specifies the length of time in milliseconds that the method should wait for the SOA session to close.</para>
		/// </param>
		/// <remarks>
		///   <para>To use the default timeout period of 120,000 milliseconds (2 minutes), you can use the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.CloseSession" /> method. </para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.CloseSession" />
		/// <seealso cref="Microsoft.Hpc.Scheduler.Session.Session.Close(System.Boolean,System.Int32)" />
        public void CloseSession(int timeoutMilliseconds)
        {
            // Timeout specified, so set timeoutSpecified to true. Use specified timeout.
            this.CloseSessionInternal(true, timeoutMilliseconds);
        }

		/// <summary>
		///   <para>Releases all of the resources that the object allocated.</para>
		/// </summary>
		/// <remarks>
		///   <para>This method does not close or release resources for open instances of Excel.</para>
		/// </remarks>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

		/// <summary>
		///   <para>Starts a calculation asynchronously for the Excel workbook using the partition, calculate, and merge model that Microsoft HPC Pack Services 
		/// for Excel uses to run calculations. The calculation runs on the HPC cluster with the head node that you specified when you called the  
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method, unless you specify that the calculation should run on the local computer.</para> 
		/// </summary>
		/// <param name="executeLocally">
		///   <para>A Boolean that indicates whether to run the calculation for the Excel workbook on the local computer. 
		/// 
		/// True indicates that the calculation should run on the local computer. 
		/// False indicates that the calculation should run on the HPC cluster with the head node that you specified when you called the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method, using the partition, calculate, and merge model that Microsoft HPC Pack Services for Excel uses to run calculations on an HPC cluster.</para> 
		/// </param>
		/// <param name="userCallback">
		///   <para>An 
		/// <see cref="System.AsyncCallback" /> delegate that references a callback method to call when the asynchronous calculation completes. Specify 
		/// null (Nothing in Visual Basic) if you do not want a method called when the calculation completes.</para>
		/// </param>
		/// <param name="userStateInfo">
		///   <para>A user-defined 
		/// 
		/// <see cref="System.Object" /> that contains application-specific state information that you want to pass to the callback method called when the calculation completes.</para> 
		/// </param>
		/// <returns>
		///   <para>An 
		/// <see cref="System.IAsyncResult" /> interface to an object that contains information about the status of the asynchronous calculation.</para>
		/// </returns>
		/// <remarks>
		///   <para>To start a synchronous calculation, use the <see cref="Microsoft.Hpc.Excel.ExcelClient.Run(System.Boolean)" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Run(System.Boolean)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" />
		/// <seealso cref="System.IAsyncResult" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.EndRun(System.IAsyncResult)" />
		/// <seealso cref="System.AsyncCallback" />
        public IAsyncResult BeginRun(bool executeLocally, AsyncCallback userCallback, object userStateInfo)
        {
            this.runAction = this.Run;
            return this.runAction.BeginInvoke(executeLocally, userCallback, userStateInfo);
        }

		/// <summary>
		///   <para>Ends a asynchronous calculation from an Excel workbook on an HPC cluster that you began by calling the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.BeginRun(System.Boolean,System.AsyncCallback,System.Object)" /> method.</para>
		/// </summary>
		/// <param name="asyncResult">
		///   <para>An 
		/// <see cref="System.IAsyncResult" /> interface to an object that contains information about the status of the asynchronous calculation.</para>
		/// </param>
		/// <remarks>
		///   <para>If you call this method outside of the callback method that you specified when you called the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.BeginRun(System.Boolean,System.AsyncCallback,System.Object)" /> method, the calling method is blocked from continuing to run until the calculation completes.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.BeginRun(System.Boolean,System.AsyncCallback,System.Object)" />
		/// <seealso cref="System.IAsyncResult" />
        public void EndRun(IAsyncResult asyncResult)
        {
            this.runAction.EndInvoke(asyncResult);
        }

		/// <summary>
		///   <para>Starts the calculation for the Excel workbook using the partition, calculate, and merge model that Microsoft HPC Pack Services for 
		/// Excel uses to run calculations. The calculation runs on the HPC cluster with the head node that you specified when you called the  
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method, unless you specify that the calculation should run on the local computer.</para> 
		/// </summary>
		/// <param name="executeLocally">
		///   <para>A Boolean that indicates whether to run the calculation for the Excel workbook on the local computer. 
		/// 
		/// True indicates that the calculation should run on the local computer. 
		/// False indicates that the calculation should run on the HPC cluster with the head node that you specified when you called the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method, using the partition, calculate, and merge model that Microsoft HPC Pack Services for Excel uses to run calculations on an HPC cluster.</para> 
		/// </param>
		/// <remarks>
		///   <para>You should call the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" /> method before you call the  
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelClient.Run(System.Boolean)" /> method if you want to run the calculations in the workbook on an HPC cluster.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Initialize(System.String)" />
        public void Run(bool executeLocally)
        {
            // Check if workbook path has been provided (ExcelClient initialize called)
            if (this.initialized != true)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_RunBeforeInitialize);
                throw new InvalidOperationException(Resources.ExcelClient_RunBeforeInitialize);
            }

            // Check if Run is already executing in another thread.
            lock (this.runningLock)
            {
                if (!this.alreadyRunning)
                {
                    this.alreadyRunning = true;
                }
                else
                {
                    Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientMultipleRuns);
                    throw new InvalidOperationException(Resources.ExcelClientMultipleRuns);
                }

                this.cancelled = false;
                this.responseException = false;
                try
                {
                    if (executeLocally)
                    {
                        Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_RunLocal, this.localWorkbookPath);
                        this.CalculateLocal();
                    }
                    else
                    {
                        // Check for valid parameters
                        if (string.IsNullOrEmpty(this.remoteWorkbookPath))
                        {
                            Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientWBPathNull);
                            throw new InvalidOperationException(Resources.ExcelClientWBPathNull);
                        }

                        // Client creation
                        lock (this.sessionLock)
                        {
                            if (this.sessionOpen)
                            {
                                try
                                {
                                    this.OpenClient();
                                    this.client.SetResponseHandler<CalculateResponse>(this.ResponseHandler);
                                }
                                catch
                                {
                                    // Rethrow the exception if not cancelled
                                    if (!this.cancelled)
                                    {
                                        throw;
                                    }
                                }
                            }
                            else
                            {
                                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientSessionNotOpen);
                                throw new InvalidOperationException(Resources.ExcelClientSessionNotOpen);
                            }
                        }

                        // Send requests only if not cancelled
                        if (!this.cancelled)
                        {
                            Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_RunOnCluster, this.headNode, this.localWorkbookPath);
                            this.CalculateCluster();
                        }

                        // Throw exception during sending or receiving SOA messages
                        if (this.responseException)
                        {
                            throw new ExcelClientException(Resources.ExcelClient_ErrorOnResponse);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelClient, ex.ToString(), delegate { Tracing.EventProvider.LogExcelClient_RunError(this.localWorkbookPath, ex.ToString()); });

                    // Try to tell the workbook about the error
                    try
                    {
                        this.InvokeMacro(ERRORMACRO, ex);
                    }
                    catch
                    {
                        Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_MacroFailed, this.errorMacro);
                    }

                    // Rethrow the exception to tell any .Net Client about the error
                    throw;
                }
                finally
                {
                    // Make sure that the response handler knows that we're closing the client
                    // and any additional responses don't need to be handled including the 
                    // client is purged sessionexception
                    this.cancelled = true;

                    // if client used and not cancelled
                    if (this.client != null)
                    {
                        this.CloseClient();
                    }

                    try
                    {
                        // Make sure that finalize does not run right before another merge in the response handler
                        // This will be done by taking the response lock around the invokemacro call. This along with 
                        // setting the cancelled flag before calling CloseClient will ensure that no additional merge
                        // macros can run.
                        lock (this.responseLock)
                        {
                            // Finally, call Finalize Macro. Do not fail, as it is an optional callback. 
                            this.InvokeMacro(FINALIZEMACRO);
                        }
                    }
                    catch
                    {
                        Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_MacroFailed, this.finalizeMacro);
                    }

                    // allow another run
                    this.alreadyRunning = false;

                }
            } // lock running run
        }

		/// <summary>
		///   <para>Cancels any currently running Excel calculations without closing the SOA session.</para>
		/// </summary>
		/// <remarks>
		///   <para>When you call this method, the Excel SOA client stops sending requests, directs the 
		/// broker to stop processing requests that are already queued, and closes the connection to the HPC  
		/// cluster. If you run a calculation again after canceling the calculation, the client uses the same 
		/// SOA session that the client originally used for the canceled calculation as long as the session is valid.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.Run(System.Boolean)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.OpenSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo,System.String)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelClient.BeginRun(System.Boolean,System.AsyncCallback,System.Object)" />
        public void Cancel()
        {
            lock (this.clientLock)
            {
                this.cancelled = true;
                if (this.client != null)
                {
                    try
                    {
                        this.client.Close(true);
                    }
                    catch (Exception ex)
                    {
                        Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelClient, ex.ToString(), delegate { Tracing.EventProvider.LogExcelClient_CancelError(ex.ToString(), this.session.Id); });
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Remove web api prefix from head node name
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <returns>true if specified prefix is removed from head node name; false, otherwise</returns>
        internal static bool TryRemoveWebApiPrefix(ref string headNode)
        {
            if (headNode != null && Uri.IsWellFormedUriString(headNode, UriKind.Absolute))
            {
                Uri uri = new Uri(headNode);
                if (uri.Scheme == Uri.UriSchemeHttps)
                {
                    headNode = uri.Host;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///   <para>Disposes of COM object references. Leaves Excel Application open</para>
        /// </summary>
        /// <param name="disposing">
        ///   <para>Flag set true on dispose call</para>
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Close the session
                    if (this.session != null)
                    {
                        this.CloseSession();
                    }
                }
                catch
                {
                    // Exception logged in CloseSession. Should continue cleaning up regardless.
                }

                if (this.excelDriver != null)
                {
                    this.excelDriver.Dispose();
                    this.excelDriver = null;

                    // Suggested by Excel Dev team as solution preventing Excel process from staying after closing

                    // Force garbage collection of COM objects. 
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Force garbage collection of COM object members
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                // Dispose other disposable types
                try
                {
                    this.done.Close();
                }
                catch (Exception ex)
                {
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, ex.ToString());
                }

                if (this.client != null)
                {
                    this.client.Dispose();
                }

                this.invocationHelper?.Dispose();
            }
        }

        /// <summary>
        /// Initialize NetTcpBinding object with appropriate defaults
        /// </summary>
        /// <returns>
        /// NetTcpBinding for use in Client creation
        /// </returns>
        private static NetTcpBinding InitBinding()
        {
            // Recommended Default Values
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.Transport, false);
            binding.MaxBufferSize = 102400;
            binding.MaxReceivedMessageSize = 102400;
            binding.MaxBufferPoolSize = 102400;
            binding.ReaderQuotas.MaxArrayLength = 102400;
            binding.ReaderQuotas.MaxBytesPerRead = 102400;

            return binding;
        }

        /// <summary>
        /// Initialize SessionStartInfo object with supplied values and defaults
        /// </summary>
        /// <param name="type">Job unit type (Cores, nodes, etc)</param>
        /// <param name="min">minimum number of job units</param>
        /// <param name="max">maximum number of job units</param>
        /// <param name="headNode">head node name</param>
        /// <returns>SessionStartInfo for use in Session creation</returns>
        private static SessionStartInfo InitSession(SessionUnitType type, int min, int max, string headNode)
        {
            // Use ExcelService with supplied min/max cores
            TransportScheme scheme = TryRemoveWebApiPrefix(ref headNode) ? TransportScheme.WebAPI : TransportScheme.NetTcp;
            SessionStartInfo info = new SessionStartInfo(headNode, SERVICE);
            info.SessionResourceUnitType = type;
            info.MinimumUnits = min;
            info.MaximumUnits = max;
            info.Secure = true;
            info.ShareSession = true;
            info.TransportScheme = scheme;

            return info;
        }

        /// <summary>
        /// Tests if exception means that UI is busy. 
        /// </summary>
        /// <param name="ex">Exception returned from Excel</param>
        /// <returns>Flag indicating UI busy (true) or not (false)</returns>
        private static bool TestForBusyUI(TargetInvocationException ex)
        {
            bool uiBusy = false;

            // Check for innerexception on innerexception. 
            // Busy comexceptions come back packaged in a targetinvocationexception inside a targetinvocationexception
            if (ex.InnerException != null ? ex.InnerException.InnerException != null : false)
            {
                // If this inner exception exists, check if it is a com exception so we can get the error code.
                if (ex.InnerException.InnerException.GetType().Equals(typeof(COMException)))
                {
                    COMException innerCOMEx = (COMException)ex.InnerException.InnerException;

                    // Check the error code to determine if the UI is busy or if there is some other error
                    // 0x800AC472 indicates that the host (Excel's UI) is blocked.
                    if ((uint)innerCOMEx.ErrorCode == 0x800AC472)
                    {
                        // Return true if error code indicates the excel is busy
                        uiBusy = true;
                    }
                }
            }

            return uiBusy;
        }

        /// <summary>
        /// Helper method to invoke a macro in the ExcelClient framework
        /// </summary>
        /// <param name="macroName">Name of macro to invoke</param>
        /// <param name="input">Inputs to the macro</param>
        /// <returns>Return value of macro or null if don't care.</returns>
        private object InvokeMacro(string macroName, params object[] input)
        {
            bool uiBusy;
            object result = null;

            // Try to call the specified macro until the Excel UI is not busy.
            do
            {
                uiBusy = false;
                try
                {
                    // If the invocation helper has been created use it, otherwise invoke directly.
                    if (this.invocationHelper != null)
                    {
                        lock (this.invocationLock)
                        {
                            switch (macroName)
                            {
                                case ERRORMACRO:
                                    // Exception is input to the macro
                                    this.macroInput = input[0];
                                    this.invocationHelper.InvokeError();
                                    break;
                                case EXECUTEMACRO:
                                    // Result from partition will be input to macro
                                    this.macroInput = input[0];
                                    this.invocationHelper.InvokeExecute();

                                    // Result from execute will be used for merge
                                    result = this.macroOutput;
                                    break;
                                case FINALIZEMACRO:
                                    // No inputs or outputs
                                    this.invocationHelper.InvokeFinalize();
                                    break;
                                case VERSIONMACRO:
                                    // Record output to test for valid version
                                    this.invocationHelper.InvokeGetVersion();
                                    result = this.macroOutput;
                                    break;
                                case INITIALIZEMACRO:
                                    // No inputs or outputs
                                    this.invocationHelper.InvokeInitialize();
                                    break;
                                case MERGEMACRO:
                                    // Use result from execute (local or from response) as input
                                    this.macroInput = input[0];
                                    this.invocationHelper.InvokeMerge();
                                    break;
                                case PARTITIONMACRO:
                                    // Record output to send to cluster or Execute
                                    this.invocationHelper.InvokePartition();
                                    result = this.macroOutput;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        switch (macroName)
                        {
                            case ERRORMACRO:
                                // Write the error message and string dump to Excel
                                Exception ex = (Exception)input[0];
                                this.Driver.RunMacro(this.errorMacro, ex.Message, ex.ToString());
                                break;
                            case EXECUTEMACRO:
                                // Use input from partition macro
                                result = this.excelDriver.RunMacro(this.executeMacro, input);
                                break;
                            case FINALIZEMACRO:
                                // No inputs or outputs
                                this.excelDriver.RunMacro(this.finalizeMacro);
                                break;
                            case VERSIONMACRO:
                                // Record output to test for valid version
                                result = this.excelDriver.RunMacro(this.versionMacro);
                                break;
                            case INITIALIZEMACRO:
                                // No inputs or outputs
                                this.excelDriver.RunMacro(this.initializeMacro);
                                break;
                            case MERGEMACRO:
                                // Use input from execute
                                this.macroInput = input[0];
                                this.excelDriver.RunMacro(this.mergeMacro, input);
                                break;
                            case PARTITIONMACRO:
                                // Record output for execute
                                result = this.excelDriver.RunMacro(this.partitionMacro);
                                break;
                        }
                    }
                }
                catch (TargetInvocationException ex)
                {
                    // Test exception for indication that the UI is busy. If it isn't busy then there is a real error.
                    uiBusy = TestForBusyUI(ex);
                    if (!uiBusy)
                    {
                        throw;
                    }
                }
            }
            while (uiBusy);

            return result;
        }

        /// <summary>
        /// Verifies that a session is open and then attempts to close it using the supplied timeout if specified.
        /// </summary>
        /// <param name="timeoutSpecified">Flag to note whether a timeout has been specified</param>
        /// <param name="timeoutMilliseconds">Timeout for close operation in milliseconds. Only used if timeoutSpecified is true. </param>
        private void CloseSessionInternal(bool timeoutSpecified, int timeoutMilliseconds)
        {
            lock (this.sessionLock)
            {
                // Check if OpenSession has been called by testing this.session
                if (this.session != null)
                {
                    try
                    {
                        // If timeout is specified, use it. Otherwise, use a default of 2 minutes.
                        if (timeoutSpecified)
                        {
                            this.session.Close(true, timeoutMilliseconds);
                        }
                        else
                        {
                            this.session.Close(true, DEFAULTCLOSETIMEOUT);
                        }
                    }
                    catch (Exception ex)
                    {
                        Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelClient, ex.ToString(), delegate { Tracing.EventProvider.LogExcelClient_CloseSessionError(ex.ToString()); });
                        throw;
                    }

                    // Set flags for session status
                    this.sessionOpen = false;
                    this.session = null;
                }
                else
                {
                    Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, Resources.ExcelClientCloseBeforeOpen);
                    throw new InvalidOperationException(Resources.ExcelClientCloseBeforeOpen);
                }
            }
        }

        /// <summary>
        /// Method that verifies the version of the workbook attached to this instance of ExcelClient matches the supported versions
        /// </summary>
        private void VerifyWorkbookVersion()
        {
            // Get the workbook version
            object versionObj = null;
            try
            {
                versionObj = this.InvokeMacro(VERSIONMACRO);
            }
            catch (TargetInvocationException ex)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_GetVersionMissing, this.versionMacro), ex);
            }

            // Versions returned - only used in case of error.
            string versionsSpecified = string.Empty;
            string currentElement = string.Empty;

            // Check if version is returned and try to parse it.
            bool compatible = false;
            try
            {
                // Check for null and fail in that case
                if (versionObj == null ? true : versionObj.Equals(System.DBNull.Value))
                {
                    throw new ArgumentNullException(VERSIONMACRO);
                }

                if (versionObj.GetType().Equals(typeof(object[])))
                {
                    object[] supportedVersions = (object[])versionObj;
                    foreach (object supportedVersion in supportedVersions)
                    {
                        // Ignore missing/null elements
                        if (supportedVersion != System.Reflection.Missing.Value && supportedVersion != System.DBNull.Value && (supportedVersion != null ? !string.IsNullOrEmpty(supportedVersion.ToString()) : false))
                        {
                            currentElement = string.Format(CultureInfo.CurrentCulture, " \"{0}\"", supportedVersion.ToString());
                            Version workbookVersion = new Version(supportedVersion.ToString());

                            // If this is the first supported version, start the list
                            if (string.IsNullOrEmpty(versionsSpecified))
                            {
                                versionsSpecified = workbookVersion.ToString(2);
                            }
                            else
                            {
                                // Append each subsequent version
                                versionsSpecified = versionsSpecified + ", " + workbookVersion.ToString(2);
                            }

                            // Ensure that the build and revision numbers are not set before checking compatibility
                            if (workbookVersion.Build == -1 && workbookVersion.Revision == -1)
                            {
                                // Compare versions up to 2 significant digits. I expect we'll only use one, but this gives us flexibility
                                if (workbookVersion.ToString(2).Equals(MACROSPECVERSION))
                                {
                                    // If workbook is supported
                                    compatible = true;
                                }
                            }
                        }
                    }
                }
                else if (versionObj.GetType().Equals(typeof(string[])))
                {
                    string[] supportedVersions = (string[])versionObj;
                    foreach (string supportedVersion in supportedVersions)
                    {
                        // Ignore missing/null elements
                        if (!string.IsNullOrEmpty(supportedVersion))
                        {
                            currentElement = string.Format(CultureInfo.CurrentCulture, " \"{0}\"", supportedVersion);
                            Version workbookVersion = new Version(supportedVersion);

                            // If this is the first supported version, start the list
                            if (string.IsNullOrEmpty(versionsSpecified))
                            {
                                versionsSpecified = workbookVersion.ToString(2);
                            }
                            else
                            {
                                // Append each subsequent version
                                versionsSpecified = versionsSpecified + ", " + workbookVersion.ToString(2);
                            }

                            // Ensure that the build and revision numbers are not set before checking compatibility
                            if (workbookVersion.Build == -1 && workbookVersion.Revision == -1)
                            {
                                // Compare versions up to 2 significant digits. I expect we'll only use one, but this gives us flexibility
                                if (workbookVersion.ToString(2).Equals(MACROSPECVERSION))
                                {
                                    // If workbook is supported
                                    compatible = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    currentElement = string.Format(CultureInfo.CurrentCulture, " \"{0}\"", versionObj.ToString());
                    Version workbookVersion = new Version(versionObj.ToString());
                    versionsSpecified = workbookVersion.ToString(2);

                    // Compare versions up to 2 significant digits. I expect we'll only use one, but this gives us flexibility
                    if (workbookVersion.ToString(2).Equals(MACROSPECVERSION))
                    {
                        // If workbook is supported
                        compatible = true;
                    }
                }

                // If an empty version string or empty list of version strings is returned, fail
                if (versionsSpecified.Equals(string.Empty))
                {
                    throw new ArgumentException(VERSIONMACRO);
                }
            }
            catch (Exception ex)
            {
                throw new ExcelClientException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_InvalidVersionString, VERSIONMACRO) + currentElement, ex);
            }

            // If workbook is not compatible with any specified version, fail.
            if (!compatible)
            {
                throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelClient_VersionMismatch, VERSIONMACRO, versionsSpecified, MACROSPECVERSION));
            }
        }

        /// <summary>
        /// Perform all calculations locally. Does not use response handler, just waits for macros to complete.
        /// </summary>
        private void CalculateLocal()
        {
            try
            {
                this.InvokeMacro(INITIALIZEMACRO);
            }
            catch
            {
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_MacroFailed, this.initializeMacro);
            }

            // Run partition macro on local workbook. Continue looping partition/execute/merge until empty array
            // is returned (appears as null) or the user cancels the job.
            object partitionResult = this.InvokeMacro(PARTITIONMACRO);

            while ((partitionResult == null ? false : !partitionResult.Equals(System.DBNull.Value)) && this.cancelled == false)
            {
                // Test serialized message length to generate same error when executing locally as when using SOA on cluster
                WorkItem workItem = new WorkItem();
                workItem.Insert(0, partitionResult);
                byte[] serializedWI = WorkItem.Serialize(workItem);

                // Execute locally and pass execution result to merge macro
                object execResult = this.InvokeMacro(EXECUTEMACRO, partitionResult);

                // Test serialized result length to generate same error when executing locally as when using SOA on cluster
                workItem = new WorkItem();
                workItem.Insert(0, execResult);
                serializedWI = WorkItem.Serialize(workItem);

                this.InvokeMacro(MERGEMACRO, execResult);

                // Call partition again for next loop
                partitionResult = this.InvokeMacro(PARTITIONMACRO);
            }
        } // Calculate

        /// <summary>
        /// Creates client for open session
        /// </summary>
        private void OpenClient()
        {
            // Client configuration and creation
            NetTcpBinding binding = InitBinding();
            lock (this.clientLock)
            {
                if (!this.cancelled)
                {
                    Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_OpenClient, this.session.Id, this.headNode);
                    if (this.scheme == TransportScheme.NetTcp)
                    {
                        this.client = new BrokerClient<IExcelService>(System.Guid.NewGuid().ToString(), this.session, binding);
                    }
                    else
                    {
                        this.client = new BrokerClient<IExcelService>(System.Guid.NewGuid().ToString(), this.session);
                    }
                    this.done.Reset();
                }
            }
        }

        /// <summary>
        /// Closes Open Client
        /// </summary>
        private void CloseClient()
        {
            // Client configuration and creation
            lock (this.clientLock)
            {
                this.client.Close();
            }
        }

        /// <summary>
        /// Perform full split/calculate/reduce on cluster
        /// </summary>
        private void CalculateCluster()
        {
            // Result Initialization
            bool nullOnFirstPartition = false;

            // Build macro name based on remote workbook name and EXECUTEMACRO unless alternative name specified
            string clusterExecuteMacroName;
            if (string.IsNullOrEmpty(this.macroResourceName))
            {
                clusterExecuteMacroName = "'" + Path.GetFileName(this.remoteWorkbookPath) + "'!" + EXECUTEMACRO;
            }
            else
            {
                clusterExecuteMacroName = "'" + this.macroResourceName + "'!" + EXECUTEMACRO;
            }

            // Call Initialize Macro first, but it is optional, so don't fail if it dne.
            try
            {
                // Try to call the initialize macro until the Excel UI is not busy.
                this.InvokeMacro(INITIALIZEMACRO);
            }
            catch
            {
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_MacroFailed, this.initializeMacro);
            }

            // Run partition macro on local workbook. Continue looping partition/execute/merge until empty array
            // is returned (appears as null) or the user cancels the job.
            object partitionResult = this.InvokeMacro(PARTITIONMACRO);

            // If Partition returns null immediately, report as error. There is no valid use case for this.
            if (!(partitionResult == null ? false : !partitionResult.Equals(System.DBNull.Value)))
            {
                nullOnFirstPartition = true;
            }

            while ((partitionResult == null ? false : !partitionResult.Equals(System.DBNull.Value)) && this.cancelled == false)
            {
                // Build HPCExcelWorkItem to send serialized work item to SOA service
                WorkItem workItem = new WorkItem();
                workItem.Insert(0, partitionResult);

                // Test serialized message size to determine if it fits within the set constraint
                byte[] serializedWI = WorkItem.Serialize(workItem);

                // Generate new Calculate Request
                CalculateRequest myReq = new CalculateRequest(clusterExecuteMacroName, serializedWI, null);

                // Ensure session has not been closed
                lock (this.sessionLock)
                {
                    if (this.sessionOpen)
                    {
                        try
                        {
                            if (!this.cancelled)
                            {
                                this.client.SendRequest<CalculateRequest>(myReq);
                            }
                            else
                            {
                                return;
                            }
                        }
                        catch
                        {
                            if (this.cancelled)
                            {
                                // Stop sending requests if cancelled
                                return;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                    else
                    {
                        // Throw if session is closed
                        throw new InvalidOperationException(Resources.ExcelClientSessionNotOpen);
                    }
                }

                // Call partition again for next loop
                partitionResult = this.InvokeMacro(PARTITIONMACRO);
            } // for

            if (!nullOnFirstPartition)
            {
                // Finish Batch of CalculateRequests if there were any
                try
                {
                    this.client.EndRequests();
                }
                catch
                {
                    // Throw exception if brokerclient is not cancelled
                    if (!this.cancelled)
                    {
                        throw;
                    }
                }

                // Wait for all requests to be processed or an error to occur
                this.done.WaitOne();
            }
        }

        /// <summary>
        /// Callback handling responses from CalculateRequests
        /// </summary>
        /// <param name="response">
        /// CalculateResponse received from ExcelService Calculate
        /// </param>
        private void ResponseHandler(BrokerResponse<CalculateResponse> response)
        {
            byte[] results;
            Exception userException = null;
            bool stopCondition = false;
            WorkItem calcResult = null;

            try
            {
                // Parse results
                results = response.Result.CalculateResult;
                calcResult = WorkItem.Deserialize(results);
            }
            catch (FaultException ex)
            {
                // Application exceptions are recoverable
                userException = ex;
                stopCondition = false;
            }
            catch (RetryOperationException ex)
            {
                // RetryOperationExceptions may or may not be recoverable
                // Therefore we have to default to a stop condition to avoid hangs
                userException = new ExcelClientException(ex.Reason, ex);
                stopCondition = true;
            }
            catch (TimeoutException ex)
            {
                // Timeout exceptions are unrecoverable
                userException = ex;
                stopCondition = true;
            }
            catch (SessionException ex)
            {
                if (SOAFaultCode.Broker_BrokerUnavailable == ex.ErrorCode)
                {
                    // If cancellation occured on job manager pane, report that.
                    userException = new ExcelClientException(Resources.ExcelClientJobCancelled, ex);
                }
                else if (SOAFaultCode.ClientPurged == ex.ErrorCode)
                {
                    // If cancellation occurred locally, then just let Run complete.
                    lock (this.responseLock)
                    {
                        this.done.Set();
                    }
                }
                else
                {
                    userException = ex;
                }

                // Session Exceptions are unrecoverable unless they are application errors
                if (SOAFaultCode.Category(ex.ErrorCode).Equals(SOAFaultCodeCategory.ApplicationError))
                {
                    stopCondition = false;
                }
                else
                {
                    stopCondition = true;
                }
            }
            catch (CommunicationException ex)
            {
                // Communication Exceptions are unrecoverable
                userException = ex;
                stopCondition = true;
            }
            catch (Exception ex)
            {
                // Any other exception should be treated as unrecoverable.
                userException = ex;
                stopCondition = true;
            }
            finally
            {
                lock (this.responseLock)
                {
                    // Fail silently if user has cancelled the Run or a stop condition error has already been received.
                    if (!this.cancelled)
                    {
                        // Stop condition errors should be written to the operations log, while other exceptions are just for debug
                        if (stopCondition)
                        {
                            Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelClient, userException.ToString(), delegate { Tracing.EventProvider.LogExcelClient_FatalResponse(userException.ToString(), this.session.Id); });
                        }
                        else
                        {
                            if (userException != null)
                            {
                                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelClient, userException.ToString());
                            }
                        }

                        // If Exception was caught, send it to user and workbook
                        if (userException != null)
                        {
                            this.CallResponseErrorHandlers(userException);
                        }
                        else
                        {
                            // If no errors, try to call Merge. Report any errors in merge to the caller.
                            try
                            {
                                this.InvokeMacro(MERGEMACRO, calcResult.Get<object>(0));
                            }
                            catch (Exception ex)
                            {
                                this.CallResponseErrorHandlers(ex);
                            }
                        }

                        // Run should return when a stop condition exception is handled or the last response has been received.
                        if (stopCondition || response.IsLastResponse)
                        {
                            this.cancelled = true;
                            this.done.Set();
                        }
                    }
                }
            }
        } // Response Handler

        /// <summary>
        /// Construct macro names using [workbookname]![macroname] for MDI support and allowing
        /// P/E/M macros to be implemented in separate resources
        /// </summary>
        private void BuildMacroNames()
        {
            string workbookNamePrefix;
            if (string.IsNullOrEmpty(this.macroResourceName))
            {
                // Use Workbook name to avoid MDI confusion locally if nothing explicitly set
                workbookNamePrefix = "'" + this.Driver.Workbook.Name + "'!";
            }
            else
            {
                // Otherwise, use provided resource prefix
                workbookNamePrefix = "'" + this.macroResourceName + "'!";
            }

            this.initializeMacro = workbookNamePrefix + INITIALIZEMACRO;
            this.partitionMacro = workbookNamePrefix + PARTITIONMACRO;
            this.executeMacro = workbookNamePrefix + EXECUTEMACRO;
            this.mergeMacro = workbookNamePrefix + MERGEMACRO;
            this.finalizeMacro = workbookNamePrefix + FINALIZEMACRO;
            this.versionMacro = workbookNamePrefix + VERSIONMACRO;
            this.errorMacro = workbookNamePrefix + ERRORMACRO;
        }

        /// <summary>
        /// Internal helper method that calls the user error handler (if it exists) and the ExecutionError macro
        /// </summary>
        /// <param name="ex"> Exception to communicate to workbook and user</param>
        private void CallResponseErrorHandlers(Exception ex)
        {
            // Let Run know that there has been an error in response handling
            this.responseException = true;

            // Event will be null if there are no subscribers
            if (this.ErrorHandler != null)
            {
                try
                {
                    this.ErrorHandler(this, new ResponseErrorEventArgs(ex));
                }
                catch
                {
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_ErrorHandlerMissing);
                }
            }

            // Try to call the error macro in the workbook
            try
            {
                this.InvokeMacro(ERRORMACRO, ex);
            }
            catch
            {
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_MacroFailed, this.errorMacro);
            }
        } // CallErrorHandlers
        
        private const string ExcelAssemblyName = "Microsoft.Hpc.Excel.dll";

        private const string HpcAssemblyDir = @"%CCP_HOME%bin";

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        private static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(args.Name))
            {
                return null;
            }

            // Microsoft.Hpc.Excel.dll is moved from GAC to %CCP_HOME%Bin
            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(ExcelAssemblyName), StringComparison.OrdinalIgnoreCase))
            {
                string assemblyPath = Path.Combine(Environment.ExpandEnvironmentVariables(HpcAssemblyDir), ExcelAssemblyName);
                if (!File.Exists(assemblyPath))
                {
                    // return the executing assembly if the file cannot be found by the assembly path
                    return Assembly.GetExecutingAssembly();
                }

                try
                {
                    return Assembly.LoadFrom(assemblyPath);
                }
                catch (Exception ex)
                {
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelClient, Resources.ExcelClient_FailedLoadAssembly, assemblyPath, ex);
                    return null;
                }
            }
            return null;
        }
    } // Engine Class
} // ExcelDriver Namespace
