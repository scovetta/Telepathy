//------------------------------------------------------------------------------
// <copyright file="Constant.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Global constants
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel;

    /// <summary>
    /// The Constant variables
    /// </summary>
    internal static class Constant
    {
        /// <summary>
        /// Stores the string separator
        /// </summary>
        internal const char WebAPI_GenericServiceRequestSeparator = '\x0000';

        /// <summary>
        /// Stores the user data separator
        /// </summary>
        internal const char WebAPI_GenericServiceUserDataSeparator = '\xffff';

        /// <summary>
        /// Stores the constant string indicating broker not exist for ping broker result
        /// </summary>
        internal const string PingBroker2Result_BrokerNotExist = "PingBroker2Result_BrokerNotExist";

        /// <summary>
        /// SOAP Action of broker's EOM message
        /// </summary>
        internal const string EndOfMessageAction = @"http://hpc.microsoft.com/EndOfGetResponse";

        /// <summary>
        /// SOAP Action of broker's Exit ServiceHost message
        /// </summary>
        internal const string ExitServiceHostAction = @"http://hpc.microsoft.com/ExitServiceHost";

        /// <summary>
        /// SOAP Action of passing servicehost binding info to broker proxy
        /// </summary>
        internal const string PassBindingAction = @"http://hpc.microsoft.com/PassBindingToProxy";

        /// <summary>
        /// SOAP Action of pinging broker proxy
        /// </summary>
        internal const string PingBrokerProxyAction = @"http://hpc.microsoft.com/PingBrokerProxy";

        /// <summary>
        /// SOAP Action of the heart beat for the broker on Azure
        /// </summary>
        internal const string BrokerHeartbeatAction = @"http://hpc.microsoft.com/BrokerHeartbeat";

        /// <summary>
        /// SOAP Action of invalid argument for SOA web service
        /// </summary>
        internal const string WebAPI_ArgumentExceptionFaultAction = @"http://hpc.microsoft.com/InvalidArgument";

        /// <summary>
        /// SOAP action of wrapper message of client side exception
        /// </summary>
        internal const string WebAPI_ClientSideException = @"http://hpc.microsoft.com/ClientSideExeption";

        /// <summary>
        /// Element Name of Message Array for Web API
        /// </summary>
        internal const string WSMessageArrayElementName = "HPCServer2008_WebAPI_MessageArray";

        /// <summary>
        /// Element name of user data SOAP header
        /// </summary>
        internal const string UserDataHeaderName = "HPCServer2008_Broker_UserDataNS";

        /// <summary>
        /// Element name of message Id SOAP header
        /// </summary>
        internal const string MessageIdHeaderName = "HPCServer2008_Broker_MessageIdNS";

        /// <summary>
        /// Element name of client id SOAP header
        /// </summary>
        internal const string ClientIdHeaderName = "HPCServer2008_Broker_ClientIdNS";

        /// <summary>
        /// Element name of user name SOAP header
        /// </summary>
        internal const string UserNameHeaderName = "HPCServer2008_Broker_UserNameNS";

        /// <summary>
        /// Element name of client instance id SOAP header
        /// </summary>
        internal const string ClientInstanceIdHeaderName = "HPCServer2008_Broker_ClientInstanceIdNS";

        /// <summary>
        /// Element name of request action SOAP header
        /// </summary>
        internal const string ActionHeaderName = "HPCServer2008_Broker_RequestActionNS";

        /// <summary>
        /// Element name of correlation id SOAP header
        /// </summary>
        internal const string CorrelationIdHeaderName = "HPCServer2008_Broker_CorrelationIdNS";

        /// <summary>
        /// Element name of dispatch id SOAP header
        /// </summary>
        internal const string DispatchIdHeaderName = "HPCServer2008_Broker_DispatchIdNS";

        /// <summary>
        /// Element name of security header
        /// </summary>
        internal const string SecurityHeaderName = "Security";

        /// <summary>
        /// Namespace of security header
        /// </summary>
        internal const string SecurityHeaderNamespace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        /// <summary>
        /// Fault code of retry limit exceed
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        internal static readonly FaultCode RetryLimitExceedFaultCode = FaultCode.CreateReceiverFaultCode("RetryOperationError", HpcHeaderNS);

        /// <summary>
        /// Fault code of authentication failure
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        internal static readonly FaultCode AuthenticationFailureFaultCode = FaultCode.CreateReceiverFaultCode("AuthenticationFailure", HpcHeaderNS);

        /// <summary>
        /// Fault code of broker proxy exceptions
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        internal static readonly FaultCode ProxyFaultCode = FaultCode.CreateReceiverFaultCode("ProxyEncountersException", HpcHeaderNS);

        /// <summary>
        /// Fault code for ArgumentException in SOA Web service
        /// </summary>
        internal static readonly FaultCode WebAPI_ArgumentExceptionFaultCode = FaultCode.CreateReceiverFaultCode("InvalidArgument", HpcHeaderNS);

        /// <summary>
        /// the env string for diagnostic broker node
        /// </summary>
        internal const string DiagnosticBrokerNode = "HPCDiagnosticBroker";

        /// <summary>
        /// Namespace of user data SOAP header
        /// </summary>
        internal const string HpcHeaderNS = "http://www.microsoft.com/hpc";

        /// <summary>
        /// Stores the prefix of the namespace
        /// </summary>
        internal const string HpcNSPrefix = "hpc";

        /// <summary>
        /// Element name of client callback ID header
        /// </summary>
        internal const string ResponseCallbackIdHeaderName = "HPCServer2008_Broker_ResponseCallbackId";

        /// <summary>
        /// Namespace name of client callback ID header
        /// </summary>
        internal const string ResponseCallbackIdHeaderNS = HpcHeaderNS;

        /// <summary>
        /// Maximum length of user data SOAP header
        /// </summary>
        internal const int MaxUserDataLen = 1024;

        /// <summary>
        /// Maximum size of message 
        /// TODO: Need to pull from current binding
        /// </summary>
        internal const int MaxBufferSize = int.MaxValue;

        /// <summary>
        /// Default EOM timeout is 60 min
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields", Justification = "This is a shared source file")]
        internal readonly static TimeSpan SendRequestTimeout = TimeSpan.MaxValue;

        /// <summary>
        /// Default EOM timeout is 60 sec
        /// </summary>
        internal const int EOMTimeoutMS = 60 * 1000;

        /// <summary>
        /// Default Purge timeout is 60 sec
        /// </summary>
        internal const int PurgeTimeoutMS = 60 * 1000;

        /// <summary>
        /// Min WCF operation timeout
        /// </summary>
        internal const int MinOperationTimeout = 60 * 1000;

        /// <summary>
        /// Request all responses
        /// </summary>
        internal const int GetResponse_All = -1;

        /// <summary>
        /// Stores the max client id length
        /// </summary>
        internal const int MaxClientIdLength = 128;

        /// <summary>
        /// Stores the user name for anonymous user
        /// </summary>
        internal const string AnonymousUserName = "Anonymous";

        /// <summary>
        /// Stores the initialization wait handle name format
        /// </summary>
        internal const string InitializationWaitHandleNameFormat = "HpcBroker{0}";

        /// <summary>
        /// Environment Variable to pass service initialization timeout
        /// </summary>
        internal const string ServiceInitializationTimeoutEnvVar = "CCP_SERVICE_INITIALIZATION_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass service host idle timeout
        /// </summary>
        internal const string ServiceHostIdleTimeoutEnvVar = "CCP_SERVICEHOST_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass service hang timeout
        /// </summary>
        internal const string ServiceHangTimeoutEnvVar = "CCP_SERVICE_HANG_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass cancel task grace period
        /// </summary>
        internal const string CancelTaskGracePeriodEnvVar = "CCP_CANCEL_TASK_GRACEPERIOD";

        /// <summary>
        /// File name for the service config file
        /// </summary>
        internal const string ServiceConfigFileNameEnvVar = "CCP_SERVICE_CONFIG_FILENAME";

        /// <summary>
        /// Environment Variable to pass the value indicating enable MessageLevelPreemption or not.
        /// </summary>
        internal const string EnableMessageLevelPreemptionEnvVar = "CCP_MESSAGE_LEVEL_PREEMPTION";

        internal const string DataServiceSharedFileEnvVar = "CCP_DATA_SERVICE_SHARED";

        /// <summary>
        /// Environment Variable to pass the service trace switch value (SoaDiagTraceLevel)
        /// </summary>
        internal const string TraceSwitchValue = "CCP_TRACE_SWITCHVALUE";

        /// <summary>
        /// Environment Variable to pass the localtion of the service package
        /// </summary>
        internal const string PackageRootEnvVar = "CCP_PACKAGE_ROOT";

        /// <summary>
        /// Default value for cancel task grace period. Same as node managers
        /// </summary>
        internal const int DefaultCancelTaskGracePeriod = 15000;

        /// <summary>
        /// Cluster parameter name for TaskCancelGracePeriod
        /// </summary>
        internal const string TaskCancelGracePeriodClusParam = "TaskCancelGracePeriod";

        /// <summary>
        /// Cluster parameter name for AutomaticShrinkEnabled
        /// </summary>
        internal const string AutomaticShrinkEnabled = "AutomaticShrinkEnabled";

        /// <summary>
        /// Cluster parameter name for SchedulingMode
        /// </summary>
        internal const string SchedulingMode = "SchedulingMode";

        /// <summary>
        /// Cluster parameter Balanced value for SchedulingMode
        /// </summary>
        internal const string SchedulingMode_Balanced = "Balanced";

        /// <summary>
        /// Cluster parameter name for JobRetryCount
        /// </summary>
        internal const string JobRetryCountParam = "JobRetryCount";

        /// <summary>
        /// Cluster parameter name for HpcSoftCardTemplate
        /// </summary>
        internal const string HpcSoftCardTemplateParam = "HpcSoftCardTemplate";

        /// <summary>
        /// Cluster parameter name for HpcSoftCard
        /// </summary>
        internal const string HpcSoftCard = "HpcSoftCard";

        /// <summary>
        /// Cluster parameter name for DisableCredentialReuse
        /// </summary>
        internal const string DisableCredentialReuse = "DisableCredentialReuse";

        /// <summary>
        /// Name for the NetworkTopology of the cluster
        /// </summary>
        internal const string NetworkTopology = "NetworkTopology";

        /// <summary>
        /// Default for AutomaticShrinkEnabledDefault
        /// </summary>
        internal const bool AutomaticShrinkEnabledDefault = true;

        /// <summary>
        /// Name of the ServiceJob property
        /// </summary>
        internal const string ServiceJobId = "HPC_ServiceJobId";

        /// <summary>
        /// Name of the headnode
        /// </summary>
        internal const string HeadnodeName = "HPC_Headnode";

        /// <summary>
        /// status of the service job
        /// </summary>
        internal const string ServiceJobStatus = "HPC_ServiceJobStatus";

        /// <summary>
        /// The filename for versioned service config files (service_version.config)
        /// </summary>
        internal const string ServiceConfigFileNameFormat = "{0}_{1}.config";

        /// <summary>
        /// Version object to use for versionless services
        /// </summary>
        internal static Version VersionlessServiceVersion = new Version();

        /// <summary>
        /// Env var to pass maxMessageSize to service hosts
        /// </summary>
        internal const string ServiceConfigMaxMessageEnvVar = "CCP_SERVICE_MAXMESSAGESIZE";

        /// <summary>
        /// Env var to pass serviceOperationTimeout to service hosts
        /// </summary>
        internal const string ServiceConfigServiceOperatonTimeoutEnvVar = "CCP_SERVICE_SERVICEOPERATIONTIMEOUT";

        /// <summary>
        /// Default value for serviceOperationTimeout service config setting
        /// </summary>
        internal const int DefaultServiceOperationTimeout = 24 * 60 * 60 * 1000;

        /// <summary>
        /// Default value for maxMessageSize service config setting
        /// </summary>
        internal const int DefaultMaxMessageSize = 64 * 1024; // 64K 

        /// <summary>
        /// Default value for maxSessionPoolSize service config setting
        /// </summary>
        internal const int DefaultMaxSessionPoolSize = 1;

        /// <summary>
        /// Service host endpoint address format for azure
        /// </summary>
        internal const string ServiceHostEndpointFormatOnAzure = "{0}/{1}/{2}/_defaultEndpoint";

        /// <summary>
        /// Endpoint path for service host endpoint
        /// </summary>
        internal const string ServiceHostEndpointPath = @"/_defaultEndpoint";

        /// <summary>
        /// Endpoint path for service host controller endpoint
        /// </summary>
        internal const string ServiceHostControllerEndpointPath = @"/controller";

        /// <summary>
        /// Store the service host port
        /// Update ServiceHostPortDiff if this value is changed.
        /// </summary>
        internal const int ServiceHostPort = 9100;

        /// <summary>
        /// Store the service host port for IHpcServiceHost
        /// Update ServiceHostPortDiff if this value is changed.
        /// </summary>
        internal const int ServiceHostControllerPort = 9200;

        /// <summary>
        /// The diff between ServiceHostControllerPort and ServiceHostPort.
        /// It needs to be updated if ServiceHostPort or ServiceHostControllerPort is changed.
        /// </summary>
        internal const int ServiceHostPortDiff = 100;

        /// <summary>
        /// The base port for the host if #cores >= 100
        /// </summary>
        internal const int ServiceHostBasePort = 9300;

        /// <summary>
        /// The base port for the controller if #cores >= 100
        /// </summary>
        internal const int ServiceHostControllerBasePort = 9301;

        /// <summary>
        /// Service host controller endpoint address format
        /// </summary>
        internal const string ServiceHostControllerEndpointFormatOnAzure = "{0}/{1}/{2}/_defaultEndpoint/controller";

        /// <summary>
        /// Web API Service EPR
        /// </summary>
        internal const string WebAPIServiceEpr = "https://localhost:443/SOA";

        /// <summary>
        /// Stores the network prefix env to get the network prefix
        /// </summary>
        internal const string NetworkPrefixEnv = "WCF_NETWORKPREFIX";

        /// <summary>
        /// Stores the cluster config name.
        /// </summary>
        internal const string NettcpOver443 = "NettcpOver443";

        /// <summary>
        /// Stores the cluster env name for EnableFQDN setting.
        /// </summary>
        internal const string EnableFqdnEnv = "HPC_ENABLEFQDN";

        /// <summary>
        /// Stores the enterprise network prefix
        /// </summary>
        internal const string EnterpriseNetwork = "Enterprise";

        /// <summary>
        /// Store registry path environment
        /// </summary>
        internal const string RegistryPathEnv = "CCP_SERVICEREGISTRATION_PATH";

        /// <summary>
        /// Store service name environment
        /// </summary>
        internal const string ServiceNameEnvVar = "CCP_SERVICENAME";

        /// <summary>
        /// Store the env var to indicate if the session is using a session pool
        /// </summary>
        internal const string ServiceUseSessionPoolEnvVar = "CCP_SERVICE_USESESSIONPOOL";

        /// <summary>
        /// Store cluster name environment
        /// </summary>
        internal const string HeadnodeEnvVar = "CCP_CLUSTER_NAME";

        /// <summary>
        /// Stores the home path environment
        /// </summary>
        internal const string HomePathEnvVar = "CCP_HOME";

        /// <summary>
        /// Stores the ccp data path environment
        /// </summary>
        internal const string DataPathEnvVar = "CCP_DATA";

        /// <summary>
        /// Store job id environment
        /// </summary>
        internal const string JobIDEnvVar = "CCP_JOBID";

        /// <summary>
        /// Store task id environment
        /// </summary>
        internal const string TaskIDEnvVar = "CCP_TASKSYSTEMID";

        /// <summary>
        /// Store processor number environment
        /// </summary>
        internal const string ProcNumEnvVar = "CCP_NUMCPUS";

        internal const string OverrideProcNumEnvVar = "CCP_OVERRIDENUMCPUS";

        /// <summary>
        /// Store core id list environment
        /// </summary>
        internal const string CoreIdsEnvVar = "CCP_COREIDS";

        /// <summary>
        /// Env var used to store prepost command for HpcServiceHost to use when launching the command 
        /// </summary>
        internal const string PrePostTaskCommandLineEnvVar = "CCP_PREPOST_COMMANDLINE";

        /// <summary>
        /// Env var used to store job owner's user name
        /// </summary>
        internal const string JobOwnerNameEnvVar = "HPC_JOB_OWNER";

        /// <summary>
        /// Env var used to store prepost working dir for HpcServiceHost to use when launching the command on premise
        /// </summary>
        internal const string PrePostTaskOnPremiseWorkingDirEnvVar = "CCP_PREPOST_ONPREMISE_WORKINGDIR";

        /// <summary>
        /// Azure proxy's identity (based on its server cert)
        /// </summary>
        internal const string HpcAzureProxyServerIdentity = "Microsoft HPC Azure Service";

        /// <summary>
        /// Name of the Azure proxy SSL server cert that must be specifed as the endpoint's identity when connecting to the proxy
        /// </summary>
        internal const string HpcAzureProxyServerCertName = "CN=" + Constant.HpcAzureProxyServerIdentity;

        /// <summary>
        /// Name of the Azure proxy SSL client cert
        /// </summary>
        internal const string HpcAzureProxyClientCertName = "CN=Microsoft HPC Azure Client";

        /// <summary>
        /// Name of the Java WSS4J client cert
        /// </summary>
        internal const string HpcWssClientCertName = "CN=Microsoft HPC WSS Client";

        /// <summary>
        /// Java WSS4J server's identity (based on its server cert)
        /// </summary>
        internal const string HpcWssServiceIdentity = "Microsoft HPC WSS Service";

        /// <summary>
        /// Name of the Java WSS4J server cert
        /// </summary>
        internal const string HpcWssServiceCertName = "CN=" + Constant.HpcWssServiceIdentity;

        /// <summary>
        /// Environment variable key for enalbe backend security, only used in java soa service
        /// </summary>
        internal const string IsEnableBackendSecurityEnvVar = "ENABLE_BACKEND_SECURITY";

        /// <summary>
        /// Broker node name for inprocess broker
        /// </summary>
        internal const string InprocessBrokerNode = "$InprocessBroker$";

        /// <summary>
        /// Environment variable key for storing runtime share path
        /// </summary>
        internal const string RuntimeSharePathEnvVar = "HPC_RUNTIMESHARE";

        /// <summary>
        /// Env var used to pass soa data server information to service hosts
        /// </summary>
        internal const string SoaDataServerInfoEnvVar = "HPC_SOADATASERVER";

        /// <summary>
        /// Env var used to pass job secret to service hosts
        /// </summary>
        internal const string JobSecretEnvVar = "HPC_JOB_SECRET";

        /// <summary>
        /// Env var used to pass data directory for the SOA job
        /// </summary>
        internal const string SoaDataJobDirEnvVar = "HPC_SOADATAJOBDIR";

        /// <summary>
        /// default timeout for session creation.
        /// </summary>
        internal const int DefaultCreateSessionTimeout = 5 * 60 * 1000;

        internal const string WFE_Role_Caller_HeaderNameSpace = @"http://hpc.microsoft.com";
        internal const string WFE_Role_Caller_HeaderName = "WFE-Role-Caller";

        #region broker proxy related

        /// <summary>
        /// Message header name for binding info
        /// </summary>
        internal const string MessageHeaderBinding = "binding";

        /// <summary>
        /// Message header name for service operation timeout
        /// </summary>
        internal const string MessageHeaderServiceOperationTimeout = "operationtimeout";

        /// <summary>
        /// Message header name of MachineName
        /// </summary>
        internal const string MessageHeaderMachineName = "machinename";

        /// <summary>
        /// Message header name of Port
        /// </summary>
        internal const string MessageHeaderCoreId = "coreid";

        /// <summary>
        /// Message header name of JobId
        /// </summary>
        internal const string MessageHeaderJobId = "jobid";

        /// <summary>
        /// Message header name of job requeue count
        /// </summary>
        internal const string MessageHeaderRequeueCount = "requeuecount";

        /// <summary>
        /// Message header name of TaskId
        /// </summary>
        internal const string MessageHeaderTaskId = "taskid";

        /// <summary>
        /// Message header name of the flag indicating if the message is stored in Azure storage blob.
        /// </summary>
        internal const string MessageHeaderBlob = "blob";

        /// <summary>
        /// Message header name of Preemption
        /// The service host adds the header to the response message when the process is preempted,
        /// and the broker reads it. The value type is bool.
        /// </summary>
        internal const string MessageHeaderPreemption = "preemption";

        /// <summary>
        /// Broker proxy endpoint address format
        /// </summary>
        internal const string BrokerProxyEndpointFormat = "net.tcp://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy endpoint address format for https
        /// </summary>
        internal const string BrokerProxyEndpointFormatHttps = "https://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management endpoint address format
        /// </summary>
        internal const string BrokerProxyManagementEndpointFormat = "net.tcp://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management endpoint address format for https
        /// </summary>
        internal const string BrokerProxyManagementEndpointFormatHttps = "https://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management listen address format
        /// </summary>
        internal const string BrokerProxyManagementListenEndpointFormat = "net.tcp://{0}/{1}";

        /// <summary>
        /// Broker proxy service management listen address format for https
        /// </summary>
        internal const string BrokerProxyManagementListenEndpointFormatHttps = "https://{0}/{1}";

        #endregion

        /// <summary>
        /// The upper limit of the batch size for the CloudQueue.GetMessages method.
        /// </summary>
        internal const int GetQueueMessageBatchSize = 32;

        /// <summary>
        /// MSMQ max message chunk size < 4MB
        /// </summary>
        internal const int MSMQChunkSize = 4192000;

        /// <summary>
        /// AzureQueue max message chunk size < 64KB and body <48KB
        /// </summary>
        internal const int AzureQueueMsgChunkSize = 49152;
    }
}
