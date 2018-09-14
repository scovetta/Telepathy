namespace Microsoft.Hpc.Scheduler
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Hpc.Azure.FileStaging.Client;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Store;
    using Microsoft.Win32.SafeHandles;

    /// <summary>
    ///   <para>Defines the delegate to implement when you subscribe to the
    /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandTaskState" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>The Scheduler object.</para>
    /// </param>
    /// <param name="arg">
    ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateEventArg" /> object that provides the state information for the command.</para>
    /// </param>
    /// <remarks>
    ///   <para>To get the job that contains the command, cast the <paramref name="sender" /> object to an
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. Then, cast the <paramref name="arg" /> parameter to a
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg" /> object and pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.JobId" /> property to the
    ///
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get the job. To get the task that contains the command, pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.TaskId" /> property to the
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.JobStateHandler" />
    public delegate void CommandTaskStateHandler(object sender, CommandTaskStateEventArg arg);
    /// <summary>
    ///   <para>Defines the delegate to implement when you subscribe to the
    /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>The Scheduler object.</para>
    /// </param>
    /// <param name="arg">
    ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandOutputEventArg" /> object that provides the output and related information.</para>
    /// </param>
    /// <remarks>
    ///   <para>To get the job that contains the command, cast the <paramref name="sender" /> object to an
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. Then, cast the <paramref name="arg" /> parameter to a
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg" /> object and pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.JobId" /> property to the
    ///
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get the job. To get the task that contains the command, pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.TaskId" /> property to the
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandRawOutputHandler" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" />
    public delegate void CommandOutputHandler(object sender, CommandOutputEventArg arg);
    /// <summary>
    ///   <para>Defines the delegate to implement when you subscribe to the
    /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandRawOutput" /> event.</para>
    /// </summary>
    /// <param name="sender">
    ///   <para>The Scheduler object.</para>
    /// </param>
    /// <param name="arg">
    ///   <para>A <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputEventArg" /> object that provides the output and related information.</para>
    /// </param>
    /// <remarks>
    ///   <para>To get the job that contains the command, cast the <paramref name="sender" /> object to an
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler" /> interface. Then, cast the <paramref name="arg" /> parameter to an
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg" /> object and pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.JobId" /> property to the
    ///
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.OpenJob(System.Int32)" /> method to get the job. To get the task that contains the command, pass the
    /// <see cref="Microsoft.Hpc.Scheduler.TaskStateEventArg.TaskId" /> property to the
    /// <see cref="Microsoft.Hpc.Scheduler.ISchedulerJob.OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId)" /> method.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" />
    /// <seealso cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" />
    public delegate void CommandRawOutputHandler(object sender, CommandRawOutputEventArg arg);

    /// <summary>
    ///   <para>Defines a command to execute on one or more nodes in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To get this interface, call the
    ///
    /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateCommand(System.String,Microsoft.Hpc.Scheduler.ICommandInfo,Microsoft.Hpc.Scheduler.IStringCollection)" /> method.</para>
    ///   <para>Only cluster administrators can run commands.</para>
    ///   <para>Commands are special jobs that run immediately on the specified nodes. You cannot rerun a command.</para>
    /// </remarks>
    /// <seealso cref="Microsoft.Hpc.Scheduler.ICommandInfo" />
    [ComVisible(true)]
    [Guid(ComGuids.GuidIRemoteCommand)]
    public interface IRemoteCommand
    {
        /// <summary>
        ///   <para>Retrieves the command identifier that uniquely identifies the command in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier that uniquely identifies the command.</para>
        /// </value>
        int Id { get; }

        /// <summary>
        ///   <para>Retrieves the unique task identifier for the proxy task in a remote command job. The proxy task is
        /// the task that forwards the output and error streams from all of the nodes in a remote command to the client.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> object that specifies the unique task identifier for the proxy task in a remote command job.</para>
        /// </value>
        /// <remarks>
        ///   <para>Each remote command job has a proxy task in addition to a task for each node in the remote command.</para>
        /// </remarks>
        int ProxyTaskId { get; }

        /// <summary>
        ///   <para>Retrieves the command line to execute.</para>
        /// </summary>
        /// <value>
        ///   <para>The command line.</para>
        /// </value>
        /// <remarks>
        ///   <para>The command line is specified when you call the
        ///
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateCommand(System.String,Microsoft.Hpc.Scheduler.ICommandInfo,Microsoft.Hpc.Scheduler.IStringCollection)" /> method to create this object.</para>
        /// </remarks>
        string CommandLine { get; }

        /// <summary>
        ///   <para>Retrieves the collection of node names on which the command will run or has run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the collection of node names.</para>
        /// </value>
        /// <remarks>
        ///   <para>The list of nodes is specified when you call the
        ///
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.CreateCommand(System.String,Microsoft.Hpc.Scheduler.ICommandInfo,Microsoft.Hpc.Scheduler.IStringCollection)" /> method to create this object.</para>
        /// </remarks>
        IStringCollection NodeNames { get; }

        /// <summary>
        ///   <para>Retrieves or sets the encoding to use on the output that the command generates.</para>
        /// </summary>
        /// <value>
        ///   <para>An
        /// <see cref="System.Text.Encoding" /> object that specifies the encoding to apply to the output that is sent to your
        /// <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" /> event handler in response to an
        /// <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" /> event. </para>
        /// </value>
        [ComVisible(false)]
        Encoding OutputEncoding { get; set; }

        /// <summary>
        ///   <para>Runs the command.</para>
        /// </summary>
        /// <remarks>
        ///   <para>The credentials used to run the command are requested from the user.</para>
        ///   <para>After starting the command, your application must continue to run until the job that run the command finishes.
        /// If your application exits too soon, the HPC Job Scheduler Service may not create and start the task that runs the command.</para>
        /// </remarks>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.StartWithCredentials(System.String,System.String)" />
        void Start();

        /// <summary>
        ///   <para>Runs the command using the specified credentials.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form domain\username. The user name is limited to 80 characters. </para>
        ///   <para>If this parameter is NULL, the method uses the owner of the job.
        /// If this parameter is an empty string, the service searches the credentials cache for the
        /// credentials to use. If the cache contains the credentials for a single user, those credentials
        /// are used. However, if multiple credentials exist in the cache, the user is prompted for the credentials.</para>
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses
        /// the cached password if cached; otherwise, the user is prompted for the password.</para>
        /// </param>
        /// <remarks>
        ///   <para>After starting the command, your application must continue to run until the job that run the command finishes.
        /// If your application exits too soon, the HPC Job Scheduler Service may not create and start the task that runs the command.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see href="https://msdn.microsoft.com/en-us/library/cc853432(v=vs.85).aspx">Executing Commands</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.Start" />
        void StartWithCredentials(string userName, string password);

        /// <summary>
        ///   <para>Cancels the command.</para>
        /// </summary>
        /// <remarks>
        ///   <para>You can cancel the command at any time before the command finishes. </para>
        ///   <para>The TTLCompletedJobs cluster parameter (for details, see the Remarks section of
        ///
        /// <see cref="Microsoft.Hpc.Scheduler.IScheduler.SetClusterParameter(System.String,System.String)" />) determines when the command is removed from the scheduler after it has been canceled. </para>
        /// </remarks>
        void Cancel();

        /// <summary>
        ///   <para>An event that is raised when the state of the command changes on a node.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see
        /// href="https://msdn.microsoft.com/en-us/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandJobState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandRawOutput" />
        event EventHandler<CommandTaskStateEventArg> OnCommandTaskState;

        /// <summary>
        ///   <para>An event that is raised when the state of the job that contains the command changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.JobStateHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see
        /// href="https://msdn.microsoft.com/en-us/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandRawOutput" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandTaskState" />
        event EventHandler<JobStateEventArg> OnCommandJobState;

        /// <summary>
        ///   <para>An event that is raised when the command generates a line of output.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see
        /// href="https://msdn.microsoft.com/en-us/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandJobState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandTaskState" />
        event EventHandler<CommandOutputEventArg> OnCommandOutput;

        /// <summary>
        ///   <para>An event that is raised when the command generates output. The output is provided as a byte blob without encoding.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputHandler" />.</para>
        /// </remarks>
        /// <example>
        ///   <para>For an example, see <see
        /// href="https://msdn.microsoft.com/library/cc853480(v=vs.85).aspx">Implementing the Event Handlers for Command Events in C#</see>.</para>
        /// </example>
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandJobState" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandOutput" />
        /// <seealso cref="Microsoft.Hpc.Scheduler.IRemoteCommand.OnCommandTaskState" />
        event EventHandler<CommandRawOutputEventArg> OnCommandRawOutput;
    }


    /// <summary>
    ///   <para>Defines a command to run on one or more nodes in the cluster.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Do not use this class. Instead use the <see cref="Microsoft.Hpc.Scheduler.IRemoteCommand" /> interface.</para>
    /// </remarks>
    /// <example />
    [ComVisible(true)]
    [Guid(ComGuids.GuidRemoteCommandClass)]
    [ClassInterface(ClassInterfaceType.None)]
    [ComSourceInterfaces(typeof(IRemoteCommandEvents))]
    public class RemoteCommand : IRemoteCommand, IDisposable
    {
        #region Helper classes

        //
        // Comparer class to make sure node name list is sorted in case-insensitive way
        //
        class NodeNameComparer : IComparer<string>, IEqualityComparer<string>
        {
            #region IComparer<string> Members

            public int Compare(string x, string y)
            {
                return string.Compare(x, y, true);
            }

            #endregion

            #region IEqualityComparer<string> Members

            public bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }

            #endregion

            static NodeNameComparer comparer = new NodeNameComparer();
            internal static NodeNameComparer Comparer
            {
                get { return comparer; }
            }
        }
        #endregion

        const int InputBufferSize = 256 * 1024;
        const int CreationTimeout = 10 * 1000;
        const int CancelGracePeriod = 10 * 1000;

        const int NumberOfRetries = 60;
        static readonly RetryWaitTimer _retryTimer = new ExponentialBackoffRetryTimer(2000, 30000);

        const int NumberOfFileStagingRetries = 10;

        Scheduler scheduler;
        IClusterJob job;

        string command;
        ICommandInfo cmdInfo;
        SortedDictionary<string, NodeData> dataByNode = new SortedDictionary<string, NodeData>(NodeNameComparer.Comparer);

        Dictionary<int, NodeData> dataByTaskId = new Dictionary<int, NodeData>();

        Encoding outputEncoding = Encoding.Default;

        SafeFileHandle pipeHandle;
        FileStream pipeFile;
        Thread worker;
        private bool workerStop = false;

        string proxyNode = null;
        int proxyTaskId;
        Timer cancelTimer;
        string proxyPipeName = null;
        bool _redirectOutput = true;
        int _eventsAdded = 0;
        int _eventReconnectAdded = 0;

        Thread _pollingThread = null;
        private bool _pollingThreadStop = false;
        int _pollingPeriod = 30 * 1000;
        DateTime _lastChangeTime = DateTime.UtcNow;
        JobState _lastJobState = JobState.Configuring;
        TaskState _lastProxyTaskState = TaskState.Configuring;

        /// <summary>
        ///   <para>An event that is raised when the command generates a line of output.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" />.</para>
        /// </remarks>
        public event CommandOutputHandler OnCommandOutput;
        /// <summary>
        ///   <para>An event that is raised when the command generates output. The output is provided as a byte blob without encoding.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandRawOutputHandler" />.</para>
        /// </remarks>
        public event CommandRawOutputHandler OnCommandRawOutput;
        /// <summary>
        ///   <para>An event that is raised when the state of the command changes on a node.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.CommandTaskStateHandler" />.</para>
        /// </remarks>
        public event CommandTaskStateHandler OnCommandTaskState;
        /// <summary>
        ///   <para>An event that is raised when the state of the job that contains the command changes.</para>
        /// </summary>
        /// <remarks>
        ///   <para>For details on the delegate used with this event, see <see cref="Microsoft.Hpc.Scheduler.JobStateHandler" />.</para>
        /// </remarks>
        public event JobStateHandler OnCommandJobState;

        object _eventlock = new object();

        private static string HttpsClusrunUriFormat = string.Concat("https://{0}:", RestServiceUtil.InternalRestHttpsPort, "/clusrun/{1}/");
        private static string HttpClusrunUriFormat = string.Concat("http://{0}:", RestServiceUtil.InternalRestHttpPort, "/clusrun/{1}/");
        string restServerAddress;

        const string pipeProxyNodeList = "PIPEPROXY_NODELIST";
        const string pipeProxyNumNodeList = "PIPEPROXY_NUMNODELIST";
        const int envVarLength = 2047; //This is the size of an environment variable used in the scheduler database.

        const string PipeProxyNodeDomainJoined = "PIPEPROXY_DOMAINJOINED";

        internal RemoteCommand(
            Scheduler scheduler,
            string command,
            ICommandInfo info,
            IStringCollection nodes)
        {
            this.scheduler = scheduler;
            this.command = command;
            this.cmdInfo = info;
            this.worker = new Thread(Run);
            this.worker.IsBackground = true;

            InitNodeList(nodes);
            CreateJob();

            scheduler.Store.JobEvent += OnJobEvent;
            job.TaskEvent += OnTaskEvent;
            AddReconnectEventHandler();

            Interlocked.Increment(ref _eventsAdded);
            //scheduler.Store.TaskEvent += OnTaskEvent;
        }


        internal RemoteCommand(
            Scheduler scheduler,
            string command,
            ICommandInfo info,
            IStringCollection nodes,
            bool redirectOutput)
            : this(scheduler, command, info, nodes)
        {
            _redirectOutput = redirectOutput;
        }

        /// <summary>
        ///   <para>Retrieves the command identifier that uniquely identifies the command in the scheduler.</para>
        /// </summary>
        /// <value>
        ///   <para>The identifier that uniquely identifies the command.</para>
        /// </value>
        public int Id
        {
            get { return job.Id; }
        }

        /// <summary>
        ///   <para>Retrieves the unique task identifier for the proxy task in a remote command job. The proxy task is
        /// the task that forwards the output and error streams from all of the nodes in a remote command to the client.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="System.Int32" /> object that specifies the unique task identifier for the proxy task in a remote command job.</para>
        /// </value>
        /// <remarks>
        ///   <para>Each remote command job has a proxy task in addition to a task for each node in the remote command.</para>
        /// </remarks>
        public int ProxyTaskId
        {
            get { return proxyTaskId; }
        }

        /// <summary>
        ///   <para>Retrieves the command line to execute.</para>
        /// </summary>
        /// <value>
        ///   <para>The command line.</para>
        /// </value>
        public string CommandLine
        {
            get { return command; }
        }

        /// <summary>
        ///   <para>Retrieves the collection of node names on which the command will run or has run.</para>
        /// </summary>
        /// <value>
        ///   <para>An <see cref="Microsoft.Hpc.Scheduler.IStringCollection" /> interface that contains the collection of node names.</para>
        /// </value>
        public IStringCollection NodeNames
        {
            get { return new StringCollection(dataByNode.Keys); }
        }

        /// <summary>
        ///   <para>Retrieves or sets the encoding to use on the output that the command generates.</para>
        /// </summary>
        /// <value>
        ///   <para>An
        /// <see cref="System.Text.Encoding" /> object that specifies the encoding to apply to the output that is sent to your
        /// <see cref="Microsoft.Hpc.Scheduler.CommandOutputHandler" /> event handler in response to an
        /// <see cref="Microsoft.Hpc.Scheduler.RemoteCommand.OnCommandOutput" /> event. </para>
        /// </value>
        [ComVisible(false)]
        public Encoding OutputEncoding
        {
            get { return outputEncoding; }
            set { outputEncoding = value; }
        }

        event EventHandler<CommandTaskStateEventArg> IRemoteCommand.OnCommandTaskState
        {
            add
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandTaskState += value.Invoke;
                }

            }

            remove
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandTaskState -= value.Invoke;
                }
            }
        }

        event EventHandler<JobStateEventArg> IRemoteCommand.OnCommandJobState
        {
            add
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandJobState += value.Invoke;
                }
            }

            remove
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandJobState -= value.Invoke;
                }
            }
        }

        event EventHandler<CommandOutputEventArg> IRemoteCommand.OnCommandOutput
        {
            add
            {
                if (!_redirectOutput)
                {
                    throw new Exception("Output not available when redirection of output is turned off");
                }
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandOutput += value.Invoke;
                }
            }

            remove
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandOutput -= value.Invoke;
                }
            }
        }

        event EventHandler<CommandRawOutputEventArg> IRemoteCommand.OnCommandRawOutput
        {
            add
            {
                if (!_redirectOutput)
                {
                    throw new Exception("Output not available when redirection of output is turned off");
                }
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandRawOutput += value.Invoke;
                }
            }

            remove
            {
                if (value != null)
                {
                    ((RemoteCommand)this).OnCommandRawOutput -= value.Invoke;
                }
            }
        }

        /// <summary>
        ///   <para>Runs the command. </para>
        /// </summary>
        /// <remarks>
        ///   <para>The credentials used to run the command are requested from the user.</para>
        /// </remarks>
        /// <example />
        public void Start()
        {
            StartWithCredentials(null, null);
        }

        /// <summary>
        ///   <para>Runs the command using the specified credentials.</para>
        /// </summary>
        /// <param name="userName">
        ///   <para>The name of the RunAs user, in the form domain\username. The user name is limited to 80 characters.</para>
        ///   <para>If this parameter is null, empty, or not valid, the service searches the credential cache for the credentials to use. If the cache
        /// contains the credentials for a single user, those credentials are used. However,
        /// if multiple credentials exist in the cache, the user is prompted for the credentials.</para>
        /// </param>
        /// <param name="password">
        ///   <para>The password for the RunAs user. The password is limited to 127 characters.</para>
        ///   <para>If this parameter is null or empty, the method uses the cached password if cached; otherwise, the user is prompted for the password.
        /// </para>
        /// </param>
        public void StartWithCredentials(string userName, string password)
        {
            //if pipe proxy is not the named pipe server and output needs to be redirected and the connection is not over http
            //then the remote command needs to open the named pipe as a server
            if (!isPipeProxyServer() && _redirectOutput && !scheduler.Store.OverHttp)
            {
                OpenPipeAsServer(userName);
            }

            //Call the async method synchronously due to the current StartWithCredentials method implements the public interface IRemoteCommand
            SubmitJobAndProxy(userName, password).GetAwaiter().GetResult();
        }

        private void OpenPipeAsServer(string userName)
        {
            //create a right ACL for the command pipe so that only current user and the "runas" user could access the pipe
            SecurityIdentifier currentUser = WindowsIdentity.GetCurrent().User;
            DiscretionaryAcl acl = new DiscretionaryAcl(false, false, 1);
            acl.AddAccess(AccessControlType.Allow, currentUser, (int)NativeWrapper.PipeAccessRights.FullControl, InheritanceFlags.None, PropagationFlags.None);
            SecurityIdentifier runasUser = null;
            if (!string.IsNullOrEmpty(userName))
            {
                try
                {
                    runasUser = (SecurityIdentifier)(new NTAccount(userName).Translate(typeof(SecurityIdentifier)));
                }
                catch (IdentityNotMappedException ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand.OpenPipeAsServer Exception: {0}", ex);
                }

            }

            if (runasUser == null)
            {
                //if this user is not known by the client, we have to allow everyone to access
                runasUser = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            }
            acl.AddAccess(AccessControlType.Allow, runasUser, (int)NativeWrapper.PipeAccessRights.FullControl, InheritanceFlags.None, PropagationFlags.None);

            CommonSecurityDescriptor securityDesc = new CommonSecurityDescriptor(false, false, ControlFlags.None, currentUser, null, null, acl);
            byte[] secDescBinary = new byte[securityDesc.BinaryLength];
            securityDesc.GetBinaryForm(secDescBinary, 0);
            IntPtr nativeSecDesc = Marshal.AllocHGlobal(secDescBinary.Length);

            try
            {
                Marshal.Copy(secDescBinary, 0, nativeSecDesc, secDescBinary.Length);

                NativeWrapper.SECURITY_ATTRIBUTES secAttrs = new NativeWrapper.SECURITY_ATTRIBUTES();
                secAttrs.nLength = Marshal.SizeOf(secAttrs);
                secAttrs.bInheritHandle = 0;
                secAttrs.pSecurityDescriptor = nativeSecDesc;

                pipeHandle = NativeWrapper.CreateNamedPipe(
                    @"\\.\pipe\Hpc\RemoteCommand\" + job.Id,
                    NativeWrapper.PIPE_ACCESS_INBOUND,
                    NativeWrapper.PIPE_TYPE_BYTE | NativeWrapper.PIPE_READMODE_BYTE | NativeWrapper.PIPE_WAIT,
                    1,
                    0,
                    InputBufferSize,
                    CreationTimeout,
                    ref secAttrs);

                TraceHelper.TraceInfo("RemoteCommand: Created Named Pipe {0} as server, and the pipe handle is {1}", @"\\.\pipe\Hpc\RemoteCommand\" + job.Id, pipeHandle);
            }
            finally
            {
                Marshal.FreeHGlobal(nativeSecDesc);
            }
        }


        /// <summary>
        /// This method can be used to determine which version of the RemoteCommand - pipeproxy set up is being used
        /// In V2, Remotecommand acted as the NamedPipe Server and the pipe proxy conntected to it
        /// Later, The PipeProxy started acting as the NamedPipe Server and RemoteCommand connected to it
        /// </summary>
        /// <returns></returns>
        private bool isPipeProxyServer()
        {
            if (scheduler.GetServerVersion().Major >= 3)
            {
                return true;
            }
            return false;
        }

        Dictionary<string, string> GetValidNodes()
        {
            Dictionary<string, string> validNodes = new Dictionary<string, string>();
            using (IRowEnumerator reachableNodes = scheduler.Store.OpenNodeEnumerator())
            {
                reachableNodes.SetColumns(NodePropertyIds.Name);
                reachableNodes.SetFilter(new FilterProperty(FilterOperator.HasBitSet, NodePropertyIds.JobType, JobType.Admin),
                    new FilterProperty(FilterOperator.Equal, NodePropertyIds.Reachable, true));

                foreach (PropertyRow row in reachableNodes)
                {
                    string nodeName = row[NodePropertyIds.Name].Value as string;
                    validNodes.Add(nodeName.ToUpper(), null);
                }
            }

            return validNodes;
        }

        List<NodeData> requestedNodeData;

        async Task SubmitJobAndProxy(string userName, string password)
        {
            #region first find out all reachable nodes in the system
            Dictionary<string, string> validNodes = GetValidNodes();
            #endregion

            #region Read the data location

            //filter out reachable nodes in the requested node list
            //for unreachable nodes, send out the task failure callback
            List<string> requestedNodes = new List<string>();
            List<string> proxyRequestedNodes = new List<string>();
            requestedNodeData = new List<NodeData>();

            bool allAzure = true;

            foreach (NodeData data in dataByNode.Values)
            {
                if (!validNodes.ContainsKey(data.NodeName))
                {
                    CmdEndBeforeTaskRun(data, SR.RemoteCommand_UnreachabeNode);
                }
                else
                {
                    if (data.Location == NodeLocation.OnPremise || data.Location == NodeLocation.Linux || data.Location == NodeLocation.AzureBatch || data.Location == NodeLocation.NonDomainJoined)
                    {
                        allAzure = false;
                        proxyRequestedNodes.Add(string.Format("{0} {1}", data.NodeName, data.Location == NodeLocation.NonDomainJoined || data.Location == NodeLocation.Linux ? "N" : "D"));
                    }
                    requestedNodes.Add(data.NodeName);
                    requestedNodeData.Add(data);
                }
            }

            if (requestedNodes.Count == 0)
            {
                Cancel();
                return;
            }

            #endregion


            #region figure out the job's proxynode



            proxyNode = (await scheduler.Store.GetSchedulerNodeNameAsync().ConfigureAwait(false)).ToUpper();

            // Try to find a valid headnode to create the proxy server, if not all the nodes on azure or we are connecting over http
            if (!allAzure && _redirectOutput && !scheduler.Store.OverHttp)
            {
                //if the cluster name is a valid node, use it as the proxynode
                if (!validNodes.ContainsKey(proxyNode))
                {
                    //We need to find the headnodes of the cluster
                    string[] headNodeNames = scheduler.Store.GetNodesFromGroup("HeadNodes").Select(n => n.ToUpper()).ToArray();

                    //if there are multiple headnodes, this is probably a HA scenario and we need
                    //to use the name of one of the reachable headnodes as the proxyNode

                    bool foundReachableHeadNode = false;
                    //find the first reachable headnode
                    foreach (string headNodeName in headNodeNames)
                    {
                        if (validNodes.ContainsKey(headNodeName))
                        {
                            proxyNode = headNodeName;
                            foundReachableHeadNode = true;
                            break;
                        }
                    }
                    if (!foundReachableHeadNode)
                    {
                        job.Cancel(SR.RemoteCommand_NoReachableHeadNode);
                        throw new SchedulerException(SR.RemoteCommand_NoReachableHeadNode);
                    }
                }
            }
            #endregion

            restServerAddress = string.Format(CultureInfo.InvariantCulture, (scheduler.Store.GetServerVersion() >= VersionControl.V4SP5 && scheduler.Store.GetServerLinuxHttpsValue() > 0) ? HttpsClusrunUriFormat : HttpClusrunUriFormat, proxyNode, job.Id);

            #region figure out the job's requested nodes

            //if we are the server, we need to start listening for the incoming connection as long as we are
            //redirecting output and are not over http
            if (!isPipeProxyServer() && _redirectOutput && !scheduler.Store.OverHttp)
            {
                worker.Start();
            }


            //set the job's requested nodes to the filtered node list with proxy node prepended
            StringBuilder requestedNodesString = new StringBuilder();
            if (!allAzure)
            {
                requestedNodesString.Append(proxyNode);
            }
            foreach (string nodeName in requestedNodes)
            {
                if (nodeName != proxyNode)
                {
                    requestedNodesString.Append("," + nodeName);
                }
            }

            job.SetProps(new StoreProperty(JobPropertyIds.RequestedNodes, requestedNodesString.ToString()));
            #endregion

            if (allAzure || !_redirectOutput || scheduler.Store.OverHttp)
            {
                AddTasks();
            }
            else
            {
                #region add the proxy task
                string pipeName;
                string proxyCommandLine;

                if (isPipeProxyServer())
                {
                    pipeName = @"\pipe\Hpc\RemoteCommand\" + job.Id;
                    proxyPipeName = pipeName;
                    proxyCommandLine = "PipeProxy " + pipeName + " Server";
                }
                else
                {
                    pipeName = @"\\" + Environment.MachineName + @"\pipe\Hpc\RemoteCommand\" + job.Id;
                    proxyCommandLine = "PipeProxy " + pipeName;
                }

                proxyCommandLine = string.Format(CultureInfo.InvariantCulture, "{0} {1}", proxyCommandLine, restServerAddress);

                string nodeList = string.Join(",", proxyRequestedNodes.ToArray());
                int numEnvironmentVars = (int)Math.Ceiling((float)nodeList.Length / (float)envVarLength);

                IClusterTask task = job.CreateTask(
                            new StoreProperty(TaskPropertyIds.CommandLine, proxyCommandLine),
                            new StoreProperty(TaskPropertyIds.Name, SR.RemoteCommand_ProxyTaskName),
                            new StoreProperty(TaskPropertyIds.MinCores, 1),
                            new StoreProperty(TaskPropertyIds.MaxCores, 1),
                            new StoreProperty(TaskPropertyIds.RequiredNodes, proxyNode),
                            new StoreProperty(TaskPropertyIds.IsRerunnable, false));

                TraceHelper.TraceInfo("RemoteCommand: Created task {0} on node {1}", proxyCommandLine, proxyNode);

                if (cmdInfo != null && cmdInfo.EnvironmentVariables != null)
                {
                    foreach (NameValue env in cmdInfo.EnvironmentVariables)
                    {
                        task.SetEnvironmentVariable(env.Name, env.Value);
                    }
                }

                proxyTaskId = task.Id;

                task.SetEnvironmentVariable(pipeProxyNumNodeList, numEnvironmentVars.ToString());
                for (int i = 0; i < numEnvironmentVars; i++)
                {
                    int firstIndex = i * envVarLength;
                    int length = envVarLength;
                    if (firstIndex + length > nodeList.Length)
                    {
                        length = nodeList.Length - firstIndex;
                    }
                    String envName = pipeProxyNodeList + i.ToString();
                    task.SetEnvironmentVariable(envName, nodeList.Substring(firstIndex, length));
                }

                PropertyRow locationRow = scheduler.Store.OpenNode(proxyNode).GetProps(NodePropertyIds.Location);
                NodeLocation proxyNodeLocation = PropertyUtil.GetValueFromPropRow<NodeLocation>(locationRow, NodePropertyIds.Location, NodeLocation.OnPremise);
                task.SetEnvironmentVariable(PipeProxyNodeDomainJoined, proxyNodeLocation != NodeLocation.NonDomainJoined ? "TRUE" : "FALSE");
                #endregion
            }

            //add the proxy task
            //submit the job
            List<StoreProperty> submitProps = new List<StoreProperty>();

            if (!string.IsNullOrEmpty(userName))
            {
                submitProps.Add(new StoreProperty(JobPropertyIds.UserName, userName));
            }

            if (password != null)
            {
                submitProps.Add(new StoreProperty(JobPropertyIds.Password, password));
            }

            job.BeginSubmit(
                submitProps.ToArray(),
                new AsyncCallback(SubmitJobCallBack),
                job);

            TraceHelper.TraceInfo("RemoteCommand: Submitted job {0}", job.Id);
        }

        private static void SubmitJobCallBack(IAsyncResult iAsyncResult)
        {
            AsyncResult result = iAsyncResult as AsyncResult;

            IClusterJob job = result.AsyncState as IClusterJob;

            job.EndSubmit(result);
        }

        void InitNodeList(IStringCollection nodes)
        {
            Dictionary<string, NodeLocation> locationMap = new Dictionary<string, NodeLocation>(StringComparer.InvariantCultureIgnoreCase);

            if (nodes == null)
            {
                nodes = new StringCollection();

                using (IRowEnumerator allnodes = scheduler.Store.OpenNodeEnumerator())
                {
                    allnodes.SetColumns(NodePropertyIds.Name, NodePropertyIds.Location);

                    foreach (PropertyRow row in allnodes)
                    {
                        string nodeName = row[NodePropertyIds.Name].Value as string;
                        nodes.Add(nodeName);

                        // Add the job location to the location map
                        StoreProperty locationProp = row[NodePropertyIds.Location];
                        if (locationProp == null)
                        {
                            // This is for V3RTM and before back comptability
                            locationMap[nodeName] = NodeLocation.OnPremise;
                        }
                        else
                        {
                            locationMap[nodeName] = (NodeLocation)locationProp.Value;
                        }

                    }
                }
            }
            else
            {
                //find the location of each specified node.
                //however, for servers older than v3sp1, the node will always be onpremise
                foreach (string nodeName in nodes)
                {
                    PropertyRow locationRow = scheduler.Store.OpenNode(nodeName).GetProps(NodePropertyIds.Location);
                    locationMap[nodeName] = PropertyUtil.GetValueFromPropRow<NodeLocation>(locationRow, NodePropertyIds.Location, NodeLocation.OnPremise);
                }
            }

            if (nodes.Count == 0)
            {
                throw new InvalidOperationException(SR.RemoteCommand_NoNodeList);
            }

            foreach (string nodeName in nodes)
            {
                string nodeNameUpper = nodeName.ToUpperInvariant();
                dataByNode.Add(nodeNameUpper, new NodeData(nodeNameUpper, outputEncoding, locationMap[nodeName]));
            }
        }

        void Run()
        {
            try
            {
                if (isPipeProxyServer())
                {
                    //connect to the proxy's server

                    string fullPipeName;
                    fullPipeName = @"\\" + proxyNode + proxyPipeName;

                    bool success = false;

                    RetryManager retry = new RetryManager(_retryTimer, NumberOfRetries);
                    while (!success)
                    {
                        if (workerStop) return;

                        success = NativeWrapper.WaitNamedPipe(fullPipeName, CreationTimeout);

                        if (!success)
                        {
                            int errorCode = Marshal.GetLastWin32Error();

                            TraceHelper.TraceWarning("RemoteCommand: WaitNamedPipe returns error code {0} in {1} retry", errorCode, retry.RetryCount);

                            //the pipe has not been created by the pipe proxy yet.. wait a little
                            if (errorCode == NativeWrapper.ERROR_FILE_NOT_FOUND)
                            {
                                if (!retry.HasAttemptsLeft)
                                {
                                    break;
                                }
                                retry.WaitForNextAttempt();
                            }
                            else
                            {
                                throw new Win32Exception();
                            }
                        }
                    }

                    if (!success)
                    {
                        TraceHelper.TraceError("RemoteCommand: Cancel Job because it failed to wait for the named pipe");
                        job.Cancel(SR.RemoteCommand_NoStreamForOutput);
                    }


                    pipeHandle = NativeWrapper.CreateFile(fullPipeName, FileSystemRights.ReadData, FileShare.Read,
                                                      IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

                    TraceHelper.TraceInfo("RemoteCommand: Connected to named pipe: {0}", fullPipeName);

                    if (pipeHandle.IsInvalid)
                    {
                        TraceHelper.TraceError("RemoteCommand: Cancel Job because it failed to connect to the named pipe");
                        job.Cancel(SR.RemoteCommand_NoStreamForOutput);
                    }
                }
                else
                {
                    //waiting for proxy to connect
                    if (!NativeWrapper.ConnectNamedPipe(
                       pipeHandle,
                       null))
                    {
                        throw new Win32Exception();
                    }
                }

                pipeFile = new FileStream(pipeHandle, FileAccess.Read, InputBufferSize, false);

                BinaryFormatter serializer = new BinaryFormatter();

                //waiting for the signal when the proxy is ready
                PipePacket readyPacket = (PipePacket)serializer.Deserialize(pipeFile);

                if (workerStop) return;

                //start command tasks on reachable nodes
                if (!AddTasks())
                {
                    EndBeforeTaskRun();
                }

                while (!workerStop)
                {
                    PipePacket packet = (PipePacket)serializer.Deserialize(pipeFile);

                    NodeData data = null;

                    if (!string.IsNullOrEmpty(packet.NodeName))
                    {
                        if (!dataByNode.TryGetValue(packet.NodeName, out data))
                        {
                            continue;
                        }
                    }

                    switch (packet.Type)
                    {
                        case PipePacket.PacketType.Stdout:
                            ProcessNodeData(data.OutBuffer, packet.Data, packet.NodeName, CommandOutputType.Output);
                            break;
                        case PipePacket.PacketType.SystemError:
                            ProcessNodeOutput(packet.NodeName, packet.Message, CommandOutputType.Error);
                            break;
                        case PipePacket.PacketType.Eof:
                            SendEof(data);
                            CheckFinish();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                TraceHelper.TraceWarning("RemoteCommand.Run: Exception {0}", ex);
            }
            finally
            {
                try
                {
                    Dispose();
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand.Run dispose Exception: {0}", ex);
                }

                foreach (NodeData data in dataByNode.Values)
                {
                    SendEof(data);
                }

                CheckFinish();
            }
        }

        void ProcessNodeData(LineBuffer buffer, byte[] data, string nodeName, CommandOutputType type)
        {
            ProcessNodeRawOutput(nodeName, data, data.Length, type);
            IEnumerable<string> lines = buffer.ProcessOutput(data);
            ProcessNodeMessages(nodeName, lines, type);
        }

        void ProcessNodeMessages(string nodeName, IEnumerable<string> lines, CommandOutputType type)
        {
            if (lines != null)
            {
                foreach (string line in lines)
                {
                    ProcessNodeOutput(nodeName, line, type);
                }
            }
        }

        /// <summary>
        ///   <para>Cancels the command.</para>
        /// </summary>
        public void Cancel()
        {
            if (job != null)
            {
                try
                {
                    TraceHelper.TraceInfo("RemoteCommand: Cancel Job because the Remote Command is cancelled");
                    job.Cancel(SR.RemoteCommand_Canceled);
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand: Cancel job Exception {0}", ex);
                }

                //Since the job is being canceled, canceled is the default state
                JobState finalState = JobState.Canceled;

                try
                {
                    PropertyRow jobPropRow = job.GetProps(JobPropertyIds.State);
                    finalState = PropertyUtil.GetValueFromPropRow<JobState>(jobPropRow, JobPropertyIds.State, finalState);
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand: Get job properties exception {0}", ex);
                }

                if (worker.IsAlive)
                {
                    if (!isPipeProxyServer())
                    {
                        //try open the pipe to unblock the pipe reader thread in case the proxy has never connected
                        try
                        {
                            using (StreamWriter pipe = new StreamWriter(@"\\.\pipe\Hpc\RemoteCommand\" + job.Id))
                            {
                            }
                        }
                        catch (Exception ex)
                        {
                            TraceHelper.TraceWarning("RemoteCommand open StreamWriter Exception: {0}", ex);
                        }
                    }

                    workerStop = true;
                    worker.Join();
                }

                try
                {
                    //Before detaching event handlers trigger the job completion handler on the overlying command
                    if (OnCommandJobState != null)
                    {
                        OnCommandJobState(this, new JobStateEventArg(job.Id, finalState, _lastJobState));
                    }
                    if (TryRemoveEvents())
                    {
                        scheduler.Store.JobEvent -= OnJobEvent;
                        job.TaskEvent -= OnTaskEvent;
                    }
                    RemoveReconnectEventHandler();
                    Dispose();
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand.Cancel Exception: {0}", ex);
                }

                foreach (NodeData data in dataByNode.Values)
                {
                    SendEof(data);
                }
            }
        }

        void SendEof(NodeData data)
        {
            if (!data.EofSent)
            {
                ProcessNodeMessages(data.NodeName, data.OutBuffer.FlushLines(), CommandOutputType.Output);
                ProcessNodeOutput(data.NodeName, null, CommandOutputType.Eof);
                ProcessNodeRawOutput(data.NodeName, new byte[] { }, 0, CommandOutputType.Eof);
                data.EofSent = true;
            }
        }

        bool _tasksFinished = false;

        void CheckFinish()
        {
            //if all command has finished and EOFed, we should wait for the job to finish, but if the job does not finish by itself,
            //we will cancel it in 30 seconds anyway
            foreach (NodeData data in dataByNode.Values)
            {
                if (!data.EofSent || !data.Finished)
                {
                    return;
                }
            }

            _tasksFinished = true;

            //make sure the timer is only created once
            Timer tempTimer = new Timer(CancelWorker, null, Timeout.Infinite, Timeout.Infinite);
            if (Interlocked.CompareExchange<Timer>(ref cancelTimer, tempTimer, null) == null)
            {
                cancelTimer.Change(CancelGracePeriod, Timeout.Infinite);
            }
            else
            {
                tempTimer.Dispose();
            }
        }

        //
        // Create the command job
        //
        void CreateJob()
        {
            job = scheduler.Store.CreateJob(new StoreProperty(JobPropertyIds.JobType, JobType.Admin),
                    new StoreProperty(JobPropertyIds.Name, SR.RemoteCommand_JobName),
                    new StoreProperty(JobPropertyIds.MinCores, 1),
                    new StoreProperty(JobPropertyIds.MaxCores, dataByNode.Count + 1));
        }

        //
        //
        //


        string GetAzureOutputFilename(NodeData data)
        {
            return @"%CCP_OUTPUT%\Clusrun." + job.Id + "." + data.NodeName;
        }

        bool AddTasks()
        {
            try
            {
                bool hasEnvironmentVariableSettings = false;
                if (cmdInfo != null
                    && cmdInfo.EnvironmentVariables != null
                    && cmdInfo.EnvironmentVariables.Count != 0)
                {
                    hasEnvironmentVariableSettings = true;
                }

                bool submitTasks = true;

                Version serverVersion = scheduler.Store.GetServerVersion();
                if (serverVersion.Major == 2
                    && serverVersion.Build <= 1575)
                {
                    // Pre V2 SP1 server that starts the tasks when they are added. Submitting
                    // these tasks would cause lots of user visible issues.
                    // Unfortunately this may cause a race where the environment variables are not always set.
                    submitTasks = false;
                }


                //check if the server supports job environment variables
                bool hasJobEnvVars = false;
                if (serverVersion.Major >= 3)
                {
                    hasJobEnvVars = true;
                }

                //if the server supports job environment variables and we have
                // environment variables set them on the job
                if (hasJobEnvVars && hasEnvironmentVariableSettings)
                {
                    foreach (NameValue env in cmdInfo.EnvironmentVariables)
                    {
                        job.SetEnvironmentVariable(env.Name, env.Value);
                    }
                    //we do not need to set environment variables on the task any more
                    hasEnvironmentVariableSettings = false;
                }

                //create one task per node,
                //for each task
                // 1.its required nodes is the node which they want to run on
                // 2.its stdout/stderr is redirect to the proxy's incoming pipe indexed by the node name
                List<StoreProperty[]> taskPropList = new List<StoreProperty[]>();
                foreach (NodeData data in requestedNodeData)
                {
                    string pipeName = null;
                    if (_redirectOutput)
                    {
                        if (data.Location == NodeLocation.OnPremise || data.Location == NodeLocation.AzureBatch)
                        {
                            //if connecting over http we will simply use the output property of the task
                            //to get the task's output. It will have the limitation of being restricted to 4096
                            if (!scheduler.Store.OverHttp)
                            {
                                pipeName = "\\\\" + proxyNode + "\\pipe\\Hpc\\Proxy\\" + job.Id + "\\" + data.NodeName;
                            }
                        }
                        else if (data.Location == NodeLocation.Linux || data.Location == NodeLocation.NonDomainJoined)
                        {
                            pipeName = restServerAddress + "api/message";
                        }
                        else
                        {
                            //for azure the output is not written to a pipe on the headnode, instead it is written
                            //to a file on the local azure worker role
                            //If this is a clusrun being run over http to scheduler on azure, we cannot use
                            //file staging as the file staging service does not have a public endpoint
                            //So, we let the output go to the task output in that case
                            if (!scheduler.Store.OverHttp)
                            {
                                pipeName = GetAzureOutputFilename(data);
                            }
                        }
                    }
                    if (hasEnvironmentVariableSettings
                        && submitTasks == true)
                    {
                        StoreProperty[] taskProps =
                        {
                            new StoreProperty(TaskPropertyIds.CommandLine, command),
                            new StoreProperty(TaskPropertyIds.Name, SR.RemoteCommand_TaskName + "_" + data.NodeName),
                            new StoreProperty(TaskPropertyIds.MinCores, 1),
                            new StoreProperty(TaskPropertyIds.MaxCores, 1),
                            new StoreProperty(TaskPropertyIds.StdOutFilePath, pipeName),
                            new StoreProperty(TaskPropertyIds.StdErrFilePath, pipeName),
                            new StoreProperty(TaskPropertyIds.WorkDirectory, cmdInfo == null ? null : cmdInfo.WorkingDirectory),
                            new StoreProperty(TaskPropertyIds.StdInFilePath, cmdInfo == null ? null : cmdInfo.StdIn),
                            new StoreProperty(TaskPropertyIds.IsRerunnable, false),
                            new StoreProperty(TaskPropertyIds.RequiredNodes, data.NodeName),
                            new StoreProperty(TaskPropertyIds.State, TaskState.Configuring), // Don't run until env set
                        };

                        taskPropList.Add(taskProps);
                    }
                    else
                    {
                        StoreProperty[] taskProps =
                        {
                            new StoreProperty(TaskPropertyIds.CommandLine, command),
                            new StoreProperty(TaskPropertyIds.Name, SR.RemoteCommand_TaskName + "_" + data.NodeName),
                            new StoreProperty(TaskPropertyIds.MinCores, 1),
                            new StoreProperty(TaskPropertyIds.MaxCores, 1),
                            new StoreProperty(TaskPropertyIds.StdOutFilePath, pipeName),
                            new StoreProperty(TaskPropertyIds.StdErrFilePath, pipeName),
                            new StoreProperty(TaskPropertyIds.WorkDirectory, cmdInfo == null ? null : cmdInfo.WorkingDirectory),
                            new StoreProperty(TaskPropertyIds.StdInFilePath, cmdInfo == null ? null : cmdInfo.StdIn),
                            new StoreProperty(TaskPropertyIds.IsRerunnable, false),
                            new StoreProperty(TaskPropertyIds.RequiredNodes, data.NodeName),
                        };

                        taskPropList.Add(taskProps);
                    }
                }

                List<IClusterTask> tasks = null;
                int taskIdx = 0;

                lock (dataByTaskId)
                {
                    tasks = job.CreateTasks(taskPropList);

                    if (tasks == null
                        || tasks.Count != requestedNodeData.Count)
                    {
                        return false;
                    }





                    foreach (NodeData data in requestedNodeData)
                    {
                        if (!dataByTaskId.ContainsKey(tasks[taskIdx].TaskId))
                        {
                            dataByTaskId.Add(tasks[taskIdx].TaskId, data);
                        }
                        taskIdx++;
                    }
                }

                taskIdx = 0;
                foreach (NodeData data in requestedNodeData)
                {
                    //set the env vars and submit the tasks
                    if (hasEnvironmentVariableSettings)
                    {

                        foreach (NameValue env in cmdInfo.EnvironmentVariables)
                        {
                            tasks[taskIdx].SetEnvironmentVariable(env.Name, env.Value);
                        }

                        // Now the task has environment variables set, it can proceed.
                        if (submitTasks == true)
                        {
                            try
                            {
                                tasks[taskIdx].SubmitTask();
                            }
                            catch (SchedulerException ex)
                            {
                                //Task Submission failed, However we need to continue submitting other tasks

                                TraceHelper.TraceWarning("RemoteCommand.AddTasks Exception: {0}", ex);
                            }
                        }
                    }
                    taskIdx++;
                }

                TraceHelper.TraceInfo("RemoteCommand: Submitted tasks {0} for requested nodes", command);
                return true;
            }
            catch (Exception ex)
            {
                TraceHelper.TraceError("RemoteCommand: Add tasks excpetion {0}", ex);
                return false;
            }
        }

        //
        // Process command output from a given node
        //
        internal void ProcessNodeOutput(string nodeName, string line, CommandOutputType outputType)
        {
            if (OnCommandOutput != null)
            {
                OnCommandOutput(this, new CommandOutputEventArg(nodeName, line, outputType));
            }
        }

        //
        // Process command output from a given node
        //
        internal void ProcessNodeRawOutput(string nodeName, byte[] data, int size, CommandOutputType outputType)
        {
            if (OnCommandRawOutput != null)
            {
                byte[] dataCopy = new byte[size];
                Array.Copy(data, dataCopy, size);
                OnCommandRawOutput(this, new CommandRawOutputEventArg(nodeName, dataCopy, outputType));
            }
        }

        void OnJobEvent(Int32 jobId, EventType eventType, StoreProperty[] props)
        {
            if (OnCommandJobState != null
                && jobId == job.Id
                && eventType == EventType.Modify
                && props != null)
            {
                StoreProperty newState = null;
                StoreProperty oldState = null;

                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == JobPropertyIds.State)
                    {
                        newState = prop;
                    }
                    else if (prop.Id == JobPropertyIds.PreviousState)
                    {
                        oldState = prop;
                    }

                    if (newState != null && oldState != null)
                    {
                        break;
                    }
                }

                if (newState != null)
                {
                    _lastJobState = PropertyUtil.GetValueFromProp<JobState>(newState, JobPropertyIds.State, _lastJobState);
                    if ((JobState)newState.Value == JobState.Finished ||
                        (JobState)newState.Value == JobState.Failed ||
                        (JobState)newState.Value == JobState.Canceled)
                    {
                        if (TryRemoveEvents())
                        {
                            scheduler.Store.JobEvent -= OnJobEvent;
                            job.TaskEvent -= OnTaskEvent;
                        }

                        RemoveReconnectEventHandler();
                    }

                    OnCommandJobState(scheduler,
                        new JobStateEventArg(jobId, (JobState)newState.Value, oldState == null ? JobState.Configuring : (JobState)oldState.Value));
                }
            }
        }

        void OnTaskEvent(Int32 jobId, Int32 taskSystemId, TaskId taskId, EventType eventType, StoreProperty[] props)
        {
            if (jobId == job.Id
                && eventType == EventType.Modify
                && props != null)
            {
                StoreProperty newStateProp = null;
                StoreProperty oldStateProp = null;

                foreach (StoreProperty prop in props)
                {
                    if (prop.Id == TaskPropertyIds.State)
                    {
                        newStateProp = prop;
                    }
                    else if (prop.Id == TaskPropertyIds.PreviousState)
                    {
                        oldStateProp = prop;
                    }

                    if (newStateProp != null && oldStateProp != null)
                    {
                        break;
                    }
                }


                if (newStateProp != null)
                {
                    TaskState newState = (TaskState)newStateProp.Value;
                    TaskState oldState = oldStateProp == null ? TaskState.Configuring : (TaskState)oldStateProp.Value;

                    int exitCode = 0;
                    int errorCode = 0;
                    string errorMessage = string.Empty;

                    //find out the node and the data for this task
                    string nodeName = string.Empty;
                    NodeData data = null;
                    if (taskSystemId == proxyTaskId)
                    {
                        nodeName = proxyNode;
                        _lastProxyTaskState = newState;
                    }
                    else
                    {
                        lock (dataByTaskId)
                        {
                            if (dataByTaskId.TryGetValue(taskSystemId, out data))
                            {
                                nodeName = data.NodeName;
                                data.LastState = newState;
                            }
                        }
                    }



                    //find out the task's exit code and error message if the task is done
                    if (newState == TaskState.Failed
                            || newState == TaskState.Canceled
                            || newState == TaskState.Finished)
                    {
                        IClusterTask task = job.OpenTask(taskSystemId);
                        PropertyRow exitProps = task.GetProps(
                                    TaskPropertyIds.ExitCode,
                                    TaskPropertyIds.ErrorMessage,
                                    TaskPropertyIds.RequiredNodes
                                    );

                        StoreProperty prop = exitProps[TaskPropertyIds.ExitCode];
                        if (prop != null)
                        {
                            exitCode = (int)prop.Value;
                        }

                        prop = exitProps[TaskPropertyIds.ErrorCode];
                        if (prop != null)
                        {
                            errorCode = (int)prop.Value;
                        }

                        prop = exitProps[TaskPropertyIds.ErrorMessage];
                        if (prop != null)
                        {
                            errorMessage = (string)prop.Value;
                        }
                        prop = exitProps[TaskPropertyIds.RequiredNodes];
                        if (prop != null)
                        {
                            nodeName = (string)prop.Value;
                        }

                        if (data != null && _redirectOutput)
                        {
                            if (scheduler.Store.OverHttp)
                            {
                                //If the client is running over http, there is no proxy task running
                                //The task output data will have to be fetched from the task output in the scheduler database
                                //This will restrict data to only 4096 characters.
                                //Moreover, we should signal the event handlers after the data from the database has been fetched and
                                //processed.
                                //So we need to store the data needed for invoking the task state handlers and pass in as as data
                                //to the call back method used to fetch task output data

                                AzureNodeTerminalStateData azureEndData = new AzureNodeTerminalStateData(
                                  data,
                                  new CommandTaskStateEventArg(jobId, taskId, newState, oldState, nodeName, exitCode, errorMessage, taskSystemId == proxyTaskId),
                                  newState,
                                  errorCode,
                                  taskSystemId
                                  );

                                ThreadPool.QueueUserWorkItem(FetchDataOverHttp, azureEndData);
                                return;
                            }
                            else
                            {
                                if ((data.Location != NodeLocation.OnPremise && data.Location != NodeLocation.Linux && data.Location != NodeLocation.AzureBatch && data.Location != NodeLocation.NonDomainJoined))
                                {
                                    //if it is a task running on an azure node, we need to first fetch the data from
                                    //the azure node using the file staging client
                                    //Moreover, we should only signal the event handlers after the data from azure has been fetched
                                    //and processed
                                    //So we need to store the data needed for invoking the task state handlers and pass in as as data
                                    //to the call back method used to fetch azure data

                                    AzureNodeTerminalStateData azureEndData = new AzureNodeTerminalStateData(
                                        data,
                                        new CommandTaskStateEventArg(jobId, taskId, newState, oldState, nodeName, exitCode, errorMessage, taskSystemId == proxyTaskId),
                                        newState,
                                        errorCode,
                                        taskSystemId
                                        );

                                    ThreadPool.QueueUserWorkItem(FetchAzureData, azureEndData);

                                    return;
                                }
                            }
                        }

                    }

                    //Call the command's registered state handlers
                    InvokeCommandStateHandlers(
                        data,
                        new CommandTaskStateEventArg(jobId, taskId, newState, oldState, nodeName, exitCode, errorMessage, taskSystemId == proxyTaskId),
                        newState,
                        errorCode,
                        taskSystemId
                        );
                }
            }
        }

        private void OnReconnect(object sender, ConnectionEventArg msg)
        {
            if (msg.Code == ConnectionEventCode.EventReconnect || msg.Code == ConnectionEventCode.StoreReconnect)
            {
                //once we have lost conneciton, clusrun needs to shift to polling mode
                //to make sure that it tracks all task state changes
                if (_pollingThread == null)
                {
                    _pollingThread = new Thread(pollJobAndTaskStates);
                    _pollingThread.IsBackground = true;
                    _pollingThread.Start();
                }
            }
        }

        private void pollJobAndTaskStates()
        {
            while (!_pollingThreadStop)
            {
                try
                {
                    //check job state
                    PropertyId[] jobProps = { JobPropertyIds.State, JobPropertyIds.PreviousState };
                    PropertyRow jobPropRow = job.GetProps(jobProps);

                    JobState currentState = PropertyUtil.GetValueFromPropRow<JobState>(jobPropRow, JobPropertyIds.State, _lastJobState);
                    if (currentState != _lastJobState)
                    {
                        //if the job's state has changed since last time invoke the job event handler
                        try
                        {
                            OnJobEvent(job.Id, EventType.Modify, jobPropRow.Props);
                        }
                        catch (Exception e)
                        {
                            //User event handlers throwing exception should not cause us to hang.
                            Debug.Print("Exception from RemoteCommand Job Event Handler " + e.Message);
                        }
                    }

                    //check task states now
                    PropertyId[] taskProps = { TaskPropertyIds.Id, TaskPropertyIds.State, TaskPropertyIds.PreviousState, TaskPropertyIds.TaskId, TaskPropertyIds.ChangeTime };

                    ITaskRowSet taskRowSet = job.OpenTaskRowSet();
                    taskRowSet.SetFilter(new FilterProperty(FilterOperator.GreaterThan, TaskPropertyIds.ChangeTime, _lastChangeTime));
                    taskRowSet.SetColumns(taskProps);

                    foreach (PropertyRow taskRow in taskRowSet)
                    {
                        int taskSystemId = PropertyUtil.GetValueFromPropRow<int>(taskRow, TaskPropertyIds.Id, 0);
                        NodeData data = null;
                        if (taskSystemId != 0)
                        {
                            TaskState lastTaskState = TaskState.Configuring;

                            if (taskSystemId == proxyTaskId)
                            {
                                lastTaskState = _lastProxyTaskState;
                            }
                            else
                            {
                                lock (dataByTaskId)
                                {
                                    dataByTaskId.TryGetValue(taskSystemId, out data);
                                }
                                if (data == null)
                                {
                                    continue;
                                }
                                lastTaskState = data.LastState;
                            }
                            TaskState currentTaskState = PropertyUtil.GetValueFromPropRow<TaskState>(taskRow, TaskPropertyIds.State, lastTaskState);
                            TaskId taskId = PropertyUtil.GetValueFromPropRow<TaskId>(taskRow, TaskPropertyIds.TaskId, null);
                            DateTime changeTime = PropertyUtil.GetValueFromPropRow<DateTime>(taskRow, TaskPropertyIds.ChangeTime, DateTime.MinValue);

                            if (currentTaskState != lastTaskState)
                            {
                                try
                                {
                                    //if the task's state has changed since last time invoke the task event handler
                                    OnTaskEvent(job.Id, taskSystemId, taskId, EventType.Modify, taskRow.Props);
                                }
                                catch (Exception e)
                                {
                                    //User event handlers throwing exception should not cause us to hang.
                                    Debug.Print("Exception from RemoteCommand Task Event Handler " + e.Message);
                                }
                            }
                            if (changeTime > _lastChangeTime)
                            {
                                _lastChangeTime = changeTime;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Print("Polling Thread received an exception" + e.Message);
                }

                //if all the task state transitions as well as job state transitions are done, exit the thread
                if (_tasksFinished)
                {
                    if (_lastJobState == JobState.Failed || _lastJobState == JobState.Finished || _lastJobState == JobState.Canceled)
                    {
                        if (_lastProxyTaskState == TaskState.Failed || _lastProxyTaskState == TaskState.Failed || _lastProxyTaskState == TaskState.Canceled)
                        {
                            break;
                        }
                    }
                }

                Thread.Sleep(_pollingPeriod);
            }
        }

        /// <summary>
        /// Container class to contain the data needed by the callback used to fetch the azure data
        /// and invoke the task state handlers
        /// </summary>
        private class AzureNodeTerminalStateData
        {
            private NodeData _nodeData = null;
            private CommandTaskStateEventArg _arg = null;
            private TaskState _taskState = TaskState.All;
            private int _errorCode = 0;
            private int _taskSystemId = 0;

            internal NodeData NodeData { get { return _nodeData; } }
            internal CommandTaskStateEventArg Arg { get { return _arg; } }
            internal TaskState TaskState { get { return _taskState; } }
            internal int ErrorCode { get { return _errorCode; } }
            internal int TaskSystemId { get { return _taskSystemId; } }

            internal AzureNodeTerminalStateData(
                NodeData nodeData,
                CommandTaskStateEventArg arg,
                TaskState taskState,
                int errorCode,
                int taskSystemId
                )
            {
                _nodeData = nodeData;
                _arg = arg;
                _taskState = taskState;
                _errorCode = errorCode;
                _taskSystemId = taskSystemId;
            }

            internal void FailDueToStagingError()
            {
                _taskState = TaskState.Failed;
                _errorCode = 2;

                // Override the original _arg, so that the outside will see the task being failed.
                _arg = new CommandTaskStateEventArg(
                                _arg.JobId,
                                (TaskId)_arg.TaskId,
                                TaskState.Failed,
                                _arg.NewState,
                                _arg.NodeName,
                                _arg.ExitCode,
                                _arg.ErrorMessage,
                                _arg.IsProxy);
            }

        }

        /// <summary>
        /// Callback method used to fetch the azure data and invoke the state handlers after that
        /// </summary>
        /// <param name="state"></param>
        private async void FetchAzureData(object state)
        {
            AzureNodeTerminalStateData azureEndData = state as AzureNodeTerminalStateData;
            NodeData data = azureEndData.NodeData;
            int outputBufferSize = 4096;
            byte[] outputBuffer = new byte[outputBufferSize];

            try
            {
                RetryManager retry = new RetryManager(_retryTimer, NumberOfFileStagingRetries);
                Stream outputStream = null;
                while (true)
                {
                    try
                    {
                        string schedulerNodeName = await this.scheduler.Store.GetSchedulerNodeNameAsync().ConfigureAwait(false);
                        //Open a stream to read the data file for this task on the azure node
                        outputStream = ClusterFile.Read(schedulerNodeName, data.NodeName, GetAzureOutputFilename(data), 0);
                        {

                            if (outputStream != null)
                            {
                                #region read data
                                int readBytes = 0;
                                bool returnDetected = false;
                                do
                                {
                                    //read the stream in blocks
                                    readBytes = outputStream.Read(outputBuffer, 0, outputBufferSize);

                                    if (readBytes != 0)
                                    {
                                        if (returnDetected)
                                        {
                                            // If the return is detected in the middle of the string, then output it
                                            // Bug number 12322

                                            ProcessNodeData(data.OutBuffer, new byte[] { (byte)10 }, data.NodeName, CommandOutputType.Output);
                                            returnDetected = false;
                                        }

                                        if (readBytes == 1 && outputBuffer[0] == (byte)10)
                                        {
                                            // This is a special case where the last read will have nothing but a return
                                            // We need to remove it to make the output exactly the same with on-premise clusrun

                                            returnDetected = true;
                                            continue;
                                        }

                                        //after a block has been read, we need to copy it to a buffer the exact size of the data read
                                        //since ProcessNodeData does not take the length of the received buffer as input and instead
                                        //relies on the buffer's actual length
                                        byte[] nodeDataBuffer = new byte[readBytes];
                                        Array.Copy(outputBuffer, nodeDataBuffer, readBytes);
                                        ProcessNodeData(data.OutBuffer, nodeDataBuffer, data.NodeName, CommandOutputType.Output);
                                    }
                                } while (readBytes != 0);
                                #endregion
                            }
                        }
                        break;
                    }
                    catch (FileStagingException fse)
                    {
                        if (!retry.HasAttemptsLeft)
                        {
                            HandleFileStagingError(data, azureEndData, fse.ToString());
                            break;
                        }
                        retry.WaitForNextAttempt();
                    }
                    catch (Exception e)
                    {
                        HandleFileStagingError(data, azureEndData, e.ToString());
                        break;
                    }
                    finally
                    {
                        if (outputStream != null)
                        {
                            try
                            {
                                outputStream.Dispose();
                            }
                            catch (Exception ex)
                            {
                                //swallow any exception in closing the stream
                                TraceHelper.TraceWarning("RemoteCommand dispose outputStream Exception: {0}", ex);
                            }
                            outputStream = null;
                        }
                    }
                }

                try
                {
                    // At the end delete the file on the azure node
                    ClusterFile.Delete(proxyNode, data.NodeName, GetAzureOutputFilename(data));
                }
                catch (Exception ex)
                {
                    //swallow any exception
                    TraceHelper.TraceWarning("RemoteCommand delete Cluster file Exception: {0}", ex);
                }

                SendEof(data);

                //Once the data has been received and processed, we can invoke the command state handlers for this task
                InvokeCommandStateHandlers(
                    data,
                    azureEndData.Arg,
                    azureEndData.TaskState,
                    azureEndData.ErrorCode,
                    azureEndData.TaskSystemId
                    );
            }
            catch (Exception ex)
            {
                //Swallow exception, since we do not want an exception going up the thread pool
                TraceHelper.TraceWarning("RemoteCommand.FetchAzureData Exception: {0}", ex);
            }
        }

        private void FetchDataOverHttp(object state)
        {
            AzureNodeTerminalStateData azureEndData = state as AzureNodeTerminalStateData;
            NodeData data = azureEndData.NodeData;


            RetryManager retry = new RetryManager(_retryTimer, NumberOfFileStagingRetries);
            while (true)
            {
                try
                {
                    IClusterTask task = job.OpenTask(azureEndData.TaskSystemId);

                    PropertyRow propRow = task.GetProps(new PropertyId[] { TaskPropertyIds.Output });
                    string output = null;

                    if (propRow != null)
                    {
                        output = PropertyUtil.GetValueFromPropRow<string>(propRow, TaskPropertyIds.Output, null);
                    }

                    if (output != null)
                    {
                        MemoryStream memStream = new MemoryStream();
                        StreamWriter memStreamWriter = new StreamWriter(memStream);

                        memStreamWriter.Write(output);
                        memStreamWriter.Flush();

                        byte[] outputByteArray = memStream.ToArray();

                        ProcessNodeData(data.OutBuffer, outputByteArray, data.NodeName, CommandOutputType.Output);
                    }

                    break;
                }
                catch (SchedulerException se)
                {
                    if (!retry.HasAttemptsLeft)
                    {
                        HandleFileStagingError(data, azureEndData, se.ToString());
                        break;
                    }
                    retry.WaitForNextAttempt();
                }
                catch (Exception e)
                {
                    HandleFileStagingError(data, azureEndData, e.ToString());
                    break;
                }
            }

            SendEof(data);

            //Once the data has been received and processed, we can invoke the command state handlers for this task
            InvokeCommandStateHandlers(
                data,
                azureEndData.Arg,
                azureEndData.TaskState,
                azureEndData.ErrorCode,
                azureEndData.TaskSystemId
                );
        }

        private void HandleFileStagingError(NodeData data, AzureNodeTerminalStateData azureEndData, string errorMsg)
        {
            try
            {
                //If there is an exception during data transfer we write that to the output stream
                //and invoke the handler as having failed so that the node is reported as failed
                azureEndData.FailDueToStagingError();
                MemoryStream outputMs = new MemoryStream();
                StreamWriter writer = new StreamWriter(outputMs, data.OutBuffer.Encoding);
                writer.Write("Error while transfering data from azure node {0}. Detail: {1}", data.NodeName, errorMsg);
                writer.Flush();
                ProcessNodeData(data.OutBuffer, outputMs.GetBuffer(), data.NodeName, CommandOutputType.Output);
            }
            catch (Exception ex)
            {
                //swallow any exception
                TraceHelper.TraceWarning("RemoteCommand.HandleFileStagingError Exception: {0}", ex);
            }

        }

        /// <summary>
        /// Method to invoke the command event handlers for this remote command in a serialized fashion,
        /// so that even if this method is called from different threads, the event handlers get called one by one
        /// </summary>
        /// <param name="data"></param>
        /// <param name="arg"></param>
        /// <param name="taskState"></param>
        /// <param name="errorCode"></param>
        /// <param name="taskSystemId"></param>
        private void InvokeCommandStateHandlers(
            NodeData data,
            CommandTaskStateEventArg arg,
            TaskState taskState,
            int errorCode,
            int taskSystemId
            )
        {
            lock (_eventlock)
            {
                if (OnCommandTaskState != null)
                {
                    OnCommandTaskState(scheduler,
                        arg);
                }

                //call internal handlers
                if (taskSystemId == proxyTaskId)
                {
                    OnProxyTaskStateChange(taskState, errorCode);
                }
                else
                {
                    OnCommandTaskStateChange(taskSystemId, data, taskState, errorCode);
                }
            }
        }


        void OnCommandTaskStateChange(int taskId, NodeData data, TaskState state, int errorCode)
        {
            if (data != null)
            {
                //if a task has failed before it starts to run, send out the EOF messages
                if ((state == TaskState.Failed || state == TaskState.Canceled)
                    && errorCode != ErrorCode.Execution_TaskExecutionFailure)
                {
                    SendEof(data);
                }

                if (state == TaskState.Failed || state == TaskState.Finished || state == TaskState.Canceled)
                {

                    data.Finished = true;
                    CheckFinish();
                }
            }
        }

        void OnProxyTaskStateChange(TaskState state, int errorCode)
        {
            if (state == TaskState.Running)
            {
                //if the pipe proxy has started running and it is the server, it is time to start
                //our thread
                if (isPipeProxyServer())
                {
                    worker.Start();
                }
            }
            //if the proxy task has failed before it starts to run
            if ((state == TaskState.Failed || state == TaskState.Canceled)
                && errorCode != ErrorCode.Execution_TaskExecutionFailure)
            {
                EndBeforeTaskRun();
            }

            CheckFinish();
        }

        //
        // In case where something goes wrong before any command really starts to run,
        // send out all the EOF and the task cancel messages
        //
        void EndBeforeTaskRun()
        {
            foreach (NodeData data in dataByNode.Values)
            {
                CmdEndBeforeTaskRun(data, SR.RemoteCommand_ProxyFailed);
            }
        }

        //
        // If the command does not start on a node,
        // send out EOF and task cancel messages
        //
        void CmdEndBeforeTaskRun(NodeData data, string message)
        {
            SendEof(data);
            data.Finished = true;
            //call client registered handler
            if (OnCommandTaskState != null)
            {
                OnCommandTaskState(scheduler,
                    new CommandTaskStateEventArg(job.Id, new TaskId(), TaskState.Canceled, TaskState.Configuring, data.NodeName, 1, message, false));
            }
        }

        void CancelWorker(object state)
        {
            Cancel();
        }

        #region IDisposable Members
        /// <summary>
        ///   <para>Frees resources before the object is reclaimed by garbage collection.</para>
        /// </summary>
        /// <remarks>
        ///   <para>This method is called automatically by the runtime.</para>
        /// </remarks>
        ~RemoteCommand()
        {
            Dispose(false);
        }

        /// <summary>
        ///   <para>Releases all unmanaged resources used by the command.</para>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (pipeFile != null)
                {
                    pipeFile.Close();
                }

                if (pipeHandle != null)
                {
                    pipeHandle.Close();
                }

                if (cancelTimer != null)
                {
                    cancelTimer.Dispose();
                }

                if (_pollingThread != null)
                {
                    try
                    {
                        if (_pollingThread.IsAlive)
                        {
                            if (!_pollingThread.Join(100))
                            {
                                _pollingThreadStop = true;
                                _pollingThread.Join();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceHelper.TraceWarning("RemoteCommand abort _pollingThread Exception: {0}", ex);
                    }
                }

                //Remove the eventhandler from scheduler, if there is no client subscrited to OnCommandJobState event
                if (OnCommandJobState == null)
                {
                    if (TryRemoveEvents())
                    {
                        try
                        {
                            if (scheduler != null && scheduler.Store != null)
                            {
                                scheduler.Store.JobEvent -= OnJobEvent;
                            }
                            if (job != null)
                            {
                                job.TaskEvent -= OnTaskEvent;
                            }
                        }
                        catch (Exception ex)
                        {
                            TraceHelper.TraceWarning("RemoteCommand remove event handler Exception: {0}", ex);
                        }
                    }
                }

                RemoveReconnectEventHandler();
            }

            pipeFile = null;
            pipeHandle = null;
            cancelTimer = null;
            _pollingThread = null;
        }

        /// <summary>
        /// Add Reconnect event handler
        /// </summary>
        private void AddReconnectEventHandler()
        {
            scheduler.OnReconnect += OnReconnect;
            Interlocked.Exchange(ref _eventReconnectAdded, 1);
        }

        /// <summary>
        /// Remove Reconnect event handler
        /// </summary>
        void RemoveReconnectEventHandler()
        {
            if (TryRemoveReconnectEvent())
            {
                try
                {
                    if (scheduler != null)
                    {
                        scheduler.OnReconnect -= OnReconnect;
                    }
                }
                catch (Exception ex)
                {
                    TraceHelper.TraceWarning("RemoteCommand remove OnReconnect event handler Exception: {0}", ex);
                }
            }
        }

        /// <summary>
        /// If this method is called and it returns true, you must remove the events
        /// otherwise the events will leak
        /// </summary>
        /// <returns></returns>
        bool TryRemoveEvents()
        {
            return Interlocked.Exchange(ref _eventsAdded, 0) == 1;
        }

        /// <summary>
        /// If this method is called and it returns true, you must remove the Reconnect event
        /// otherwise the events will leak
        /// </summary>
        /// <returns></returns>
        bool TryRemoveReconnectEvent()
        {
            return Interlocked.Exchange(ref _eventReconnectAdded, 0) == 1;
        }

        #endregion
    }
}
