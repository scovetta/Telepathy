namespace AzureBatchAdminCli.Session
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Auth;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Extensions.Configuration;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    internal class SessionLauncherStarter
    {
        const string OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine =
            @"cmd /c ""sc.exe config NetTcpPortSharing start= demand & reg ADD ^""HKLM\Software\Microsoft\StrongName\Verification\*,*^"" /f & reg ADD ^""HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^"" /f""";

        private const string AzureBatchTaskWorkingDirEnvVar = "%AZ_BATCH_TASK_WORKING_DIR%";

        private const string RuntimeContainer = "runtime";

        private const string SessionLauncherFolder = "session-launcher";

        private const string SessionLauncherJobId = "SessionLauncherJob";

        private const string SessionLauncherTaskId = "SessionLauncherTask";

        private AccountSettings accountSettings = LoadAccountSettings();

        private static AccountSettings LoadAccountSettings()
        {
            AccountSettings accountSettings = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("accountsettings.json").Build().Get<AccountSettings>();
            return accountSettings;
        }

        public async Task StartAsync()
        {
            TimeSpan timeout = TimeSpan.FromMinutes(30);

            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(this.accountSettings.BatchServiceUrl, this.accountSettings.BatchAccountName, this.accountSettings.BatchAccountKey);

            using (BatchClient batchClient = BatchClient.Open(cred))
            {
                await CreateJobAsync(batchClient, SessionLauncherJobId, this.accountSettings.PoolId);
                CloudTask CreateTask()
                {
                    List<ResourceFile> resourceFiles = new List<ResourceFile>();
                    resourceFiles.Add(GetResourceFileReference(this.accountSettings.StorageConnectionString, RuntimeContainer, SessionLauncherFolder));
                    string sessionLauncherParameters =
                        $"--AzureBatchServiceUrl {this.accountSettings.BatchServiceUrl} --AzureBatchAccountName {this.accountSettings.BatchAccountName} --AzureBatchAccountKey {this.accountSettings.BatchAccountKey} --AzureBatchPoolName {this.accountSettings.PoolId} -c {this.accountSettings.StorageConnectionString} --SessionLauncherStorageConnectionString {this.accountSettings.StorageConnectionString}";

                    CloudTask cloudTask = new CloudTask(SessionLauncherTaskId, $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\{SessionLauncherFolder}\HpcSession.exe -d " + sessionLauncherParameters);
                    cloudTask.ResourceFiles = resourceFiles;
                    cloudTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                    return cloudTask;
                }

                Console.WriteLine($"Adding task [{SessionLauncherTaskId}] to job [{SessionLauncherJobId}]...");
                await batchClient.JobOperations.AddTaskAsync(SessionLauncherJobId, CreateTask());

                CloudTask mainTask = await batchClient.JobOperations.GetTaskAsync(SessionLauncherJobId, SessionLauncherTaskId);
                Console.WriteLine($"Awaiting task completion, timeout in {timeout}...");
            }
        }

        static ResourceFile GetResourceFileReference(string cloudStorageConnectionString, string containerName, string blobPrefix)
        {
            var sasToken = ConstructContainerSas(cloudStorageConnectionString, containerName);
            ResourceFile rf;
            if (string.IsNullOrEmpty(blobPrefix))
            {
                rf = ResourceFile.FromStorageContainerUrl(sasToken);
            }
            else
            {
                rf = ResourceFile.FromStorageContainerUrl(sasToken, blobPrefix: blobPrefix);
            }

            return rf;
        }

        public static string ConstructContainerSas(
            string storageConnectionString,
            string containerName,
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            return ConstructContainerSas(storageAccount, containerName, permissions);
        }

        public static string ConstructContainerSas(
            CloudStorageAccount cloudStorageAccount,
            string containerName,
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read)
        {
            containerName = containerName.ToLower();

            CloudBlobClient client = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            DateTimeOffset sasStartTime = DateTime.UtcNow;
            TimeSpan sasDuration = TimeSpan.FromHours(2);
            DateTimeOffset sasEndTime = sasStartTime.Add(sasDuration);

            SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy() { Permissions = permissions, SharedAccessExpiryTime = sasEndTime };

            string sasString = container.GetSharedAccessSignature(sasPolicy);
            return $"{container.Uri}{sasString}";
        }

        private static async Task CreateJobAsync(BatchClient batchClient, string jobId, string poolId)
        {
            Console.WriteLine($"Creating job [{jobId}]...");
            CloudJob unboundJob = batchClient.JobOperations.CreateJob(jobId, new PoolInformation() { PoolId = poolId });
            unboundJob.JobPreparationTask = new JobPreparationTask(
                OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine);
            unboundJob.JobPreparationTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
            await unboundJob.CommitAsync();
        }
    }
}