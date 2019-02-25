namespace Microsoft.Hpc.Scheduler.Session.SchedulerAdapter.HpcPack
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session.SchedulerPort;

    public class HpcPackSchedulerAdapter : IScheduler
    {
        private Scheduler schedulerImplementation;

        public void Connect(string cluster)
        {
            this.schedulerImplementation.Connect(cluster);
        }

        public void SetInterfaceMode(bool isConsole, IntPtr hwnd)
        {
            this.schedulerImplementation.SetInterfaceMode(isConsole, hwnd);
        }

        public ISchedulerJob CreateJob()
        {
            return this.schedulerImplementation.CreateJob();
        }

        public ISchedulerJob OpenJob(int id)
        {
            return this.schedulerImplementation.OpenJob(id);
        }

        public ISchedulerJob CloneJob(int jobId)
        {
            return this.schedulerImplementation.CloneJob(jobId);
        }

        public void AddJob(ISchedulerJob job)
        {
            this.schedulerImplementation.AddJob(job);
        }

        public void SubmitJob(ISchedulerJob job, string username, string password)
        {
            this.schedulerImplementation.SubmitJob(job, username, password);
        }

        public void SubmitJobById(int jobId, string username, string password)
        {
            this.schedulerImplementation.SubmitJobById(jobId, username, password);
        }

        public void CancelJob(int jobId, string message)
        {
            this.schedulerImplementation.CancelJob(jobId, message);
        }

        public void ConfigureJob(int jobId)
        {
            this.schedulerImplementation.ConfigureJob(jobId);
        }

        public string CreateTaskId(int jobTaskId)
        {
            return this.schedulerImplementation.CreateTaskId(jobTaskId);
        }

        public string CreateParametricTaskId(int jobTaskId, int instanceId)
        {
            return this.schedulerImplementation.CreateParametricTaskId(jobTaskId, instanceId);
        }

        public void SetEnvironmentVariable(string name, string value)
        {
            this.schedulerImplementation.SetEnvironmentVariable(name, value);
        }

        public IDictionary<string, string> EnvironmentVariables => this.schedulerImplementation.EnvironmentVariables;

        public void SetClusterParameter(string name, string value)
        {
            this.schedulerImplementation.SetClusterParameter(name, value);
        }

        public IDictionary<string, string> ClusterParameters => this.schedulerImplementation.ClusterParameters;

        public ICollection<string> GetJobTemplateList()
        {
            return this.schedulerImplementation.GetJobTemplateList();
        }

        public ICollection<string> GetNodeGroupList()
        {
            return this.schedulerImplementation.GetNodeGroupList();
        }

        public ICollection<string> GetNodesInNodeGroup(string nodeGroup)
        {
            return this.schedulerImplementation.GetNodesInNodeGroup(nodeGroup);
        }

        public IDictionary<string, string> CreateNameValueCollection()
        {
            return this.schedulerImplementation.CreateNameValueCollection();
        }

        public ICollection<string> CreateStringCollection()
        {
            return this.schedulerImplementation.CreateStringCollection();
        }

        public void DeleteCachedCredentials(string userName)
        {
            this.schedulerImplementation.DeleteCachedCredentials(userName);
        }

        public void SetCachedCredentials(string userName, string password)
        {
            this.schedulerImplementation.SetCachedCredentials(userName, password);
        }

        public void Close()
        {
            this.schedulerImplementation.Close();
        }

        public void CancelJob(int jobId, string message, bool isForced)
        {
            this.schedulerImplementation.CancelJob(jobId, message, isForced);
        }

        public void DeletePool(string poolName)
        {
            this.schedulerImplementation.DeletePool(poolName);
        }

        public void DeletePool(string poolName, bool force)
        {
            this.schedulerImplementation.DeletePool(poolName, force);
        }

        public void SetCertificateCredentials(string userName, string thumbprint)
        {
            this.schedulerImplementation.SetCertificateCredentials(userName, thumbprint);
        }

        public void SetCertificateCredentialsPfx(string userName, string pfxPassword, byte[] certBytes)
        {
            this.schedulerImplementation.SetCertificateCredentialsPfx(userName, pfxPassword, certBytes);
        }

        public string EnrollCertificate(string templateName)
        {
            return this.schedulerImplementation.EnrollCertificate(templateName);
        }

        public string GetActiveHeadNode()
        {
            return this.schedulerImplementation.GetActiveHeadNode();
        }

        public void SetEmailCredentials(string userName, string password)
        {
            this.schedulerImplementation.SetEmailCredentials(userName, password);
        }

        public void DeleteEmailCredentials()
        {
            this.schedulerImplementation.DeleteEmailCredentials();
        }

        public void RequeueJob(int jobId)
        {
            this.schedulerImplementation.RequeueJob(jobId);
        }

        public void CancelJob(int jobId, string message, bool isForced, bool isGraceful)
        {
            this.schedulerImplementation.CancelJob(jobId, message, isForced, isGraceful);
        }

        public void FinishJob(int jobId, string message, bool isForced, bool isGraceful)
        {
            this.schedulerImplementation.FinishJob(jobId, message, isForced, isGraceful);
        }

        public void DeleteJob(int jobId)
        {
            this.schedulerImplementation.DeleteJob(jobId);
        }
    }
}
