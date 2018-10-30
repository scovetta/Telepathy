namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.QueueAdapter.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    public class BrokerLauncherCloudQueueCmdTypeBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return this.ParameterTypes.SingleOrDefault(t => t.Name == typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public IList<Type> ParameterTypes { get; }
    }
}