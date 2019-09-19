// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter
{
    using System.Collections.Generic;

    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO;
    using Microsoft.Hpc.Scheduler.Session.QueueAdapter.Interface;

    using Newtonsoft.Json;

    public class CloudQueueSerializer : IQueueSerializer
    {
        private readonly JsonSerializerSettings setting = null;

        public CloudQueueSerializer(CloudQueueCmdTypeBinder binder, List<JsonConverter> converters)
        {
            this.setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Binder = binder, Converters = converters };
        }

        public CloudQueueSerializer(CloudQueueCmdTypeBinder binder) : this(binder, null)
        {
        }

        public CloudQueueSerializer() 
        {
            this.setting = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }

        public string Serialize<T>(T item) => JsonConvert.SerializeObject(item, this.setting);

        public T Deserialize<T>(string item) => JsonConvert.DeserializeObject<T>(item, this.setting);
    }
}