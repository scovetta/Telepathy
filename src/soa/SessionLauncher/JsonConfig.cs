
namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class JsonConfig
    {
        public string AzureBatchServiceUrl;

        public string AzureBatchAccountName;

        public string AzureBatchAccountKey;

        public string AzureBatchPoolName;

        public string AzureStorageConnectionString;

        public string BrokerLauncherExePath;

        public string SessionLauncherStorageConnectionString;

    }
}
