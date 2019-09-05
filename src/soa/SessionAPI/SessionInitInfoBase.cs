using TelepathyCommon.HpcContext;

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.ServiceModel.Channels;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;

    using TelepathyCommon;

    /// <summary>
    /// Base class implements <see cref="IConnectionInfo"/>
    /// </summary>
    public abstract class SessionInitInfoBase : IConnectionInfo
    {
        /// <summary>
        /// headnode name
        /// </summary>
        protected string headnode;

        /// <summary>
        /// Gets the headnode name
        /// </summary>
        public string Headnode
        {
            get
            {
                return headnode;
            }
        }

        public string AzureStorageConnectionString { get; set; }

        public string AzureTableStoragePartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TransportScheme"/>
        /// </summary>
        public abstract TransportScheme TransportScheme { get; set; }

        /// <summary>
        /// Get or set whether AAD integration is used for authentication.
        /// </summary>
        public bool UseAad { get; set; }

        private bool useLocalUser = false;

        /// <summary>
        /// Get or set whether login as local user. 
        /// This flag only tasks effort if client machine is non-domain joined.
        /// </summary>
        public virtual bool LocalUser
        {
            get => this.useLocalUser && SoaHelper.IsCurrentUserLocal();
            set => this.useLocalUser = value && SoaHelper.IsCurrentUserLocal();
        }

        /// <summary>
        /// Gets a value indicates if to authenticate as AAD or local user
        /// </summary>
        public bool IsAadOrLocalUser => this.UseAad || this.LocalUser;

        public abstract string Username { get; set; }

        public abstract string InternalPassword { get; set; }

        /// <summary>
        /// Resolved machine name of head node for internal use.
        /// </summary>
        public Task<string> ResolveHeadnodeMachineAsync() => this.Context.ResolveSessionLauncherNodeOnIaasAsync(this.Headnode);

        /// <summary>
        /// Stores the fabric cluster context
        /// </summary>
        internal ITelepathyContext Context { get; set; }

        protected SessionInitInfoBase(string headnode)
        {
            if (string.IsNullOrEmpty(headnode))
            {
                // retrieve the head node name from the %ccp_scheduler% environment if it is empty
                headnode = Environment.GetEnvironmentVariable(TelepathyCommon.TelepathyConstants.SchedulerEnvironmentVariableName);
                if (string.IsNullOrEmpty(headnode))
                {
                    throw new ArgumentNullException(SR.HeadnodeCantBeNull);
                }
            }
            else
            {
                // expand the environment variables in the headnode if any
                headnode = Environment.ExpandEnvironmentVariables(headnode);
            }

            this.headnode = headnode;


            this.Context = TelepathyContext.GetOrAdd(this.headnode, CancellationToken.None);
        }

        protected SessionInitInfoBase()
        {
            //this.headnode is the hostname of local machine.
            this.headnode= System.Net.Dns.GetHostName();
            this.Context = TelepathyContext.GetOrAdd(this.headnode, CancellationToken.None);
        }

        /// <summary>
        /// Get session launcher node
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSessionLauncherAddressAsync()
        {
            // AAD ready
            string hostname = await this.ResolveHeadnodeMachineAsync().ConfigureAwait(false);
            var scheme = this.TransportScheme;

            if ((scheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                if (this.UseAad)
                {
                    return string.Format(SoaHelper.SessionLauncherAadAddressFormat, SoaHelper.NetTcpPrefix, hostname, SoaHelper.SessionLauncherPort(SoaHelper.IsSchedulerOnAzure(hostname)));
                }
                else if (this.LocalUser)
                {
                    return string.Format(SoaHelper.SessionLauncherInternalAddressFormat, SoaHelper.NetTcpPrefix, hostname, SoaHelper.SessionLauncherPort(SoaHelper.IsSchedulerOnAzure(hostname)));
                }
                else
                {
                    return string.Format(SoaHelper.SessionLauncherAddressFormat, SoaHelper.NetTcpPrefix, hostname, SoaHelper.SessionLauncherPort(SoaHelper.IsSchedulerOnAzure(hostname)));
                }
            }
            else if ((scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                return string.Format(SoaHelper.SessionLauncherNetHttpAddressFormat, SoaHelper.HttpsPrefix, hostname, SoaHelper.HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Http) == TransportScheme.Http)
            {
                return string.Format(SoaHelper.SessionLauncherAddressFormat, SoaHelper.HttpsPrefix, hostname, SoaHelper.HttpsDefaultPort);
            }
            else if ((scheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return string.Format(SoaHelper.SessionLauncherAddressFormat, SoaHelper.NetTcpPrefix, hostname, SoaHelper.SessionLauncherPort(SoaHelper.IsSchedulerOnAzure(hostname)));
            }
            else
            {
                return string.Empty;
            }
        }
    }
}