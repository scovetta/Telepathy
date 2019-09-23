// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Microsoft.WindowsAzure.Storage.Table.Protocol;

    /// <summary>
    /// This is the interface for proxy modules to access the logical node name -> physial endpoint mapping.
    /// The user has to manually control the cache refreshing.
    /// This is not a static class, so every object has its own mapping cache.
    /// Only the scheduler should call the method UpdateNodeMapping. Other modules should only read.
    /// </summary>
    internal partial class NodeMapping
    {

        /// <summary>
        /// Save the name of the node mapping table
        /// </summary>
        string _mappingTableName = null;

        /// <summary>
        /// Save the time at which the table was last modified
        /// </summary>
        DateTime _lastModified = DateTime.MinValue;

        /// <summary>
        /// Save the last node refresh time
        /// </summary>
        DateTimeOffset lastNodeRefreshDate = TableConstants.MinDateTime;

        /// <summary>
        /// Get the name of the node mapping table
        /// Initialized at the first time using it
        /// </summary>
        internal string NodeMappingTableName
        {
            get
            {
                if (this._mappingTableName == null)
                {
                    // It must be On-premise now
                    this._mappingTableName = AzureNaming.GenerateAzureEntityName(SchedulerTableNames.NodeMapping, this._clusterName, new Guid(this._subscriptionId), this._serviceName);
                }

                return this._mappingTableName;
            }
        }

        private CloudTableClient tableClient;

        /// <summary>
        /// The cache for mapping from logical names to physical epr strings
        /// </summary>
        Dictionary<string, string> _logicalNameToPhysicalEprMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The cache for mapping from physical epr strings to logical names
        /// </summary>
        Dictionary<string, string> _physicalEprToLogicalNameMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The cache for mapping from logical names to physical instance name
        /// </summary>
        Dictionary<string, string> _logicalNameToPhysicalInstanceMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The lock for any operation on the mapping
        /// </summary>
        object _operationLock = new object();

        /// <summary>
        /// Save the service name for the Azure deployment
        /// </summary>
        string _serviceName = null;

        /// <summary>
        /// Save the subscription Id for the Azure deployment
        /// </summary>
        string _subscriptionId;

        /// <summary>
        /// Save the cluster's headnode name
        /// </summary>
        string _clusterName = null;

        /// <summary>
        /// Indicates whether the cache is dirty
        /// </summary>
        bool _needRefreshOnNextOperation = true;

        /// <summary>
        /// The storage account
        /// </summary>
        CloudStorageAccount _storageAccount = null;

        public NodeMapping(CloudStorageAccount storageAccount, string clusterName, string subscriptionId, string serviceName)
        {
            this._clusterName = clusterName;
            this._subscriptionId = subscriptionId;
            this._serviceName = serviceName;

            this.Init(storageAccount);
        }

        internal NodeMapping(CloudStorageAccount storageAccount, string tableName, string virtualSchedulerName)
        {
            this._virtualSchedulerName = virtualSchedulerName;
            this._mappingTableName = tableName;

            this.Init(storageAccount);
        }

        /// <summary>
        /// This constructor should be called from the Azure
        /// The constructor will NOT do an initial load on the table. Please do a RefreshMapping before your first query.
        /// </summary>
        /// <param name="storageAccount"></param>
        public NodeMapping(CloudStorageAccount storageAccount, string tableName) : this(storageAccount, tableName, null)
        {
        }

        CloudTableClient TableClient
        {
            get
            {
                lock (this._tableClientLock)
                {
                    if (this.tableClient == null)
                    {
                        if (this._storageAccount == null)
                        {
                            throw new Exception("The storage account has not been initialized.");
                        }

                        this.tableClient = this._storageAccount.CreateCloudTableClient();
                    }

                    return this.tableClient;
                }
            }
            set
            {
                lock (this._tableClientLock)
                {
                    this.tableClient = value;
                }
            }
        }        

        public DateTime LastModified
        {
            get { return this._lastModified; }
        }
        
        public Type ResolveEntityType(string name)
        {
            Type type = typeof(NodeMappingTableEntry);
            return type;
        }  

        object _tableClientLock = new object();

        /// <summary>
        /// Initialize the node mapping
        /// </summary>
        /// <param name="storageAccount"></param>
        private void Init(CloudStorageAccount storageAccount)
        {
#if DEBUG
            // In release build, we expect the table is already created by the setup
            CloudTableClient client = new CloudTableClient(new Uri(storageAccount.TableEndpoint.AbsoluteUri), storageAccount.Credentials);
            try
            {
                var table = client.GetTableReference(this.NodeMappingTableName);
                table.CreateIfNotExists();
            }
            catch { }
#endif
            this._storageAccount = storageAccount;

            // Comment out this since it is not a good idea to do a long operation within constructor.
            // RefreshMapping();
        }

        /// <summary>
        /// Return the endpoint address of an Azure node corresponding to the given logical node name
        /// This overload is for modules other than SOA
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="module"></param>
        /// <param name="azureEndpoint"></param>
        /// <returns></returns>
        public bool TryGetNodeEndpoint(string nodeName, Module module, out string azureEndpoint)
        {
            return this.TryGetNodeEndpoint(nodeName, module, 0, out azureEndpoint);
        }


        /// <summary>
        /// Return the endpoint address of an Azure node corresponding to the given logical node name
        /// This overload is for SOA modules
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="module"></param>
        /// <param name="azureEndpoint"></param>
        /// <returns></returns>
        public bool TryGetNodeEndpoint(string nodeName, Module module, int coreId, out string azureEndpoint)
        {
            string endpointString;

            azureEndpoint = null;
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                bool result = this._logicalNameToPhysicalEprMapping.TryGetValue(nodeName, out endpointString);

                if (result)
                {
                    try
                    {
                        azureEndpoint = GetModuleAddress(endpointString, module, coreId);
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning("Exception in TryGetNodeEndpoint: {0}", e);
                        result = false;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Return the instance name of an Azure node corresponding to the given logical node name
        /// </summary>
        /// <param name="nodeName"></param>
        /// <param name="module"></param>
        /// <param name="azureEndpoint"></param>
        /// <returns></returns>
        public bool TryGetNodeInstanceId(string nodeName, out string instanceName)
        {
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                return this._logicalNameToPhysicalInstanceMapping.TryGetValue(nodeName, out instanceName);
            }
        }

        /// <summary>
        /// Reflect the logical name from the given physical endpoint string
        /// </summary>
        /// <param name="endpointString"></param>
        /// <param name="nodeName"></param>
        /// <returns></returns>
        public bool TryGetLogicalName(string endpointString, out string nodeName)
        {
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                return this._physicalEprToLogicalNameMapping.TryGetValue(endpointString, out nodeName);
            }
        }

        /// <summary>
        /// Store the logical node name  for each given physical endpoint string.
        /// If a physical endpoint is not mapped there is no entry for it in the nodeNameMap
        /// </summary>
        /// <param name="endPointStrings">list of physical end point streams</param>
        /// <param name="nodeNameMap">dictionary of physical end point to logical node name</param>
        public void TryGetLogicalNames(IEnumerable<string> endPointStrings, IDictionary<string,string> nodeNameMap)
        {
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                foreach (string endPointString in endPointStrings)
                {
                    string nodeName;
                    if (this._physicalEprToLogicalNameMapping.TryGetValue(endPointString, out nodeName))
                    {
                        nodeNameMap[endPointString] = nodeName;
                    }                   
                }

            }
        }

        /// <summary>
        /// Get the node count
        /// </summary>
        /// <returns></returns>
        public int GetNodeCount()
        {
            return this._logicalNameToPhysicalEprMapping.Count;
        }

        /// <summary>
        /// THIS IS FOR SCHEDULER INTERNAL USE ONLY!
        /// The format of endpoint string is: [ip]:[port1]:[port2]:...
        /// There is no protocol prefix!
        /// </summary>
        /// <param name="items"></param>
        public void UpdateNodeMapping(KeyValuePair<string, string>[] items, Dictionary<string, string> nodeNameToRoleName, Dictionary<string, string> nodeNameToInstanceName)
        {
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                try
                {
                    CloudTable table = this.TableClient.GetTableReference(this.NodeMappingTableName);
                    foreach (KeyValuePair<string, string> item in items)
                    {
                        string nodeName = item.Key;
                        string endpointString = item.Value;

                        string instanceId = null;
                        nodeNameToInstanceName.TryGetValue(nodeName, out instanceId);

                        // Update the storage first

                        string existingEndpointStr = null;
                        string existingInstanceId = null;

                        if (this._logicalNameToPhysicalEprMapping.TryGetValue(nodeName, out existingEndpointStr))
                        {
                            if (string.Compare(endpointString, existingEndpointStr, StringComparison.InvariantCultureIgnoreCase) == 0)
                            {
                                if (!this._logicalNameToPhysicalInstanceMapping.TryGetValue(nodeName, out existingInstanceId) ||
                                    string.Compare(instanceId, existingInstanceId, StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    // No need to update
                                    continue;
                                }
                            }

                            // Update the storage

                            NodeMappingTableEntry entry = null;                            
                            try
                            {                                
                                TableOperation retrieveOperation = TableOperation.Retrieve<NodeMappingTableEntry>(AzureNaming.AzurePartitionKey, nodeName.ToUpper());
                                TableResult retrievedResult  = table.Execute(retrieveOperation);
                                if (retrievedResult.Result != null)
                                {
                                    entry = (NodeMappingTableEntry)retrievedResult.Result;
                                }
                            }
                            catch
                            {
                                // Swallow the ResourceNotFound exception, and leave the entry to be null
                            }

                            if (endpointString == null)
                            {
                                // Remove the binding
                                TableOperation deleteOperation = TableOperation.Delete(entry);
                                table.Execute(deleteOperation);
                            }
                            else
                            {
                                entry.EndpointString = endpointString;
                                entry.RoleInstanceName = instanceId;

                                TableOperation updateOperation = TableOperation.Replace(entry);
                                table.Execute(updateOperation);
                            }
                        }
                        else
                        {
                            if (endpointString == null)
                            {
                                // We don't save a null phyical address
                                continue;
                            }

                            // Add to the storage table

                            Debug.Assert(nodeNameToRoleName.ContainsKey(nodeName));
                            NodeMappingTableEntry entry = new NodeMappingTableEntry(nodeName.ToUpper(), endpointString, nodeNameToRoleName[nodeName], nodeNameToInstanceName[nodeName]);
                            TableOperation insertOperation = TableOperation.Insert(entry);
                            table.Execute(insertOperation);
                        }

                    }

                    // Now update the cache at last 

                    foreach (KeyValuePair<string, string> item in items)
                    {
                        string originalEndpointString;
                        if (this._logicalNameToPhysicalEprMapping.TryGetValue(item.Key, out originalEndpointString))
                        {
                            this._physicalEprToLogicalNameMapping.Remove(originalEndpointString);
                        }

                        if (item.Value == null)
                        {
                            // This means remove the binding

                            this._logicalNameToPhysicalEprMapping.Remove(item.Key);
                            this._logicalNameToPhysicalInstanceMapping.Remove(item.Key);
                        }
                        else
                        {
                            // This means update the binding

                            this._logicalNameToPhysicalEprMapping[item.Key] = item.Value;
                            this._physicalEprToLogicalNameMapping[item.Value] = item.Key;

                            string instanceId = null;
                            if (nodeNameToRoleName.TryGetValue(item.Key, out instanceId))
                            {
                                this._logicalNameToPhysicalInstanceMapping[item.Key] = instanceId;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    this.TableClient = null;
                    this._needRefreshOnNextOperation = true;
                    throw;
                }
            }
        }

        /// <summary>
        /// Delete the entry corresponding to this logical node name from the node mapping table
        /// as well as the local cache.        
        /// </summary>
        /// <param name="logicalNodeName"></param>
        public void DeleteNodeMappingEntry(string logicalNodeName)
        {
            lock (this._operationLock)
            {
                try
                {
                    this.RefreshWhenNeeded();

                    //Update the node mapping table by deleting the entry corresponding to this logical node name
                    CloudTable table = this.TableClient.GetTableReference(this.NodeMappingTableName);
                    TableOperation retrieveOperation = TableOperation.Retrieve<NodeMappingTableEntry>(AzureNaming.AzurePartitionKey, logicalNodeName.ToUpper());
                    NodeMappingTableEntry entry = null;
                    TableResult retrievedResult = table.Execute(retrieveOperation);
                    if (retrievedResult.Result != null)
                    {
                        entry = (NodeMappingTableEntry)retrievedResult.Result;
                    }

                    if (entry != null)
                    {
                        TableOperation deleteOperation = TableOperation.Delete(entry);
                        table.Execute(deleteOperation);
                    }


                    //Update the cache
                    string physicalEpr = null;
                    if (this._logicalNameToPhysicalEprMapping.TryGetValue(logicalNodeName, out physicalEpr))
                    {
                        this._logicalNameToPhysicalEprMapping.Remove(logicalNodeName);
                        this._logicalNameToPhysicalInstanceMapping.Remove(logicalNodeName);
                    }

                    if (physicalEpr != null)
                    {
                        this._physicalEprToLogicalNameMapping.Remove(physicalEpr);
                    }
                }
                catch (Exception)
                {                    
                    //It will get traced in the calling module
                    this.TableClient = null;
                    //If there is any error deleting the entry from the node mapping table
                    //mark the cache for refresh the next time it is accessed
                    this._needRefreshOnNextOperation = true;
                    //Any retry logic should be in the caller 
                    throw;
                }
            }
        }

        string _virtualSchedulerName = null;

        /// <summary>
        /// Reload the mapping table from Azure storage
        /// </summary>
        public void RefreshMapping()
        {
            lock (this._operationLock)
            {
                try
                {
                    this._logicalNameToPhysicalEprMapping.Clear();
                    this._physicalEprToLogicalNameMapping.Clear();
                    this._logicalNameToPhysicalInstanceMapping.Clear();

                    CloudTable table = this.TableClient.GetTableReference(this.NodeMappingTableName);
                    TableQuery<NodeMappingTableEntry> tableQuery = new TableQuery<NodeMappingTableEntry>();
                    TableContinuationToken continuationToken = null;
                    do
                    {
                        // Retrieve a segment (up to 1000 entities)
                        TableQuerySegment<NodeMappingTableEntry> tableQueryResult = table.ExecuteQuerySegmented(tableQuery, continuationToken);
                        continuationToken = tableQueryResult.ContinuationToken;
                        foreach (NodeMappingTableEntry entry in tableQueryResult.Results)
                        {
                            Debug.Assert(entry.EndpointString != null);

                            this._logicalNameToPhysicalEprMapping[entry.NodeName] = entry.EndpointString;

                            if (this._virtualSchedulerName == null || string.Compare(this._virtualSchedulerName, entry.NodeName, true) != 0)
                            {
                                // Don't do this for virtual scheduler name
                                // This is valid only in scheduler on azure 
                                this._physicalEprToLogicalNameMapping[entry.EndpointString] = entry.NodeName;
                            }

                            this._logicalNameToPhysicalInstanceMapping[entry.NodeName] = entry.RoleInstanceName;

                            if (entry.Timestamp.DateTime > this._lastModified)
                                this._lastModified = entry.Timestamp.DateTime;
                        }
                    } while (continuationToken != null);

                    this._needRefreshOnNextOperation = false;
                }
                catch (Exception)
                {
                    this._needRefreshOnNextOperation = true;
                    throw;
                }
            }
        }

        /// <summary>
        /// Refresh when needed
        /// </summary>
        void RefreshWhenNeeded()
        {
            if (this._needRefreshOnNextOperation)
            {
                this.RefreshMapping();
            }
        }

        /// <summary>
        /// Get IP address from the endpoint
        /// </summary>
        /// <param name="epr"></param>
        /// <returns></returns>
        private string GetIPAddressFromEpr(string epr)
        {
            int endIdx = epr.IndexOf("::");
            if (endIdx != -1)
            {
                return epr.Substring(0, epr.IndexOf("::"));
            }
            return epr;
        }

        /// <summary>
        /// Get logical name to ip address map from the nodemapping table
        /// </summary>
        /// <param name="forceRefresh">if set to <c>true</c> [force refresh].</param>
        /// <returns>mapping dictionary</returns>
        public Dictionary<string, string> GetLogicalNameIPMapping(bool forceRefresh = true)
        {
            if (forceRefresh)
            {
                this.RefreshMapping();
            }
            else
            {
                this.RefreshWhenNeeded();
            }

            Dictionary<string, string> mapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var item in this._logicalNameToPhysicalEprMapping)
            {
                mapping.Add(item.Key, this.GetIPAddressFromEpr(item.Value));
            }

            return mapping;
        }

        /// <summary>
        /// Remove all mappings that the physical address doesn't exist anymore
        /// </summary>
        /// <param name="validIps"></param>
        public void RemoveInvalidMapping(string[] validIps, out string[] removedLogicalNames, out string[] removedPhysicalAddr)
        {
            Dictionary<string, bool> physicalIps = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string physicalIp in validIps)
            {
                physicalIps[physicalIp] = true;
            }

            List<string> removedLogicalNameList = new List<string>();
            List<string> removedPhysicalAddrList = new List<string>();
            lock (this._operationLock)
            {
                this.RefreshWhenNeeded();

                foreach (KeyValuePair<string, string> item in this._logicalNameToPhysicalEprMapping)
                {
                    string ip = GetModuleAddress(item.Value, Module.Ip);
                    if (!physicalIps.ContainsKey(ip))
                    {
                        removedLogicalNameList.Add(item.Key);
                        removedPhysicalAddrList.Add(item.Value);
                    }
                }
            }


            removedLogicalNames = removedLogicalNameList.ToArray();
            removedPhysicalAddr = removedPhysicalAddrList.ToArray();

        }

        /// <summary>
        /// refresh the node mapping from Azure storage table for a specific logical node
        /// this is used for SOA proxy to update the node mapping info
        /// </summary>
        /// <param name="logicalNodeName">the node logical name</param>
        public void RefreshNodeMapping(string logicalNodeName)
        {
            lock (this._operationLock)
            {
                try
                {
                    //Retrieve the node entry from node mapping table by the logical node name
                    CloudTable table = this.TableClient.GetTableReference(this.NodeMappingTableName);
                    TableOperation retrieveOperation = TableOperation.Retrieve<NodeMappingTableEntry>(AzureNaming.AzurePartitionKey, logicalNodeName.ToUpper());
                    NodeMappingTableEntry entry = null;
                    TableResult retrievedResult = table.Execute(retrieveOperation);
                    if (retrievedResult.Result != null)
                    {
                        entry = (NodeMappingTableEntry)retrievedResult.Result;
                    }

                    if (entry != null)
                    {
                        //Update the cache if the epr in the cache is out-of-date
                        string physicalEpr = null;
                        if (this._logicalNameToPhysicalEprMapping.TryGetValue(entry.NodeName, out physicalEpr))
                        {
                            if (!string.Equals(physicalEpr, entry.EndpointString, StringComparison.OrdinalIgnoreCase))
                            {
                                this._logicalNameToPhysicalEprMapping[entry.NodeName] = entry.EndpointString;
                                this._logicalNameToPhysicalInstanceMapping[entry.NodeName] = entry.RoleInstanceName;
                                this._physicalEprToLogicalNameMapping[entry.EndpointString] = entry.NodeName;
                            }
                        }
                        else
                        { 
                            //insert the node mapping info in the cache
                            this._logicalNameToPhysicalEprMapping[entry.NodeName] = entry.EndpointString;
                            this._logicalNameToPhysicalInstanceMapping[entry.NodeName] = entry.RoleInstanceName;
                            this._physicalEprToLogicalNameMapping[entry.EndpointString] = entry.NodeName;
                        }
                    }
                }
                catch (Exception)
                {
                    //It will get traced in the calling module
                    this.TableClient = null;

                    //Any retry logic should be in the caller 
                    throw;
                }
            }
        }

        /// <summary>
        /// Refresh node mapping table according to the last node refresh time
        /// this is used for SOA proxy to update the node mapping info in timer callback
        /// </summary>
        public void RefreshNodeMapping()
        {
            lock (this._operationLock)
            {
                CloudTable table = this.TableClient.GetTableReference(this.NodeMappingTableName);
                TableQuery<NodeMappingTableEntry> tableQuery = new TableQuery<NodeMappingTableEntry>().Where(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, this.lastNodeRefreshDate)
                    );

                TableContinuationToken continuationToken = null;
                do
                {
                    // Retrieve a segment (up to 1000 entities)
                    TableQuerySegment<NodeMappingTableEntry> tableQueryResult = table.ExecuteQuerySegmented(tableQuery, continuationToken);
                    continuationToken = tableQueryResult.ContinuationToken;
                    foreach (NodeMappingTableEntry entry in tableQueryResult.Results)
                    {
                        Debug.Assert(entry.EndpointString != null);

                        //Update the cache if the epr in the cache is out-of-date
                        string physicalEpr = null;
                        if (this._logicalNameToPhysicalEprMapping.TryGetValue(entry.NodeName, out physicalEpr))
                        {
                            if (!string.Equals(physicalEpr, entry.EndpointString, StringComparison.OrdinalIgnoreCase))
                            {
                                this._logicalNameToPhysicalEprMapping[entry.NodeName] = entry.EndpointString;
                                this._logicalNameToPhysicalInstanceMapping[entry.NodeName] = entry.RoleInstanceName;
                                this._physicalEprToLogicalNameMapping[entry.EndpointString] = entry.NodeName;
                            }
                        }
                        else
                        {
                            //insert the node mapping info in the cache
                            this._logicalNameToPhysicalEprMapping[entry.NodeName] = entry.EndpointString;
                            this._logicalNameToPhysicalInstanceMapping[entry.NodeName] = entry.RoleInstanceName;
                            this._physicalEprToLogicalNameMapping[entry.EndpointString] = entry.NodeName;
                        }

                        if (entry.Timestamp > this.lastNodeRefreshDate)
                            this.lastNodeRefreshDate = entry.Timestamp;
                    }
                } while (continuationToken != null);

                this._needRefreshOnNextOperation = false;
            }
        }
    }

    /// <summary>
    /// The cloud table entry
    /// </summary>
    internal class NodeMappingTableEntry : TableEntity
    {
        /// <summary>
        /// The node's endpoint epr string
        /// </summary>
        string _endpointString;

        /// <summary>
        /// Role name
        /// </summary>
        string _roleName;

        /// <summary>
        /// Role instance name
        /// </summary>
        string _roleInstanceName;

        /// <summary>
        /// The node's endpoint epr string
        /// </summary>
        public string EndpointString
        {
            get { return this._endpointString; }
            set { this._endpointString = value; }
        }

        /// <summary>
        /// The node's logical name, which is also the rowkey
        /// </summary>
        public string NodeName
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public string RoleName
        {
            get { return this._roleName; }
            set { this._roleName = value; }
        }

        public string RoleInstanceName
        {
            get { return this._roleInstanceName; }
            set { this._roleInstanceName = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logicalName"></param>
        /// <param name="physicalEpr"></param>
        public NodeMappingTableEntry(string logicalName, string physicalEpr, string roleName, string roleInstanceName)
        {
            // Here we use a single partition key to make all records within the same partition
            // This is because only the scheduler changes this table and everybody else reads it
            // We need to make the read very fast.

            this.PartitionKey = AzureNaming.AzurePartitionKey;  
            this._endpointString = physicalEpr;
            this.RowKey = logicalName;
            this._roleName = roleName;
            this._roleInstanceName = roleInstanceName;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NodeMappingTableEntry()
        {
            this.PartitionKey = AzureNaming.AzurePartitionKey;
        }
    }    
}
