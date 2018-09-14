using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Azure.Common
{
    
    internal enum StorageOperation
    {
        CreateNew,
        CreateIfNotExist,
    }

    internal enum StorageType
    {
        Queue,
        Blob,
        Table,
    }

    internal class StorageRequest
    {
        public StorageType Type;
        public StorageOperation Op;
        public string Name;

        public StorageRequest(StorageType type, StorageOperation op, string name)
        {
            Type = type;
            Op = op;
            Name = name;
        }

        public string GetDeployedName(string clusterName, Guid subscriptionId, string serviceName)
        {
            return AzureNaming.GenerateAzureEntityName(Name, clusterName, subscriptionId, serviceName);
        }
    }

    internal static class StorageEntityList
    {
        public static StorageRequest[] List = new StorageRequest[]
            {
                // Scheduler storages 
                new StorageRequest(StorageType.Queue, StorageOperation.CreateNew, SchedulerQueueNames.NodeMessageQueue),
                new StorageRequest(StorageType.Table, StorageOperation.CreateNew, SchedulerTableNames.HeartBeats),
                new StorageRequest(StorageType.Table, StorageOperation.CreateNew, SchedulerTableNames.NodeMapping),
                new StorageRequest(StorageType.Table, StorageOperation.CreateNew, SchedulerTableNames.Counters),
                new StorageRequest(StorageType.Table, StorageOperation.CreateNew, AzureMetricsConstants.AzureMetricsTableName),
                
                // TODO: Add more

                // TroubleShootingService storages
                new StorageRequest(StorageType.Table, StorageOperation.CreateNew, TroubleShootingServiceTableNames.RepositorySas)
            };
    }
}
