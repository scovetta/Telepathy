namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter
{
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter.DTO;

    using Newtonsoft.Json;

    public class BrokerLauncherCloudQueueSerializer
    {
        private JsonSerializerSettings setting = null;

        public BrokerLauncherCloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder binder, List<JsonConverter> converters = null)
        {
            this.setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Binder = binder, Converters = converters};
        }

        public string Serialize<T>(T item) => JsonConvert.SerializeObject(item, this.setting);

        public T Deserialize<T>(string item) => JsonConvert.DeserializeObject<T>(item, this.setting);
    }
}