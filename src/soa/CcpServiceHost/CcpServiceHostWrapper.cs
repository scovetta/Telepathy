// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.CcpServiceHosting
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;

    using TelepathyCommon;

    using RuntimeTraceHelper = Microsoft.Hpc.RuntimeTrace.TraceHelper;

    /// <summary>
    /// This class works in the newly created domain. 
    /// Its application path is service's path.
    /// Its configure file is service's configure file.
    /// Its main job includes:
    /// 1. Fetch the configurations from env and service config file
    /// 2. Create a CcpServiceHost in the current domain,
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Created by CreateInstanceFromAndUnwrap")]
    public class CcpServiceHostWrapper : MarshalByRefObject, IDisposable
    {
        // Represents how an endpoint address is constructed
        // net.tcp://<wcfnetworkprefix>.<nodename>:<port>/<jobId>/<taskId>
        private const string BaseAddrTemplate = "net.tcp://{0}:{1}/{2}/{3}";

        /// <summary>
        /// env var passes net.tcp://ip:port to task in azure
        /// </summary>
        private const string BaseAddrTemplateOnAzure = "{0}/{1}/{2}";

        /// <summary>
        /// Session API library name
        /// </summary>
        private const string SessionAssemblyName = "Microsoft.Hpc.Scheduler.Session.dll";

        /// <summary>
        /// Data movement library name
        /// </summary>
        private const string DataMovementAssemblyName = "Microsoft.WindowsAzure.Storage.DataMovement.dll";

        /// <summary>
        /// Storage client library name
        /// </summary>
        private const string StorageClientAssemblyName = "Microsoft.WindowsAzure.Storage.dll";

        // Timeout for Exiting event to execute. This must be the same as node manager's default CTRL+C timeout
        private int _cancelTaskGracePeriod = Constant.DefaultCancelTaskGracePeriod;

        private string _jobId;
        private string _taskId;
        private int _procNum;

        private string _serviceTypeName;
        private string _serviceContractName;
        private string _serviceAssemblyFileName;

        private int _maxConcurrentCalls;
        private bool _includeFaultedException;

        private int _retryTimeoutInMilliSecond;
        private const int defaultRetryTimeout = 60 * 1000; // 60s

        // The real WCF service host enclosed
        private ServiceHost _host;
        private ServiceHost _hostController;

        /// <summary>
        /// Enable the new feature MessageLevelPreemption or not.
        /// </summary>
        private bool enableMessageLevelPreemption;

        /// <summary>
        /// This flag is set when host receives Ctrl-B event.
        /// </summary>
        private bool receivedCancelEvent;

        /// <summary>
        /// It is used as a sync object.
        /// </summary>
        private object syncObjOnExitingCalled = new object();

        /// <summary>
        /// This flag indicates if the OnExiting event is triggered.
        /// </summary>
        private bool isOnExitingCalled;

        /// <summary>
        /// This list stores ids of messages which are invoking the hosted service.
        /// When the response is sent back to the broker, the id is removed from this list.
        /// If a message come to host after Ctrl-B event, its id won't be saved in this list.
        /// </summary>
        private ArrayList processingMessageIds = ArrayList.Synchronized(new ArrayList());

        /// <summary>
        /// This list stores ids of all the messages which are skipped by the host.
        /// </summary>
        private ArrayList skippedMessageIds = ArrayList.Synchronized(new ArrayList());

        /// <summary>
        /// This list stores all the messages the service host is dealing with.
        /// </summary>
        private ArrayList allMessageIds = ArrayList.Synchronized(new ArrayList());

        /// <summary>
        /// id of the core running this task
        /// </summary>
        private int coreId;

        /// <summary>
        /// the service config file name
        /// </summary>
        private string _serviceConfigFile;

        /// <summary>
        /// on azure or not
        /// </summary>
        private bool _onAzure;

        /// <summary>
        /// The service host idle time out, in milliseconds
        /// </summary>
        private int serviceHostIdleTimeout;


        private bool _standAlone;

        /// <summary>
        /// The service host idle time out, in milliseconds
        /// </summary>
        public int ServiceHostIdleTimeout
        {
            get { return serviceHostIdleTimeout; }
        }

        /// <summary>
        /// The service host idle timer
        /// </summary>
        private Timer serviceHostIdleTimer;

        /// <summary>
        /// The service host idle timer
        /// </summary>
        public Timer SerivceHostIdleTimer
        {
            get { return serviceHostIdleTimer; }
        }

        /// <summary>
        /// The service hang time out, in milliseconds
        /// </summary>
        private int serviceHangTimeout;

        /// <summary>
        /// Get the service hang time out, in milliseconds
        /// </summary>
        public int ServiceHangTimeout
        {
            get { return serviceHangTimeout; }
        }

        /// <summary>
        /// The service hang timer
        /// </summary>
        private Timer serviceHangTimer;

        /// <summary>
        /// Get the service hang timer
        /// </summary>
        public Timer ServiceHangTimer
        {
            get { return serviceHangTimer; }
        }

        public CcpServiceHostWrapper(string serviceConfigFile, bool onAzure, bool standAlone)
        {
            // Here we put the resolve handler in case there is assembly not found exception.
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;

            _onAzure = onAzure;
            _serviceConfigFile = serviceConfigFile;
            _standAlone = standAlone;
        }

        /// <summary>
        /// Initialize this CcpServiceHostWrapper instance
        /// </summary>
        public void Initialize()
        {
            // Set soa diag trace level.
            Utility.SetTraceSwitchLevel();

            // Initialize Ctrl-Break handler to receive shutdown events from node manager
            this.InvokeInitializeControlBreakHandler();

            // initialize the service host idle timer
            this.serviceHostIdleTimer = new Timer(new TimerCallback(this.ServiceHostIdleCallBack), null, Timeout.Infinite, Timeout.Infinite);

            // initialize the service hang timer
            this.serviceHangTimer = new Timer(new TimerCallback(this.ServiceHangCallBack), null, Timeout.Infinite, Timeout.Infinite);
        }

        private int GetEnvironmentVariables()
        {
            string jobIdEnvVar = Environment.GetEnvironmentVariable(Constant.JobIDEnvVar);

            if (string.IsNullOrEmpty(jobIdEnvVar))
            {
                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, StringTable.CantFindJobId);
                return ErrorCode.ServiceHost_UnexpectedException;
            }
            else
            {
                this._jobId = jobIdEnvVar;
            }

            RuntimeTraceHelper.RuntimeTrace.LogHostStart(_jobId);

            string taskIdEnvVar = Environment.GetEnvironmentVariable(Constant.TaskIDEnvVar);

            if (string.IsNullOrEmpty(taskIdEnvVar))
            {
                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, StringTable.CantFindTaskId);
                return ErrorCode.ServiceHost_UnexpectedException;
            }
            else
            {
                this._taskId = taskIdEnvVar;
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Verbose,
                "[HpcServiceHost]: Task Id = {0}",
                _taskId);

            string procNumEnvVar = Environment.GetEnvironmentVariable(Constant.ProcNumEnvVar);
            Debug.WriteLine($"{Constant.ProcNumEnvVar}={procNumEnvVar}");
            string overrideProcNumEnvVar = Environment.GetEnvironmentVariable(Constant.OverrideProcNumEnvVar);
            Debug.WriteLine($"{Constant.OverrideProcNumEnvVar}={overrideProcNumEnvVar}");

            if (bool.TryParse(overrideProcNumEnvVar, out var ov) && ov)
            {
                _procNum = Environment.ProcessorCount;
            }
            else
            {
                if (string.IsNullOrEmpty(procNumEnvVar) || !int.TryParse(procNumEnvVar, out _procNum))
                {
                    RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, StringTable.CantFindProcNum);
                    return ErrorCode.ServiceHost_UnexpectedException;
                }
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Verbose,
                "[HpcServiceHost]: Number of processors (service capability) = {0}",
                _procNum);

            string strServiceInitializationTimeout = Environment.GetEnvironmentVariable(Constant.ServiceInitializationTimeoutEnvVar);
            if (string.IsNullOrEmpty(strServiceInitializationTimeout) ||
               !int.TryParse(strServiceInitializationTimeout, out _retryTimeoutInMilliSecond))
            {
                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Warning,
                    "[HpcServiceHost]: invalid serviceInitializationTimeout value. Fall back to default value = 60s");

                _retryTimeoutInMilliSecond = defaultRetryTimeout;
            }

            string cancelTaskGracePeriodEnvVarStr = Environment.GetEnvironmentVariable(Constant.CancelTaskGracePeriodEnvVar);

            if (!String.IsNullOrEmpty(cancelTaskGracePeriodEnvVarStr))
            {
                int cancelTaskGracePeriod = 0;

                if (Int32.TryParse(cancelTaskGracePeriodEnvVarStr, out cancelTaskGracePeriod))
                {
                    // Convert to ms from sec
                    _cancelTaskGracePeriod = cancelTaskGracePeriod * 1000;
                }
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Information,
                "[HpcServiceHost]: Cancel Task Grace Period = {0}",
                _cancelTaskGracePeriod);

            // parse the coreID list to get the first allocate core
            // the coreids is like "0 1 2 5" if allocate 4 cores
            string CoreIds = Environment.GetEnvironmentVariable(Constant.CoreIdsEnvVar);
            if (!String.IsNullOrEmpty(CoreIds))
            {
                string[] cores = CoreIds.Split(' ');
                if (cores.Length < 1 || !Int32.TryParse(cores[0], out coreId))
                {
                    RuntimeTraceHelper.TraceEvent(
                        this._jobId,
                        TraceEventType.Warning,
                        "[HpcServiceHost]: Fail to get CoreId use default value.");
                }
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Information,
                "[HpcServiceHost]: First Allocated CoreId = {0}", coreId);

            // get the preemption switcher from the env var, the default value is true.
            string preemption = Environment.GetEnvironmentVariable(Constant.EnableMessageLevelPreemptionEnvVar);
            if (!bool.TryParse(preemption, out this.enableMessageLevelPreemption))
            {
                this.enableMessageLevelPreemption = true;
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Information,
                "[HpcServiceHost]: EnableMessageLevelPreemption = {0}",
                enableMessageLevelPreemption);

            string serviceHostIdleTimeoutEnvVar = Environment.GetEnvironmentVariable(Constant.ServiceHostIdleTimeoutEnvVar);

            if (string.IsNullOrEmpty(serviceHostIdleTimeoutEnvVar) || !int.TryParse(serviceHostIdleTimeoutEnvVar, out serviceHostIdleTimeout))
            {
                RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Warning,
                "[HpcServiceHost]: Failed to obtain or parse ServiceHostIdleTimeout {0} from env, set it to Timeout.Infinite as default.",
                serviceHostIdleTimeoutEnvVar);

                // set to Timeout.Infinite if not specified or cannot be parsed.
                serviceHostIdleTimeout = Timeout.Infinite;
            }

            string serviceHangTimeoutEnvVar = Environment.GetEnvironmentVariable(Constant.ServiceHangTimeoutEnvVar);

            if (string.IsNullOrEmpty(serviceHangTimeoutEnvVar) || !int.TryParse(serviceHangTimeoutEnvVar, out serviceHangTimeout))
            {
                RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Warning,
                "[HpcServiceHost]: Failed to obtain or parse ServiceHangTimeout {0} from env, set it to Timeout.Infinite as default.",
                serviceHangTimeoutEnvVar);

                // set to Timeout.Infinite if not specified or cannot be parsed.
                serviceHangTimeout = Timeout.Infinite;
            }

            return ErrorCode.Success;
        }

        private int GetServiceInfo()
        {
            ServiceRegistration registration;
            int errorCode = Utility.GetServiceRegistration(_serviceConfigFile, _onAzure, out registration, out _serviceAssemblyFileName);
            if (errorCode != ErrorCode.Success)
            {
                return errorCode;
            }

            string assemblyConfigFile = string.Concat(_serviceAssemblyFileName, ".config");
            if (File.Exists(assemblyConfigFile))
            {
                // We discard the <assembly_name>.dll.config in v3. Everything goes to service
                // registration file. To improve the discoverability of this break change, we
                // want to add a check if this dll.config exists with the service assembly.

                // this is used by diagnostics, don't delete
                RuntimeTraceHelper.RuntimeTrace.LogHostServiceConfigCheck(assemblyConfigFile);
            }

            _serviceContractName = registration.Service.ContractType;
            _serviceTypeName = registration.Service.ServiceType;

            _maxConcurrentCalls = registration.Service.MaxConcurrentCalls == 0 ? _procNum : registration.Service.MaxConcurrentCalls;
            Debug.WriteLine($"{nameof(this._maxConcurrentCalls)}={this._maxConcurrentCalls}");
            _includeFaultedException = registration.Service.IncludeExceptionDetailInFaults;

            // we have to use this to avoid to load the service config in main domain
            AppDomain.CurrentDomain.AppendPrivatePath(Path.GetDirectoryName(_serviceAssemblyFileName));

            return ErrorCode.Success;
        }

        /// <summary>
        /// Load the assembly from some customized path, if it cannot be found automatically.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">A System.ResolveEventArgs that contains the event data.</param>
        /// <returns>targeted assembly</returns>
        static Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            AssemblyName targetAssemblyName = null;
            try
            {
                targetAssemblyName = new AssemblyName(args.Name);
            }
            catch (ArgumentException)
            {
                return null;
            }

            if (targetAssemblyName != null)
            {
                foreach (string assemblyName in new string[] { SessionAssemblyName, DataMovementAssemblyName, StorageClientAssemblyName })
                {
                    // Microsoft.Hpc.Scheduler.Session.dll is required by the service registration file,
                    // but it is not installed in GAC on the Azure node. So need to load it from home folder.
                    //
                    // Microsoft.Hpc.Azure.Datamovement.dll and Microsft.WindowsAzure.StorageClient.dll
                    // are also located in home folder, load it explicitly.
                    if (targetAssemblyName.Name.Equals(Path.GetFileNameWithoutExtension(assemblyName), StringComparison.OrdinalIgnoreCase))
                    {
                        string homeFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string targetedAssemblyPath = Path.Combine(homeFolder, assemblyName);

                        try
                        {
                            return Assembly.LoadFrom(targetedAssemblyPath);
                        }
                        catch (Exception ex)
                        {
                            RuntimeTraceHelper.TraceEvent(
                                TraceEventType.Warning,
                                "[CcpServiceHostWrapper].ResolveHandler: Exception {0} while load assembly {1}",
                                ex,
                                targetedAssemblyPath);

                            return null;
                        }
                    }
                }
            }

            return null;
        }

        public void Run()
        {
            int errorCode = ErrorCode.Success;
            string errorMsg = string.Empty;

            int retryWaitPeriodInMilliSecond = 500;  // initially retry wait period is 0.5 second
            Stopwatch serviceStartTimeWatch = new Stopwatch();

            errorCode = GetEnvironmentVariables();
            if (errorCode != ErrorCode.Success)
            {
                return;
            }

            uint retry = 0;
            serviceStartTimeWatch.Start();
            while (true)
            {
                if (string.IsNullOrEmpty(this._serviceConfigFile))
                {
                    // service configuration file is not found
                    errorCode = ErrorCode.ServiceHost_ServiceRegistrationFileNotFound;
                    errorMsg = "[HpcServiceHost]: Cannot find service registration file.";
                    RuntimeTraceHelper.TraceEvent(
                        this._jobId,
                        TraceEventType.Error,
                        errorMsg);
                    break;
                }

                try
                {
                    errorCode = RunInternal();

                    if (retry == uint.MaxValue)
                    {
                        retry = 0;
                    }
                    else
                    {
                        retry++;
                    }

                    if (errorCode != ErrorCode.Success)
                    {
                        if (errorCode < ErrorCode.ServiceHost_ExitCode_Start ||
                            errorCode > ErrorCode.ServiceHost_ExitCode_End)
                        {
                            errorCode = ErrorCode.ServiceHost_UnexpectedException;
                        }

                        errorMsg = string.Format(CultureInfo.CurrentCulture, StringTable.FailedInStartingService);
                        RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, errorMsg);
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.Message);

                    RuntimeTraceHelper.TraceError(
                        this._jobId,
                        e.ToString());

                    errorCode = ErrorCode.ServiceHost_UnexpectedException;
                    errorMsg = e.Message;
                }

                if (errorCode == ErrorCode.ServiceHost_AssemblyFileNotFound
                    || errorCode == ErrorCode.ServiceHost_ServiceTypeLoadingError
                    || errorCode == ErrorCode.ServiceHost_NoContractImplemented)
                {
                    if (retry >= 3)
                    {
                        break;
                    }
                }

                if (errorCode == ErrorCode.Success)
                {
                    break;
                }

                serviceStartTimeWatch.Stop();
                // if retry timeout is reached, done 
                int elapsedStartTimeInMilliSecond = (int)serviceStartTimeWatch.ElapsedMilliseconds;
                if (elapsedStartTimeInMilliSecond > _retryTimeoutInMilliSecond)
                {
                    break;
                }

                serviceStartTimeWatch.Start();

                if (elapsedStartTimeInMilliSecond + retryWaitPeriodInMilliSecond > _retryTimeoutInMilliSecond)
                {
                    retryWaitPeriodInMilliSecond = _retryTimeoutInMilliSecond - elapsedStartTimeInMilliSecond;
                }

                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Information,
                    "Wait {0} milliseconds and retry",
                    retryWaitPeriodInMilliSecond);

                // wait and retry
                Thread.Sleep(retryWaitPeriodInMilliSecond);

                // back off
                retryWaitPeriodInMilliSecond *= 2;
            }

            // if AddressAlreadyInUse, exit the service host with the specific exit code for scheduler force resync
            if (errorCode == ErrorCode.ServiceHost_ServiceHostFailedToOpen_AddressAlreadyInUse)
            {
                Environment.Exit(ProductConstants.ErrorCodes.ForceNodeManagerResync);
            }

            if (errorCode != ErrorCode.Success)
            {
                RunAsDummy();
            }
        }

        /// <summary>
        /// Open the service host.
        /// All exceptions will be directly thrown out. So the outer program must know
        /// how to deal with those exceptions.
        /// </summary>
        private int RunInternal()
        {
            // Initialize
            int error = GetServiceInfo();
            if (error != ErrorCode.Success)
            {
                return error;
            }

            RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Verbose, "[HpcServiceHost]: Start opening");

            #region Load service assemblies
            // Load the service assembly from the specified path

            Assembly asm = Assembly.LoadFrom(_serviceAssemblyFileName);
            if (asm == null)
            {
                return ErrorCode.ServiceHost_AssemblyLoadingError;
            }

            RuntimeTraceHelper.RuntimeTrace.LogHostAssemblyLoaded(this._jobId);

            // Try to auto discover the service contract interface from the assembly
            if (string.IsNullOrEmpty(_serviceContractName))
            {
                Type[] allTypes = asm.GetTypes();

                foreach (Type tmpType in allTypes)
                {
                    object[] snAttrs = tmpType.GetCustomAttributes(typeof(ServiceContractAttribute), false);

                    if (tmpType.IsInterface && snAttrs != null && snAttrs.Length > 0)
                    {
                        _serviceContractName = tmpType.FullName;  // the first interface type that has the service contract attribute
                        break;
                    }
                }

                // Can't find the contract from the assembly.
                // Notice: generic service's contract is in the session.dll, so can't be found in the assembly.
                // User should specified it in the service registration file.
                if (string.IsNullOrEmpty(_serviceContractName))
                {
                    return ErrorCode.ServiceHost_ContractDiscoverError;
                }
            }

            // Try to auto discover the service type from the assembly

            if (string.IsNullOrEmpty(_serviceTypeName))
            {
                Type[] allTypes = asm.GetTypes();

                foreach (Type tmpType in allTypes)
                {
                    if (tmpType.GetInterface(_serviceContractName, true) != null)
                    {
                        _serviceTypeName = tmpType.FullName;     // The first type implementing the service contract interface
                        break;
                    }
                }

                if (_serviceTypeName == null) // Not found
                {
                    return ErrorCode.ServiceHost_ServiceTypeDiscoverError;
                }
            }

            // Load the service type

            Type serviceType = asm.GetType(_serviceTypeName);
            if (serviceType == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, StringTable.CantFindServiceType, _serviceTypeName, asm.FullName);
                Console.Error.WriteLine(message);
                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, message);
                return ErrorCode.ServiceHost_ServiceTypeLoadingError;
            }

            // Check whether the service contract is implemented by that type

            Type serviceContractInterface = serviceType.GetInterface(_serviceContractName, true);
            if (serviceContractInterface == null)
            {
                string message = string.Format(CultureInfo.CurrentCulture, StringTable.CantFindServiceContract, _serviceContractName, asm.FullName);
                Console.Error.WriteLine(message);
                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, message);
                return ErrorCode.ServiceHost_NoContractImplemented;
            }

            object[] attrs = serviceContractInterface.GetCustomAttributes(typeof(ServiceContractAttribute), false);
            if (attrs == null || attrs.Length == 0)
            {
                return ErrorCode.ServiceHost_NoContractImplemented;
            }

            #endregion

            string defaultBaseAddr = CreateEndpointAddress(PortHelper.ConvertToPort(coreId, false));

            // the backend binding returned by following method is un-secure on Azure.
            Binding binding = BindingHelper.GetBackEndBinding(out var isSecure);

            // Update backend binding's receive timeout and max message settings with global settings if they are enabled
            UpdateServiceBinding(binding);

            Uri listenUri;

            try
            {
                RuntimeTraceHelper.TraceInfo(
                    this._jobId,
                    "defaultBaseAddr = {0}",
                    defaultBaseAddr);

                _host = new ServiceHost(serviceType, new Uri(defaultBaseAddr));
                ServiceEndpoint endpoint = _host.AddServiceEndpoint(_serviceContractName, binding, "_defaultEndpoint");
                listenUri = endpoint.ListenUri;

                RuntimeTraceHelper.TraceInfo(
                   this._jobId,
                   "listenUri = {0}",
                   listenUri);

                BindingHelper.ApplyDefaultThrottlingBehavior(_host, _maxConcurrentCalls);

                // Add endpoint behavior
                TraceServiceBehavior tsb = new TraceServiceBehavior(this._jobId, this);
                endpoint.Behaviors.Add(tsb);

                // Add operation behavior if the config enables the message level preemption.
                if (this.enableMessageLevelPreemption)
                {
                    foreach (OperationDescription op in endpoint.Contract.Operations)
                    {
                        // Add a customized OperationBehavior in order to hook our OperationInvokerWrapper.
                        op.Behaviors.Add(new OperationBehavior(this));
                    }
                }

                // Add debug behavior
                ServiceDebugBehavior sdb = _host.Description.Behaviors.Find<ServiceDebugBehavior>();
                if (sdb == null)
                {
                    sdb = new ServiceDebugBehavior();
                    sdb.IncludeExceptionDetailInFaults = _includeFaultedException;
                    _host.Description.Behaviors.Add(sdb);
                }
                else
                {
                    sdb.IncludeExceptionDetailInFaults = _includeFaultedException;
                }

                if (!_onAzure && isSecure && !_standAlone)
                {
                    _host.Authorization.ServiceAuthorizationManager = new BrokerNodeAuthManager(this._jobId);
                }

                RuntimeTraceHelper.TraceInfo(
                   this._jobId,
                   "Try to call _host.Open() below");
                _host.Open();

                RuntimeTraceHelper.TraceInfo(
                   this._jobId,
                   "Try to open host controller below");

                string endpointAddress = CreateEndpointAddress(PortHelper.ConvertToPort(coreId, true));
                OpenHostController(endpointAddress, binding, isSecure);
            }
            catch (AddressAlreadyInUseException e)
            {
                Console.Error.WriteLine(e.Message);

                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: {0}", e);

                if (_host != null)
                {
                    try
                    {
                        if (_host.State != CommunicationState.Faulted)
                        {
                            _host.Close();
                        }
                        else
                        {
                            _host.Abort();
                        }
                    }
                    catch (Exception exp)
                    {
                        RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: Service host failed to close {0}", exp);
                    }

                    _host = null;
                }

                if (_hostController != null)
                {
                    try
                    {
                        if (_hostController.State != CommunicationState.Faulted)
                        {
                            _hostController.Close();
                        }
                        else
                        {
                            _hostController.Abort();
                        }
                    }
                    catch (Exception exp)
                    {
                        RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: Service host controller failed to close {0}", exp);
                    }

                    _hostController = null;
                }

                return ErrorCode.ServiceHost_ServiceHostFailedToOpen_AddressAlreadyInUse;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: {0}", e);

                if (_host != null)
                {
                    try
                    {
                        if (_host.State != CommunicationState.Faulted)
                        {
                            _host.Close();
                        }
                        else
                        {
                            _host.Abort();
                        }
                    }
                    catch (Exception exp)
                    {
                        RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: Service host failed to close {0}", exp);
                    }

                    _host = null;
                }

                if (_hostController != null)
                {
                    try
                    {
                        if (_hostController.State != CommunicationState.Faulted)
                        {
                            _hostController.Close();
                        }
                        else
                        {
                            _hostController.Abort();
                        }
                    }
                    catch (Exception exp)
                    {
                        RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: Service host controller failed to close {0}", exp);
                    }

                    _hostController = null;
                }

                return ErrorCode.ServiceHost_ServiceHostFailedToOpen;
            }

            RuntimeTraceHelper.TraceEvent(
                this._jobId,
                TraceEventType.Verbose,
                "[HpcServiceHost]: Service host successfully opened on {0} ",
                listenUri);

            return ErrorCode.Success;
        }

        public bool RunAsDummy()
        {
            string defaultBaseAddr = CreateEndpointAddress(PortHelper.ConvertToPort(coreId, false));

            // the backend binding returned by following method is un-secure on Azure.
            bool isDefaultBackEndBinding;
            Binding binding = BindingHelper.GetBackEndBinding(out isDefaultBackEndBinding);

            // Update backend binding's receive timeout and max message settings with global settings if they are enabled
            UpdateServiceBinding(binding);

            // Note: use wcf default receive timeout.
            // TODO: configure ReceiveTimeout ?
            // binding.ReceiveTimeout = new TimeSpan(0, 0, 0, 0, timeout);

            _host = null;
            try
            {
                _host = new ServiceHost(typeof(DummyService), new Uri(defaultBaseAddr));
                ServiceEndpoint endpoint = _host.AddServiceEndpoint(typeof(DummyService), binding, "_defaultEndpoint");
                BindingHelper.ApplyDefaultThrottlingBehavior(_host);

                TraceServiceBehavior tsb = new TraceServiceBehavior(this._jobId, this);
                endpoint.Behaviors.Add(tsb);

                // Add operation behavior if the config enables the message level preemption.
                if (this.enableMessageLevelPreemption)
                {
                    foreach (OperationDescription op in endpoint.Contract.Operations)
                    {
                        // Add a customized OperationBehavior in order to hook our OperationInvokerWrapper.
                        op.Behaviors.Add(new OperationBehavior(this));
                    }
                }

                if (!_onAzure && !isDefaultBackEndBinding)
                {
                    _host.Authorization.ServiceAuthorizationManager = new BrokerNodeAuthManager(this._jobId);
                }

                _host.Open();

                OpenHostController(defaultBaseAddr, binding, isDefaultBackEndBinding);

                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: Dummy service opened on {0}",
                    endpoint.ListenUri);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Error, "[HpcServiceHost]: {0}", e);

                if (_host != null)
                {
                    try
                    {
                        _host.Close();
                    }
                    catch (Exception)
                    {
                        // do nothing
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Opens the HpcServiceHost controller service
        /// </summary>
        /// <param name="defaultBaseAddr"></param>
        /// <param name="binding"></param>
        /// <param name="isSecure">indicate if the binding is secure</param>
        /// <remarks>If we disable port sharing on CNs/WNs, we need to open another port</remarks>
        private void OpenHostController(string defaultBaseAddr, Binding binding, bool isDefaultBackEndBinding)
        {
            RuntimeTraceHelper.TraceInfo(
                this._jobId,
                "defaultBaseAddr of HostController is {0}",
                defaultBaseAddr);

            _hostController = new ServiceHost(new HpcServiceHost(this._jobId, _cancelTaskGracePeriod, this), new Uri(defaultBaseAddr));
            RuntimeTraceHelper.TraceInfo(
                this._jobId,
                "Created ServiceHost for controller.");

            _hostController.AddServiceEndpoint(typeof(IHpcServiceHost), binding, @"_defaultEndpoint/controller");
            RuntimeTraceHelper.TraceInfo(
                this._jobId,
                "Added endpoint to controller.");

            if (!_onAzure && !isDefaultBackEndBinding)
            {
                _hostController.Authorization.ServiceAuthorizationManager = new BrokerNodeAuthManager(this._jobId);
            }

            RuntimeTraceHelper.TraceInfo(
                this._jobId,
                "Try to call _hostController.Open() below.");

            _hostController.Open();

            RuntimeTraceHelper.TraceInfo(
                this._jobId,
                "Controller opened.");
        }

        private string CreateEndpointAddress(int port)
        {
            // The format of default base addr:
            // net.tcp://<wcfnetworkprefix>.<nodename>:<port>/<jobId>/<taskId>

            string wcfNetworkPrefix = null;

            if (SoaHelper.IsOnAzure())
            {
                // Azure node does not need network prefix
                wcfNetworkPrefix = string.Empty;
            }
            else if (SoaHelper.IsWorkstationNode())
            {
                // workstation does not need network prefix
                wcfNetworkPrefix = string.Empty;
            }
            else
            {
                wcfNetworkPrefix = Environment.GetEnvironmentVariable(Constant.NetworkPrefixEnv);
            }

            if (wcfNetworkPrefix != null)
            {
                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: WCF network prefix = {0}",
                    wcfNetworkPrefix);
            }
            else
            {
                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: WCF network prefix is not set.");
            }

            string hostnameWithPrefix = Environment.MachineName;

            if (!string.IsNullOrEmpty(wcfNetworkPrefix))
            {
                hostnameWithPrefix = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", wcfNetworkPrefix, hostnameWithPrefix);
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                BaseAddrTemplate,
                hostnameWithPrefix,
                port,
                _jobId,
                _taskId);
        }

        /// <summary>
        /// Create the base endpoint of the service host on azure
        /// </summary>
        /// <param name="env">env var name</param>
        /// <returns>endpoint address</returns>
        private string CreateBaseEndpointAddressOnAzure(string env)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                BaseAddrTemplateOnAzure,
                Environment.GetEnvironmentVariable(env),
                _jobId,
                _taskId);
        }

        /// <summary>
        /// Update backend binding's receive timeout and max message settings with global settings if they are enabled
        /// </summary>
        /// <param name="binding">backend binding to be updated</param>
        private void UpdateServiceBinding(Binding binding)
        {
            try
            {
                int serviceTimeout = 0;
                int maxMessageSize = 0;

                GetServiceSettings(out serviceTimeout, out maxMessageSize);

                if (serviceTimeout > 0)
                {
                    binding.ReceiveTimeout = new TimeSpan(0, 0, 0, 0, serviceTimeout);
                }

                if (maxMessageSize > 0)
                {
                    BindingHelper.ApplyMaxMessageSize(binding, maxMessageSize);
                }

                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: ServiceOperationTimeout = {0}, MaxMessageSize = {1}",
                    serviceTimeout,
                    maxMessageSize);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);

                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Verbose,
                    "[HpcServiceHost]: Error retrieving ServiceOperationTimeout. Defaulting to binding's default - {0}",
                    e.ToString());
            }
        }

        /// <summary>
        /// Get service's loadBalancing.serviceOperationTimeout and serviceConfiguration.MaxMessageSize;
        /// </summary>
        /// <returns></returns>
        private void GetServiceSettings(out int serviceOperationTimeout, out int maxMessageSize)
        {
            string maxMessageSizeString = Environment.GetEnvironmentVariable(Constant.ServiceConfigMaxMessageEnvVar);

            if (!Int32.TryParse(maxMessageSizeString, out maxMessageSize))
            {
                maxMessageSize = Constant.DefaultMaxMessageSize;
            }

            string serviceOperationTimeoutString = Environment.GetEnvironmentVariable(Constant.ServiceConfigServiceOperatonTimeoutEnvVar);

            if (!Int32.TryParse(serviceOperationTimeoutString, out serviceOperationTimeout))
            {
                serviceOperationTimeout = Constant.DefaultServiceOperationTimeout;
            }
        }

        /// <summary>
        ///  Replace the URI's hostname with the given one
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="newHostName"></param>
        /// <returns></returns>
        private static Uri ProcessUri(Uri uri, string newHostName)
        {
            string template = "{0}://{1}:{2}/";
            string oldBase = string.Format(CultureInfo.InvariantCulture, template, uri.Scheme, uri.Host, uri.Port);
            string relative = new Uri(oldBase).MakeRelativeUri(uri).ToString();
            string newBase = string.Format(CultureInfo.InvariantCulture, template, uri.Scheme, newHostName, uri.Port);

            return new Uri(new Uri(newBase), relative);
        }

        #region IDisposable Members

        public void Dispose()
        {
            // The rest commands can never be executed at the moment.
            // The service host can only be killed together with process.
            // But anyway we put it here for clean, in case someday it might be used in another way.

            // Stop and clean up the service host
            try
            {
                if (this._host != null)
                {
                    if (this._host.State != CommunicationState.Opened)
                    {
                        this._host.Abort();
                    }
                    else
                    {
                        this._host.Close();
                    }

                    this._host = null;
                }
            }
            catch (Exception)
            {
            }

            try
            {
                if (this._hostController != null)
                {
                    if (this._hostController.State != CommunicationState.Opened)
                    {
                        this._hostController.Abort();
                    }
                    else
                    {
                        this._hostController.Close();
                    }

                    this._hostController = null;
                }
            }
            catch (Exception)
            {
            }

            Interlocked.Exchange(ref this.serviceHostIdleTimer, null)?.Dispose();

            RuntimeTraceHelper.RuntimeTrace.LogHostStop(this._jobId);


            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        #endregion

        // Gets the value of enableMessageLevelPreemption.
        public bool EnableMessageLevelPreemption
        {
            get
            {
                return this.enableMessageLevelPreemption;
            }
        }

        // Gets the value of receivedCancelEvent.
        public bool ReceivedCancelEvent
        {
            get
            {
                return this.receivedCancelEvent;
            }
        }

        // Gets the value of this.syncObjOnExitingCalled.
        public object SyncObjOnExitingCalled
        {
            get
            {
                return this.syncObjOnExitingCalled;
            }
        }

        // Gets or sets the value of isOnExitingCalled.
        public bool IsOnExitingCalled
        {
            get
            {
                return this.isOnExitingCalled;
            }

            set
            {
                this.isOnExitingCalled = value;
            }
        }

        // Gets the value of processingMessageIds.
        public ArrayList ProcessingMessageIds
        {
            get
            {
                return this.processingMessageIds;
            }
        }

        // Gets the value of skippedMessageIds.
        public ArrayList SkippedMessageIds
        {
            get
            {
                return skippedMessageIds;
            }
        }

        // Gets the value of allMessageIds.
        public ArrayList AllMessageIds
        {
            get
            {
                return allMessageIds;
            }
        }

        /// <summary>
        /// Initializes Control+Break handler. Node manager uses this to all tasks to shutdown cleanly. ServiceHost's can intercept with
        /// OnExiting event. However the Control+Break handler must always be registered or ^C show up in stdout
        /// </summary>
        private void InvokeInitializeControlBreakHandler()
        {
            try
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler(this.Console_CancelKeyPress);
                AppDomain.CurrentDomain.DomainUnload += new EventHandler(this.CurrentDomain_DomainUnload);
            }
            catch (Exception e)
            {
                RuntimeTraceHelper.TraceEvent(
                    this._jobId,
                    TraceEventType.Warning,
                    "[HpcServiceHost]: Error initializing CTRL+BREAK handler for OnExiting - {0}",
                    e);
            }
        }

        /// <summary>
        /// hook the Ctrl-Break event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            RuntimeTraceHelper.TraceEvent(this._jobId, TraceEventType.Warning, "ServiceHost get canceled by scheduler");

            if (e.SpecialKey == ConsoleSpecialKey.ControlBreak)
            {
                this.receivedCancelEvent = true;

                try
                {
                    lock (this.SyncObjOnExitingCalled)
                    {
                        if (!this.isOnExitingCalled)
                        {
                            // Trigger OnExiting event as soon as Ctrl-B signal is received.
                            // Expose the SOAFaultCode.Service_Preempted error code to users through the EventArgs.
                            SOAEventArgs soaEventArgs = new SOAEventArgs(SOAFaultCode.Service_Preempted);
                            Type serviceContextType = typeof(ServiceContext);
                            serviceContextType.InvokeMember(
                                "FireExitingEvent",
                                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static,
                                null,
                                null,
                                new object[] { sender, soaEventArgs },
                                CultureInfo.CurrentCulture);

                            this.isOnExitingCalled = true;
                        }
                    }
                }
                catch (Exception excep)
                {
                    RuntimeTraceHelper.TraceEvent(
                        this._jobId,
                        TraceEventType.Error,
                        "[HpcServiceHost]: Calling ServiceContext.FireExitingEvent failed. {0}",
                        excep);
                }

                if (enableMessageLevelPreemption)
                {
                    if (this.processingMessageIds.Count == 0)
                    {
                        // Two cases can't depend on the response message to notice the broker that the host is preempted, so should exit process.
                        // (1) This service host never receives any message.
                        // (2) The Ctrl-B signal happens after the last response is sent back.
                        Environment.Exit(-1);
                    }
                    else
                    {
                        // Wait for broker to call Exit method after it receives all the responses.
                        Thread.Sleep(Timeout.Infinite);
                    }
                }
                else
                {
                    // Exit directly if message level preemption is disabled.
                    Environment.Exit(-1);
                }
            }
        }

        /// <summary>
        /// deregister the console event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            Console.CancelKeyPress -= new ConsoleCancelEventHandler(Console_CancelKeyPress);
        }

        /// <summary>
        /// Call back when service host idle time out
        /// </summary>
        private void ServiceHostIdleCallBack(object state)
        {
            RuntimeTraceHelper.TraceEvent(
                        this._jobId,
                        TraceEventType.Warning,
                        "[HpcServiceHost]: SERVICEHOST_IDLE_TIMEOUT.");

            Console.WriteLine("SERVICEHOST_IDLE_TIMEOUT.");
            Environment.Exit(0);
        }

        /// <summary>
        /// Call back when service hang time out
        /// </summary>
        private void ServiceHangCallBack(object state)
        {
            RuntimeTraceHelper.TraceEvent(
                        this._jobId,
                        TraceEventType.Warning,
                        "[HpcServiceHost]: SERVICE_HANG_TIMEOUT.");

            Console.WriteLine("SERVICE_HANG_TIMEOUT.");
            Environment.Exit(-3);
        }
    }
}
