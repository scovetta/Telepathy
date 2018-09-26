//------------------------------------------------------------------------------
// <copyright file="HPCExcelService.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Generic Service for Excel Runner
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
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using System.Threading;
    using Microsoft.Hpc.Excel.Internal;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Office.Interop.Excel;
    
    /// <summary>
    /// Provides the service behavior for the HPCExcelService
    /// </summary>
    [ServiceBehavior(IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Single)]
    public class ExcelService : IExcelService
    {
        /// <summary>
        /// Registry key containing document recovery information.
        /// </summary>
        private readonly string RECOVERYLISTKEY = @"software\Microsoft\office\14.0\Excel\Resiliency\DocumentRecovery";

        /// <summary>
        /// Name of service
        /// </summary>
        private readonly string SERVICENAME = "ExcelService";

        /// <summary>
        /// Environment variable with node name
        /// </summary>
        private readonly string NODENAMEEV = "COMPUTERNAME";

        /// <summary>
        /// Environment variable with session ID
        /// </summary>
        private readonly string SESSIONEV = "CCP_JOBID";

        /// <summary>
        /// Environment variable with session ID
        /// </summary>
        private readonly string TASKEV = "CCP_TASKID";

        /// <summary>
        /// Name of environment variable with workbook path
        /// </summary>
        private readonly string WORKBOOKENVVAR = "Microsoft.Hpc.Excel.WorkbookPath";

        private readonly string DATASERVICESHAREDENVVAR = "CCP_DATA_SERVICE_SHARED";

        /// <summary>
        /// Does a workbook exist for reuse?
        /// </summary>
        private bool workbookLoaded = false;

        /// <summary>
        /// Excel Driver abstracting excel application and workbook interaction
        /// </summary>
        private ExcelDriver xl = new ExcelDriver();

        /// <summary>
        /// Newest experimental calculate method
        /// </summary>
        /// <param name="macroName">Macro Name</param>
        /// <param name="inputs">Serialized Inputs</param>
        /// <param name="lastSaveDate">Last save date of workbook (or null)</param>
        /// <returns>Serialized result from macro </returns>
        public byte[] Calculate(string macroName, byte[] inputs, DateTime? lastSaveDate)
        {
            try
            {
                byte[] retVal = null;

                // Check if driver and excel process are available and if they are, find out if the excel process has gone down.
                if (this.xl != null ? (this.xl.ExcelProcess != null ? this.xl.ExcelProcess.HasExited : false) : false)
                {
                    // If the process has gone down, clean up the previously created ExcelDriver 
                    // and create a new one before trying to open the workbook again
                    this.xl.Dispose();
                    this.xl = new ExcelDriver();
                    this.workbookLoaded = false;
                }

                // Check if workbook already loaded
                if (this.workbookLoaded == false)
                {  
                    // Note whether workbook was loaded
                    Tracing.WriteDebugTextVerbose(Tracing.ComponentId.ExcelService, Resources.ExcelService_FirstRequest);

                    // Try to get Microsoft.Hpc.Excel.WorkbookPath environment variable
                    

                    string FindWorkBookPath()
                    {
                        var path = Environment.ExpandEnvironmentVariables(Environment.GetEnvironmentVariable(this.WORKBOOKENVVAR));
                        if (string.IsNullOrEmpty(path))
                        {
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Resources.HPCExcelServiceWorkbookNotSet, this.WORKBOOKENVVAR));
                        }

                        if (File.Exists(path))
                        {
                            return path;
                        }
                        else
                        {
                            string dataServiceShared = Environment.GetEnvironmentVariable(this.DATASERVICESHAREDENVVAR);
                            if (string.IsNullOrEmpty(dataServiceShared))
                            {
                                throw new InvalidOperationException(string.Format($"Can't find {path}, no files are shared through data service."));
                            }
                            else
                            {
                                var workbookName = Path.GetFileName(path);
                                foreach (var filePath in dataServiceShared.Split(';'))
                                {
                                    if (Path.GetFileName(filePath) == workbookName)
                                    {
                                        return filePath;
                                    }
                                }
                                throw new InvalidOperationException(string.Format($"Can't find {path} in files shared through data service: {dataServiceShared}."));
                            }
                        }
                    }

                    string workbookPath = FindWorkBookPath();
                    Tracing.SoaTrace(XlTraceLevel.Information, $"Find workbook in {workbookPath}");


                    // Perform different workbook open logic on Azure than on premise
                    if (AzureUtil.IsOnAzure())
                    {
                        this.OpenWorkbookOnAzure(workbookPath, lastSaveDate);
                    }
                    else
                    {
                        this.xl.OpenReadOnly = true;
                        this.OpenWorkbook(workbookPath, lastSaveDate);
                    }

                    // Register OnExiting callback
                    this.SetOnExitingHook();
                }
                else
                {
                    Tracing.WriteDebugTextVerbose(Tracing.ComponentId.ExcelService, Resources.ExcelService_OtherRequest);
                }

                object returnedValue = null;
                WorkItem workItem = null;

                // Deserialize inputs
                try
                {
                    workItem = WorkItem.Deserialize(inputs);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.HPCExcelServiceDeserializeFailed, this.SERVICENAME), ex);
                }

                // Run the macro of provided name with inputs converted into an object                
                returnedValue = this.xl.RunMacro(macroName, workItem.GetAll());

                // Convert the returned object into work item
                WorkItem outputs = null;

                outputs = new WorkItem();
                outputs.Insert(0, returnedValue);

                // Serialize the work item for WCF
                try
                {
                    retVal = WorkItem.Serialize(outputs);
                }
                catch (Exception ex)
                {
                    throw new SerializationException(string.Format(CultureInfo.CurrentCulture, Resources.HPCExcelServiceSerializeFailed, macroName), ex);
                }

                return retVal;    
            }
            catch (Exception ex)
            {
                // Handle corrupt workbooks as hard faults rather than retry operation exceptions. Bug 10285.
                bool corruptWorkbook = false;

                // Check for IO Exception containing COM Exception specific to corrupt workbook.
                if (ex.GetType().Equals(typeof(System.IO.IOException)) && ex.InnerException != null && ex.InnerException.GetType().Equals(typeof(COMException)))
                {
                    COMException cex = (COMException)ex.InnerException;
                    if ((uint)cex.ErrorCode == 0x800A03EC)
                    {
                        corruptWorkbook = true;
                    }
                }

                // Failure logged and returned to client via new Exception
                if (this.workbookLoaded || corruptWorkbook)
                {
                    // If the workbook has been loaded successfully, then the compute node is set up correctly and it is an
                    // application error. Also report corrupt workbooks in this way to avoid retrying.
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelService, ex.ToString(), delegate { Tracing.EventProvider.LogExcelService_ApplicationError(Environment.GetEnvironmentVariable(this.SESSIONEV), Environment.GetEnvironmentVariable(this.TASKEV), Environment.GetEnvironmentVariable(this.NODENAMEEV), ex.ToString()); });
                    throw new FaultException<ExcelServiceError>(new ExcelServiceError(ex), Environment.GetEnvironmentVariable(this.NODENAMEEV) + ": " + ex.ToString());
                }
                else
                {
                    // If the workbook has not been loaded successfully, then the compute node is not set up correctly.
                    // Either it cannot access the workbook, it cannot open excel, or it cannot load other installed HPC components.
                    Tracing.TraceEvent(XlTraceLevel.Error, Tracing.ComponentId.ExcelService, ex.ToString(), delegate { Tracing.EventProvider.LogExcelService_SystemicFailure(Environment.GetEnvironmentVariable(this.SESSIONEV), Environment.GetEnvironmentVariable(this.TASKEV), Environment.GetEnvironmentVariable(this.NODENAMEEV), ex.ToString()); });
                    throw new FaultException<RetryOperationError>(new RetryOperationError(Environment.GetEnvironmentVariable(this.NODENAMEEV) + ": " + ex.ToString()), Environment.GetEnvironmentVariable(this.NODENAMEEV) + ": " + ex.ToString());
                }
            }
        } // Calculate

        /// <summary>
        /// Open a workbook on Azure
        /// </summary>
        /// <param name="workbookPath">path specified in calculate call</param>
        /// <param name="lastSaveDate">last save date if specified</param>
        private void OpenWorkbookOnAzure(string workbookPath, DateTime? lastSaveDate)
        {
            // If on azure, get the package path
            string packageName = Path.GetFileNameWithoutExtension(workbookPath);
            string packagePath = AzureUtil.GetServiceLocalCacheFullPath(packageName);
            if (!string.IsNullOrEmpty(packagePath))
            {
                // If package path was found, try to open workbook there
                string packageWorkbookPath = Path.Combine(packagePath, Path.GetFileName(workbookPath));
                try
                {
                    this.OpenWorkbook(packageWorkbookPath, lastSaveDate);
                }
                catch (FileNotFoundException)
                {
                    // If unable to find workbook, retry with 
                    Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelService, Resources.ExcelService_CantFindPackageWB, packageWorkbookPath, workbookPath);
                    this.AzureRetryWithWorkbookPath(workbookPath, lastSaveDate, packageWorkbookPath);
                }
            }
            else
            {
                // Log warning about being unable to find package name
                Tracing.WriteDebugTextWarning(Tracing.ComponentId.ExcelService, Resources.ExcelService_CantFindPackage, packageName, workbookPath);
                this.AzureRetryWithWorkbookPath(workbookPath, lastSaveDate, packageName);
            }
        }

        /// <summary>
        /// Helper to try to open the workbook path and correctly log and throw and errors
        /// </summary>
        /// <param name="workbookPath">path to try</param>
        /// <param name="lastSaveDate">last save date if specified</param>
        /// <param name="packageWorkbookPath">package name or path</param>
        private void AzureRetryWithWorkbookPath(string workbookPath, DateTime? lastSaveDate, string packageWorkbookPath)
        {
            // If unable to find workbook in package path, fall back to full path provided in service call
            try
            {
                this.OpenWorkbook(workbookPath, lastSaveDate);
            }
            catch (FileNotFoundException)
            {
                // If unable to find workbook at workbookpath either, throw a filenotfoundexception explaining where all we looked
                throw new FileNotFoundException(string.Format(System.Globalization.CultureInfo.CurrentCulture, Resources.ExcelService_CantFindWbAzure, packageWorkbookPath, workbookPath), Path.GetFileName(workbookPath));
            }
        }

        /// <summary>
        /// Helper method to try to open a workbook and set the appropriate properties
        /// </summary>
        /// <param name="workbookPath">path to workbook</param>
        /// <param name="lastSaveDate">last save date if specified</param>
        private void OpenWorkbook(string workbookPath, DateTime? lastSaveDate)
        {
            // Log workbook name on first load for security concerns
            string message = string.Format(
                                            CultureInfo.CurrentCulture,
                                            Resources.ExcelService_OpeningWorkbook,
                                            Environment.GetEnvironmentVariable(this.SESSIONEV),
                                            Environment.GetEnvironmentVariable(this.TASKEV),
                                            workbookPath,
                                            Environment.GetEnvironmentVariable(this.NODENAMEEV));

            Console.WriteLine(message);
            Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelService, message);

            // Create new instance of excel driver and open the specified workbook
            this.xl.OpenWorkbook(workbookPath, lastSaveDate);

            this.xl.App.EnableCancelKey = XlEnableCancelKey.xlErrorHandler;
            this.xl.App.ScreenUpdating = false;
            this.xl.App.Calculation = XlCalculation.xlCalculationAutomatic;
            this.workbookLoaded = true;
        }

        /// <summary>
        /// Adds OnExiting event handler to dispose ExcelDriver and clean up workbook recovery data
        /// </summary>
        private void SetOnExitingHook()
        {
            // Set OnExiting event handler
            ServiceContext.OnExiting += new EventHandler<EventArgs>(delegate
            {
                Tracing.WriteDebugTextInfo(Tracing.ComponentId.ExcelService, Resources.HPCExcelServiceExiting, this.SERVICENAME);

                try
                {
                    // Clean up document recovery list
                    Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(this.RECOVERYLISTKEY);
                }
                catch
                {
                    // Document Recovery list will not always be populated
                }                
            });
        }
    } // Class
} // Namespace
