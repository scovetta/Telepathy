namespace Microsoft.Hpc.Scheduler.Store
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Scheduler.Properties;

    public class StoreConnectionContext
    {
        private Func<CancellationToken, Task<string>> Resolver;

        private string schedulerNode;

        private IHpcContext context;
        public IHpcContext Context
        {
            get { return this.context; }
            private set
            {
                this.context = value;
                if (this.context.FabricContext.UseInternalConnection())
                {
                    this.InternalConnection = true;
                }
            }
        }

        public StoreConnectionContext(Func<CancellationToken, Task<string>> resolver)
        {
            this.Resolver = resolver;
        }

        public StoreConnectionContext(string oldMultiFormatName, CancellationToken token)
        {
            int port;

            if (this.IsHttp = HpcContext.CheckIfHttps(oldMultiFormatName, out this.schedulerNode, out port))
            {
                RestServiceUtil.IgnoreCertNameMismatchValidation();
                this.Port = port;
            }
            else
            {
                this.Context = HpcContext.GetOrAdd(oldMultiFormatName, token);
            }
        }

        public StoreConnectionContext(IHpcContext context)
        {
            if (context.FabricContext.UseInternalConnection())
            {
                this.InternalConnection = true;
            }

            this.Context = context;
        }

        private void Validate()
        {
            if (this.IsHttp && this.ServiceAsClient)
            {
                throw new SchedulerException(ErrorCode.Operation_ServiceAsClientNotSupportedOverHttps, string.Empty);
            }
        }

        private bool serviceAsClient;
        public bool ServiceAsClient
        {
            get
            {
                return this.serviceAsClient;
            }
            set
            {
                this.serviceAsClient = value;
                this.Validate();
            }
        }

        private ServiceAsClientIdentityProvider identityProvider;
        public ServiceAsClientIdentityProvider IdentityProvider
        {
            get { return this.identityProvider; }
            set
            {
                this.identityProvider = value;
                this.ServiceAsClient = this.identityProvider != null;
            }
        }

        public ServiceAsClientPrincipalProvider PrincipalProvider { get; set; }

        public int Port { get; set; } = 5802;

        public int RemotingPort { get; set; } = 5800;

        private bool isHttp;
        public bool IsHttp
        {
            get
            {
                return this.isHttp;
            }

            set
            {
                this.isHttp = value;
                this.Validate();
            }
        }

        private string userName;
        public string UserName
        {
            get { return this.userName; }
            set
            {
                this.userName = value;
                if (!string.IsNullOrEmpty(value))
                {
                    string domainName, rawUserName;
                    ExtractDomainAndUser(this.userName, out domainName, out rawUserName);
                    this.DomainName = domainName;
                    this.RawUserName = rawUserName;
                }
            }
        }

        public string Password { get; set; }

        public string DomainName { get; private set; }

        public string RawUserName { get; private set; }

        public bool InternalConnection { get; set; } = false;

        /// <summary>
        /// Examines the raw ServiceAsClient username and extracts any domain
        /// information (domain/user or user@domain) seperate from user information.
        /// </summary>
        /// <param name="sacUser"></param>
        /// <param name="domain"></param>
        /// <param name="user"></param>
        private static void ExtractDomainAndUser(string sacUser, out string domain, out string user)
        {
            string[] byBackslant = sacUser.Split(new char[] { '\\' });

            // domain\user
            if ((null != byBackslant) && (byBackslant.Length == 2))
            {
                domain = byBackslant[0];
                user = byBackslant[1];
            }
            else
            {
                string[] byAt = sacUser.Split(new char[] { '@' });

                // user@domain
                if ((null != byAt) && (byAt.Length == 2))
                {
                    user = byAt[0];
                    domain = byAt[1];
                }
                else
                {
                    user = sacUser;
                    domain = null;
                }
            }
        }

        public async Task<string> ResolveSchedulerNodeAsync(CancellationToken token)
        {
            if (this.schedulerNode != null)
            {
                return this.schedulerNode;
            }
            else if (this.Resolver != null)
            {
                return await this.Resolver(token).ConfigureAwait(false);
            }
            else if (this.Context != null)
            {
                return await this.Context.ResolveSchedulerNodeAsync().ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException("Cannot resolve the scheduler node with all methods");
            }
        }
    }
}
