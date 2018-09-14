using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    public class AzureDeploymentPropertyIds
    {
        /// <summary>
        ///   <para />
        /// </summary>
        protected AzureDeploymentPropertyIds()
        {
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId DeploymentId
        {
            get { return _DeploymentId; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId SubscriptionId
        {
            get { return _SubscriptionId; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId ServiceName
        {
            get { return _ServiceName; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId StorageConnectionString
        {
            get { return _StorageConnectionString; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId ProxyMultiplicity
        {
            get { return _ProxyMultiplicity; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId ProxyAddress
        {
            get { return _ProxyAddress; }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="Microsoft.Hpc.Scheduler.Properties.PropertyId" />.</para>
        /// </value>
        public static PropertyId EncryptedStorageConnectionString
        {
            get { return _EncryptedStorageConnectionString; }
        }

        static PropertyId _DeploymentId = new PropertyId(StorePropertyType.String, "DeploymentId", PropertyIdConstants.AzureDeploymentPropertyIdStart + 1);
        static PropertyId _SubscriptionId = new PropertyId(StorePropertyType.String, "SubscriptionId", PropertyIdConstants.AzureDeploymentPropertyIdStart + 2);
        static PropertyId _ServiceName = new PropertyId(StorePropertyType.String, "ServiceName", PropertyIdConstants.AzureDeploymentPropertyIdStart + 3);
        static PropertyId _StorageConnectionString = new PropertyId(StorePropertyType.String, "StorageConnectionString ", PropertyIdConstants.AzureDeploymentPropertyIdStart + 4);
        static PropertyId _ProxyMultiplicity = new PropertyId(StorePropertyType.Int32, "ProxyMultiplicity", PropertyIdConstants.AzureDeploymentPropertyIdStart + 5);
        static PropertyId _ProxyAddress = new PropertyId(StorePropertyType.String, "ProxyAddress ", PropertyIdConstants.AzureDeploymentPropertyIdStart + 6);
        static PropertyId _EncryptedStorageConnectionString = new PropertyId(StorePropertyType.Binary, "EncryptedStorageConnectionString", PropertyIdConstants.AzureDeploymentPropertyIdStart + 7);
    }
}
