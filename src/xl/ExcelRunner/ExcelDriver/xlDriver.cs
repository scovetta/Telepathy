//------------------------------------------------------------------------------
// <copyright file="xlDriver.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Excel interaction driver
// </summary>
//------------------------------------------------------------------------------

[assembly: System.Resources.NeutralResourcesLanguageAttribute("en-US")]
namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Threading;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Excel.Win32;
    using Microsoft.Office.Interop.Excel;
    
	/// <summary>
	///   <para>Provides a wrapper around an instance of Excel that simplifies starting Excel and interacting with 
	/// the component object model (COM) objects that represent items in Excel such as the application, workbook, and worksheets.</para>
	/// </summary>
    public class ExcelDriver : IDisposable
    {
        #region Private Members
        /// <summary>
        /// Number of seconds to wait for application to gain focus
        /// </summary>
        private readonly int FOCUSTIME = 4;

        /// <summary>
        /// Open workbook code to update links
        /// </summary>
        private readonly int DoUpdateLinks = 3;

        /// <summary>
        /// Open workbook code to not update links
        /// </summary>
        private readonly int DoNotUpdateLinks = 0;

        /// <summary>
        /// Open workbook format
        /// </summary>
        private readonly int WorkbookFormat = 5;

        /// <summary>
        /// Retry pause of 3 seconds
        /// </summary>
        private readonly int OpenRetryTime = 3000;

        /// <summary>
        /// Retry limit of 5 times
        /// </summary>
        private readonly int OpenRetryCount = 5;

        /// <summary>
        /// Open workbook and ignore read only
        /// </summary>
        private readonly bool IgnoreReadOnly = false;

        /// <summary>
        /// Open workbook as editable
        /// </summary>
        private readonly bool WorkbookEditable = true;

        /// <summary>
        /// Open workbook with notifications enabled
        /// </summary>
        private readonly bool WorkbookNotify = false;

        /// <summary>
        /// Add workbook to most recently used list
        /// </summary>
        private readonly bool WorkbookAddToMRU = false;

        /// <summary>
        /// Open workbook local parameter
        /// </summary>
        private readonly bool WorkbookLocal = false;

        /// <summary>
        /// Open workbook converter
        /// </summary>
        private readonly int WorkbookConverter = 0;

        /// <summary>
        /// Open workbook as read only
        /// </summary>
        private bool openReadOnly = false;

        /// <summary>
        /// Excel Application Launched
        /// </summary>
        private Application myApp;

        /// <summary>
        /// Workbook Opened
        /// </summary>
        private Workbook myWorkbook;

        /// <summary>
        /// Workbooks
        /// </summary>
        private Workbooks myWorkbooks;

        /// <summary>
        /// Worksheets in the current workbook
        /// </summary>
        private Sheets mySheets;

        /// <summary>
        /// Excel process attached to
        /// </summary>
        private Process myProcess;

        /// <summary>
        /// Track whether Dispose has been called
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Name of currently open workbook
        /// </summary>
        private string workbookName;

        /// <summary>
        /// PopupBasher instance
        /// </summary>
        private PopupBasher basher;

        /// <summary>
        /// Multi-thread macro invocation synchronization
        /// </summary>
        private object macroLock = new object();

        /// <summary>
        /// Lock to prevent multiple launches of excel
        /// </summary>
        private object launchExcelLock = new object();

        /// <summary>
        /// Global mutex for locking around launching excel processes and assigning application objects
        /// </summary>
        private Mutex excelLaunchMutex;

        #endregion

        #region Constructors/Destructors

		/// <summary>
		///   <para>Initializes a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> class without specifying the workbook or instance of Excel that the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object should use.</para>
		/// </summary>
		/// <remarks>
		///   <para>To open a workbook after creating a 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object with this version of the constructor, call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method.</para>
		///   <para>To create a new instance of the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> class that uses a specified workbook that is already open, use the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" /> form of the constructor.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" />
        public ExcelDriver()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ExcelDriver class.
        /// </summary>
        /// <param name="workbook"> Excel Workbook to wrap around</param>
        public ExcelDriver(Workbook workbook)
        {
            this.App = workbook.Application;
            this.Workbook = workbook;
            this.workbookName = workbook.FullName;
        }

		/// <summary>
		///   <para />
		/// </summary>
        ~ExcelDriver()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        #endregion

        #region Public Accessors

        /// <summary>
        ///   <para>Gets or sets open read only.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool OpenReadOnly
        {
            get
            {
                return this.openReadOnly;
            }
            set
            {
                this.openReadOnly = value;
            }
        }
		/// <summary>
		///   <para>Gets or sets a reference to the 
		/// <see cref="Microsoft.Office.Interop.Excel.Application" /> object that the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> class uses for component object model (COM) interoperability with the running instance of Excel.</para> 
		/// </summary>
		/// <value>
		///   <para>The 
		/// <see cref="Microsoft.Office.Interop.Excel.Application" /> that the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> class uses for component object model (COM) interoperability with the running instance of Excel.</para> 
		/// </value>
        public Application App
        {
            get
            {
                return this.myApp;
            }

            private set
            {
                this.myApp = value;
            }
        }

        /// <summary>
        ///   <para>Gets Workbooks.</para>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public Workbooks Workbooks
        {
            get
            {
                return this.myWorkbooks;
            }

            private set
            {
                this.myWorkbooks= value;
            }
        }

		/// <summary>
		///   <para>Gets the currently open Excel workbook.</para>
		/// </summary>
		/// <value>
		///   <para>A <see cref="Microsoft.Office.Interop.Excel.Workbook" /> object that represents the currently open Excel workbook.</para>
		/// </value>
        public Workbook Workbook
        {
            get
            {
                return this.myWorkbook;
            }

            private set
            {
                this.myWorkbook = value;
            }
        }

		/// <summary>
		///   <para>Gets or sets the collection of worksheets in the currently open workbook.</para>
		/// </summary>
		/// <value>
		///   <para>A 
		/// <see cref="Microsoft.Office.Interop.Excel.Sheets" /> object that represents the collection of worksheets in the currently open workbook.</para>
		/// </value>
        public Sheets Sheets
        {
            get
            {
                return this.mySheets;
            }

            set
            {
                this.mySheets = value;
            }
        }

		/// <summary>
		///   <para>Gets the <see cref="System.Diagnostics.Process" /> object that represents the process that is running Excel on the local computer.</para>
		/// </summary>
		/// <value>
		///   <para>A <see cref="System.Diagnostics.Process" /> object that represents the process that is running Excel on the local computer.</para>
		/// </value>
        public Process ExcelProcess
        {
            get
            {
                return this.myProcess;
            }

            private set
            {
                this.myProcess = value;
            }
        }

        #endregion

        #region Public Methods

		/// <summary>
		///   <para>Opens the specified Excel workbook on the local computer.</para>
		/// </summary>
		/// <param name="filePath">
		///   <para>A string that specifies the full path to the Excel workbook that you want to open.</para>
		/// </param>
		/// <remarks>
		///   <para>This method does not require a previous state. If Excel is not 
		/// already running on the local machine when you call this method, this method starts Excel.</para>
		///   <para>To open the Excel workbook only if the last saved date matches a specific date and time, use the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String,System.Boolean,System.String,System.String,System.Nullable{System.DateTime})" /> or  
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String,System.Nullable{System.DateTime})" /> method instead.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Office.Interop.Excel.Workbooks.Open(System.String,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object)" 
		/// /> 
        public void OpenWorkbook(string filePath)
        {
            this.OpenWorkbook(filePath, false, string.Empty, string.Empty, null);
        }

		/// <summary>
		///   <para>Opens the specified Excel workbook on the local computer if the date that the workbook was last saved matches the specified date.</para>
		/// </summary>
		/// <param name="filePath">
		///   <para>A string that specifies the full path to the Excel workbook that you want to open.</para>
		/// </param>
		/// <param name="lastSaveDate">
		///   <para>A 
		/// 
		/// <see cref="System.DateTime" /> structure that specifies the date and time that the Excel workbook should have most recently been saved in order for the method to open the workbook, or  
		/// null if the method should open the workbook regardless of the date and the time that the workbook was last saved.</para>
		/// </param>
		/// <exception cref="System.ArgumentException">
		///   <para>Indicates that the value of a parameter was incorrect. This exception can occur if the date and 
		/// time that the Excel workbook was actually last saved differs from the value specified for the <paramref name="lastSaveDate" /> parameter.</para>
		/// </exception>
		/// <remarks>
		///   <para>This method does not require a previous state. If Excel is not 
		/// already running on the local machine when you call this method, this method starts Excel.</para>
		///   <para>To open a protected or write-reserved workbook with the required password, use the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String,System.Boolean,System.String,System.String,System.Nullable{System.DateTime})" /> method instead. To open the Excel workbook without specifying passwords and regardless of the date and time that workbook was last saved, use the  
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Office.Interop.Excel.Workbooks.Open(System.String,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object)" 
		/// /> 
        public void OpenWorkbook(string filePath, DateTime? lastSaveDate)
        {
            this.OpenWorkbook(filePath, false, string.Empty, string.Empty, lastSaveDate);
        }

		/// <summary>
		///   <para>Opens the specified Excel workbook on the local computer if the date that the workbook 
		/// was last saved matches the specified date, using the specified passwords if the workbook is protected or write-reserved.</para>
		/// </summary>
		/// <param name="filePath">
		///   <para>A string that specifies the full path to the Excel workbook that you want to open.</para>
		/// </param>
		/// <param name="updateLinks">
		///   <para>
		///     <see cref="System.Boolean" /> that indicates whether Excel should try to update the links in the workbook. 
		/// A True value indicates that Excel should try to update the links. 
		/// A False value indicates that Excel should not try to update the links.</para>
		/// </param>
		/// <param name="password">
		///   <para>A string that specifies in plain text the password that is required to open the workbook, if the workbook is a protected workbook.</para>
		/// </param>
		/// <param name="writeResPassword">
		///   <para>A string that specifies in plain text the password that 
		/// is required to write to the workbook, if the workbook is a write-reserved workbook.</para>
		/// </param>
		/// <param name="lastSaveDate">
		///   <para>A 
		/// 
		/// <see cref="System.DateTime" /> structure that specifies the date and time that the Excel workbook should have most recently been saved in order for the method to open the workbook, or  
		/// null if the method should open the workbook regardless of the date and the time that the workbook was last saved.</para>
		/// </param>
		/// <exception cref="System.Runtime.InteropServices.COMException">
		///   <para>Indicates that the workbook is protected or write-reserved and the corresponding password is missing or incorrect.</para>
		/// </exception>
		/// <remarks>
		///   <para>This method does not require a previous state. If Excel is not 
		/// already running on the local machine when you call this method, this method starts Excel.</para>
		///   <para>To open the Excel workbook only if the last saved date matches a specific date and time, but without specifying passwords, use the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String,System.Nullable{System.DateTime})" /> method instead. To open the Excel workbook without specifying passwords and regardless of the date and time that workbook was last saved, use the  
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Office.Interop.Excel._Workbook.SaveAs(System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,Microsoft.Office.Interop.Excel.XlSaveAsAccessMode,System.Object,System.Object,System.Object,System.Object,System.Object)" 
		/// /> 
		/// <seealso cref="Microsoft.Office.Interop.Excel._Workbook.WriteReserved()" />
		/// <seealso cref="Microsoft.Office.Interop.Excel.Workbooks.Open(System.String,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object,System.Object)" 
		/// /> 
        public void OpenWorkbook(
            string filePath,
            bool updateLinks,
            string password,
            string writeResPassword,
            DateTime? lastSaveDate)
        { 
            try
            {
                // Launch new excel process if none started
                if (this.ExcelProcess == null)
                {
                    this.LaunchExcelProcess();

                    // Trace Workbook opening information
                    Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_OpeningWorkbook, filePath);
                    Tracing.SoaTrace(XlTraceLevel.Information, Resources.ExcelDriver_OpeningWorkbook, filePath);
                }

                try
                {
                    this.OpenWorkbookInternal(filePath, updateLinks, true, password, writeResPassword, lastSaveDate);
                }
                catch (FileNotFoundException ex)
                {
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelDriver, ex.ToString(), delegate { Tracing.EventProvider.LogExcelDriver_WorkbookDNE(filePath, ex.ToString()); });
                    throw;
                }
            } 
            catch (Exception ex)
            {
                if (this.App == null)
                {
                    // Launch Excel Process Failed
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelDriver, ex.ToString(), delegate { Tracing.EventProvider.LogExcelDriver_StartProcessFailed(ex.ToString()); });
                }
                else
                {
                    // Check for COM Exception specific to corrupt workbook. Bug 8314 - confusing error message when workbook corrupt.
                    if (ex.GetType().Equals(typeof(COMException)))
                    {
                        COMException cex = (COMException)ex;
                        if ((uint)cex.ErrorCode == 0x800A03EC)
                        {
                            // Use IOException for more accurate exception type and easier classification of problems reading workbook
                            ex = new IOException(string.Format(CultureInfo.CurrentCulture, Resources.ExcelDriver_CorruptWorkbook, filePath), ex);
                        }
                    }

                    // Opening workbook failed
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelDriver, ex.ToString(), delegate { Tracing.EventProvider.LogExcelDriver_OpenWorkbookFailed(filePath, ex.ToString()); });
                }

                throw ex;
            }

            this.workbookName = filePath;
        }

		/// <summary>
		///   <para>Starts a process running Excel and sets the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.App" /> and 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.ExcelProcess" /> properties.</para>
		/// </summary>
		/// <remarks>
		///   <para>You can user the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.App" /> and 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.ExcelProcess" /> properties to continue to interact with Excel.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.App" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.ExcelProcess" />
        public void LaunchExcelProcess()
        {
            lock (this.launchExcelLock)
            {
                if (this.ExcelProcess != null)
                {
                    throw new InvalidOperationException(Resources.ExcelDriver_LaunchExcelOnce);
                }

                // Create the mutex. This function is only called once and the mutex is only needed here, so it's robust.
                bool createdNew = false;

                // Give access to mutex to everyone
                SecurityIdentifier sid = new SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
                MutexSecurity mutSec = new MutexSecurity();
                MutexAccessRule mutRule = new MutexAccessRule(sid, MutexRights.FullControl, AccessControlType.Allow);
                mutSec.AddAccessRule(mutRule);

                // Create mutex with full control for everyone. Doesn't matter if mutex is created new.
                this.excelLaunchMutex = new Mutex(false, "47FFDA0F-26D3-49fa-8E65-FC7590EFB95F", out createdNew, mutSec);
                bool ownMutex = false;

                Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_OpeningExcel);

                try
                {
                    this.basher = new PopupBasher();

                    // Create an Excel COM Server process so that it'll populate the ROT
                    ProcessStartInfo info = new ProcessStartInfo();

                    info.FileName = "excel.exe";
                    info.Arguments = @"/automation -Embedding";

                    try
                    {
                        // Take lock on global mutex to ensure 'new ApplicationClass()' returns application
                        // associated with the created excel process.
                        this.excelLaunchMutex.WaitOne();
                        ownMutex = true;
                    }
                    catch (AbandonedMutexException ex)
                    {
                        // This mutex will only be abandoned on hard faults.
                        // In the case of a SOA service, this can happen when the job is cancelled or preempted.
                        // In any case, this should not produce an unstable state.
                        Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, ex.ToString());
                    }

                    // Start the process and wait until it is usable.
                    try
                    {
                        this.ExcelProcess = new Process();
                        this.ExcelProcess.StartInfo = info;
                        this.ExcelProcess.Start();
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        // If there is a problem starting the excel process, null out the process and rethrow 
                        this.ExcelProcess = null;
                        throw;
                    }

                    Process currentProcess = Process.GetCurrentProcess();
                    this.SwitchToWindow(currentProcess.MainWindowHandle);
                    this.SwitchToWindow(this.ExcelProcess.MainWindowHandle);
                    this.ExcelProcess.WaitForInputIdle();

                    // Let popupbasher know what the process ID for Excel is
                    PopupBasher.ExcelProcessId = this.ExcelProcess.Id;

                    // Creates a new Excel Application
                    this.App = new ApplicationClass();
                }
                finally
                {
                    // Release the global mutex
                    if (ownMutex)
                    {
                        this.excelLaunchMutex.ReleaseMutex();
                    }
                }

                this.App.Visible = true;  // Makes Excel visible to the user.
                this.App.DisplayAlerts = false;  // Don't pop up message about other users using the same workbook
            }
        }

		/// <summary>
		///   <para>Runs the specified macro with the specified input on the currently open Excel workbook.</para>
		/// </summary>
		/// <param name="macroName">
		///   <para>String that specifies the name of the macro that you want to run on the currently open Excel workbook.</para>
		/// </param>
		/// <param name="inputs">
		///   <para>An array of <see cref="System.Object" /> objects that represent the values of the parameters that you want to pass to the macro.</para>
		/// </param>
		/// <returns>
		///   <para>An <see cref="System.Object" /> object that represents the return value of the macro.</para>
		/// </returns>
		/// <remarks>
		///   <para>If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object on which you call this method with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> form of the constructor, you must call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> before you call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.RunMacro(System.String,System.Object[])" /> method. If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" />, you do not need to call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method first.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" />
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" />
        public object RunMacro(string macroName, params object[] inputs)
        {
            object retVal = null;
            try
            {
                Tracing.WriteDebugTextVerbose(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_InvokeMacro, macroName, this.workbookName);
                
                object[] parameters = null;
                if (inputs != null)
                {
                    parameters = new object[inputs.Length + 1];
                }
                else
                {
                    parameters = new object[1];
                }

                // The parameter list must include the macro name followed by each parameter.
                parameters[0] = macroName;
                int count = 1;
                foreach (object input in inputs)
                {
                    parameters[count] = input;
                    count++;
                }

                // Invoke the Run method on the application. This should be locked such that no two macros can be invoked 
                // simultaneously.
                Type t = typeof(Application);
                lock (this.macroLock)
                {
                    retVal = t.InvokeMember(
                                            "Run",
                                            BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod,
                                            null,
                                            this.App,
                                            parameters,
                                            null);
                }
            }
            catch (TargetInvocationException ex)
            {
                Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelDriver, ex.ToString(), delegate { Tracing.EventProvider.LogExcelDriver_MacroFailed(macroName, this.workbookName, ex.ToString()); });
                throw new TargetInvocationException(String.Format(CultureInfo.CurrentCulture, Resources.ExcelDriver_MacroFailed, macroName, this.workbookName, ex.Message), ex);
            }
            catch (ArgumentException ex)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_MacroInvocationFail, macroName, this.workbookName, ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_MacroExecutionFail, macroName, this.workbookName, ex.ToString());
                throw;
            }
            
            return retVal;
        }

		/// <summary>
		///   <para>Sets the values in one or more specified cells in the currently open workbook.</para>
		/// </summary>
		/// <param name="cellReference">
		///   <para>String that specifies one or more cells in A1 notation for which you want 
		/// to set the value. If you specify a range that consists of more than one cell,  
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.SetCellValue(System.String,System.String)" /> inserts the same value in each cell.</para>
		/// </param>
		/// <param name="value">
		///   <para>String that specifies the value to insert in the cells.</para>
		/// </param>
		/// <remarks>
		///   <para>If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object on which you call this method with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> form of the constructor, you must call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> before you call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.SetCellValue(System.String,System.String)" /> method. If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" />, you do not need to call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method first.</para>
		///   <para>For information on A1 notation, see <see 
		/// href="http://go.microsoft.com/fwlink/?LinkID=195285">How to: Refer to Cells and Ranges by Using A1 Notation</see>.</para> 
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.GetCellValue(System.String)" />
        public void SetCellValue(string cellReference, string value)
        {
            try
            {
                Range r;
                Worksheet activeSheet = (Worksheet)this.App.ActiveSheet;
                r = (Range)activeSheet.get_Range(cellReference, cellReference);
                if (r != null)
                {
                    r.Value2 = value;
                }
            }
            catch (COMException)
            {
                // Fail when cellReference invalid
                string message = string.Format(CultureInfo.CurrentCulture, Resources.ExcelDriverSetCellInvalid, cellReference);
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, message);
                throw new ArgumentException(message);
            }
        }

		/// <summary>
		///   <para>Gets the values from one or more specified cells in the currently open workbook.</para>
		/// </summary>
		/// <param name="cellReference">
		///   <para>String that specifies one or more cells in A1 notation for which you want to get the values.</para>
		/// </param>
		/// <returns>
		///   <para>An <see cref="System.Object" /> object that represents the values in the specified cells.</para>
		/// </returns>
		/// <remarks>
		///   <para>If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object on which you call this method with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> form of the constructor, you must call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> before you call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.GetCellValue(System.String)" /> method. If you created the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object with the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor(Microsoft.Office.Interop.Excel.Workbook)" />, you do not need to call the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.OpenWorkbook(System.String)" /> method first.</para>
		///   <para>For information on A1 notation, see <see 
		/// href="http://go.microsoft.com/fwlink/?LinkID=195285">How to: Refer to Cells and Ranges by Using A1 Notation</see>.</para> 
		///   <para>If you specify a range of cells rather than a single cell for 
		/// the <paramref name="cellReference" /> parameter, you need to cast the return value to an array of  
		/// <see cref="System.Object" /> objects before you can access the values for each of the cells in the range. For example:</para>
		///   <code language="c#">ExcelDriver xl = new ExcelDriver();
		/// 
		/// xl.OpenWorkbook("C:\ExcelWorkbooks\MyWorkbook.xlsm");
		/// Object[,] obj = (Object[,])xl.GetCellValue("A2:B4");
		/// 
		/// for(int i = 1; i &lt;= obj.GetLength(0); i++)
		/// {
		///     for(int j = 1; j &lt;= obj.GetLength(1); j++)
		///     {
		///         Console.WriteLine(obj[i,j].ToString());
		///     
		/// }</code>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.SetCellValue(System.String,System.String)" />
        public object GetCellValue(string cellReference)
        {
            try
            {
                Range r;
                Worksheet activeSheet = (Worksheet)this.App.ActiveSheet;
                r = (Range)activeSheet.get_Range(cellReference, cellReference);
                if (r != null)
                {
                    return r.Value2;
                }
            }
            catch (COMException)
            {
                // Fail when cell reference invalid
                string message = string.Format(CultureInfo.CurrentCulture, Resources.ExcelDriverGetCellInvalid, cellReference);
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, message);
                throw new ArgumentException(message);
            }

            return null;
        }

		/// <summary>
		///   <para>Gets the 
		/// 
		/// <see cref="Microsoft.Hpc.Excel.PopupMessage" /> objects that represent the message boxes that the pop-up blocker dismissed, and clears the internal list of messages.</para> 
		/// </summary>
		/// <returns>
		///   <para>An array of 
		/// <see cref="Microsoft.Hpc.Excel.PopupMessage" /> objects that represent the message boxes that the pop-up blocker dismissed.</para>
		/// </returns>
        public PopupMessage[] GetBashedPopups()
        {
            // Temporary solution to limit popup messages to 1K:
            int size = 0;
            for (int i = 0; i < this.basher.BashedPopups.Count; i++)
            {
                // Assume Unicode two-bytes-per-char on each string
                size += (this.basher.BashedPopups[i].MessageText.Length + this.basher.BashedPopups[i].TitleBar.Length) * 2;
                if (size > 1024)
                {
                    // Remove the popups that go beyond 1K
                    this.basher.BashedPopups.RemoveAt(i);
                    break;
                }
            }

            PopupMessage[] popups = this.basher.BashedPopups.ToArray();
            this.basher.BashedPopups.Clear();
            return popups;
        }

        #endregion

		/// <summary>
		///   <para>Releases all of the resources that the <see cref="Microsoft.Hpc.Excel.ExcelDriver" /> object used.</para>
		/// </summary>
		/// <remarks>
		///   <para>This method closes any open workbooks and any open instances of Excel that the 
		/// <see cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" /> object used.</para>
		/// </remarks>
		/// <seealso cref="Microsoft.Hpc.Excel.ExcelDriver.#ctor" />
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        #region Private Methods

        /// <summary>
        /// Compares two date times down to second
        /// </summary>
        /// <param name="first">One datetime</param>
        /// <param name="second">another datetime</param>
        /// <returns>true or false evaluation of whether the datetimes are equal</returns>
        private static bool DateTimeEqual(DateTime first, DateTime second)
        {
            // Check date/time up to second and return whether they are the same.
            // Not using .Equals to allow programmatically created date times and
            // improve testability.
            if (first.Year == second.Year &&
                first.Month == second.Month &&
                first.Day == second.Day &&
                first.Minute == second.Minute &&
                first.Second == second.Second)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"> Dispose of Excel Process as well? </param>
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (this.Sheets != null)
                    {
                        try
                        {
                            Marshal.FinalReleaseComObject(this.Sheets);
                            this.Sheets = null;
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_CloseWBFailed, ex.ToString());
                        }
                    }

                    if (this.Workbook != null)
                    {
                        try
                        {
                            this.Workbook.Close(false, null, false);
                            Marshal.FinalReleaseComObject(this.Workbook);
                            this.Workbook = null; 
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_CloseWBFailed, ex.ToString()); 
                        }
                    }

                    if (this.Workbooks != null)
                    {
                        try
                        {
                            this.Workbooks.Close();
                            Marshal.FinalReleaseComObject(this.Workbooks);
                            this.Workbooks = null;
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_CloseWBFailed, ex.ToString());
                        }
                    }

                    if (this.App != null)
                    {
                        try
                        {
                            this.App.Visible = false;
                            this.App.Quit();
                            Marshal.FinalReleaseComObject(this.App);
                            this.App = null;
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_QuitExcelFailed, ex.ToString()); 
                        }
                    }

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    if (this.ExcelProcess != null)
                    {
                        try
                        {
                            this.ExcelProcess.Close();
                            this.ExcelProcess.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_KillExcelFailed, ex.ToString()); 
                        }
                    }

                    if (this.excelLaunchMutex != null)
                    {
                        try
                        {
                            this.excelLaunchMutex.Close();
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_MutexCloseFailed, ex.ToString());
                        }
                    }

                    if (this.basher != null)
                    {
                        try
                        {
                            this.basher.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, ex.ToString());
                        }
                    }
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.
                // If disposing is false,
                // only the following code is executed.

                // Note disposing has been done.
                this.disposed = true;
            }
        }
       
        /// <summary>
        /// Opens workbook and sets desired attributes
        /// </summary>
        /// <param name="filePath"> location of workbook </param>
        /// <param name="updateLinks"> flag: update links?</param>
        /// <param name="enableMacros"> flag: enable macros?</param>
        /// <param name="password"> Open password </param>
        /// <param name="writeResPassword">Edit password </param>
        /// <param name="lastSaveDate"> Expected last save date </param>
        private void OpenWorkbookInternal(
            string filePath,
            bool updateLinks,
            bool enableMacros,
            string password,
            string writeResPassword,
            DateTime? lastSaveDate)
        {
            // Macros should typically be enabled
            if (enableMacros)
            {
                // Enables all macros. This is the default value when the application is started
                this.App.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityLow;
            }
            else
            {
                this.App.AutomationSecurity = Microsoft.Office.Core.MsoAutomationSecurity.msoAutomationSecurityForceDisable;
            }

            if (this.Workbook != null)
            {
                try
                {
                    this.Workbook.Close(false, null, false);
                }
                catch (COMException ex)
                {
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_WorkbookClosed, ex.ToString());
                }
            }

            // Check if workbook exists
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(Resources.ExcelDriverWorkbookDNE, filePath);
            }

            // If file exists, let excel try 5 times to open it.
            int tryCount = 0;
            while (true)
            {
                try
                {
                    // Open the workbook, not recovering files nor data.
                    this.Workbooks = this.App.Workbooks;
                    this.Workbook = this.Workbooks.Open(
                        filePath,
                        updateLinks ? this.DoUpdateLinks : this.DoNotUpdateLinks,
                        this.OpenReadOnly,
                        this.WorkbookFormat,
                        password,
                        writeResPassword,
                        this.IgnoreReadOnly,
                        XlPlatform.xlWindows,
                        string.Empty,
                        this.WorkbookEditable,
                        this.WorkbookNotify,
                        this.WorkbookConverter,
                        this.WorkbookAddToMRU,
                        this.WorkbookLocal,
                        XlCorruptLoad.xlNormalLoad);
                    
                    // Do not retry if successful
                    break;
                }
                catch
                {
                    if (tryCount < this.OpenRetryCount)
                    {
                        Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriver_RetryWorkbookOpen, filePath);
                        tryCount++;
                        
                        // Provide time for file to become available to Excel
                        Thread.Sleep(this.OpenRetryTime);
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // If non-null, the verify save date of opened workbook is same as expected
            if (lastSaveDate.HasValue)
            {
                try
                {
                    this.CheckWorkbookVersionsMatch(lastSaveDate.Value);
                }
                catch
                {
                    this.Workbook.Close(false, null, false);
                    throw;
                }
            }

            // The following gets the Worksheets collection
            this.Sheets = this.App.Sheets;
        }

        /// <summary>
        /// Checks workbook last save date against expected last save date
        /// </summary>
        /// <param name="lastSaveDate"> DataTime with expected last save date</param>
        private void CheckWorkbookVersionsMatch(DateTime lastSaveDate)
        {
            // Get last save date of opened file
            object documentProperties = this.Workbook.BuiltinDocumentProperties;
            Type documentPropertiesType = documentProperties.GetType();

            object version = documentPropertiesType.InvokeMember(
                                                                 "Item", 
                                                                 BindingFlags.Default | BindingFlags.GetProperty,
                                                                 null, 
                                                                 documentProperties, 
                                                                 new object[] { "Last Save Time" },
                                                                 CultureInfo.InvariantCulture);

            Type versionType = version.GetType();
            string value = versionType.InvokeMember("Value", BindingFlags.Default | BindingFlags.GetProperty, null, version, new object[] { }, CultureInfo.InvariantCulture).ToString();

            DateTime currentSaveTime = DateTime.Parse(value, CultureInfo.InvariantCulture);

            // Compare expected last save date to save date on file
            if (!DateTimeEqual(lastSaveDate, currentSaveTime))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Resources.ExcelDriverWorkbookVersionMismatch, currentSaveTime.ToString(), lastSaveDate.ToString());
                Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelDriver, message, delegate { Tracing.EventProvider.LogExcelDriver_WorkbookVersionMismatch(this.Workbook.FullName, message); });
                throw new ArgumentException(message);
            }
        }

        /// <summary>
        /// Helper method to change current focus to a specified window
        /// </summary>
        /// <param name="hwnd"> pointer to window </param>
        private void SwitchToWindow(IntPtr hwnd)
        {
            if (hwnd != (IntPtr)0)
            {
                // Set focus to the window
                NativeMethods.SetForegroundWindow(hwnd);

                // Wait for focus to actually change
                for (int j = 0; j < this.FOCUSTIME * 10; j++)
                {
                    Thread.Sleep(100);
                    if (NativeMethods.GetForegroundWindow() == hwnd)
                    {
                        return;
                    }
                }

                // If we hit this code, then the window was not switched in 1s
                Tracing.WriteDebugTextError(Tracing.ComponentId.ExcelDriver, Resources.ExcelDriverNotResponsive);
                throw new TimeoutException(Resources.ExcelDriverNotResponsive);
            }
        }

        #endregion
    } // Class
} // Namespace
