namespace Microsoft.Hpc
{
    using System;
    using System.Linq;
    using System.Threading;

    public class SqlConnectionStringProvider
    {
        private const string ConnectionStringProviderName = "SqlConnectionStringProvider.dll";

        private static readonly Lazy<ISqlConnectionStringProvider> Instance =
            new Lazy<ISqlConnectionStringProvider>(
                () => PluginUtil.CreateInstances<ISqlConnectionStringProvider>(ConnectionStringProviderName).SingleOrDefault(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static ISqlConnectionStringProvider Provider => Instance.Value;
    }
}
