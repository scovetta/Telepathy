namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Scheduler.Store;
    using Properties;

    /// <summary>
    /// <para>The connection context used to connect to Scheduler service.</para>
    /// </summary>
    // This class simply wraps the StoreConnectionContext and hides the FabricClient type.
    // All implementations should go to StoreConnectionContext class.
    public class SchedulerConnectionContext
    {
        internal StoreConnectionContext Context { get; private set; }

        /// <summary>
        /// <para>Initialize a new instance of the SchedulerConnectionContext class.</para>
        /// </summary>
        /// <param name="resolver">an async method which resolves the scheduler's node when connection and reconnection.</param>
        public SchedulerConnectionContext(Func<CancellationToken, Task<string>> resolver)
        {
            this.Context = new StoreConnectionContext(resolver);
        }

        /// <summary>
        /// <para>Initialize a new instance of the SchedulerConnectionContext class.</para>
        /// </summary>
        /// <param name="hpcContext">The instance which implemented IHpcContext.</param>
        public SchedulerConnectionContext(IHpcContext hpcContext)
        {
            this.Context = new StoreConnectionContext(hpcContext);
        }

        /// <summary>
        /// <para>Initialize a new instance of the SchedulerConnectionContext class.</para>
        /// </summary>
        /// <param name="oldMultiFormatString">This thing could be of following format.
        /// 1. hostname or hostname.domainname.com, so the string will be used as the scheduler's node.
        /// 2. hostname1:port1;hostname2.domainname.com:port2;... which is the connection string to the cluster. 
        ///     It is used to connect to the name service to resolve or re-resolve the scheduler's location, and then connect to scheduler.
        ///     It is equivalent to the overload which accepts the EndpointsConnectionString.
        /// 3. https://schedulernode:port, in this case, the connection will use https as the protocol to connect to scheduler, unless you set the IsHttp property to false explicit.
        /// </param>
        /// <param name="token">The cancellation token.</param>
        public SchedulerConnectionContext(string oldMultiFormatString, CancellationToken token)
        {
            this.Context = new StoreConnectionContext(oldMultiFormatString, token);
        }

        /// <summary>
        /// <para>Initialize a new instance of the SchedulerConnectionContext class.</para>
        /// </summary>
        /// <param name="connectionString">the endpoint connection string which is used to connect to the name service to resolve or re-resolve the scheduler's location, and then connect to scheduler.</param>
        /// <param name="token">The cancellation token.</param>
        public SchedulerConnectionContext(EndpointsConnectionString connectionString, CancellationToken token)
        {
            this.Context = new StoreConnectionContext(HpcContext.GetOrAdd(connectionString, token));
        }

        /// <summary>
        /// <para>Gets or sets a value indicating whether the connection is in ServiceAsClient mode.</para>
        /// </summary>
        /// <value>True if the connection is in Service as client mode.</value>
        public bool ServiceAsClient
        {
            get { return this.Context.ServiceAsClient; }
            set { this.Context.ServiceAsClient = value; }
        }

        /// <summary>
        /// <para>Gets or sets the identity provider instance which is used by the ServiceAsClient mode.</para>
        /// </summary>
        /// <value>The instance of the identity provider.</value>
        public ServiceAsClientIdentityProvider IdentityProvider
        {
            get { return this.Context.IdentityProvider; }
            set { this.Context.IdentityProvider = value; }
        }

        /// <summary>
        /// <para>Gets or sets the principle provider instance which is used by the ServiceAsClient mode.</para>
        /// </summary>
        /// <value>the instance of the principle provider.</value>
        public ServiceAsClientPrincipalProvider PrincipalProvider
        {
            get { return this.Context.PrincipalProvider; }
            set { this.Context.PrincipalProvider = value; }
        }

        /// <summary>
        /// <para>Gets or sets the port to connect.</para>
        /// </summary>
        /// <value>The port of the connection.</value>
        public int Port
        {
            get { return this.Context.Port; }
            set { this.Context.Port = value; }
        }

        /// <summary>
        /// <para>Gets or sets a value indicating whether the connection will use https protocol.</para>
        /// </summary>
        /// <value>True if the connection is using https.</value>
        public bool IsHttp
        {
            get { return this.Context.IsHttp; }
            set { this.Context.IsHttp = value; }
        }

        /// <summary>
        /// <para>Gets or sets the user name.</para>
        /// </summary>
        /// <value>The user name.</value>
        public string UserName
        {
            get { return this.Context.UserName; }
            set { this.Context.UserName = value; }
        }

        /// <summary>
        /// <para>Gets or sets the password.</para>
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get { return this.Context.Password; }
            set { this.Context.Password = value; }
        }

        /// <summary>
        /// <para>Gets or sets the domain name.</para>
        /// </summary>
        /// <value>The domain name.</value>
        public string DomainName { get { return this.Context.DomainName; } }

        /// <summary>
        /// <para>Gets or sets the raw user name.</para>
        /// </summary>
        /// <value>The raw user name.</value>
        public string RawUserName { get { return this.Context.RawUserName; } }
    }
}
