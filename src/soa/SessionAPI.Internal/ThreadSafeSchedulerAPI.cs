//------------------------------------------------------------------------------
// <copyright file="ThreadSafeSchedulerAPI.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      ThreadSafe wrappers for scheduler API
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    using System;

    /// <summary>
    /// Wrapper class
    /// Wraps a object
    /// </summary>
    /// <typeparam name="T">TYpe of wrapped object</typeparam>
    public class Wrapper<T>
    {
        /// <summary>
        /// Instance member
        /// </summary>
        private T _instance;

        /// <summary>
        /// Returns the wrapped instance
        /// </summary>
        public T Instance
        {
            get
            {
                return _instance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instance">The instance to wrap</param>
        public Wrapper(T instance)
        {
            _instance = instance;
        }
    }

    /// <summary>
    /// A thread safe wrapper with lock
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThreadSafeWrapper<T> : Wrapper<T>, IDisposable
    {
        /// <summary>
        /// The object that will be used for synchronization
        /// </summary>
        private volatile object _syncRoot;

        /// <summary>
        /// Get/Set the internal object used for sync.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                return _syncRoot;
            }
            set
            {
                _syncRoot = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="instance">The instance</param>
        public ThreadSafeWrapper(T instance) :
            base(instance)
        {
            // In order to minimize risk, we'll hold locks on the object instance level because
            // there is no guarantee another thread could get hold of the same underlying object by calling some API
            _syncRoot = instance;
        }

        /// <summary>
        /// Dispose method. Only valid when the object implements dispose interface
        /// </summary>
        public void Dispose()
        {
            lock (SyncRoot)
            {
                IDisposable disp = Instance as IDisposable;
                if (disp != null)
                    disp.Dispose();
            }
        }
    }

#if HPCPACK
    /// <summary>
    /// Thread-safe wrapper of Scheduler object. Currently only implement IScheduler interface
    /// </summary>
    internal class ThreadSafeScheduler : ThreadSafeWrapper<IScheduler>, IScheduler
    {

        public ThreadSafeScheduler(IScheduler scheduler) :
            base(scheduler)
        {
        }

#region IScheduler Members

        /// <summary>
        /// Connect to the scheduler
        /// </summary>
        /// <param name="context">the scheduler connection context.</param>
        /// <param name="token">the cancellation token</param>
        /// <returns></returns>
        public Task ConnectAsync(SchedulerConnectionContext context, CancellationToken token)
        {
            lock (SyncRoot) { return Instance.ConnectAsync(context, token); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowSet OpenJobRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.OpenJobRowSet(properties, filter, sort); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenJobHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.OpenJobHistoryEnumerator(properties, filter, sort); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenNodeHistoryEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.OpenNodeHistoryEnumerator(properties, filter, sort); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenNodeEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.OpenNodeEnumerator(properties, filter, sort); }
        }

        public void DeleteCachedCredentials(System.String userName)
        {
            lock (SyncRoot) { Instance.DeleteCachedCredentials(userName); }
        }

        public void SetCachedCredentials(System.String userName, System.String password)
        {
            lock (SyncRoot) { Instance.SetCachedCredentials(userName, password); }
        }

        public void Close()
        {
            lock (SyncRoot) { Instance.Close(); }
        }

        public System.Xml.XmlReader GetJobTemplateXml(string jobTemplateName)
        {
            lock (SyncRoot) { return Instance.GetJobTemplateXml(jobTemplateName); }
        }

        public Microsoft.Hpc.Scheduler.JobTemplateInfo GetJobTemplateInfo(System.String jobTemplateName)
        {
            lock (SyncRoot) { return Instance.GetJobTemplateInfo(jobTemplateName); }
        }

        public void CancelJob(System.Int32 jobId, System.String message, System.Boolean isForced)
        {
            lock (SyncRoot) { Instance.CancelJob(jobId, message, isForced); }
        }

        public void RequeueJob(System.Int32 jobId)
        {
            lock (SyncRoot) { Instance.RequeueJob(jobId); }
        }

        public void Connect(System.String cluster)
        {
            lock (SyncRoot) { Instance.Connect(cluster); }
        }

#pragma warning disable 618 // disable obsolete warning for UserPrivilege

        public Microsoft.Hpc.Scheduler.Properties.UserPrivilege GetUserPrivilege()
        {
            lock (SyncRoot) { return Instance.GetUserPrivilege(); }
        }

#pragma warning restore 618 // disable obsolete warning for UserPrivilege

        public Microsoft.Hpc.Scheduler.Properties.UserRoles GetUserRoles()
        {
            lock (SyncRoot) { return Instance.GetUserRoles(); }
        }

        public void SetInterfaceMode(System.Boolean isConsole, System.IntPtr hwnd)
        {
            lock (SyncRoot) { Instance.SetInterfaceMode(isConsole, hwnd); }
        }

        public Microsoft.Hpc.Scheduler.IServerVersion GetServerVersion()
        {
            lock (SyncRoot) { return Instance.GetServerVersion(); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerJob CreateJob()
        {
            lock (SyncRoot) { return new ThreadSafeSchedulerJob(Instance.CreateJob()); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerJob OpenJob(System.Int32 id)
        {
            lock (SyncRoot) { return new ThreadSafeSchedulerJob(Instance.OpenJob(id)); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerJob CloneJob(System.Int32 jobId)
        {
            lock (SyncRoot) { return new ThreadSafeSchedulerJob(Instance.CloneJob(jobId)); }
        }

        public void AddJob(Microsoft.Hpc.Scheduler.ISchedulerJob job)
        {
            lock (SyncRoot)
            {
                // Retrieve the scheduler job instance if we are dealing with a wrapper
                ThreadSafeSchedulerJob threadSafeSchedulerJob = job as ThreadSafeSchedulerJob;
                if (threadSafeSchedulerJob != null)
                    job = threadSafeSchedulerJob.Instance;

                Instance.AddJob(job);
            }
        }

        public void SubmitJob(Microsoft.Hpc.Scheduler.ISchedulerJob job, System.String username, System.String password)
        {
            lock (SyncRoot)
            {
                // Retrieve the scheduler job instance if we are dealing with a wrapper
                ThreadSafeSchedulerJob threadSafeSchedulerJob = job as ThreadSafeSchedulerJob;
                if (threadSafeSchedulerJob != null)
                    job = threadSafeSchedulerJob.Instance;

                Instance.SubmitJob(job, username, password);
            }
        }

        public void SubmitJobById(System.Int32 jobId, System.String username, System.String password)
        {
            lock (SyncRoot) { Instance.SubmitJobById(jobId, username, password); }
        }

        public void CancelJob(System.Int32 jobId, System.String message)
        {
            lock (SyncRoot) { Instance.CancelJob(jobId, message); }
        }

        public void CancelJob(System.Int32 jobId, System.String message, bool isForce, bool isGraceful)
        {
            lock (SyncRoot) { Instance.CancelJob(jobId, message, isForce, isGraceful); }
        }

        public void FinishJob(System.Int32 jobId, System.String message, bool isForce, bool isGraceful)
        {
            lock (SyncRoot) { Instance.FinishJob(jobId, message, isForce, isGraceful); }
        }

        public void ConfigureJob(System.Int32 jobId)
        {
            lock (SyncRoot) { Instance.ConfigureJob(jobId); }
        }

        /// <remarks>
        /// IScheduler.GetJobList returns a new collection. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.ISchedulerCollection GetJobList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return ConvertJobList(Instance.GetJobList(filter, sort)); }
        }

        public Microsoft.Hpc.Scheduler.IIntCollection GetJobIdList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.GetJobIdList(filter, sort); }
        }

        /// <remarks>
        /// IScheduler.GetNodeList returns a new collection. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.ISchedulerCollection GetNodeList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.GetNodeList(filter, sort); }
        }

        public Microsoft.Hpc.Scheduler.IIntCollection GetNodeIdList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.GetNodeIdList(filter, sort); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerNode OpenNode(System.Int32 nodeId)
        {
            lock (SyncRoot) { return Instance.OpenNode(nodeId); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerNode OpenNodeByName(System.String nodeName)
        {
            lock (SyncRoot) { return Instance.OpenNodeByName(nodeName); }
        }

        public Microsoft.Hpc.Scheduler.Properties.ITaskId CreateTaskId(System.Int32 jobTaskId)
        {
            lock (SyncRoot) { return Instance.CreateTaskId(jobTaskId); }
        }

        public Microsoft.Hpc.Scheduler.Properties.ITaskId CreateParametricTaskId(System.Int32 jobTaskId, System.Int32 instanceId)
        {
            lock (SyncRoot) { return Instance.CreateParametricTaskId(jobTaskId, instanceId); }
        }

        public void SetEnvironmentVariable(System.String name, System.String value)
        {
            lock (SyncRoot) { Instance.SetEnvironmentVariable(name, value); }
        }

        public void SetClusterParameter(System.String name, System.String value)
        {
            lock (SyncRoot) { Instance.SetClusterParameter(name, value); }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection GetJobTemplateList()
        {
            lock (SyncRoot) { return Instance.GetJobTemplateList(); }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection GetNodeGroupList()
        {
            lock (SyncRoot) { return Instance.GetNodeGroupList(); }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection GetNodesInNodeGroup(System.String nodeGroup)
        {
            lock (SyncRoot) { return Instance.GetNodesInNodeGroup(nodeGroup); }
        }

        public Microsoft.Hpc.Scheduler.INameValueCollection CreateNameValueCollection()
        {
            lock (SyncRoot) { return Instance.CreateNameValueCollection(); }
        }

        public Microsoft.Hpc.Scheduler.IFilterCollection CreateFilterCollection()
        {
            lock (SyncRoot) { return Instance.CreateFilterCollection(); }
        }

        public Microsoft.Hpc.Scheduler.ISortCollection CreateSortCollection()
        {
            lock (SyncRoot) { return Instance.CreateSortCollection(); }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection CreateStringCollection()
        {
            lock (SyncRoot) { return Instance.CreateStringCollection(); }
        }

        public Microsoft.Hpc.Scheduler.IIntCollection CreateIntCollection()
        {
            lock (SyncRoot) { return Instance.CreateIntCollection(); }
        }

        public Microsoft.Hpc.Scheduler.ICommandInfo CreateCommandInfo(Microsoft.Hpc.Scheduler.INameValueCollection envs, System.String workDir, System.String stdIn)
        {
            lock (SyncRoot) { return Instance.CreateCommandInfo(envs, workDir, stdIn); }
        }

        public Microsoft.Hpc.Scheduler.IRemoteCommand CreateCommand(System.String commandLine, Microsoft.Hpc.Scheduler.ICommandInfo info, Microsoft.Hpc.Scheduler.IStringCollection nodes)
        {
            lock (SyncRoot) { return Instance.CreateCommand(commandLine, info, nodes); }
        }


        public Microsoft.Hpc.Scheduler.IRemoteCommand CreateCommand(string commandLine, ICommandInfo info, IStringCollection nodes, bool redirectOutput)
        {
            lock (SyncRoot) { return Instance.CreateCommand(commandLine, info, nodes, redirectOutput); }
        }
        /// <remarks>
        /// GetCounters return a new ISchedulerCounter instance and the instance is immutable and is initialized inside the function. No worries for concurrency
        /// </remarks>
        public Microsoft.Hpc.Scheduler.ISchedulerCounters GetCounters()
        {
            lock (SyncRoot) { return Instance.GetCounters(); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenJobEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort)
        {
            lock (SyncRoot) { return Instance.OpenJobEnumerator(properties, filter, sort); }
        }

        /// <remarks>
        /// Returns a new collection of immutable objects. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.INameValueCollection EnvironmentVariables
        {
            get { lock (SyncRoot) { return Instance.EnvironmentVariables; } }
        }

        /// <remarks>
        /// Returns a new collection of immutable objects. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.INameValueCollection ClusterParameters
        {
            get { lock (SyncRoot) { return Instance.ClusterParameters; } }
        }

        public event EventHandler<ConnectionEventArg> OnSchedulerReconnect
        {
            add { lock (SyncRoot) { Instance.OnSchedulerReconnect += (EventHandler<ConnectionEventArg>)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method); } }
            remove { lock (SyncRoot) { Instance.OnSchedulerReconnect -= (EventHandler<ConnectionEventArg>)Delegate.CreateDelegate(typeof(ReconnectHandler), value.Target, value.Method); } }
        }

        public ISchedulerPool CreatePool(string poolName)
        {
            lock (SyncRoot) { return Instance.CreatePool(poolName); }
        }

        public ISchedulerPool CreatePool(string poolName, int poolWeight)
        {
            lock (SyncRoot) { return Instance.CreatePool(poolName, poolWeight); }
        }

        public ISchedulerPool OpenPool(string poolName)
        {
            lock (SyncRoot) { return Instance.OpenPool(poolName); }
        }

        public void DeletePool(string poolName)
        {
            lock (SyncRoot) { Instance.DeletePool(poolName); }
        }

        public void DeletePool(string poolName, bool force)
        {
            lock (SyncRoot) { Instance.DeletePool(poolName, force); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowSet OpenPoolRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties)
        {
            lock (SyncRoot) { return Instance.OpenPoolRowSet(properties); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerCollection GetPoolList()
        {
            lock (SyncRoot) { return Instance.GetPoolList(); }
        }

        public void SetCertificateCredentials(string userName, string thumbprint)
        {
            lock (SyncRoot) { Instance.SetCertificateCredentials(userName, thumbprint); }
        }

        public void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes)
        {
            lock (SyncRoot) { Instance.SetCertificateCredentialsPfx(userName, pfxPassword, certBytes); }
        }

        public byte[] GetCertificateFromStore(string thumbprint, out SecureString pfxPassword)
        {
            lock (SyncRoot) { return Instance.GetCertificateFromStore(thumbprint, out pfxPassword); }
        }

        public string EnrollCertificate(string templateName)
        {
            lock (SyncRoot) { return Instance.EnrollCertificate(templateName); }
        }

        public bool ValidateAzureUser(string username, string password)
        {
            lock (SyncRoot) { return Instance.ValidateAzureUser(username, password); }
        }

        public void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider)
        {
            lock (SyncRoot) { Instance.ConnectServiceAsClient(cluster, identityProvider); }
        }

        public void ConnectServiceAsClient(string cluster, ServiceAsClientIdentityProvider identityProvider, string userName, string password)
        {
            lock (SyncRoot) { Instance.ConnectServiceAsClient(cluster, identityProvider, userName, password); }
        }

        public Task ConnectServiceAsClientAsync(IHpcContext hpcContext, ServiceAsClientIdentityProvider identityProvider, string userName, string password)
        {
            lock (SyncRoot) { return Instance.ConnectServiceAsClientAsync(hpcContext, identityProvider, userName, password); }
        }

        public string GetActiveHeadNode()
        {
            lock (SyncRoot) { return Instance.GetActiveHeadNode(); }
        }

        public void SetEmailCredentials(string userName, string password)
        {
            lock (SyncRoot) { Instance.SetEmailCredentials(userName, password); }
        }

        public void DeleteEmailCredentials()
        {
            lock (SyncRoot) { Instance.DeleteEmailCredentials(); }
        }


        public Task ConnectAsync(SchedulerConnectionContext context, CancellationToken token, ConnectMethod method)
        {
            lock (SyncRoot) { return Instance.ConnectAsync(context, token, method); }
        }

        public Task ConnectServiceAsClientAsync(IHpcContext hpcContext, ServiceAsClientIdentityProvider identityProvider, string userName, string password, ConnectMethod method)
        {
            lock (SyncRoot) { return Instance.ConnectServiceAsClientAsync(hpcContext, identityProvider, userName, password, method); }
        }

        public void Connect(string cluster, ConnectMethod method)
        {
            lock (SyncRoot) { Instance.Connect(cluster, method); }
        }

        public void DeleteJob(int jobId)
        {
            lock (SyncRoot) { Instance.DeleteJob(jobId); }
        }

#endregion

        /// <summary>
        /// Converts a ISchedulerCollection of ISchedulerJob into corresponding wrappers
        /// </summary>
        /// <param name="collection">Collection to be converted</param>
        /// <returns>A new collection with wrappers</returns>
        private static ISchedulerCollection ConvertJobList(ISchedulerCollection collection)
        {
            SchedulerCollection<ISchedulerJob> newCollection = new SchedulerCollection<ISchedulerJob>();
            foreach (ISchedulerJob job in collection)
                newCollection.Add(job);

            return newCollection;
        }
    }

    /// <summary>
    /// Thread-safe wrapper of SchedulerJob object. Currently only implement ISchedulerJob interface
    /// </summary>
    internal class ThreadSafeSchedulerJob : ThreadSafeWrapper<ISchedulerJob>, ISchedulerJob
    {
        public ThreadSafeSchedulerJob(ISchedulerJob schedulerJob) :
            base(schedulerJob)
        {
        }

#region ISchedulerJob Members

        public IAsyncResult BeginFinishTask(int taskSystemId, string message, bool isForced, AsyncCallback callback, object state)
        {
            lock (SyncRoot) { return Instance.BeginFinishTask(taskSystemId, message, isForced, callback, state); }
        }

        /// <summary>
        /// Don't lock EndXXX method to avoid deadlock with the callback thread.
        /// </summary>
        /// <param name="ar">async result</param>
        /// <returns>the task state</returns>
        public TaskState EndFinishTask(IAsyncResult ar)
        {
            return Instance.EndFinishTask(ar);
        }

        public bool GetBalanceRequest(out IList<BalanceRequest> request)
        {
            lock (SyncRoot) { return Instance.GetBalanceRequest(out request); }
        }

        public void Finish()
        {
            lock (SyncRoot) { Instance.Finish(); }
        }

        public void Finish(bool isForced, bool isGraceful)
        {
            lock (SyncRoot) { Instance.Finish(isForced, isGraceful); }
        }

        public void Cancel(bool isForced, bool isGraceful)
        {
            lock (SyncRoot) { Instance.Cancel(isForced, isGraceful); }
        }

        public void Requeue()
        {
            lock (SyncRoot) { Instance.Requeue(); }
        }

        public void SetEnvironmentVariable(System.String name, System.String value)
        {
            lock (SyncRoot) { Instance.SetEnvironmentVariable(name, value); }
        }

        public void AddTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[] taskList)
        {
            lock (SyncRoot) { Instance.AddTasks(taskList); }
        }

        public void SubmitTasks(Microsoft.Hpc.Scheduler.ISchedulerTask[] taskList)
        {
            lock (SyncRoot) { Instance.SubmitTasks(taskList); }
        }

        public void SetHoldUntil(System.DateTime holdUntil)
        {
            lock (SyncRoot) { Instance.SetHoldUntil(holdUntil); }
        }

        public void ClearHold()
        {
            lock (SyncRoot) { Instance.ClearHold(); }
        }

        public void CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId, System.String message)
        {
            lock (SyncRoot) { Instance.CancelTask(taskId, message); }
        }

        public void CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId, System.String message, System.Boolean isForce)
        {
            lock (SyncRoot) { Instance.CancelTask(taskId, message, isForce); }
        }

        public void AddExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection nodeNames)
        {
            lock (SyncRoot) { Instance.AddExcludedNodes(nodeNames); }
        }

        public void RemoveExcludedNodes(Microsoft.Hpc.Scheduler.IStringCollection nodeNames)
        {
            lock (SyncRoot) { Instance.RemoveExcludedNodes(nodeNames); }
        }

        public void ClearExcludedNodes()
        {
            lock (SyncRoot) { Instance.ClearExcludedNodes(); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerTask CreateTask()
        {
            lock (SyncRoot) { return Instance.CreateTask(); }
        }

        public void AddTask(Microsoft.Hpc.Scheduler.ISchedulerTask task)
        {
            lock (SyncRoot) { Instance.AddTask(task); }
        }

        public void Refresh()
        {
            lock (SyncRoot) { Instance.Refresh(); }
        }

        public void Commit()
        {
            lock (SyncRoot) { Instance.Commit(); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerTask OpenTask(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId)
        {
            lock (SyncRoot) { return Instance.OpenTask(taskId); }
        }

        public void CancelTask(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId)
        {
            lock (SyncRoot) { Instance.CancelTask(taskId); }
        }

        public void RequeueTask(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId)
        {
            lock (SyncRoot) { Instance.RequeueTask(taskId); }
        }

        public void SubmitTaskById(Microsoft.Hpc.Scheduler.Properties.ITaskId taskId)
        {
            lock (SyncRoot) { Instance.SubmitTaskById(taskId); }
        }

        public void SubmitTask(Microsoft.Hpc.Scheduler.ISchedulerTask task)
        {
            lock (SyncRoot) { Instance.SubmitTask(task); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerCollection GetTaskList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, System.Boolean expandParametric)
        {
            lock (SyncRoot) { return Instance.GetTaskList(filter, sort, expandParametric); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerCollection GetTaskIdList(Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, System.Boolean expandParametric)
        {
            lock (SyncRoot) { return Instance.GetTaskIdList(filter, sort, expandParametric); }
        }

        public Microsoft.Hpc.Scheduler.ISchedulerRowEnumerator OpenTaskEnumerator(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, System.Boolean expandParametric)
        {
            lock (SyncRoot) { return Instance.OpenTaskEnumerator(properties, filter, sort, expandParametric); }
        }

        /// <remarks>
        /// TODO
        /// </remarks>
        public Microsoft.Hpc.Scheduler.ISchedulerRowSet OpenTaskRowSet(Microsoft.Hpc.Scheduler.IPropertyIdCollection properties, Microsoft.Hpc.Scheduler.IFilterCollection filter, Microsoft.Hpc.Scheduler.ISortCollection sort, System.Boolean expandParametric)
        {
            lock (SyncRoot) { return Instance.OpenTaskRowSet(properties, filter, sort, expandParametric); }
        }

        public void SetJobTemplate(System.String templateName)
        {
            lock (SyncRoot) { Instance.SetJobTemplate(templateName); }
        }

        public void RestoreFromXml(System.String url)
        {
            lock (SyncRoot) { Instance.RestoreFromXml(url); }
        }

        public void RestoreFromXmlEx(string url, bool includeTaskGroup)
        {
            lock (SyncRoot) { Instance.RestoreFromXmlEx(url, includeTaskGroup); }
        }

        public void RestoreFromXml(System.Xml.XmlReader reader)
        {
            lock (SyncRoot) { Instance.RestoreFromXml(reader); }
        }

        public void RestoreFromXmlEx(System.Xml.XmlReader reader, bool includeTaskGroup)
        {
            lock (SyncRoot) { Instance.RestoreFromXmlEx(reader, includeTaskGroup); }
        }

        public void SetCustomProperty(System.String name, System.String value)
        {
            lock (SyncRoot) { Instance.SetCustomProperty(name, value); }
        }

        /// <remarks>
        /// Returns a new collection of immutable objects. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.INameValueCollection GetCustomProperties()
        {
            lock (SyncRoot) { return Instance.GetCustomProperties(); }
        }

        /// <remarks>
        /// TODO
        /// </remarks>
        public Microsoft.Hpc.Scheduler.ISchedulerJobCounters GetCounters()
        {
            lock (SyncRoot) { return Instance.GetCounters(); }
        }


        public System.String JobTemplate
        {
            get { lock (SyncRoot) { return Instance.JobTemplate; } }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection AllocatedNodes
        {
            get { lock (SyncRoot) { return Instance.AllocatedNodes; } }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection EndpointAddresses
        {
            get { lock (SyncRoot) { return Instance.EndpointAddresses; } }
        }

        public System.String OrderBy
        {
            get { lock (SyncRoot) { return Instance.OrderBy; } }
            set { lock (SyncRoot) { Instance.OrderBy = value; } }
        }

        public System.Int32 Id
        {
            get { lock (SyncRoot) { return Instance.Id; } }
        }

        public System.String Name
        {
            get { lock (SyncRoot) { return Instance.Name; } }
            set { lock (SyncRoot) { Instance.Name = value; } }
        }

        public System.String Owner
        {
            get { lock (SyncRoot) { return Instance.Owner; } }
        }

        public System.String UserName
        {
            get { lock (SyncRoot) { return Instance.UserName; } }
            set { lock (SyncRoot) { Instance.UserName = value; } }
        }

        public Microsoft.Hpc.Scheduler.Properties.JobPriority Priority
        {
            get { lock (SyncRoot) { return Instance.Priority; } }
            set { lock (SyncRoot) { Instance.Priority = value; } }
        }

        public System.String Project
        {
            get { lock (SyncRoot) { return Instance.Project; } }
            set { lock (SyncRoot) { Instance.Project = value; } }
        }

        public System.Int32 Runtime
        {
            get { lock (SyncRoot) { return Instance.Runtime; } }
            set { lock (SyncRoot) { Instance.Runtime = value; } }
        }

        public System.DateTime SubmitTime
        {
            get { lock (SyncRoot) { return Instance.SubmitTime; } }
        }

        public System.DateTime CreateTime
        {
            get { lock (SyncRoot) { return Instance.CreateTime; } }
        }

        public System.DateTime EndTime
        {
            get { lock (SyncRoot) { return Instance.EndTime; } }
        }

        public System.DateTime StartTime
        {
            get { lock (SyncRoot) { return Instance.StartTime; } }
        }

        public System.DateTime ChangeTime
        {
            get { lock (SyncRoot) { return Instance.ChangeTime; } }
        }

        public Microsoft.Hpc.Scheduler.Properties.JobState State
        {
            get { lock (SyncRoot) { return Instance.State; } }
        }

        public Microsoft.Hpc.Scheduler.Properties.JobState PreviousState
        {
            get { lock (SyncRoot) { return Instance.PreviousState; } }
        }

        public System.Int32 MinimumNumberOfCores
        {
            get { lock (SyncRoot) { return Instance.MinimumNumberOfCores; } }
            set { lock (SyncRoot) { Instance.MinimumNumberOfCores = value; } }
        }

        public System.Int32 MaximumNumberOfCores
        {
            get { lock (SyncRoot) { return Instance.MaximumNumberOfCores; } }
            set { lock (SyncRoot) { Instance.MaximumNumberOfCores = value; } }
        }

        public System.Int32 MinimumNumberOfNodes
        {
            get { lock (SyncRoot) { return Instance.MinimumNumberOfNodes; } }
            set { lock (SyncRoot) { Instance.MinimumNumberOfNodes = value; } }
        }

        public System.Int32 MaximumNumberOfNodes
        {
            get { lock (SyncRoot) { return Instance.MaximumNumberOfNodes; } }
            set { lock (SyncRoot) { Instance.MaximumNumberOfNodes = value; } }
        }

        public System.Int32 MinimumNumberOfSockets
        {
            get { lock (SyncRoot) { return Instance.MinimumNumberOfSockets; } }
            set { lock (SyncRoot) { Instance.MinimumNumberOfSockets = value; } }
        }

        public System.Int32 MaximumNumberOfSockets
        {
            get { lock (SyncRoot) { return Instance.MaximumNumberOfSockets; } }
            set { lock (SyncRoot) { Instance.MaximumNumberOfSockets = value; } }
        }

        public Microsoft.Hpc.Scheduler.Properties.JobUnitType UnitType
        {
            get { lock (SyncRoot) { return Instance.UnitType; } }
            set { lock (SyncRoot) { Instance.UnitType = value; } }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection RequestedNodes
        {
            get { lock (SyncRoot) { return Instance.RequestedNodes; } }
            set { lock (SyncRoot) { Instance.RequestedNodes = value; } }
        }

        public System.Boolean IsExclusive
        {
            get { lock (SyncRoot) { return Instance.IsExclusive; } }
            set { lock (SyncRoot) { Instance.IsExclusive = value; } }
        }

        public System.Boolean RunUntilCanceled
        {
            get { lock (SyncRoot) { return Instance.RunUntilCanceled; } }
            set { lock (SyncRoot) { Instance.RunUntilCanceled = value; } }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection NodeGroups
        {
            get { lock (SyncRoot) { return Instance.NodeGroups; } }
            set { lock (SyncRoot) { Instance.NodeGroups = value; } }
        }

        public System.Boolean FailOnTaskFailure
        {
            get { lock (SyncRoot) { return Instance.FailOnTaskFailure; } }
            set { lock (SyncRoot) { Instance.FailOnTaskFailure = value; } }
        }

        public System.Boolean AutoCalculateMax
        {
            get { lock (SyncRoot) { return Instance.AutoCalculateMax; } }
            set { lock (SyncRoot) { Instance.AutoCalculateMax = value; } }
        }

        public System.Boolean AutoCalculateMin
        {
            get { lock (SyncRoot) { return Instance.AutoCalculateMin; } }
            set { lock (SyncRoot) { Instance.AutoCalculateMin = value; } }
        }

        public System.Boolean CanGrow
        {
            get { lock (SyncRoot) { return Instance.CanGrow; } }
        }

        public System.Boolean CanShrink
        {
            get { lock (SyncRoot) { return Instance.CanShrink; } }
        }

        public System.Boolean CanPreempt
        {
            get { lock (SyncRoot) { return Instance.CanPreempt; } }
            set { lock (SyncRoot) { Instance.CanPreempt = value; } }
        }

        public System.String ErrorMessage
        {
            get { lock (SyncRoot) { return Instance.ErrorMessage; } }
        }

        public System.Boolean HasRuntime
        {
            get { lock (SyncRoot) { return Instance.HasRuntime; } }
        }

        public System.Int32 RequeueCount
        {
            get { lock (SyncRoot) { return Instance.RequeueCount; } }
        }

        public System.Int32 MinMemory
        {
            get { lock (SyncRoot) { return Instance.MinMemory; } }
            set { lock (SyncRoot) { Instance.MinMemory = value; } }
        }

        public System.Int32 MaxMemory
        {
            get { lock (SyncRoot) { return Instance.MaxMemory; } }
            set { lock (SyncRoot) { Instance.MaxMemory = value; } }
        }

        public System.Int32 MinCoresPerNode
        {
            get { lock (SyncRoot) { return Instance.MinCoresPerNode; } }
            set { lock (SyncRoot) { Instance.MinCoresPerNode = value; } }
        }

        public System.Int32 MaxCoresPerNode
        {
            get { lock (SyncRoot) { return Instance.MaxCoresPerNode; } }
            set { lock (SyncRoot) { Instance.MaxCoresPerNode = value; } }
        }

        public Microsoft.Hpc.Scheduler.IStringCollection SoftwareLicense
        {
            get { lock (SyncRoot) { return Instance.SoftwareLicense; } }
            set { lock (SyncRoot) { Instance.SoftwareLicense = value; } }
        }

        public System.String ClientSource
        {
            get { lock (SyncRoot) { return Instance.ClientSource; } }
            set { lock (SyncRoot) { Instance.ClientSource = value; } }
        }

        /// <remarks>
        /// Returns a new collection of immutable objects. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.INameValueCollection EnvironmentVariables
        {
            get { lock (SyncRoot) { return Instance.EnvironmentVariables; } }
        }

        public System.Int32 Progress
        {
            get { lock (SyncRoot) { return Instance.Progress; } }
            set { lock (SyncRoot) { Instance.Progress = value; } }
        }

        public System.String ProgressMessage
        {
            get { lock (SyncRoot) { return Instance.ProgressMessage; } }
            set { lock (SyncRoot) { Instance.ProgressMessage = value; } }
        }

        public System.Int32 TargetResourceCount
        {
            get { lock (SyncRoot) { return Instance.TargetResourceCount; } }
            set { lock (SyncRoot) { Instance.TargetResourceCount = value; } }
        }

        public System.Int32 ExpandedPriority
        {
            get { lock (SyncRoot) { return Instance.ExpandedPriority; } }
            set { lock (SyncRoot) { Instance.ExpandedPriority = value; } }
        }

        public System.String ServiceName
        {
            get { lock (SyncRoot) { return Instance.ServiceName; } }
            set { lock (SyncRoot) { Instance.ServiceName = value; } }
        }

        public System.DateTime HoldUntil
        {
            get { lock (SyncRoot) { return Instance.HoldUntil; } }
        }

        public System.Boolean NotifyOnStart
        {
            get { lock (SyncRoot) { return Instance.NotifyOnStart; } }
            set { lock (SyncRoot) { Instance.NotifyOnStart = value; } }
        }

        public System.Boolean NotifyOnCompletion
        {
            get { lock (SyncRoot) { return Instance.NotifyOnCompletion; } }
            set { lock (SyncRoot) { Instance.NotifyOnCompletion = value; } }
        }

        public System.String EmailAddress
        {
            get { lock (SyncRoot) { return Instance.EmailAddress; } }
            set { lock (SyncRoot) { Instance.EmailAddress = value; } }
        }

        public System.String Pool
        {
            get { lock (SyncRoot) { return Instance.Pool; } }
        }

        public System.String ValidExitCodes
        {
            get { lock (SyncRoot) { return Instance.ValidExitCodes; } }
            set { lock (SyncRoot) { Instance.ValidExitCodes = value; } }
        }

        public Microsoft.Hpc.Scheduler.IIntCollection ParentJobIds
        {
            get { lock (SyncRoot) { return Instance.ParentJobIds; } }
            set { lock (SyncRoot) { Instance.ParentJobIds = value; } }
        }

        public Microsoft.Hpc.Scheduler.IIntCollection ChildJobIds
        {
            get { lock (SyncRoot) { return Instance.ChildJobIds; } }
        }

        public System.Boolean FailDependentTasks
        {
            get { lock (SyncRoot) { return Instance.FailDependentTasks; } }
            set { lock (SyncRoot) { Instance.FailDependentTasks = value; } }
        }

        public bool SingleNode
        {
            get { lock (SyncRoot) { return Instance.SingleNode; } }
            set { lock (SyncRoot) { Instance.SingleNode = value; } }
        }

        public JobNodeGroupOp NodeGroupOp
        {
            get { lock (SyncRoot) { return Instance.NodeGroupOp; } }
            set { lock (SyncRoot) { Instance.NodeGroupOp = value; } }
        }


        public int EstimatedProcessMemory
        {
            get { lock (SyncRoot) { return Instance.EstimatedProcessMemory; } }
            set { lock (SyncRoot) { Instance.EstimatedProcessMemory = value; } }
        }

        public Microsoft.Hpc.Scheduler.Properties.JobRuntimeType RuntimeType
        {
            get { lock (SyncRoot) { return Instance.RuntimeType; } }
            set { lock (SyncRoot) { Instance.RuntimeType = value; } }
        }

        /// <remarks>
        /// Returns a new collection of immutable strings. No worries for concurrency.
        /// </remarks>
        public Microsoft.Hpc.Scheduler.IStringCollection ExcludedNodes
        {
            get { lock (SyncRoot) { return Instance.ExcludedNodes; } }
        }

        public event EventHandler<JobStateEventArg> OnJobState
        {
            add { lock (SyncRoot) { Instance.OnJobState += value; } }
            remove { lock (SyncRoot) { Instance.OnJobState -= value; } }
        }

        public event EventHandler<TaskStateEventArg> OnTaskState
        {
            add { lock (SyncRoot) { Instance.OnTaskState += value; } }
            remove { lock (SyncRoot) { Instance.OnTaskState -= value; } }
        }

        public ISchedulerRowEnumerator OpenJobAllocationHistoryEnumerator(IPropertyIdCollection properties)
        {
            lock (SyncRoot)
            {
                return Instance.OpenJobAllocationHistoryEnumerator(properties);
            }
        }

        public ISchedulerRowEnumerator OpenTaskAllocationHistoryEnumerator(IPropertyIdCollection properties)
        {
            lock (SyncRoot)
            {
                return Instance.OpenTaskAllocationHistoryEnumerator(properties);
            }
        }

        public int PlannedCoreCount
        {
            get
            {
                lock (SyncRoot)
                {
                    return Instance.PlannedCoreCount;
                }
            }
        }

        public void FinishTask(ITaskId taskId, string message)
        {
            lock (SyncRoot)
            {
                this.Instance.FinishTask(taskId, message);
            }
        }

        public int TaskExecutionFailureRetryLimit
        {
            get
            {
                lock (SyncRoot)
                {
                    return this.Instance.TaskExecutionFailureRetryLimit;
                }
            }

            set
            {
                lock (SyncRoot)
                {
                    this.Instance.TaskExecutionFailureRetryLimit = value;
                }
            }
        }

#endregion
    }
#endif
}

