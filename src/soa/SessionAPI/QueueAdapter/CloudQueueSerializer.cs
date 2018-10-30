namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;

    using Newtonsoft.Json;

    public class CloudQueueSerializer
    {
        private readonly JsonSerializerSettings setting = null;

        public CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder binder, List<JsonConverter> converters)
        {
            this.setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Binder = binder, Converters = converters};
        }

        public CloudQueueSerializer(BrokerLauncherCloudQueueCmdTypeBinder binder) : this(binder, null)
        {
        }

        public string Serialize<T>(T item) => JsonConvert.SerializeObject(item, this.setting);

        public T Deserialize<T>(string item) => JsonConvert.DeserializeObject<T>(item, this.setting);
    }
}