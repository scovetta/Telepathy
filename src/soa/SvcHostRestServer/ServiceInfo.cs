namespace Microsoft.Hpc.RESTServiceModel
{
    using System;
    using System.Collections.Generic;

    public class ServiceInfo
    {
        public ServiceInfo()
        {
        }

        public ServiceInfo(string jobId, string taskId, int coreId, string registrationPath, string fileName, Dictionary<string, string> environment, Dictionary<string, string> dependFiles)
        {
            this.JobId = jobId;
            this.TaskId = taskId;
            this.CoreId = coreId;
            this.RegistrationPath = registrationPath;
            this.FileName = fileName;
            this.Environment = environment;
            if (dependFiles != null)
            {
                this.DependFiles = dependFiles;
            }
        }

        public string JobId { get; set; }

        public string TaskId { get; set; }

        public int CoreId { get; set; }

        public string RegistrationPath { get; set; }

        public string FileName { get; set; }

        public Dictionary<string, string> Environment { get; set; }

        public Dictionary<string, string> DependFiles { get; set; } = new Dictionary<string, string>();

        public bool Equals(ServiceInfo temp)
        {
            if (temp == null)
            {
                return false;
            }

            if (this.JobId == temp.JobId
                && this.CoreId == temp.CoreId
                && this.TaskId == temp.TaskId
                && this.FileName.Equals(temp.FileName, StringComparison.OrdinalIgnoreCase)
                && this.RegistrationPath.Equals(temp.RegistrationPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static readonly object s_lock = new object();

        public static bool SaveInfo(ServiceInfo serviceInfo)
        {
            if (SvcHostMgmtRestServer.Info != null)
            {
                return false;
            }

            lock (s_lock)
            {
                if (SvcHostMgmtRestServer.Info == null)
                {
                    SvcHostMgmtRestServer.Info = serviceInfo;
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static void DeleteInfo()
        {
            if (SvcHostMgmtRestServer.Info == null)
            {
                return;
            }

            lock (s_lock)
            {
                if (SvcHostMgmtRestServer.Info != null)
                {
                    SvcHostMgmtRestServer.Info = null;
                }
            }
        }
    }
}