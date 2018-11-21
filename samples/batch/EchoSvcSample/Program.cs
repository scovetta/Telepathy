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
        static async Task Main(string[] args)
        {
            const string poolId = "EchoSvcSamplePool";
            const string jobId = "EchoSvcSampleJob";
            const string taskId = "EchoSvcSampleTask";
            const int numberOfNodes = 1;

            const string appPackageId = "EchoSvcSample";
            const string appPackageVersion = "1.0";

            TimeSpan timeout = TimeSpan.FromMinutes(30);

            AccountSettings accountSettings = LoadAccountSettings();
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(accountSettings.BatchServiceUrl, accountSettings.BatchAccountName, accountSettings.BatchAccountKey);

            using (BatchClient batchClient = BatchClient.Open(cred))
            {
                await CreatePoolAsync(batchClient, poolId, numberOfNodes, appPackageId, appPackageVersion);
                await CreateJobAsync(batchClient, jobId, poolId);

                CloudTask multiInstanceTask = new CloudTask(
                    id: taskId,
                    commandline:
                    $@"cmd /c %AZ_BATCH_APP_PACKAGE_{appPackageId.ToUpper()}#{appPackageVersion}%\BrokerOutput\HpcBroker.exe -d -CCP_SERVICEREGISTRATION_PATH %AZ_BATCH_APP_PACKAGE_{appPackageId.ToUpper()}#{appPackageVersion}%\Registration -AzureStorageConnectionString ***REMOVED*** -EnableAzureStorageQueueEndpoint True");
                multiInstanceTask.UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin, scope: AutoUserScope.Task));
                multiInstanceTask.MultiInstanceSettings = new MultiInstanceSettings($@"cmd /c start cmd /c %AZ_BATCH_APP_PACKAGE_{appPackageId.ToUpper()}#{appPackageVersion}%\CcpServiceHost\CcpServiceHost.exe -standalone", numberOfNodes);

                Console.WriteLine($"Adding task [{taskId}] to job [{jobId}]...");
                await batchClient.JobOperations.AddTaskAsync(jobId, multiInstanceTask);

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
        private static async Task CreatePoolAsync(BatchClient batchClient, string poolId, int numberOfNodes, string appPackageId, string appPackageVersion)
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

            // REQUIRED for communication between the MS-MPI processes (in this
            // sample, MPIHelloWorld.exe) running on the different nodes
            unboundPool.InterComputeNodeCommunicationEnabled = true;

            // REQUIRED for multi-instance tasks
            unboundPool.MaxTasksPerComputeNode = 1;

            // Specify the application and version to deploy to the compute nodes.
            unboundPool.ApplicationPackageReferences = new List<ApplicationPackageReference> { new ApplicationPackageReference { ApplicationId = appPackageId, Version = appPackageVersion } };

            // Create a StartTask for the pool that we use to install MS-MPI on the nodes
            // as they join the pool.
            StartTask startTask = new StartTask
                                      {
                                          // CommandLine = $"cmd /c %AZ_BATCH_APP_PACKAGE_{appPackageId.ToUpper()}#{appPackageVersion}%\\MSMpiSetup.exe -unattend -force",
                                          CommandLine = @"cmd /c ""sc.exe config NetTcpPortSharing start= demand & reg ADD ^""HKLM\Software\Microsoft\StrongName\Verification\*,*^"" /f & reg ADD ^""HKLM\Software\Wow6432Node\Microsoft\StrongName\Verification\*,*^"" /f""",
                                          UserIdentity = new UserIdentity(new AutoUserSpecification(elevationLevel: ElevationLevel.Admin)),
                                          WaitForSuccess = true
                                      };
            unboundPool.StartTask = startTask;

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
            CloudJob unboundJob = batchClient.JobOperations.CreateJob(jobId, new PoolInformation() { PoolId = poolId });
            await unboundJob.CommitAsync();
        }
    }
}