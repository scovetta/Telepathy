using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace EchoSvcSample
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.Batch;
    using Microsoft.Azure.Batch.Auth;
    using Microsoft.Azure.Batch.Common;
    using Microsoft.Extensions.Configuration;

    class Program
    {
        const string OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine =
            @"cmd /c ""sc.exe config NetTcpPortSharing start= demand & reg ADD ^""HKLM\Software\Microsoft\StrongName\Verification\*,*^"" /f & reg ADD ^""HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^"" /f""";

        private const string AzureBatchTaskWorkingDirEnvVar = "%AZ_BATCH_TASK_WORKING_DIR%";
        private const string RuntimeContainer = "runtime";
        private const string SessionLauncherFolder = "session-launcher";


        private static Func<string> GetStorageConnectionString;


        static async Task Main(string[] args)
        {
            const string poolId = "EchoSvcSamplePool";
            const string jobId = "SessionLauncherJob";
            const string taskId = "SessionLauncherTask";
            const int numberOfNodes = 1;


            TimeSpan timeout = TimeSpan.FromMinutes(30);

            AccountSettings accountSettings = LoadAccountSettings();
            GetStorageConnectionString = () => accountSettings.BrokerStorageConnectionString;

            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(accountSettings.BatchServiceUrl, accountSettings.BatchAccountName, accountSettings.BatchAccountKey);


            using (BatchClient batchClient = BatchClient.Open(cred))
            {
                // await CreatePoolAsync(batchClient, poolId, numberOfNodes);
                // await CreateJobAsync(batchClient, jobId, poolId);

                CloudTask CreateTask()
                {
                    List<ResourceFile> resourceFiles = new List<ResourceFile>();
                    resourceFiles.Add(GetResourceFileReference(accountSettings.BrokerStorageConnectionString, RuntimeContainer, SessionLauncherFolder));
                    string sessionLauncherParameters =
                        $"--AzureBatchServiceUrl {accountSettings.BatchServiceUrl} --AzureBatchAccountName {accountSettings.BatchAccountName} --AzureBatchAccountKey {accountSettings.BatchAccountKey} --AzureBatchPoolName {poolId} -c {accountSettings.BrokerStorageConnectionString} --SessionLauncherStorageConnectionString {accountSettings.BrokerStorageConnectionString}";

                    CloudTask cloudTask = new CloudTask(taskId, $@"cmd /c {AzureBatchTaskWorkingDirEnvVar}\{SessionLauncherFolder}\HpcSession.exe -d " + sessionLauncherParameters);
                    cloudTask.ResourceFiles = resourceFiles;
                    cloudTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                    return cloudTask;
                }

                Console.WriteLine($"Adding task [{taskId}] to job [{jobId}]...");
                await batchClient.JobOperations.AddTaskAsync(jobId, CreateTask());

                CloudTask mainTask = await batchClient.JobOperations.GetTaskAsync(jobId, taskId);
                Console.WriteLine($"Awaiting task completion, timeout in {timeout}...");
            }
        }

        private static AccountSettings LoadAccountSettings()
        {
            AccountSettings accountSettings = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("accountsettings.json").Build().Get<AccountSettings>();
            return accountSettings;
        }

        /// <summary>
        /// Creates a pool of "small" Windows Server 2012 R2 compute nodes in the Batch service with the specified configuration.
        /// </summary>
        /// <param name="batchClient">The client to use for creating the pool.</param>
        /// <param name="poolId">The id of the pool to create.</param>
        /// <param name="numberOfNodes">The target number of compute nodes for the pool.</param>
        /// <param name="appPackageId">The id of the application package to install on the compute nodes.</param>
        /// <param name="appPackageVersion">The application package version to install on the compute nodes.</param>
        private static async Task CreatePoolAsync(BatchClient batchClient, string poolId, int numberOfNodes)
        {
            // Create the unbound pool. Until we call CloudPool.Commit() or CommitAsync(),
            // the pool isn't actually created in the Batch service. This CloudPool instance
            // is therefore considered "unbound," and we can modify its properties.
            Console.WriteLine($"Creating pool [{poolId}]...");
            CloudPool unboundPool = batchClient.PoolOperations.CreatePool(
                poolId: poolId,
                virtualMachineSize: "standard_d1_v2",
                targetDedicatedComputeNodes: numberOfNodes,
                cloudServiceConfiguration: new CloudServiceConfiguration(osFamily: "5"));

            unboundPool.InterComputeNodeCommunicationEnabled = true;

            // REQUIRED for multi-instance tasks
            unboundPool.MaxTasksPerComputeNode = 1;

            // Commit the fully configured pool to the Batch service to actually create
            // the pool and its compute nodes.
            await unboundPool.CommitAsync();
        }

        /// <summary>
        /// Creates a job in Batch service with the specified id and associated with the specified pool.
        /// </summary>
        /// <param name="batchClient"></param>
        /// <param name="jobId"></param>
        /// <param name="poolId"></param>
        private static async Task CreateJobAsync(BatchClient batchClient, string jobId, string poolId)
        {
            // Create the job to which the multi-instance task will be added.
            Console.WriteLine($"Creating job [{jobId}]...");
            CloudJob unboundJob = batchClient.JobOperations.CreateJob(jobId, new PoolInformation() {PoolId = poolId});
            unboundJob.JobPreparationTask = new JobPreparationTask(
                OpenNetTcpPortSharingAndDisableStrongNameValidationCmdLine);
            unboundJob.JobPreparationTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
            await unboundJob.CommitAsync();
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

        public static string ConstructContainerSas(string storageConnectionString, string containerName,
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            return ConstructContainerSas(storageAccount, containerName, permissions);
        }

        public static string ConstructContainerSas(CloudStorageAccount cloudStorageAccount, string containerName,
            SharedAccessBlobPermissions permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Read)
        {
            containerName = containerName.ToLower();

            CloudBlobClient client = cloudStorageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = client.GetContainerReference(containerName);

            DateTimeOffset sasStartTime = DateTime.UtcNow;
            TimeSpan sasDuration = TimeSpan.FromHours(2);
            DateTimeOffset sasEndTime = sasStartTime.Add(sasDuration);

            SharedAccessBlobPolicy sasPolicy = new SharedAccessBlobPolicy() {Permissions = permissions, SharedAccessExpiryTime = sasEndTime};

            string sasString = container.GetSharedAccessSignature(sasPolicy);
            return string.Format("{0}{1}", container.Uri, sasString);
        }
    }
}