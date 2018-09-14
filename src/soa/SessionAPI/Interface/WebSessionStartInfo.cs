//-----------------------------------------------------------------------
// <copyright file="WebSessionStartInfo.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Session start information data contract for SOA Web Service</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Defines a set of values that are used to create a session through the SOA web service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>The fields of this class represent the values that you can set when you use the 
    /// <see href="http://msdn.microsoft.com/library/hh770488(VS.85).aspx">Create Session</see> operation of the HPC Web Service. This class in not intended for 
    /// use when you use directly use methods in the HPC .NET Class Library, such as the  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" /> method, to create SOA sessions. When you use the HPC .NET Class Library methods to create SOA sessions, use the  
    /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" /> class to specify the configuration information for the session instead.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo" />
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com/")]
    public class WebSessionStartInfo
    {
        /// <summary>
        ///   <para>Indicates the upper threshold of available service capacity.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 125 percent.</para>
        ///   <para>The number of service instances that the broker can initiate (capacity) grows and shrinks based on the client activity (message requests). If the current 
        /// load (the ratio of pending requests to available capacity) rises above this threshold, the broker grows the service capacity until the load ratio falls within the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationShrinkLoadRatioThreshold" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationGrowLoadRatioThreshold" /> range. </para>
        ///   <para>You must cast the value to an integer. If the value is null (a null value means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationShrinkLoadRatioThreshold" />
        [DataMember]
        public int? AllocationGrowLoadRatioThreshold;

        /// <summary>
        ///   <para>Indicates the lower threshold of available service capacity.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 75 percent.</para>
        ///   <para>The number of service instances that the broker can initiate on the server (capacity) can grow and shrink based on the client activity (message requests). If 
        /// the current load (the ratio of pending requests to available capacity) falls below this threshold, the broker shrinks the service capacity until the load ratio falls within the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationShrinkLoadRatioThreshold" /> and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationGrowLoadRatioThreshold" /> range. </para>
        ///   <para>You must cast the value to an integer. If the value is null (a null value means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.AllocationGrowLoadRatioThreshold" />
        [DataMember]
        public int? AllocationShrinkLoadRatioThreshold;

        /// <summary>
        ///   <para>The amount of time, in milliseconds, that the client can go 
        /// without sending requests to the service, in Windows HPC Server 2008. The amount  
        /// of time, in milliseconds, that a client application can go without activity or 
        /// pending requests before the broker closes the connection, in Windows HPC Server 2008 R2.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 300,000 milliseconds.</para>
        ///   <para>If the idle timeout period is exceeded, the session closes. </para>
        ///   <para>In Windows HPC Server 2008 R2, the 
        /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T} method is a one-way call. If the client application times out, the 
        /// client application does not see the timeout exception until the client application calls the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests" /> method.</para>
        ///   <para>You must cast the value to an integer. If the value is null (a null value means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ClientIdleTimeout" />
        [DataMember]
        public int? ClientIdleTimeout;

        /// <summary>
        ///   <para>Indicates the amount of time, in milliseconds, in which the client must bind to the service after creating the session. </para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 300,000 milliseconds.</para>
        ///   <para>If the client does not bind to the service within the timeout 
        /// period, the broker is terminated if there are no other clients using the broker. </para>
        ///   <para>You must cast the value to an integer. If the value is null (a null value means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        [DataMember]
        public int? ClientConnectionTimeout;

        /// <summary>
        ///   <para>Indicates the amount of time in milliseconds that the broker waits for a client to bind 
        /// to the service after all previous client sessions ended, in Windows HPC Server 2008. Indicates the amount of  
        /// time in milliseconds that the broker waits for client applications to connect to a session after all previously 
        /// connected client applications time out, in Windows HPC Server 2008 R2. When this period elapses, the broker closes the session.</para> 
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is zero.</para>
        ///   <para>If the timeout period is exceeded, the broker closes. This property is useful only for shared sessions.</para>
        ///   <para>For Windows HPC Server 2008 R2, if the session uses an HTTP binding, the period for the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.SessionIdleTimeout" /> setting does not start until after the for the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ClientIdleTimeout" />setting elapses.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.SessionIdleTimeout" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ClientIdleTimeout" />
        [DataMember]
        public int? SessionIdleTimeout;

        /// <summary>
        ///   <para>Indicates the upper threshold at which point the broker stops receiving messages from the clients. </para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 5,120 messages.</para>
        ///   <para>You must cast the value to an integer. If the value is null (a null value means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.MessagesThrottleStartThreshold" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MessagesThrottleStopThreshold" />
        [DataMember]
        public int? MessagesThrottleStartThreshold;

        /// <summary>
        ///   <para>Indicates the lower threshold at which point the broker begins receiving messages from the clients.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default is 3,840 messages.</para>
        ///   <para>You must cast the value to an integer. If the value is null (a value of null means 
        /// that the value has not been set and the broker is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.MessagesThrottleStopThreshold" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MessagesThrottleStartThreshold" />
        [DataMember]
        public int? MessagesThrottleStopThreshold;

        /// <summary>
        ///   <para>Indicates the name of the SOA service to run on the nodes of the cluster.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>Specify the name of the registration file for the service. For example, 
        /// if the name of the registration file is EchoService.config, specify EchoService as the service name.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ServiceName" />
        [DataMember]
        public string ServiceName;

        /// <summary>
        ///   <para>Indicates whether the session can be preempted. 
        /// True indicates that the session can be preempted. This value is a 
        /// <see cref="System.Boolean" />. 
        /// False indicates that the session cannot be preempted.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.CanPreempt" />
        [DataMember(IsRequired = false)]
        public bool? CanPreempt = true;

        /// <summary>
        ///   <para>Indicates whether more than one user can connect to the SOA session. This value is a 
        /// <see cref="System.Boolean" />. 
        /// True indicates that more than one user can connect to the session, and anyone 
        /// who can submit jobs based on the job template can send requests to the broker.  
        /// False indicates the only one user can connect to the session, and only the 
        /// person who created the session can send requests to the broker. The default is  
        /// False.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ShareSession" />
        [DataMember]
        public bool ShareSession;

        // JobPropertyIds
        /// <summary>
        ///   <para>Indicates the template to use to set the default values and constraints for the service job.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>If this field is not set, the job uses the Default template. </para>
        ///   <para>Creating the session fails if the values that you specify for 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MaxUnits" />, 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MinUnits" />, and 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ResourceUnitType" />conflict with those that are specified in the template.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.JobTemplate" />
        [DataMember]
        public string JobTemplate;

        /// <summary>
        ///   <para>Indicates whether cores, nodes, or sockets are used to allocate resources for the service instance job. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType" /> enumeration. The default is 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core" />.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The resource units that you specify should be based on the threading model that the service uses. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core" /> if the service is linked to non-thread safe libraries. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Node" /> if the service is multithreaded. Specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Socket" /> if the service is single-threaded and memory-bus intensive.</para>
        ///   <para>By default, if you specify 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Core" />, the broker sends the service one message at a time. If you specify 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Node" />, the broker batches together the number of messages that is equal to the number of cores on the node, and then sends them to the service. If you specify  
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionUnitType.Socket" />, the broker batches together the number of messages that is equal to the number of cores on the socket, and then sends them to the service.</para> 
        ///   <para>To override the default behavior in Windows HPC Server 2008, configure the ServiceThrottlingBehavior section of your service.dll.config file to specify the 
        /// maximum concurrent calls that the service can accept. For example, if you are using the Parallel Extension, you can specify the following service behavior  
        /// in the service.dll.config file to override the default behavior for the node resource unit type so that the service receives only one request at 
        /// a time. The following example shows how to set the maximum number of concurrent calls that the service can accept in Windows HPC Server 2008.</para> 
        ///   <code>&lt;serviceBehaviors&gt;
        ///     &lt;behavior  name="Throttled"&gt;
        ///         &lt;serviceThrottling maxConcurrentCalls="1" /&gt;
        ///     &lt;/behavior&gt;
        /// &lt;/serviceBehaviors&gt;
        /// </code>
        ///   <para>For Windows HPC Server 2008, the broker uses the value of maxConcurrentCalls as the capacity of the service. This lets the 
        /// administrator or software developer use a standard WCF setting to fine tune the 
        /// dispatching algorithm of the broker node to fit the processing capacity of the service.</para> 
        ///   <para>For Windows HPC Server 2008 R2, you configure the 
        /// maxConcurrentCalls setting for the Service element in the microsoft.Hpc.Session.ServiceRegistration section of the servicename.config 
        /// file, where servicename is the same as the value you used for the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ServiceName" /> field. The value of the 
        /// maxConcurrentCalls attribute specifies the maximum number of messages that a service host can actively process. A value of 0 indicates that the maximum value should be calculated automatically based on 
        /// the service capacity of each service host. The service capacity of a service host is the number of cores for that host. The following example shows how to specify the  
        /// maxConcurrentCalls setting in Windows HPC Server 2008 R2.</para>
        ///   <code>  &lt;microsoft.Hpc.Session.ServiceRegistration&gt;
        ///     &lt;service assembly="%CCP_HOME%bin\EchoSvcLib.dll"
        ///  contract="EchoSvcLib.IEchoSvc"
        ///  type="EchoSvcLib.EchoSvc"
        ///  includeExceptionDetailInFaults="true"
        ///  maxConcurrentCalls="1"
        ///  maxMessageSize="65536"
        ///  serviceInitializationTimeout="60000" &gt;
        ///       &lt;!--The following lines add example environment variables to the service.   --&gt;
        ///       &lt;environmentVariables&gt;
        ///         &lt;add name="variable1" value="value1"/&gt;
        ///         &lt;add name="variable2" value="value2"/&gt;
        ///       &lt;/environmentVariables&gt;
        ///     &lt;/service&gt;</code>
        ///   <para>If you set the unit type, it must be the same it is as in the job template, if one is specified.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionResourceUnitType" />
        [DataMember]
        public int? ResourceUnitType;

        /// <summary>
        ///   <para>Indicates the maximum number of resource units that the scheduler can allocate for the service job.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ResourceUnitType" /> property defines the resource units; for example, nodes or cores.</para> 
        ///   <para>The maximum number of units must be within the constraints of the job template, if there are any.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MaximumUnits" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ResourceUnitType" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MinUnits" />
        [DataMember]
        public int? MaxUnits;

        /// <summary>
        ///   <para>Indicates the minimum number of resource units that the service job requires to run. </para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ResourceUnitType" /> property defines the resource units; for example, nodes or cores.</para> 
        ///   <para>The minimum number of units must be within the constraints of the job template, if there are any.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.MinimumUnits" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ResourceUnitType" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.MaxUnits" />
        [DataMember]
        public int? MinUnits;

        // User credentials
        /// <summary>
        ///   <para>Indicates the user name of the account under which the service job for the SOA session should run, in the form domain\user_name.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The user name is limited to 80 characters.</para>
        ///   <para>If the user under whose credentials the job runs differs from the job owner, the user under whose credentials the 
        /// job runs must be an administrator. If that user is not an administrator, an exception occurs because that user does not have  
        /// permission to read the job. The job owner is the user who runs the SOA client application. If you set the user 
        /// under whose credentials the job runs to be the same as the job owner, that user does not need to be an administrator.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Username" />
        [DataMember]
        public string Username;

        /// <summary>
        ///   <para>Indicates the password of the account under which the service job for the session should run.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The password is limited to 127 characters.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Password" />
        [DataMember]
        public string Password;

        /// <summary>
        ///   <para>Indicates the name to display for the service job.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ServiceJobName" />
        [DataMember]
        public string ServiceJobName;

        /// <summary>
        ///   <para>Indicates the project to use for the service job.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        [DataMember]
        public string ServiceJobProject;

        /// <summary>
        ///   <para>Specifies a list of the node groups that define the nodes on which the service job for the session can run.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>Use node groups that contain the nodes on which your job is 
        /// capable of running. For example, the nodes might contain the required software for your job.</para>
        ///   <para>If you specify multiple node groups, the resulting node list is the intersection of the groups. For example if group A 
        /// contains nodes 1, 2, 3, and 4 and group B contains nodes 3, 4, 5, and 6, the resulting list is 3 and 4. </para>
        ///   <para>If you also specify nodes in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.RequestedNodesStr" /> property, the job runs on the intersection of the requested node list and the resulting node group list.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.NodeGroupList" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.RequestedNodesStr" />
        [DataMember]
        public string NodeGroupsStr;

        /// <summary>
        ///   <para>Indicates the list of nodes that you request to run the service job for the session.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The nodes must exist in the HPC cluster.</para>
        ///   <para>Specify a list of the nodes on which your job is 
        /// capable of running. For example, the nodes might contain the required software for your job.</para>
        ///   <para>If you also specify a list of node group names in the 
        /// 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.NodeGroupsStr" /> property, the job runs on the intersection of the two lists.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.RequestedNodesList" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.NodeGroupsStr" />
        [DataMember]
        public string RequestedNodesStr;

        /// <summary>
        ///   <para>Indicates the priority to give to the service job for the session. For possible values, see the 
        /// <see cref="Microsoft.Hpc.Scheduler.Properties.StorePropertyType.JobPriority" /> enumeration. The default is Normal.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs. </para>
        ///   <para>Jobs can be preempted. The default preemption mode is Graceful, which means that the job 
        /// is preempted only after its running tasks complete. In case another preemption mode is set, consider setting the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.CanPreempt" /> service job property to 
        /// False so that the job runs until it finishes, fails, or is canceled.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.ExtendedPriority" />
        [DataMember]
        public int? Priority;

        /// <summary>
        ///   <para>Indicates the priority for the service job, using the expanded range of priority values in Windows HPC Server 
        /// 2008 R2. This priority value is between 0 and 4000, where 0 is the lowest priority and 4000 is the highest.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default priority is 2000.</para>
        ///   <para>Server resources are allocated to jobs based on job priority, except for backfill jobs.</para>
        ///   <para>Jobs can be preempted. The default preemption mode is Graceful, which means that the job 
        /// is preempted only after its running tasks complete. In case another preemption mode is set, consider setting the  
        /// <see cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.CanPreempt" />property for the service job to 
        /// False so that the job runs until it finishes, fails, or is canceled.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.SessionPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.ISchedulerJob.ExpandedPriority" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.Interface.WebSessionStartInfo.Priority" />
        [DataMember]
        public int? ExtendedPriority;

        /// <summary>
        ///   <para>Indicates the run-time limit for the service job, in seconds.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The wall clock is used to determine the run time. The time is your best guess of how long the job will take. It 
        /// needs to be fairly accurate because it is used to allocate resources. If 
        /// the job exceeds this time, the job is terminated and its state becomes Canceled.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Runtime" />
        [DataMember]
        public int Runtime = -1;

        /// <summary>
        ///   <para>Indicates the values of the environment variables of the service host. The value is a 
        /// <see cref="System.Collections.Generic.Dictionary`2" /> object that contains pairs of environment variable names and values.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.Environments" />
        [DataMember]
        public Dictionary<string, string> Environments;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        [DataMember(IsRequired = false)]
        public string DiagnosticBrokerNode;

        /// <summary>
        ///   <para />
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        [DataMember]
        public bool AdminJobForHostInDiag;

        /// <summary>
        ///   <para>Indicates the version of the SOA service to which the session should connect.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>If this value is not specified, the session uses the configuration file for the service that does not specify version information.</para>
        ///   <para>The version of a service is specified by the file name of the configuration file for the service, which has a 
        /// format of service_name_major. minor.config. For example, MyService_1.0.config. The version must include 
        /// the major and minor portions of the version identifier and no further subversions.</para> 
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.ServiceVersion" />
        [DataMember]
        public Version ServiceVersion;

        /// <summary>
        ///   <para>Indicates the maximum size, in kilobytes (kb) of a request or response message for the session.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default value of this property is 64.</para>
        ///   <para>The service-oriented architecture (SOA) runtime synchronizes all of the related 
        /// settings for all of the Windows Communication Foundation (WCF) bindings to this size.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.MaxMessageSize" />
        [DataMember]
        public int? MaxMessageSize;

        /// <summary>
        ///   <para>Indicates the amount of time in milliseconds that the service should try to perform operations before timing out.</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <remarks>
        ///   <para>The default value is 86,400,000 milliseconds, which is one day.</para>
        ///   <para>Applications can adjust this timeout for the maximum operation time of the service.</para>
        ///   <para>The service-oriented architecture (SOA) runtime synchronized all of the related settings for all 
        /// of the Windows Communication Foundation (WCF) bindings to this time-out value. The time-outs for the  
        /// "Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SendRequest{T} and 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.GetResponses" /> methods synchronize to this value.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.BrokerSettingsInfo.ServiceOperationTimeout" />
        [DataMember]
        public int? ServiceOperationTimeout;

        /// <summary>
        ///   <para>Indicates whether or not the service job for the session uses a resource pool. A 
        /// resource pool defines the proportion of cluster cores that must be guaranteed for specific user groups (or job types).</para>
        /// </summary>
        /// <returns>
        ///   <para />
        /// </returns>
        /// <seealso cref="Microsoft.Hpc.Scheduler.Session.SessionStartInfo.UseSessionPool" />
        [DataMember(IsRequired = false)]
        public bool UseSessionPool;
    }
}
