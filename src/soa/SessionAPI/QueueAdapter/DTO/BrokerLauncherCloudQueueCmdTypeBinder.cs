namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    using Microsoft.Hpc.Scheduler.Session.Interface;

    public class BrokerLauncherCloudQueueCmdTypeBinder : SerializationBinder
    {
        public static BrokerLauncherCloudQueueCmdTypeBinder Default => new BrokerLauncherCloudQueueCmdTypeBinder()
                                                                          {
                                                                              ParameterTypes = new List<Type>()
                                                                                                   {
                                                                                                       typeof(SessionStartInfoContract),
                                                                                                       typeof(BrokerInitializationResult),
                                                                                                       typeof(object[]),
                                                                                                       typeof(int[]),
                                                                                                       typeof(string[]),
                                                                                                       typeof(CloudQueueCmdDto),
                                                                                                       typeof(CloudQueueResponseDto),
                                                                                                       typeof(Dictionary<string, string>),
                                                                                                       typeof(Version)
                                                                                                   }
                                                                          };

        public override Type BindToType(string assemblyName, string typeName)
        {
            return this.ParameterTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public IList<Type> ParameterTypes { get; set; }
    }
}