// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Hpc.RESTServiceModel;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Win32.SafeHandles;

    using SoaService.DataClient;

    using TelepathyCommon;

    class Program
    {
        /// <summary>
        /// Job Id of current job. We need this when write trace.
        /// </summary>
        private static string jobId;
        static SvcHostMgmtRestServer restserver;
        private static string defaultRegistryPath = "ServiceRegistration";

        // Starter
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)]
        static int Main(string[] args)
        {
            TraceHelper.TraceEvent(
                TraceEventType.Information, "HpcServiceHost entry point is called. Time: {0}", DateTime.UtcNow.ToString());
            //
            //  Check if HpcServiceHost is being run to launch pre/post tasks
            //
            uint exitCode = 0;

            if (RunPrePostTask(ref exitCode))
            {
                return (int)exitCode;
            }

            // HpcServiceHost.exe is running without a job
            if (args.Length > 0)
            {
                if (args[0].Equals("-StandAlone", StringComparison.OrdinalIgnoreCase))
                {
                    if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
                    {
                        ServiceHostRuntimeConfiguration.StorageCredential = args[1];
                    }

                    ServiceHostRuntimeConfiguration.Standalone = true;
                    StartService();
                    ListenSvcInfoChanged();
                }
                else
                {
                    ServiceHostRuntimeConfiguration.Standalone = false;
                    ParameterContainer param = new ParameterContainer(args);
                    NormalMethod(param);
                }
            }
            //Thread.Sleep(Timeout.Infinite);

            return 0;
        }

        private static int NormalMethod(ParameterContainer param)
        {
            try
            {
                if (param.PrintHelp())
                {
                    return ErrorCode.ServiceHost_PrintCommandHelp;
                }
                else
                {
                    param.Parse();
                }
            }
            catch (ParameterException e)
            {
                Console.Error.WriteLine(e.Message);
                return ErrorCode.ServiceHost_IncorrectCommandLineParameter;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return ErrorCode.ServiceHost_UnexpectedException;
            }
            ServiceInfo serviceInfo = new ServiceInfo(param.JobId, param.TaskId, int.Parse(param.CoreId), param.RegistrationPath, param.FileName, null, null);
            OpenService(serviceInfo);
            return 0;
        }

        /// <summary>
        /// check whether the service info changed endlessly. 
        /// </summary>
        static void ListenSvcInfoChanged()
        {
            ServiceInfo present = SvcHostMgmtRestServer.Info;
            while (true)
            {
                ServiceInfo now;
                lock (ServiceInfo.s_lock)
                {
                    now = SvcHostMgmtRestServer.Info;
                }

                // if info changed, start service host by info.
                if (now != null && !now.Equals(present))
                {
                    OpenService(now);
                    present = null;
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Start REST Server for listening REST request.
        /// </summary>
        static void StartService()
        {
            string jobId = Utility.GetJobId();
            TraceHelper.TraceInfo(jobId, "RestServer Started!");
            restserver = new SvcHostMgmtRestServer("SvcHost", 80);
            restserver.Initialize();
            restserver.Start();
        }

        /// <summary>
        /// start service host by serviceInfo
        /// </summary>
        /// <param name="serviceInfo"></param>
        /// <returns></returns>
        public static int OpenService(ServiceInfo serviceInfo)
        {
            void SetEnvironment()
            {
                if (serviceInfo.Environment == null)
                {
                    TraceHelper.TraceVerbose(Utility.GetJobId(), "No environment fond.");
                    return;
                }

                foreach (var env in serviceInfo.Environment)
                {
                    Environment.SetEnvironmentVariable(env.Key, env.Value);
                    TraceHelper.TraceVerbose(Utility.GetJobId(), $"Set environment {env.Key}={env.Value}.");
                }
            }

            async Task<string[]> DownloadDependFiles()
            {
                if (serviceInfo.DependFiles != null && serviceInfo.DependFiles.Any())
                {
                    var downloadTasks = serviceInfo.DependFiles.Select(fileInfo => StandaloneDataClient.DownloadFileFromSasAsync(
                        fileInfo.Value,
                        Path.Combine(Path.Combine(Path.GetTempPath(), "ExcelOffloading"), Path.GetFileName(fileInfo.Key))));
                    return await Task.WhenAll(downloadTasks);
                }
                else
                {
                    TraceHelper.TraceVerbose(Utility.GetJobId(), "No depend files fond.");
                    return new string[0];
                }
            }

            Environment.SetEnvironmentVariable(Constant.JobIDEnvVar, serviceInfo.JobId.ToString());
            Environment.SetEnvironmentVariable(Constant.TaskIDEnvVar, serviceInfo.TaskId.ToString());
            Environment.SetEnvironmentVariable(Constant.CoreIdsEnvVar, serviceInfo.CoreId.ToString());
            Environment.SetEnvironmentVariable(Constant.ServiceConfigFileNameEnvVar, serviceInfo.FileName);
            if (!string.IsNullOrEmpty(serviceInfo.RegistrationPath))
            {
                Environment.SetEnvironmentVariable(Constant.RegistryPathEnv, serviceInfo.RegistrationPath);
            }
            else
            {
                Environment.SetEnvironmentVariable(Constant.RegistryPathEnv, Path.Combine(Environment.CurrentDirectory, defaultRegistryPath));
            }

            // use default values for following environment variables
            Environment.SetEnvironmentVariable(Constant.ProcNumEnvVar, "1");
            Environment.SetEnvironmentVariable(Constant.NetworkPrefixEnv, Constant.EnterpriseNetwork);
            Environment.SetEnvironmentVariable(Constant.ServiceInitializationTimeoutEnvVar, "60000");
            Environment.SetEnvironmentVariable(Constant.CancelTaskGracePeriodEnvVar, "15");
            Environment.SetEnvironmentVariable(Constant.ServiceConfigMaxMessageEnvVar, "65536");
            Environment.SetEnvironmentVariable(Constant.ServiceConfigServiceOperatonTimeoutEnvVar, "86400000");

            // the local host process won't be preempted by the scheduler
            Environment.SetEnvironmentVariable(Constant.EnableMessageLevelPreemptionEnvVar, bool.FalseString);

            SetEnvironment();
            var dependFilePath = DownloadDependFiles().GetAwaiter().GetResult();
            Environment.SetEnvironmentVariable(Constant.DataServiceSharedFileEnvVar, string.Join(";", dependFilePath));

#if DEBUG
            #region For EndpointNotFoundException test
            try
            {
                string strWaitPeriod = ConfigurationManager.AppSettings["Test_WaitPeriodBeforeStartup"];
                if (!string.IsNullOrEmpty(strWaitPeriod))
                {
                    int waitPeriodInMilliSecond = int.Parse(strWaitPeriod);
                    Console.Error.WriteLine("Debug: waiting {0} ms before startup service", waitPeriodInMilliSecond);
                    Thread.Sleep(waitPeriodInMilliSecond);
                }
            }
            catch (Exception)
            {
                // do nothing
            }
            #endregion
#endif

            jobId = Utility.GetJobId();

            string serviceConfigFullPath;

            bool onAzure = SoaHelper.IsOnAzure();
            TraceHelper.TraceInfo(
                jobId,
                "OnAzure = {0}",
                onAzure);

            bool bOpenDummy = false;
            try
            {
                string serviceConfigFileName = Environment.GetEnvironmentVariable(Constant.ServiceConfigFileNameEnvVar);

                // exit if no such env var
                if (string.IsNullOrEmpty(serviceConfigFileName))
                {
                    bOpenDummy = true;
                    Console.Error.WriteLine(StringTable.ServiceConfigFileNameNotSpecified);
                    return ErrorCode.ServiceHost_ServiceConfigFileNameNotSpecified;
                }

                if (onAzure)
                {
                    string localCacheFolder = Utility.GetServiceLocalCacheFullPath();
                    serviceConfigFullPath = Path.Combine(localCacheFolder, serviceConfigFileName);
                }
                else
                {
                    serviceConfigFullPath = GetServiceInfo(serviceConfigFileName);
                }

                if (!File.Exists(serviceConfigFullPath))
                {
                    bOpenDummy = true;
                    Console.Error.WriteLine(StringTable.CantFindServiceRegistrationFile, serviceConfigFullPath);
                    return ErrorCode.ServiceHost_ServiceRegistrationFileNotFound;
                }

                TraceHelper.TraceInfo(
                    jobId,
                    "ServiceConfigFullPath = {0}",
                    serviceConfigFullPath);
                ServiceRegistration registration;
                string assemblyFullPath;
                int errorCode = Utility.GetServiceRegistration(serviceConfigFullPath, onAzure, out registration, out assemblyFullPath);
                if (errorCode != ErrorCode.Success)
                {
                    bOpenDummy = true;
                    return errorCode;
                }

                // Open the host in another application domain
                AppDomain domain = CreateNewServiceDomain(serviceConfigFullPath, assemblyFullPath);



                using (CcpServiceHostWrapper host =
                    CreateInstanceFromAndUnwrap<CcpServiceHostWrapper>(
                                                            domain,
                                                            Assembly.GetExecutingAssembly().Location,
                                                            serviceConfigFullPath,
                                                            onAzure,
                                                            ServiceHostRuntimeConfiguration.Standalone))
                {
                    host.Initialize();
                    host.Run();
                    TraceHelper.TraceInfo(
                        jobId,
                        "Sleep...");

                    if (ServiceHostRuntimeConfiguration.Standalone)
                    {
                        // Endless listening, till service info deleted.
                        while (true)
                        {
                            lock (ServiceInfo.s_lock)
                            {
                                if (SvcHostMgmtRestServer.Info == null)
                                {
                                    TraceHelper.TraceInfo(
                                        jobId,
                                        "Close service host!");
                                    host.Dispose();
                                    return 0;
                                }
                            }
                            Thread.Sleep(1000);
                        }
                    }
                    else
                    {
                        // Endless waiting, till it's being killed
                        Thread.Sleep(Timeout.Infinite);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                TraceHelper.TraceError(
                    jobId,
                    e.ToString());

                // Failed to open service, fall back to open an dummy service.
                bOpenDummy = true;
            }
            finally
            {
                if (bOpenDummy)
                {
                    OpenDummyService(onAzure);
                }
            }

            return 0;
        }

        static void OpenDummyService(bool onAzure)
        {
            TraceHelper.TraceInfo(
                jobId,
                "Open dummy service...");

            AppDomain domain = CreateNewServiceDomain(null, null);

            using (CcpServiceHostWrapper host =
                CreateInstanceFromAndUnwrap<CcpServiceHostWrapper>(
                                                        domain,
                                                        Assembly.GetExecutingAssembly().Location,
                                                        string.Empty,
                                                        onAzure,
                                                        ServiceHostRuntimeConfiguration.Standalone))
            {
                host.Run();
                // Endless waiting, till it's being killed
                Thread.Sleep(Timeout.Infinite);
            }
        }

        /// <summary>
        /// Get the service info from registry (x64 boot) or info file (x86 boot)
        /// </summary>
        static string GetServiceInfo(string serviceConfigFileName)
        {
            string serviceConfigFile = string.Empty;

            try
            {
                //TODO get headnode
                string headnode; 
                if (ServiceHostRuntimeConfiguration.Standalone)
                { 
                    headnode = System.Net.Dns.GetHostName();
                }
                else
                {
                    headnode = EndpointsConnectionString.LoadFromEnvVarsOrWindowsRegistry().ConnectionString;
                }
                Debug.WriteLine($"[{nameof(GetServiceInfo)}](Debug) headnode: {headnode}.");
                Debug.Assert(!string.IsNullOrEmpty(headnode), "Head node connection string is null or empty.");
                ServiceRegistrationRepo serviceRegistration = new ServiceRegistrationRepo(Environment.GetEnvironmentVariable(Constant.RegistryPathEnv), null);

                // Get the path to the service config file name
                serviceConfigFile = serviceRegistration.GetServiceRegistrationPath(serviceConfigFileName);

                if (serviceConfigFile == null)
                {
                    // Make a part for the error message
                    string CentrialPath = serviceRegistration.CentrialPath;

                    StringBuilder serviceRegDirsBuilder = new StringBuilder();
                    if (!string.IsNullOrEmpty(CentrialPath))
                    {
                        serviceRegDirsBuilder.Append("\n\t");
                        serviceRegDirsBuilder.Append(CentrialPath);
                    }

                    serviceRegDirsBuilder.Append("\n");

                    Console.Error.WriteLine(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            StringTable.CantFindServiceRegistrationFileUnderFolders,
                            serviceRegDirsBuilder.ToString()));
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(
                    string.Format(CultureInfo.CurrentCulture, StringTable.ExceptionInReadingRegistrationFile, serviceConfigFile, e.ToString()));
            }

            return serviceConfigFile;
        }

        /// <summary>
        /// Create a new domain and set the service assembly path to be its application base,
        /// and the service configuration file to be its configuration file.
        /// </summary>
        /// <param name="serviceAssemblyFileName"></param>
        /// <param name="jobId"></param>
        /// <param name="taskId"></param>
        /// <returns></returns>
        private static AppDomain CreateNewServiceDomain(string configPath, string serviceAssemblyFileName)
        {
            AppDomain domain = null;
            AppDomainSetup ads = new AppDomainSetup();
            if (!string.IsNullOrEmpty(serviceAssemblyFileName))
            {
                ads.ApplicationBase = Path.GetDirectoryName(Path.GetFullPath(serviceAssemblyFileName));
            }
            else
            {
                ads.ApplicationBase = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            }

            if (!string.IsNullOrEmpty(configPath))
            {
                ads.ConfigurationFile = configPath;
            }

            Evidence evd = new Evidence(AppDomain.CurrentDomain.Evidence);
            evd.AddHost(SecurityZone.MyComputer);

            PermissionSet permSet = new PermissionSet(PermissionState.Unrestricted);
            permSet.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

            domain = AppDomain.CreateDomain("ServiceDomain", evd, ads, permSet);
            return domain;
        }

        /// <summary>
        /// Cross domain creating instance
        /// The be-created class and all args of its constructor must be either Serializable or MarshalByRef
        /// </summary>
        /// <typeparam name="ResultType"></typeparam>
        /// <param name="appDomain"></param>
        /// <param name="assemblyFile"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static ResultType CreateInstanceFromAndUnwrap<ResultType>(
                                                                        AppDomain appDomain,
                                                                        string assemblyFile,
                                                                        params object[] args)
        {
            Debug.Assert(appDomain != null);
            Debug.Assert(!string.IsNullOrEmpty(assemblyFile));

            return (ResultType)appDomain.CreateInstanceFromAndUnwrap(
                assemblyFile,
                typeof(ResultType).FullName, false, 0, null, args, null, null, null);
        }

        /// <summary>
        /// If HpcServiceHost is launched to run a pre/post task, launch the pre/post task
        /// </summary>
        private static bool RunPrePostTask(ref uint exitCode)
        {
            bool prePostTaskExists = false;
            string prePostTaskCommandLine = Environment.GetEnvironmentVariable(Constant.PrePostTaskCommandLineEnvVar);

            // Check if pre/post task exists
            prePostTaskExists = !string.IsNullOrEmpty(prePostTaskCommandLine);

            // if so run it
            if (prePostTaskExists)
            {
                string serviceWorkingDirectory = null;

                // Working directory is the service assembly's directory. If we are on Azure, change to the service package's dir. If
                //  not on azure use service assemblies directory from service config which is passed to svchost via env var so we dont 
                //  need to read the svc config file again on every CN (which is slow)
                if (SoaHelper.IsOnAzure())
                {
                    serviceWorkingDirectory = Utility.GetServiceLocalCacheFullPath();
                }
                else
                {
                    serviceWorkingDirectory = Environment.ExpandEnvironmentVariables(
                        Environment.GetEnvironmentVariable(Constant.PrePostTaskOnPremiseWorkingDirEnvVar));
                }

                NativeMethods.STARTUPINFO startupInfo = new NativeMethods.STARTUPINFO();
                startupInfo.cb = Marshal.SizeOf(typeof(NativeMethods.STARTUPINFO));

                NativeMethods.PROCESS_INFORMATION processInfo = new NativeMethods.PROCESS_INFORMATION();

                StringBuilder commandLine = new StringBuilder();

                // Run command from comspec (like node manager babysitter) to ensure env vars are expanded and command runs as if launched from node manager
                commandLine.AppendFormat("\"{0}\" /S /c \"{1}\"", Environment.GetEnvironmentVariable("ComSpec"), prePostTaskCommandLine);

                TraceHelper.TraceInfo(
                    jobId,
                    "Executing '{0}'",
                    prePostTaskCommandLine);

                // Start the task
                bool ret = NativeMethods.CreateProcess(null,
                                                        commandLine,
                                                        IntPtr.Zero,
                                                        IntPtr.Zero,
                                                        true,
                                                        NativeMethods.CREATE_UNICODE_ENVIRONMENT,
                                                        IntPtr.Zero,
                                                        serviceWorkingDirectory,
                                                        ref startupInfo,
                                                        out processInfo);

                // If CreateProcess succeeded
                if (ret)
                {
                    using (SafeWaitHandle processHandle = new SafeWaitHandle(processInfo.hProcess, true))
                    using (SafeWaitHandle threadHandle = new SafeWaitHandle(processInfo.hThread, true))
                    {
                        if (processHandle.IsClosed || processHandle.IsInvalid)
                        {
                            TraceHelper.TraceError(
                                jobId,
                                "Process handle is invalid or closed. Task commandline = {0}",
                                commandLine);

                            exitCode = 1;
                            return true;
                        }

                        // Wait for task to complete
                        NativeMethods.WaitForSingleObject(processInfo.hProcess, Timeout.Infinite);

                        // Trace the results
                        NativeMethods.GetExitCodeProcess(new SafeProcessHandle(processInfo.hProcess, false), out exitCode);

                        TraceHelper.TraceInfo(
                            jobId,
                            "ExitCode = {0}",
                            exitCode);
                    }
                }
                else
                {
                    int errorCode = Marshal.GetLastWin32Error();

                    TraceHelper.TraceError(
                        jobId,
                        "Cannot start pre/post task: '{0}'. Exit code = {1}",
                        prePostTaskCommandLine, errorCode);

                    exitCode = (uint)errorCode;
                }
            }

            return prePostTaskExists;
        }
    }
}
