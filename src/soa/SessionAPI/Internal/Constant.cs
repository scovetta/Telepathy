//------------------------------------------------------------------------------
// <copyright file=>"Constant.cs" company=>"Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Global constants
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Linq.Expressions;
    using System.ServiceModel;

    // TODO: remove this class

    /// <summary>
    /// The Constant variables
    /// </summary>
    public static class Constant
    {
        /// <summary>
        /// Stores the string separator
        /// </summary>
        public static char WebAPI_GenericServiceRequestSeparator => '\x0000';

        /// <summary>
        /// Stores the user data separator
        /// </summary>
        public static char WebAPI_GenericServiceUserDataSeparator => '\xffff';

        /// <summary>
        /// Stores the constant string indicating broker not exist for ping broker result
        /// </summary>
        public static string PingBroker2Result_BrokerNotExist => "PingBroker2Result_BrokerNotExist";

        /// <summary>
        /// SOAP Action of broker's EOM message
        /// </summary>
        public static string EndOfMessageAction => @"http://hpc.microsoft.com/EndOfGetResponse";

        /// <summary>
        /// SOAP Action of broker's Exit ServiceHost message
        /// </summary>
        public static string ExitServiceHostAction => @"http://hpc.microsoft.com/ExitServiceHost";

        /// <summary>
        /// SOAP Action of passing servicehost binding info to broker proxy
        /// </summary>
        public static string PassBindingAction => @"http://hpc.microsoft.com/PassBindingToProxy";

        /// <summary>
        /// SOAP Action of pinging broker proxy
        /// </summary>
        public static string PingBrokerProxyAction => @"http://hpc.microsoft.com/PingBrokerProxy";

        /// <summary>
        /// SOAP Action of the heart beat for the broker on Azure
        /// </summary>
        public static string BrokerHeartbeatAction => @"http://hpc.microsoft.com/BrokerHeartbeat";

        /// <summary>
        /// SOAP Action of invalid argument for SOA web service
        /// </summary>
        public static string WebAPI_ArgumentExceptionFaultAction => @"http://hpc.microsoft.com/InvalidArgument";

        /// <summary>
        /// SOAP action of wrapper message of client side exception
        /// </summary>
        public static string WebAPI_ClientSideException => @"http://hpc.microsoft.com/ClientSideExeption";

        /// <summary>
        /// Element Name of Message Array for Web API
        /// </summary>
        public static string WSMessageArrayElementName => "HPCServer2008_WebAPI_MessageArray";

        /// <summary>
        /// Element name of user data SOAP header
        /// </summary>
        public static string UserDataHeaderName => "HPCServer2008_Broker_UserDataNS";

        /// <summary>
        /// Element name of message Id SOAP header
        /// </summary>
        public static string MessageIdHeaderName => "HPCServer2008_Broker_MessageIdNS";

        /// <summary>
        /// Element name of client id SOAP header
        /// </summary>
        public static string ClientIdHeaderName => "HPCServer2008_Broker_ClientIdNS";

        /// <summary>
        /// Element name of user name SOAP header
        /// </summary>
        public static string UserNameHeaderName => "HPCServer2008_Broker_UserNameNS";

        /// <summary>
        /// Element name of client instance id SOAP header
        /// </summary>
        public static string ClientInstanceIdHeaderName => "HPCServer2008_Broker_ClientInstanceIdNS";

        /// <summary>
        /// Element name of request action SOAP header
        /// </summary>
        public static string ActionHeaderName => "HPCServer2008_Broker_RequestActionNS";

        /// <summary>
        /// Element name of correlation id SOAP header
        /// </summary>
        public static string CorrelationIdHeaderName => "HPCServer2008_Broker_CorrelationIdNS";

        /// <summary>
        /// Element name of dispatch id SOAP header
        /// </summary>
        public static string DispatchIdHeaderName => "HPCServer2008_Broker_DispatchIdNS";

        /// <summary>
        /// Element name of security header
        /// </summary>
        public static string SecurityHeaderName => "Security";

        /// <summary>
        /// Namespace of security header
        /// </summary>
        public static string SecurityHeaderNamespace => "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";

        /// <summary>
        /// Fault code of retry limit exceed
        /// </summary>
        public static  FaultCode RetryLimitExceedFaultCode => FaultCode.CreateReceiverFaultCode("RetryOperationError", HpcHeaderNS);

        /// <summary>
        /// Fault code of authentication failure
        /// </summary>
        public static  FaultCode AuthenticationFailureFaultCode => FaultCode.CreateReceiverFaultCode("AuthenticationFailure", HpcHeaderNS);

        /// <summary>
        /// Fault code of broker proxy exceptions
        /// </summary>
        public static  FaultCode ProxyFaultCode => FaultCode.CreateReceiverFaultCode("ProxyEncountersException", HpcHeaderNS);

        /// <summary>
        /// Fault code for ArgumentException in SOA Web service
        /// </summary>
        public static  FaultCode WebAPI_ArgumentExceptionFaultCode => FaultCode.CreateReceiverFaultCode("InvalidArgument", HpcHeaderNS);

        /// <summary>
        /// the env string for diagnostic broker node
        /// </summary>
        public static string DiagnosticBrokerNode => "HPCDiagnosticBroker";

        /// <summary>
        /// Namespace of user data SOAP header
        /// </summary>
        public static string HpcHeaderNS => "http://www.microsoft.com/hpc";

        /// <summary>
        /// Stores the prefix of the namespace
        /// </summary>
        public static string HpcNSPrefix => "hpc";

        /// <summary>
        /// Element name of client callback ID header
        /// </summary>
        public static string ResponseCallbackIdHeaderName => "HPCServer2008_Broker_ResponseCallbackId";

        /// <summary>
        /// Namespace name of client callback ID header
        /// </summary>
        public static string ResponseCallbackIdHeaderNS => HpcHeaderNS;

        /// <summary>
        /// Maximum length of user data SOAP header
        /// </summary>
        public static int MaxUserDataLen => 1024;

        /// <summary>
        /// Maximum size of message 
        /// TODO: Need to pull from current binding
        /// </summary>
        public static int MaxBufferSize => int.MaxValue;

        /// <summary>
        /// Default EOM timeout is 60 min
        /// </summary>
        public  static TimeSpan SendRequestTimeout => TimeSpan.MaxValue;

        /// <summary>
        /// Default EOM timeout is 60 sec
        /// </summary>
        public static int EOMTimeoutMS => 60 * 1000;

        /// <summary>
        /// Default Purge timeout is 60 sec
        /// </summary>
        public static int PurgeTimeoutMS => 60 * 1000;

        /// <summary>
        /// Min WCF operation timeout
        /// </summary>
        public static int MinOperationTimeout => 60 * 1000;

        /// <summary>
        /// Request all responses
        /// </summary>
        public static int GetResponse_All => -1;

        /// <summary>
        /// Stores the max client id length
        /// </summary>
        public static int MaxClientIdLength => 128;

        /// <summary>
        /// Stores the user name for anonymous user
        /// </summary>
        public static string AnonymousUserName => "Anonymous";

        /// <summary>
        /// Stores the initialization wait handle name format
        /// </summary>
        public static string InitializationWaitHandleNameFormat => "HpcBroker{0}";

        /// <summary>
        /// Environment Variable to pass service initialization timeout
        /// </summary>
        public static string ServiceInitializationTimeoutEnvVar => "CCP_SERVICE_INITIALIZATION_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass service host idle timeout
        /// </summary>
        public static string ServiceHostIdleTimeoutEnvVar => "CCP_SERVICEHOST_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass service hang timeout
        /// </summary>
        public static string ServiceHangTimeoutEnvVar => "CCP_SERVICE_HANG_TIMEOUT";

        /// <summary>
        /// Environment Variable to pass cancel task grace period
        /// </summary>
        public static string CancelTaskGracePeriodEnvVar => "CCP_CANCEL_TASK_GRACEPERIOD";

        /// <summary>
        /// File name for the service config file
        /// </summary>
        public static string ServiceConfigFileNameEnvVar => "CCP_SERVICE_CONFIG_FILENAME";

        /// <summary>
        /// Environment Variable to pass the value indicating enable MessageLevelPreemption or not.
        /// </summary>
        public static string EnableMessageLevelPreemptionEnvVar => "CCP_MESSAGE_LEVEL_PREEMPTION";

        public static string DataServiceSharedFileEnvVar => "CCP_DATA_SERVICE_SHARED";

        /// <summary>
        /// Environment Variable to pass the service trace switch value (SoaDiagTraceLevel)
        /// </summary>
        public static string TraceSwitchValue => "CCP_TRACE_SWITCHVALUE";

        /// <summary>
        /// Environment Variable to pass the localtion of the service package
        /// </summary>
        public static string PackageRootEnvVar => "CCP_PACKAGE_ROOT";

        /// <summary>
        /// Default value for cancel task grace period. Same as node managers
        /// </summary>
        public static int DefaultCancelTaskGracePeriod => 15000;

        /// <summary>
        /// Cluster parameter name for TaskCancelGracePeriod
        /// </summary>
        public static string TaskCancelGracePeriodClusParam => "TaskCancelGracePeriod";

        /// <summary>
        /// Cluster parameter name for AutomaticShrinkEnabled
        /// </summary>
        public const string AutomaticShrinkEnabled = "AutomaticShrinkEnabled";

        /// <summary>
        /// Cluster parameter name for SchedulingMode
        /// </summary>
        public static string SchedulingMode => "SchedulingMode";

        /// <summary>
        /// Cluster parameter Balanced value for SchedulingMode
        /// </summary>
        public static string SchedulingMode_Balanced => "Balanced";

        /// <summary>
        /// Cluster parameter name for JobRetryCount
        /// </summary>
        public static string JobRetryCountParam => "JobRetryCount";

        /// <summary>
        /// Cluster parameter name for HpcSoftCardTemplate
        /// </summary>
        public static string HpcSoftCardTemplateParam => "HpcSoftCardTemplate";

        /// <summary>
        /// Cluster parameter name for HpcSoftCard
        /// </summary>
        public static string HpcSoftCard => "HpcSoftCard";

        /// <summary>
        /// Cluster parameter name for DisableCredentialReuse
        /// </summary>
        public static string DisableCredentialReuse => "DisableCredentialReuse";

        /// <summary>
        /// Name for the NetworkTopology of the cluster
        /// </summary>
        public static string NetworkTopology => "NetworkTopology";

        /// <summary>
        /// Default for AutomaticShrinkEnabledDefault
        /// </summary>
        public static bool AutomaticShrinkEnabledDefault => true;

        /// <summary>
        /// Name of the ServiceJob property
        /// </summary>
        public static string ServiceJobId => "HPC_ServiceJobId";

        /// <summary>
        /// Name of the headnode
        /// </summary>
        public static string HeadnodeName => "HPC_Headnode";

        /// <summary>
        /// status of the service job
        /// </summary>
        public static string ServiceJobStatus => "HPC_ServiceJobStatus";

        /// <summary>
        /// The filename for versioned service config files (service_version.config)
        /// </summary>
        public static string ServiceConfigFileNameFormat => "{0}_{1}.config";

        /// <summary>
        /// Version object to use for versionless services
        /// </summary>
        public static Version VersionlessServiceVersion => new Version();

        /// <summary>
        /// Env var to pass maxMessageSize to service hosts
        /// </summary>
        public static string ServiceConfigMaxMessageEnvVar => "CCP_SERVICE_MAXMESSAGESIZE";

        /// <summary>
        /// Env var to pass serviceOperationTimeout to service hosts
        /// </summary>
        public static string ServiceConfigServiceOperatonTimeoutEnvVar => "CCP_SERVICE_SERVICEOPERATIONTIMEOUT";

        /// <summary>
        /// Default value for serviceOperationTimeout service config setting
        /// </summary>
        public static int DefaultServiceOperationTimeout => 24 * 60 * 60 * 1000;

        /// <summary>
        /// Default value for maxMessageSize service config setting
        /// </summary>
        public static int DefaultMaxMessageSize => 64 * 1024; // 64K 

        /// <summary>
        /// Default value for maxSessionPoolSize service config setting
        /// </summary>
        public static int DefaultMaxSessionPoolSize => 1;

        /// <summary>
        /// Service host endpoint address format for azure
        /// </summary>
        public static string ServiceHostEndpointFormatOnAzure => "{0}/{1}/{2}/_defaultEndpoint";

        /// <summary>
        /// Endpoint path for service host endpoint
        /// </summary>
        public static string ServiceHostEndpointPath => @"/_defaultEndpoint";

        /// <summary>
        /// Endpoint path for service host controller endpoint
        /// </summary>
        public static string ServiceHostControllerEndpointPath => @"/controller";

        /// <summary>
        /// Store the service host port
        /// Update ServiceHostPortDiff if this value is changed.
        /// </summary>
        public static int ServiceHostPort => 9100;

        /// <summary>
        /// Store the service host port for IHpcServiceHost
        /// Update ServiceHostPortDiff if this value is changed.
        /// </summary>
        public static int ServiceHostControllerPort => 9200;

        /// <summary>
        /// The diff between ServiceHostControllerPort and ServiceHostPort.
        /// It needs to be updated if ServiceHostPort or ServiceHostControllerPort is changed.
        /// </summary>
        public static int ServiceHostPortDiff => 100;

        /// <summary>
        /// The base port for the host if #cores >=> 100
        /// </summary>
        public static int ServiceHostBasePort => 9300;

        /// <summary>
        /// The base port for the controller if #cores >=> 100
        /// </summary>
        public static int ServiceHostControllerBasePort => 9301;

        /// <summary>
        /// Service host controller endpoint address format
        /// </summary>
        public static string ServiceHostControllerEndpointFormatOnAzure => "{0}/{1}/{2}/_defaultEndpoint/controller";

        /// <summary>
        /// Web API Service EPR
        /// </summary>
        public static string WebAPIServiceEpr => "https://localhost:443/SOA";

        /// <summary>
        /// Stores the network prefix env to get the network prefix
        /// </summary>
        public const string NetworkPrefixEnv = "WCF_NETWORKPREFIX";

        /// <summary>
        /// Stores the cluster config name.
        /// </summary>
        public const string NettcpOver443 = "NettcpOver443";

        /// <summary>
        /// Stores the cluster env name for EnableFQDN setting.
        /// </summary>
        public const string EnableFqdnEnv = "HPC_ENABLEFQDN";

        /// <summary>
        /// Stores the enterprise network prefix
        /// </summary>
        public static string EnterpriseNetwork => "Enterprise";

        /// <summary>
        /// Store registry path environment
        /// </summary>
        public const string RegistryPathEnv = "CCP_SERVICEREGISTRATION_PATH";

        /// <summary>
        /// Store service name environment
        /// </summary>
        public static string ServiceNameEnvVar => "CCP_SERVICENAME";

        /// <summary>
        /// Store the env var to indicate if the session is using a session pool
        /// </summary>
        public static string ServiceUseSessionPoolEnvVar => "CCP_SERVICE_USESESSIONPOOL";

        /// <summary>
        /// Store cluster name environment
        /// </summary>
        public static string HeadnodeEnvVar => "CCP_CLUSTER_NAME";

        /// <summary>
        /// Stores the home path environment
        /// </summary>
        public static string HomePathEnvVar => "CCP_HOME";

        /// <summary>
        /// Stores the ccp data path environment
        /// </summary>
        public static string DataPathEnvVar => "CCP_DATA";

        /// <summary>
        /// Store job id environment
        /// </summary>
        public static string JobIDEnvVar => "CCP_JOBID";

        /// <summary>
        /// Store task id environment
        /// </summary>
        public static string TaskIDEnvVar => "CCP_TASKSYSTEMID";

        /// <summary>
        /// Store processor number environment
        /// </summary>
        public static string ProcNumEnvVar => "CCP_NUMCPUS";

        public static string OverrideProcNumEnvVar => "CCP_OVERRIDENUMCPUS";

        /// <summary>
        /// Store core id list environment
        /// </summary>
        public static string CoreIdsEnvVar => "CCP_COREIDS";

        /// <summary>
        /// Env var used to store prepost command for HpcServiceHost to use when launching the command 
        /// </summary>
        public static string PrePostTaskCommandLineEnvVar => "CCP_PREPOST_COMMANDLINE";

        /// <summary>
        /// Env var used to store job owner's user name
        /// </summary>
        public static string JobOwnerNameEnvVar => "HPC_JOB_OWNER";

        /// <summary>
        /// Env var used to store prepost working dir for HpcServiceHost to use when launching the command on premise
        /// </summary>
        public static string PrePostTaskOnPremiseWorkingDirEnvVar => "CCP_PREPOST_ONPREMISE_WORKINGDIR";

        /// <summary>
        /// Azure proxy's identity (based on its server cert)
        /// </summary>
        public static string HpcAzureProxyServerIdentity => "Microsoft HPC Azure Service";

        /// <summary>
        /// Name of the Azure proxy SSL server cert that must be specifed as the endpoint's identity when connecting to the proxy
        /// </summary>
        public static string HpcAzureProxyServerCertName => "CN=>" + Constant.HpcAzureProxyServerIdentity;

        /// <summary>
        /// Name of the Azure proxy SSL client cert
        /// </summary>
        public static string HpcAzureProxyClientCertName => "CN=>Microsoft HPC Azure Client";

        /// <summary>
        /// Name of the Java WSS4J client cert
        /// </summary>
        public static string HpcWssClientCertName => "CN=>Microsoft HPC WSS Client";

        /// <summary>
        /// Java WSS4J server's identity (based on its server cert)
        /// </summary>
        public static string HpcWssServiceIdentity => "Microsoft HPC WSS Service";

        /// <summary>
        /// Name of the Java WSS4J server cert
        /// </summary>
        public static string HpcWssServiceCertName => "CN=>" + Constant.HpcWssServiceIdentity;

        /// <summary>
        /// Environment variable key for enalbe backend security, only used in java soa service
        /// </summary>
        public static string IsEnableBackendSecurityEnvVar => "ENABLE_BACKEND_SECURITY";

        /// <summary>
        /// Broker node name for inprocess broker
        /// </summary>
        public static string InprocessBrokerNode => "$InprocessBroker$";

        /// <summary>
        /// Environment variable key for storing runtime share path
        /// </summary>
        public static string RuntimeSharePathEnvVar => "HPC_RUNTIMESHARE";

        /// <summary>
        /// Env var used to pass soa data server information to service hosts
        /// </summary>
        public static string SoaDataServerInfoEnvVar => "HPC_SOADATASERVER";

        /// <summary>
        /// Env var used to pass job secret to service hosts
        /// </summary>
        public static string JobSecretEnvVar => "HPC_JOB_SECRET";

        /// <summary>
        /// Env var used to pass data directory for the SOA job
        /// </summary>
        public static string SoaDataJobDirEnvVar => "HPC_SOADATAJOBDIR";

        /// <summary>
        /// default timeout for session creation.
        /// </summary>
        public static int DefaultCreateSessionTimeout => 5 * 60 * 1000;

        public static string WFE_Role_Caller_HeaderNameSpace => @"http://hpc.microsoft.com";
        public static string WFE_Role_Caller_HeaderName => "WFE-Role-Caller";

        #region broker proxy related

        /// <summary>
        /// Message header name for binding info
        /// </summary>
        public static string MessageHeaderBinding => "binding";

        /// <summary>
        /// Message header name for service operation timeout
        /// </summary>
        public static string MessageHeaderServiceOperationTimeout => "operationtimeout";

        /// <summary>
        /// Message header name of MachineName
        /// </summary>
        public static string MessageHeaderMachineName => "machinename";

        /// <summary>
        /// Message header name of Port
        /// </summary>
        public static string MessageHeaderCoreId => "coreid";

        /// <summary>
        /// Message header name of JobId
        /// </summary>
        public static string MessageHeaderJobId => "jobid";

        /// <summary>
        /// Message header name of job requeue count
        /// </summary>
        public static string MessageHeaderRequeueCount => "requeuecount";

        /// <summary>
        /// Message header name of TaskId
        /// </summary>
        public static string MessageHeaderTaskId => "taskid";

        /// <summary>
        /// Message header name of the flag indicating if the message is stored in Azure storage blob.
        /// </summary>
        public static string MessageHeaderBlob => "blob";

        /// <summary>
        /// Message header name of Preemption
        /// The service host adds the header to the response message when the process is preempted,
        /// and the broker reads it. The value type is bool.
        /// </summary>
        public static string MessageHeaderPreemption => "preemption";

        /// <summary>
        /// Broker proxy endpoint address format
        /// </summary>
        public static string BrokerProxyEndpointFormat => "net.tcp://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy endpoint address format for https
        /// </summary>
        public static string BrokerProxyEndpointFormatHttps => "https://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management endpoint address format
        /// </summary>
        public static string BrokerProxyManagementEndpointFormat => "net.tcp://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management endpoint address format for https
        /// </summary>
        public static string BrokerProxyManagementEndpointFormatHttps => "https://{0}:{1}/{2}";

        /// <summary>
        /// Broker proxy service management listen address format
        /// </summary>
        public static string BrokerProxyManagementListenEndpointFormat => "net.tcp://{0}/{1}";

        /// <summary>
        /// Broker proxy service management listen address format for https
        /// </summary>
        public static string BrokerProxyManagementListenEndpointFormatHttps => "https://{0}/{1}";

        #endregion

        /// <summary>
        /// The upper limit of the batch size for the CloudQueue.GetMessages method.
        /// </summary>
        public static int GetQueueMessageBatchSize => 32;

        /// <summary>
        /// MSMQ max message chunk size < 4MB
        /// </summary>
        public static int MSMQChunkSize => 4192000;

        /// <summary>
        /// AzureQueue max message chunk size < 64KB and body <48KB
        /// </summary>
        public static int AzureQueueMsgChunkSize => 49152;
    }
}
