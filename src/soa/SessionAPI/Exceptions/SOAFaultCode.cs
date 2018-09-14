//------------------------------------------------------------------------------
// <copyright file="SOAFaultCode.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Define the fault code for the SOA session.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    ///   <para>Provides methods for getting information about error codes for SOA errors.</para>
    /// </summary>
    public static class SOAFaultCode
    {
        /// <summary>
        /// Stores the fault code name dictionary
        /// </summary>
        private static readonly Dictionary<int, string> faultCodeNameDic;

        #region SessionConnectionError

        /// <summary>
        ///   <para>The HPC Session Service could not connect to the scheduler yet. This error 
        /// could occur because the HPC Job Scheduler Service is not started or is busy. Try again later.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ConnectToSchedulerFailure = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0000;

        /// <summary>
        ///   <para>The specified service job could not be opened. Check the correct service job is specified and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int OpenJobFailure = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0001;

        /// <summary>
        ///   <para>An operation timed open. Try the operation again later or increase the timeout for the operation.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int OperationTimeout = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0002;

        /// <summary>
        ///   <para>The broker node is unavailable because of the loss of the heartbeat. Check that you can connect 
        /// to the broker node and that the HPC Broker Service is running on the broker node, and then try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BrokerNodeUnavailable = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0003;

        /// <summary>
        ///   <para>The client application failed to connect to HPC Session Service. See 
        /// the inner exception for more information about the error and how to fix it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ConnectSessionLauncherFailure = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0004;

        /// <summary>
        ///   <para>The timeout period elapsed while starting the custom broker. 
        /// Check the specified timeout period, adjust it if necessary, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_CustomBrokerReadyTimeout = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0005;

        /// <summary>
        ///   <para>The custom broker exited unexpectedly with the specified error 
        /// code. Check the error code for information about how to resolve the error.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_CustomBrokerExitBeforeReady = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0006;

        /// <summary>
        ///   <para>Failed to get owner of the service job from the HPC Job Scheduler Service.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_CannotGetUserSID = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0007;

        /// <summary>
        ///   <para>An attempt to get security descriptor for the job template that the service job uses from the HPC Job Scheduler Service failed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_FailedToGetSecurityDescriptor = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0008;

        /// <summary>
        ///   <para>The service job could not be registered in the specified session service.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_RegisterJobFailed = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0009;

        /// <summary>
        ///   <para>An attempt to get the state of the service job from the HPC Job Scheduler Service failed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_FailedToGetJobState = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000a;

        /// <summary>
        ///   <para>The call to the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method is not allowed because the client application is in the specified state. Wait until the client application reaches a state that allows the call to the method and try again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests(System.Int32)" />
        public const int Broker_EOMRejected = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000b;

        /// <summary>
        ///   <para>An attempt to get property identifiers for the service job from the HPC Job Scheduler Service failed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_FailedToGetJobPropertyId = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000c;

        /// <summary>
        ///   <para>The broker node is offline and cannot accept requests to create or attach 
        /// to a session. Contact a cluster administrator to bring the broker node online and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BrokerIsOffline = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000d;

        /// <summary>
        ///   <para>The attempt to start broker service process failed with the specified 
        /// error code. Check the specified error code for information about how to resolve the error.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_FailedToStartBrokerServiceProcess = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000e;

        /// <summary>
        ///   <para>The broker was already initialized.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_AlreadyInitialized = (int)SOAFaultCodeCategory.SessionConnectionError + 0x000f;

        /// <summary>
        ///   <para>The handler for the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" /> method was already disposed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" />
        public const int Broker_GetResponsesHandlerDisposed = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0010;

        /// <summary>
        ///   <para>The client application failed to connect to HPC Broker Service. See 
        /// the inner exception for more information about the error and how to fix it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ConnectBrokerLauncherFailure = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0011;

        /// <summary>
        ///   <para>The value of a job property could not be obtained because the HPC Job Scheduler 
        /// Service raised the specified exception. Check the specified exception for information about the error and how to resolve it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int GetJobPropertyFailure = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0012;

        /// <summary>
        ///   <para>The number of active sessions running on this broker node exceeded the 
        /// limit. Increase the value of the maxConcurrentSession setting in the HpcBroker.exe.config file and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_TooManyBrokerRunning = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0013;

        /// <summary>
        ///   <para>That broker has not been initialized. Initialize the broker and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_NotInitialized = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0014;

        /// <summary>
        ///   <para>The connection with the broker is suspended, and the broker cannot accept 
        /// any incoming requests. Wait for the broker to exit, then try to connect to it again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BrokerSuspending = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0015;

        /// <summary>
        ///   <para>The endpoint for the HPC Session Service could not be found on 
        /// the specified node. Check that correct name is specified for the head node, that  
        /// the HPC Session Service is started on the specified head node, and that the 
        /// client application is not using an unsupported feature with an earlier version of Microsoft HPC Pack.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int SessionLauncherEndpointNotFound = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0016;

        /// <summary>
        ///   <para>The client application cannot create or attach to a session 
        /// on Windows HPC Server 2008. Upgrade the cluster to Microsoft HPC Pack.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ConnectToV2Cluster = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0017;

        /// <summary>
        ///   <para>
        ///     <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> was already called for this 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />, or the session's service job ended.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" />
        public const int Broker_EOMReject_GetResponse = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0018;

        /// <summary>
        ///   <para>
        ///     <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> was already called for this 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" />
        public const int Broker_EOMReject_EndRequests = (int)SOAFaultCodeCategory.SessionConnectionError + 0x0019;

        /// <summary>
        ///   <para>A call to flush was rejected. See the stack trace for details.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Flush" />
        public const int Broker_FlushRejected = (int)SOAFaultCodeCategory.SessionConnectionError + 0x001a;

        /// <summary>
        ///   <para>The broker is already closed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_AlreadyClosed = (int)SOAFaultCodeCategory.SessionConnectionError + 0x001b;

        /// <summary>
        ///   <para>Failed to get the Security Descriptor Definition Language (SSDL) string 
        /// for the specified broker node. See the specified inner exception for more information.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int SessionLauncher_FailedToGetBrokerNodeSSDL = (int)SOAFaultCodeCategory.SessionConnectionError + 0x001c;

        /// <summary>
        ///   <para>The location of the service configuration was not 
        /// in the default location and the CCP_SERVICEREGISTRATION_PATH environment variable was not set.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ServiceRegistrationPathEnvironmentMissing = (int)SOAFaultCodeCategory.SessionConnectionError + 0x001d;

        /// <summary>
        ///   <para>The 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method was called before any requests were sent. Call the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method after you finish sending the SOA requests and when you want the broker to commit the requests.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_EOMReject_NoRequest = (int)SOAFaultCodeCategory.SessionConnectionError + 0x001e;
        #endregion

        #region SessionError
        /// <summary>
        ///   <para>A broker that meets the requirements for the session could not be found. Check that a broker node is correctly configured for 
        /// the HPC cluster. If the cluster uses failover broker nodes and you 
        /// create a durable session, check that at least one failover broker node is online.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int NoAvailableBrokerNodes = (int)SOAFaultCodeCategory.SessionError + 0x0000;

        /// <summary>
        ///   <para>The service job could not be created.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int CreateJobFailure = (int)SOAFaultCodeCategory.SessionError + 0x0001;

        /// <summary>
        ///   <para>The properties of the service job could not be set because the HPC Job Scheduler 
        /// Service raised the specified exception. Check the specified exception for information about the error and how to resolve it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int CreateJobPropertiesFailure = (int)SOAFaultCodeCategory.SessionError + 0x0002;

        /// <summary>
        ///   <para>The value of a cluster property could not be obtained because the HPC Job Scheduler 
        /// Service raised the specified exception. Check the specified exception for information about the error and how to resolve it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int GetClusterPropertyFailure = (int)SOAFaultCodeCategory.SessionError + 0x0003;

        /// <summary>
        ///   <para>The task could not be created because the HPC Job Scheduler Service raised 
        /// the specified exception. Check the specified exception for information about the error and how to resolve it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int CreateJobTasksFailure = (int)SOAFaultCodeCategory.SessionError + 0x0004;

        /// <summary>
        ///   <para>Submission of the service job failed for the specified reason. Resolve any issues in the specified reason and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int SubmitJobFailure = (int)SOAFaultCodeCategory.SessionError + 0x0005;

        /// <summary>
        ///   <para>The response messages could not be read from the storage for the broker.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int BrokerQueueFailure = (int)SOAFaultCodeCategory.SessionError + 0x0006;

        /// <summary>
        ///   <para>The broker is unavailable because of the loss of the heartbeat. Check that you can connect to the broker 
        /// node, that the HPC Broker Service is running on the broker node, and that the SOA session is still running, then try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BrokerUnavailable = (int)SOAFaultCodeCategory.SessionError + 0x0007;

        /// <summary>
        ///   <para>The network prefix could not be obtained from the WCF_NETWORKPREFIX environment variable for 
        /// the service task. Check the value of the environment variable, change it if necessary, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_LoadNetworkPrefixFailed = (int)SOAFaultCodeCategory.SessionError + 0x0008;

        /// <summary>
        ///   <para>The connection to the HPC Job Scheduler Service is not ready. The connection may not be ready because the scheduler HPC Job 
        /// Scheduler Service or HPC Session Service on the head node is not 
        /// started. Check that these services have started, start them if necessary, and try again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_ConnectionToSchedulerIsNotReady = (int)SOAFaultCodeCategory.SessionError + 0x0009;

        /// <summary>
        ///   <para>The broker for the session failed to read response messages from the storage for the broker.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BrokerQueueFailure = (int)SOAFaultCodeCategory.SessionError + 0x000a;

        /// <summary>
        ///   <para>The session failed or was canceled. Check if the service job includes a reason 
        /// for the failure or cancellation to get more information about the problem and how to fix it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_SessionFailure = (int)SOAFaultCodeCategory.SessionError + 0x000b;

        /// <summary>
        ///   <para>The worker process for the broker instance could not be obtained within one minute. Try again later.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int TimeoutToGetBrokerWorkerProcess = (int)SOAFaultCodeCategory.SessionError + 0x000c;

        /// <summary>
        ///   <para>No binding for the custom transport could be found. Use 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.#ctor(Microsoft.Hpc.Scheduler.Session.SessionBase,System.ServiceModel.Channels.Binding)" /> to specify the required binding.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int MustIndicateBindingForCustomTransportScheme = (int)SOAFaultCodeCategory.SessionError + 0x000d;

        /// <summary>
        ///   <para>Creation of the session was canceled.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int CreateSessionCanceled = (int)SOAFaultCodeCategory.SessionError + 0x000e;

        /// <summary>
        ///   <para>The broker for the specified session is not active. The broker could be inactive 
        /// because the session has suspended or finished because of a timeout. Try to attach to the session first.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_BrokerNotActive = (int)SOAFaultCodeCategory.SessionError + 0x000f;

        /// <summary>
        ///   <para>The client application failed to send a request to the broker. See 
        /// the specified inner error message for more information about the error and how to fix it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_FailedToSendRequest = (int)SOAFaultCodeCategory.SessionError + 0x0010;

        /// <summary>
        ///   <para>Failed to connect to the session web service for the Windows Azure HPC Scheduler. Try to connect again later.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_FailedToConnectToSessionService = (int)SOAFaultCodeCategory.SessionError + 0x0011;

        /// <summary>
        ///   <para>Failed to connect to the broker web service for the Windows Azure HPC Scheduler. Try to connect again later.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_FailedToConnectToBrokerService = (int)SOAFaultCodeCategory.SessionError + 0x0012;

        #endregion

        #region ApplicationError
        /// <summary>
        ///   <para>The <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object was deleted. The client will receive no more responses.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ClientPurged = (int)SOAFaultCodeCategory.ApplicationError + 0x0000;

        /// <summary>
        ///   <para>The request failed the specified number of times and was marked as failed with the specified 
        /// exception by the broker, because the number of attempts for the request exceeded the limit on the number  
        /// of times that the request can be retried. Check for a reason that the requests failed and address 
        /// that issue, or increase the limit on the number of times that the request can be retried, then try again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_RetryLimitExceeded = (int)SOAFaultCodeCategory.ApplicationError + 0x0001;

        /// <summary>
        ///   <para>The broker could not be send the responses back to the client application, probably because an error occurred 
        /// in the connection between the client and session. If the session is a durable session, try connecting to the session again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_SendBackResponseFailed = (int)SOAFaultCodeCategory.ApplicationError + 0x0002;

        /// <summary>
        ///   <para>The <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object timed out. Try again later.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ClientTimeout = (int)SOAFaultCodeCategory.ApplicationError + 0x0003;

        /// <summary>
        ///   <para>The service failed to initialize.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Service_InitializeFailed = (int)SOAFaultCodeCategory.ApplicationError + 0x0004;

        /// <summary>
        ///   <para>A <see cref="System.ServiceModel.EndpointNotFoundException" /> occurred.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="System.ServiceModel.EndpointNotFoundException" />
        public const int Service_Unreachable = (int)SOAFaultCodeCategory.ApplicationError + 0x0005;

        /// <summary>
        ///   <para>The task ID given to the SOA diagnostics service was not valid.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int DiagService_InvalidTaskId = (int)SOAFaultCodeCategory.ApplicationError + 0x0006;

        #endregion

        #region SessionFatalError
        /// <summary>
        ///   <para>The value specified for the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndpointReference" /> property includes a prefix that is not supported. Check that the value begins with net.tcp:// or https:// and change the value if necessary, then try again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InvalidArgument = (int)SOAFaultCodeCategory.SessionFatalError | 0x0000;

        /// <summary>
        ///   <para>A service job with an identifier that matches the specified 
        /// session identifier could not be found. Check the session identifier and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InvalidSessionId = (int)SOAFaultCodeCategory.SessionFatalError | 0x0001;

        /// <summary>
        ///   <para>The user is not authorized to use the session. Check that the user has been granted permissions to use the session, then try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AuthenticationFailure = (int)SOAFaultCodeCategory.SessionFatalError | 0x0002;

        /// <summary>
        ///   <para>The application failed to connect to the broker instance for the session because permissions to access the broker instance were not 
        /// granted. Check that the account under which your application runs has 
        /// permissions to access the broker instance, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AccessDenied_Broker = (int)SOAFaultCodeCategory.SessionFatalError | 0x0003;

        /// <summary>
        ///   <para>The application failed to connect to the HPC Broker Service because permissions to access the HPC Broker Service were not granted. 
        /// Check that the account under which your application runs has permissions 
        /// to access the HPC Broker Service, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AccessDenied_BrokerLauncher = (int)SOAFaultCodeCategory.SessionFatalError | 0x0004;

        /// <summary>
        ///   <para>The applications failed to connect to the storage for the broker because permissions to access the storage for the broker were not 
        /// granted. Check that the account under which your application runs has permissions 
        /// to access the storage for the broker, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AccessDenied_BrokerQueue = (int)SOAFaultCodeCategory.SessionFatalError | 0x0005;

        /// <summary>
        ///   <para>The storage service for the broker is not available. Check that the service is running and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int StorageServiceNotAvailble = (int)SOAFaultCodeCategory.SessionFatalError | 0x0006;

        /// <summary>
        ///   <para>The storage for the broker exceeded a quota or is full. If you use 
        /// Message Queuing (also known as MSMQ) for broker storage, contact your administrator to increase the Message Queuing  
        /// storage quota on the broker node, or clean up the storage for the sessions manually. For information 
        /// about how to change the Message Queuing storage quota, see <see href="http://go.microsoft.com/fwlink/?LinkId=210554">article 899612</see>in the Microsoft Knowledge Base (http://go.microsoft.com/fwlink/?LinkId=210554).</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int StorageSpaceNotSufficient = (int)SOAFaultCodeCategory.SessionFatalError | 0x0007;

        /// <summary>
        ///   <para>The storage for the broker failed for the specified reason.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int StorageFailure = (int)SOAFaultCodeCategory.SessionFatalError | 0x0008;

        /// <summary>
        ///   <para>The storage for the broker is closed.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int StorageClosed = (int)SOAFaultCodeCategory.SessionFatalError | 0x0009;

        /// <summary>
        ///   <para>The client application could not attach to a durable session using the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method. Use the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method instead.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InvalidAttachDurableSession = (int)SOAFaultCodeCategory.SessionFatalError | 0x000a;

        /// <summary>
        ///   <para>The client application could not attach to an interactive session using the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.DurableSession.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method. Use the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.AttachSession(Microsoft.Hpc.Scheduler.Session.SessionAttachInfo)" /> method instead.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InvalidAttachInteractiveSession = (int)SOAFaultCodeCategory.SessionFatalError | 0x000b;

        /// <summary>
        ///   <para>The specified front end could not be opened.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_OpenFrontEndFailed = (int)SOAFaultCodeCategory.SessionFatalError | 0x000c;

        /// <summary>
        ///   <para>The specified transport scheme is not supported. Specify another transport scheme and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_NotSupportedTransportScheme = (int)SOAFaultCodeCategory.SessionFatalError | 0x000d;

        /// <summary>
        ///   <para>The broker configuration section of the service configuration file contains the 
        /// specified error. Check the broker configuration section, update it if necessary, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_InvalidConfiguration = (int)SOAFaultCodeCategory.SessionFatalError | 0x000e;

        /// <summary>
        ///   <para>The specified session was already created. Either attach to the existing session or create a new one.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_SessionIdAlreadyExists = (int)SOAFaultCodeCategory.SessionFatalError | 0x000f;

        /// <summary>
        ///   <para>The specified session identifier is not valid. Check the session identifier, change it if necessary, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_InvalidSessionId = (int)SOAFaultCodeCategory.SessionFatalError | 0x0010;

        /// <summary>
        ///   <para>The security mode of the binding did not match the value of 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" /> property. Change the security mode of the binding or the value of the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Secure" /> property so that they match, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BindingSecurityModeMismatched = (int)SOAFaultCodeCategory.SessionFatalError | 0x0011;

        /// <summary>
        ///   <para>The specified binding is not supported. Check that a supported binding is specified, and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_BindingNotSupported = (int)SOAFaultCodeCategory.SessionFatalError | 0x0012;

        /// <summary>
        ///   <para>A default uniform resource indicator (URI) for the specified transport scheme 
        /// was not found. Specify a default URI for the transport scheme and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_NoDefaultUriForScheme = (int)SOAFaultCodeCategory.SessionFatalError | 0x0013;

        /// <summary>
        ///   <para>The custom binding cannot be found in the service configuration file. Check 
        /// the service configuration file and add information about the custom binding if it is not present.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_CannotFindCustomBindingConfiguration = (int)SOAFaultCodeCategory.SessionFatalError | 0x0014;

        /// <summary>
        ///   <para>The client identifier contained characters that are not allowed or was 
        /// too long. The client identifier can only contain digits, uppercase and lowercase letters, underscores  
        /// (_), hyphens (-), braces ({ and }), and spaces. The client identifier cannot contain 
        /// more than 128 characters. Specify a client identifier that meets these criteria and try again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_InvalidClientIdOrTooLong = (int)SOAFaultCodeCategory.SessionFatalError | 0x0016;

        /// <summary>
        ///   <para>The client identifier is not valid, because it does not 
        /// match the corresponding messages on this connection. Check the client identifier and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_ClientIdNotMatch = (int)SOAFaultCodeCategory.SessionFatalError | 0x0017;

        /// <summary>
        ///   <para>The specified user is not authorized to access the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object with the specified client identifier.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_UserNameNotMatch = (int)SOAFaultCodeCategory.SessionFatalError | 0x0018;

        /// <summary>
        ///   <para>The client application failed to attach to the specified job because it is not a 
        /// service job. Specify the service job for the session to which the client application should attach and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Session_ValidateJobFailed_NotServiceJob = (int)SOAFaultCodeCategory.SessionFatalError | 0x0019;

        /// <summary>
        ///   <para>The client application failed to attach to the specified session because the specified session 
        /// is an interactive session. Specify a durable session to which the client application should attach and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Session_ValidateJobFailed_NotDurableSession = (int)SOAFaultCodeCategory.SessionFatalError | 0x001a;

        /// <summary>
        ///   <para>The client application failed to attach to the specified session because the service job is finished.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Session_ValidateJobFailed_AlreadyFinished = (int)SOAFaultCodeCategory.SessionFatalError | 0x001b;

        /// <summary>
        ///   <para>The client application failed to attach to the specified session because the service job was canceled.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Session_ValidateJobFailed_JobCanceled = (int)SOAFaultCodeCategory.SessionFatalError | 0x001c;

        /// <summary>
        ///   <para>The specified configuration file is not valid. Check the configuration file and update it if necessary, then try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ConfigFile_Invalid = (int)SOAFaultCodeCategory.SessionFatalError | 0x001d;

        /// <summary>
        ///   <para>The configuration file for the service was not found. Check that the configuration 
        /// file for the specified service is deployed to the path that is specified by CCP_SERVICEREGISTRATION_PATH environment variable.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Service_NotFound = (int)SOAFaultCodeCategory.SessionFatalError | 0x001e;

        /// <summary>
        ///   <para>The CCP_SERVICEREGISTRATION_PATH environment variable does not specify any directories for registering services. Set the value of 
        /// the CCP_SERVICEREGISTRATION_PATH environment variable to a directory that contains 
        /// the configuration files that you want to use to register services.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Service_RegistrationDirsMissing = (int)SOAFaultCodeCategory.SessionFatalError | 0x001f;

        /// <summary>
        ///   <para>The specified version of the service could not be found. Check the configuration file for the 
        /// version of the service that you want to use 
        /// is deployed to the directory that the CCP_SERVICEREGISTRATION_PATH environment variable specifies.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ServiceVersion_NotFound = (int)SOAFaultCodeCategory.SessionFatalError | 0x0020;

        /// <summary>
        ///   <para>The session was created by a broker with a version that the current broker 
        /// does not support. Upgrade the version of Microsoft HPC Pack on your broker node and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_UnsupportedVersion = (int)SOAFaultCodeCategory.SessionFatalError | 0x0021;

        /// <summary>
        ///   <para>An operation was performed that is not supported. This can happen if the session was created by a broker with 
        /// a version that the current broker does not support. Upgrade the version of Windows HPC Server on your broker node and try again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_UnsupportedOperation = (int)SOAFaultCodeCategory.SessionFatalError | 0x0022;

        /// <summary>
        ///   <para>A concurrent session was started while in debug mode.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int DebugModeNotSupportConcurrentSession = (int)SOAFaultCodeCategory.SessionFatalError + 0x0023;

        /// <summary>
        ///   <para>The required service was preempted.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Service_Preempted = (int)SOAFaultCodeCategory.SessionFatalError + 0x0024;

        /// <summary>
        ///   <para>Authentication failed, and either a valid HPC Soft Card certificate or user name and password combination is required.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AuthenticationFailure_NeedEitherTypeCred = (int)SOAFaultCodeCategory.SessionFatalError + 0x0025;

        /// <summary>
        ///   <para>Authentication failed, and an HPC Soft Card certificate must be supplied.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>Note that you cannot provide a username and password to authenticate the session if this error code is returned.</para>
        /// </remarks>
        public const int AuthenticationFailure_NeedCertOnly = (int)SOAFaultCodeCategory.SessionFatalError + 0x0026;

        /// <summary>
        ///   <para>The session ID is not valid.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InprocessBroker_InvalidSessionId = (int)SOAFaultCodeCategory.SessionFatalError + 0x0027;

        /// <summary>
        ///   <para>An attempt was made to start a concurrent session in an in-process broker.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int InprocessNotSupportConcurrentSession = (int)SOAFaultCodeCategory.SessionFatalError + 0x0028;

        /// <summary>
        ///   <para>Authentication failed when the job was submitted. Either a valid HPC Soft 
        /// Card certificate or user name and password combination is required, and the credentials cannot be reused.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AuthenticationFailure_NeedEitherTypeCred_UnReusable = (int)SOAFaultCodeCategory.SessionFatalError + 0x0029;

        /// <summary>
        ///   <para>Authentication failed when the job was submitted. A valid 
        /// user name and password combination is required, and the credentials cannot be reused.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AuthenticationFailure_NeedPasswordOnly_UnReusable = (int)SOAFaultCodeCategory.SessionFatalError + 0x0030;

        /// <summary>
        ///   <para>The HPC Web Service API does not support the use of an in-process broker. To use this feature, use 
        /// the items in the namespaces that have names that 
        /// begin with Microsoft.Hpc.Scheduler.Session in the <see href="http://msdn.microsoft.com/library/hh500712(VS.85).aspx">HPC Class Library</see> (http://msdn.microsoft.com/library/hh500712(VS.85).aspx) directly.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_NotSupportInprocessBroker = (int)SOAFaultCodeCategory.SessionFatalError + 0x0031;

        /// <summary>
        ///   <para>The Windows Azure HPC Scheduler does not support durable sessions. Use an interactive session instead.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Azure_NotSupportDurableSession = (int)SOAFaultCodeCategory.SessionFatalError + 0x0032;

        /// <summary>
        ///   <para>The Windows Azure HPC Scheduler does not support the use of an in-process broker.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Azure_NotSupportInprocessBroker = (int)SOAFaultCodeCategory.SessionFatalError + 0x0033;

        /// <summary>
        ///   <para>Validation of the api-version request header or URI parameter for an operation in the web 
        /// service API failed. Create another request with a valid api-request header or URI parameter and try the operation again.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>For information about the operations in the web service API and the request 
        /// headers and URI parameters for those operations, 
        /// see the <see href="http://msdn.microsoft.com/library/hh560258(VS.85).aspx">HPC Web Service API Reference</see> (http://msdn.microsoft.com/library/hh560258(VS.85).aspx).</para> 
        /// </remarks>
        public const int WebAPI_APIVersionIncorrect = (int)SOAFaultCodeCategory.SessionFatalError + 0x0034;

        /// <summary>
        ///   <para>The application failed to connect to the SOA web service because permissions to access the web service were 
        /// not granted to the account that started or attached to the session. This error occurs only when the using the  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.TransportScheme.WebAPI" /> transport scheme to connect to the Windows Azure HPC Scheduler through the REST API. Check that the account specified in the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> or 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionAttachInfo" /> class has permissions to access the web service, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int WebAPI_AccessDenied = (int)SOAFaultCodeCategory.SessionFatalError + 0x0035;

        /// <summary>
        ///   <para>The application failed to connect to the SOA diagnostics 
        /// service because permissions to access the SOA diagnostics service were not granted.  
        /// Check that the account under which your application runs has permissions 
        /// to access the SOA diagnostics service, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AccessDenied_SoaDiagSvc = (int)SOAFaultCodeCategory.SessionFatalError + 0x0036;

        /// <summary>
        ///   <para>The application failed to connect to the SOA diagnostics cleanup 
        /// service because permissions to access the SOA diagnostics cleanup service were not  
        /// granted. Check that the account under which your application runs has permissions 
        /// to access the SOA diagnostics cleanup service, and try to run the application again.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int AccessDenied_SoaDiagCleanupSvc = (int)SOAFaultCodeCategory.SessionFatalError + 0x0037;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int Broker_AzureConnectionStringNotAvailable = (int)SOAFaultCodeCategory.SessionFatalError + 0x0038;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ClientServerVersionMismatch = (int)SOAFaultCodeCategory.SessionFatalError + 0x0039;

        #endregion

        #region Unknown

        /// <summary>
        ///   <para>An unknown error occurred. See the additional text displayed for the error for information about the error and how to resolve it.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int UnknownError = (int)SOAFaultCodeCategory.Unknown | 0x0000;

        /// <summary>
        ///   <para>An <see cref="System.ArgumentException" /> occurred. See the error message for the exception for more information.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        public const int ArgumentError = (int)SOAFaultCodeCategory.Unknown | 0xFFFF;
        #endregion

        /// <summary>
        /// Number of codes per category
        /// </summary>
        internal const int codesPerCategory = 0x01000000;

        static SOAFaultCode()
        {
            faultCodeNameDic = new Dictionary<int, string>();
            foreach (FieldInfo fi in typeof(SOAFaultCode).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                try
                {
                    int value = (int)fi.GetValue(null);
                    faultCodeNameDic.Add(value, fi.Name);
                }
                catch { }
            }
        }

        /// <summary>
        ///   <para>Gets the category of the SOA error with the specified error code.</para>
        /// </summary>
        /// <param name="code">
        ///   <para>Integer that specifies the error code of the SOA error for which you want to get the category.</para>
        /// </param>
        /// <returns>
        ///   <para>A value from the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SOAFaultCodeCategory" /> enumeration that represents the category of the error with the specified error code.</para> 
        /// </returns>
        public static SOAFaultCodeCategory Category(int code)
        {
            if ((code >= (int)SOAFaultCodeCategory.SessionConnectionError) && (code < (int)SOAFaultCodeCategory.SessionConnectionError + codesPerCategory))
            {
                return SOAFaultCodeCategory.SessionConnectionError;
            }
            else if ((code >= (int)SOAFaultCodeCategory.SessionError) && (code < (int)SOAFaultCodeCategory.SessionError + codesPerCategory))
            {
                return SOAFaultCodeCategory.SessionError;
            }
            else if ((code >= (int)SOAFaultCodeCategory.SessionFatalError) && (code < (int)SOAFaultCodeCategory.SessionFatalError + codesPerCategory))
            {
                return SOAFaultCodeCategory.SessionFatalError;
            }
            else if ((code >= (int)SOAFaultCodeCategory.ApplicationError) && (code < (int)SOAFaultCodeCategory.ApplicationError + codesPerCategory))
            {
                return SOAFaultCodeCategory.ApplicationError;
            }
            else
            {
                return SOAFaultCodeCategory.Unknown;
            }
        }

        /// <summary>
        ///   <para>Gets the name of the SOA error with the specified error code.</para>
        /// </summary>
        /// <param name="code">
        ///   <para>Integer that specifies the error code of the SOA error for which you want to get the name.</para>
        /// </param>
        /// <returns>
        ///   <para>A <see cref="System.String" /> that contains the name of the SOA error with the specified error code.</para>
        /// </returns>
        public static string GetFaultCodeName(int code)
        {
            lock (faultCodeNameDic)
            {
                return faultCodeNameDic[code];
            }
        }
    }
}
