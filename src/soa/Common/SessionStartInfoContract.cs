namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Microsoft.Hpc.Scheduler.Session;

    /// <summary>
    /// Contains information needed to start a session
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/")]
    [Serializable]
    public class SessionStartInfoContract
    {

        public SessionStartInfoContract()
        {
        }

        /// <summary>
        /// Specify allocation grow load ratio threshold
        /// </summary>
        [DataMember]
        public string RegPath { set; get; }

        [DataMember]
        public string[] IpAddress { set; get; }

        [DataMember]
        public bool IsNoSession;

        [DataMember]
        public int? AllocationGrowLoadRatioThreshold;

        /// <summary>
        /// Specify allocation shrink load ratio threshold
        /// </summary>
        [DataMember]
        public int? AllocationShrinkLoadRatioThreshold;

        /// <summary>
        /// Specify client idle timeout
        /// </summary>
        [DataMember]
        public int? ClientIdleTimeout;

        /// <summary>
        /// Specify client connection timeout
        /// </summary>
        [DataMember]
        public int? ClientConnectionTimeout;

        /// <summary>
        /// Specify session idle timeout
        /// </summary>
        [DataMember]
        public int? SessionIdleTimeout;

        /// <summary>
        /// Specify message throttle start threshold
        /// </summary>
        [DataMember]
        public int? MessagesThrottleStartThreshold;

        /// <summary>
        /// Specify message throttle stop threshold
        /// </summary>
        [DataMember]
        public int? MessagesThrottleStopThreshold;

        /// <summary>
        /// Specify the service name
        /// </summary>
        [DataMember]
        public string ServiceName;

        /// <summary>
        /// Specify the transport scheme
        /// </summary>
        [DataMember]
        public TransportScheme TransportScheme = TransportScheme.NetTcp;

        /// <summary>
        /// Specify if use secure session
        /// </summary>
        [DataMember]
        public bool Secure = true;

        /// <summary>
        /// Specify if use preempt
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool? CanPreempt = true;

        /// <summary>
        /// Specify if session is shared
        /// </summary>
        [DataMember]
        public bool ShareSession;

        // JobPropertyIds

        /// <summary>
        /// Specify the job template
        /// </summary>
        [DataMember]
        public string JobTemplate;

        /// <summary>
        /// Specify the resource unit type
        /// </summary>
        [DataMember]
        public int? ResourceUnitType;

        /// <summary>
        /// Specify the max units
        /// </summary>
        [DataMember]
        public int? MaxUnits;

        /// <summary>
        /// Specify the min units
        /// </summary>
        [DataMember]
        public int? MinUnits;

        // User credentials

        /// <summary>
        /// Specify user name
        /// </summary>
        [DataMember]
        public string Username;

        /// <summary>
        /// Specify password
        /// </summary>
        [DataMember]
        public string Password;

        /// <summary>
        /// Save the username/password or not.
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool? SavePassword;

        [DataMember(IsRequired = false)]
        public byte[] Certificate;

        /// <summary>
        /// The plain text password of the certificate
        /// </summary>
        [DataMember(IsRequired = false)]
        public string PfxPassword;

        /// <summary>
        /// Specify service jobname
        /// </summary>
        [DataMember]
        public string ServiceJobName;

        /// <summary>
        /// Specify service job project
        /// </summary>
        [DataMember]
        public string ServiceJobProject;

        /// <summary>
        /// Specify node group
        /// </summary>
        [DataMember]
        public string NodeGroupsStr;

        /// <summary>
        /// Specify requested node
        /// </summary>
        [DataMember]
        public string RequestedNodesStr;

        /// <summary>
        /// Specify priority
        /// </summary>
        [DataMember]
        public int? Priority;

        /// <summary>
        /// Specify extended priority
        /// </summary>
        [DataMember]
        public int? ExtendedPriority;

        /// <summary>
        /// specify runtime
        /// </summary>
        [DataMember]
        public int Runtime = -1;

        /// <summary>
        /// specify environment variables
        /// </summary>
        [DataMember]
        public Dictionary<string, string> Environments;

        /// <summary>
        /// specify diagnositc broker node
        /// </summary>
        [DataMember(IsRequired = false)]
        public string DiagnosticBrokerNode;

        /// <summary>
        /// specify if is admin job for host in diagnostic
        /// </summary>
        [DataMember]
        public bool AdminJobForHostInDiag;

        /// <summary>
        /// specify the service version
        /// </summary>
        [DataMember]
        public Version ServiceVersion;

        /// <summary>
        /// specify client broker heart beat interval
        /// </summary>
        [DataMember]
        public int? ClientBrokerHeartbeatInterval;

        /// <summary>
        /// specify client broker heart beat retry count
        /// </summary>
        [DataMember]
        public int? ClientBrokerHeartbeatRetryCount;

        /// <summary>
        /// specify max message size
        /// </summary>
        [DataMember]
        public int? MaxMessageSize;

        /// <summary>
        /// specify service operation timeout
        /// </summary>
        [DataMember]
        public int? ServiceOperationTimeout;

        /// <summary>
        /// specify EPR list
        /// </summary>
        public string[] EprList;

        /// <summary>
        /// Gets or sets a value indicating whether the session is inprocess
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool UseInprocessBroker;

        [DataMember(IsRequired = false)]
        public bool UseSessionPool;

        /// <summary>
        /// Set this property to false to ask broker to disable the feature to
        /// auto dispose broker client when using net.tcp frontend.
        /// </summary>
        /// <remarks>
        /// SOA web service will set this property to false to disable this
        /// feature as it uses net.tcp channels to communicate with broker
        /// and it does not maintain a live connection through broker's life
        /// cycle.
        /// </remarks>
        [DataMember]
        public bool? AutoDisposeBrokerClient;

        /// <summary>
        /// Gets or sets a value indicating whether Azure queue/blob is used for the request and response messages
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool? UseAzureQueue;

        /// <summary>
        /// Get or set whether the username and password windows client credential is used for the authentication
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool UseWindowsClientCredential = false;

        /// <summary>
        /// Gets or sets the files required for this session. 
        /// The format is: <DataClientId>=<relativePath to %HPC_SOADATAJOBDIR%>;<DataClientId>=<relativePath to %HPC_SOADATAJOBDIR%>
        /// for example: dataclientid1=workbook.xlsb;dataclientid2=Dlls\dependA.dll;dataclientid3=Dlls\dependB.dll
        /// </summary>
        [DataMember(IsRequired = false)]
        [Obsolete]
        public string DependFiles;

        /// <summary>
        /// Indicate the client api version
        /// </summary>
        [DataMember(IsRequired = false)]
        public Version ClientVersion;

        /// <summary>
        /// Specify the dispatcher capacity used in grow and shrink. By default it is zero which means the auto calculated number of allocated cores.
        /// </summary>
        [DataMember(IsRequired = false)]
        public int? DispatcherCapacityInGrowShrink;

        /// <summary>
        /// Gets or sets a value indicating whether the client is non-domain local user
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool? LocalUser;
        /// <summary>
        /// Specify the parent job Ids, upon the completion of which, this session job will begin to run.
        /// </summary>
        [DataMember(IsRequired = false)]
        public List<int> ParentJobIds;

        /// <summary>
        /// Specify the service host idle timeout, in milliseconds.
        /// </summary>
        [DataMember(IsRequired = false)]
        public int? ServiceHostIdleTimeout;

        /// <summary>
        /// Specify the service hang timeout, in milliseconds.
        /// </summary>
        [DataMember(IsRequired = false)]
        public int? ServiceHangTimeout;

        /// <summary>
        /// Specify if we are going to use AAD integration.
        /// </summary>
        [DataMember(IsRequired = false)]
        public bool UseAad = false;

        /// <summary>
        /// specify if to use AAD or local user credential
        /// </summary>
        public bool IsAadOrLocalUser => this.UseAad || this.LocalUser.GetValueOrDefault();

        /// <summary>
        /// specify the channel type
        /// </summary>
        public ChannelTypes ChannelType {
            get
            {
                if (this.UseAad)
                {
                    return ChannelTypes.AzureAD;
                }
                else if (this.LocalUser.GetValueOrDefault())
                {
                    return ChannelTypes.Certificate;
                }
                else
                {
                    return ChannelTypes.LocalAD;
                }
            }
        }

        public bool UseAzureStorage => this.TransportScheme == TransportScheme.AzureStorage || this.UseAzureQueue.GetValueOrDefault();

        [DataMember(IsRequired = false)]
        public Dictionary<string, string> DependFilesStorageInfo = new Dictionary<string, string>(0);
    }
}
