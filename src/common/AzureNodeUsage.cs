//------------------------------------------------------------------------------
// <copyright file="AzureNodeUsage.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// This is the interface for proxy modules to node role for usage count.
    /// Only the Azure metric component should only read.
    /// </summary>
    internal class NodeUsage
    {
        /// <summary>
        /// Save the name of the node mapping table
        /// </summary>
        string _mappingTableName = null;

        CloudTableClient tableClient;

        /// <summary>
        /// The node usage data
        /// </summary>
        Dictionary<string, int> _nodeUsageData = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// The storage account
        /// </summary>
        CloudStorageAccount _storageAccount = null;

        /// <summary>
        /// Stores the mapping from embedded node role names to their sizes
        /// </summary>
        private Dictionary<string, string> _embeddedNodeMapping;

        /// <summary>
        /// This constructor should be called from the Azure
        /// The constructor will NOT do an initial load on the table. Please do a RefreshMapping before your first query.
        /// </summary>
        /// <param name="storageAccount"></param>
        public NodeUsage(CloudStorageAccount storageAccount, string tableName)
        {
            _mappingTableName = tableName;
            _storageAccount = storageAccount;
            BuildEmbeddedNodeMapping();
        }

        CloudTableClient TableClient
        {
            get
            {
                if (this.tableClient == null)
                {
                    if (_storageAccount == null)
                    {
                        throw new Exception("The storage account has not been initialized.");
                    }

                    this.tableClient = this._storageAccount.CreateCloudTableClient();
                }

                return this.tableClient;
            }
        }

        public Type ResolveEntityType(string name)
        {
            Type type = typeof(NodeMappingTableEntry);
            return type;
        }  

        private void InitNodeUsageData()
        {
            _nodeUsageData.Clear();
            _nodeUsageData[RoleNames.Role1Node] = 0;
            _nodeUsageData[RoleNames.Role2Node] = 0;
            _nodeUsageData[RoleNames.Role3Node] = 0;
            _nodeUsageData[RoleNames.Role4Node] = 0;

            // For WAHS
            _nodeUsageData[AzureRoleNames.SmallEmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.MediumEmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.LargeEmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.ExtraLargeEmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.A5EmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.A6EmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.A7EmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.A8EmbeddedNode] = 0;
            _nodeUsageData[AzureRoleNames.A9EmbeddedNode] = 0;
        }

        /// <summary>
        /// Builds the mapping for scheduler on Azure nodes to their sizes. This only executes if the scheduler
        /// is in Azure and the role environment is available to the caller.
        /// </summary>
        private void BuildEmbeddedNodeMapping()
        {
            try
            {
                // See if the role environment is available. If not, return silently.
                if (!RoleEnvironment.IsAvailable)
                {
                    _embeddedNodeMapping = null;
                    return;
                }

                // See if the scheduler is on Azure
                bool isSchedulerOnAzure;
                try
                {
                    RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.SchedulerRole);
                    isSchedulerOnAzure = true;
                }
                catch
                {
                    isSchedulerOnAzure = false;
                }

                // If the scheduler is on Azure, map embedded node roles to their sizes
                if (isSchedulerOnAzure)
                {
                    _embeddedNodeMapping = new Dictionary<string, string>();
                    string nodeRoles = RoleEnvironment.GetConfigurationSettingValue(SchedulerConfigNames.NodeRoles);

                    // Node roles are split by a semicolon. Parse each one.
                    foreach (string role in nodeRoles.Split(';'))
                    {
                        // Each role is separated by an '=' sign. It is formatted ROLE_NAME=SIZE=NAMING_SCHEME=TYPE;
                        string[] roleParts = role.Split('=');
                        if (roleParts.Length >= 2)
                        {
                            string roleName = roleParts[0];
                            string roleSize = roleParts[1];

                            if (!_embeddedNodeMapping.ContainsKey(roleName))
                            {
                                _embeddedNodeMapping.Add(roleName, roleSize);
                            }
                        }
                    }
                }
                else
                {
                    // The mapping does not apply.
                    _embeddedNodeMapping = null;
                }
            }
            catch
            {
                // This failure isn't going to cause problems enough to throw the exception. Instead just make the
                // mapping unavailable.
                _embeddedNodeMapping = null;
            }
        }

        /// <summary>
        /// Reload the mapping table from Azure storage
        /// </summary>
        public void RefreshMapping()
        {
            InitNodeUsageData();

            List<string> ipAddressHistory = new List<string>();
            CloudTable table = this.TableClient.GetTableReference(this._mappingTableName);
            TableQuery<NodeMappingTableEntry> tableQuery = new TableQuery<NodeMappingTableEntry>();
            TableContinuationToken continuationToken = null;
            do
            {
                TableQuerySegment<NodeMappingTableEntry> tableQueryResult = table.ExecuteQuerySegmented(tableQuery, continuationToken);
                continuationToken = tableQueryResult.ContinuationToken;
                foreach (NodeMappingTableEntry entry in tableQueryResult.Results)
                {
                    Debug.Assert(entry.EndpointString != null);

                    if (_nodeUsageData.ContainsKey(entry.RoleName))
                    {
                        _nodeUsageData[entry.RoleName]++;
                    }
                    else
                    {
                        // When the scheduler is on Azure, nodes may be embedded in other roles. Check for these.
                        if (_embeddedNodeMapping != null && _embeddedNodeMapping.ContainsKey(entry.RoleName))
                        {
                            string size = _embeddedNodeMapping[entry.RoleName];
                            if (_nodeUsageData.ContainsKey(size) && !ipAddressHistory.Contains(entry.EndpointString))
                            {
                                _nodeUsageData[size]++;

                                // Only track nodes once. Duplicate entries may exist for HA.
                                ipAddressHistory.Add(entry.EndpointString);
                            }
                        }
                    }
                }
            } while (continuationToken != null);            
        }

        /// <summary>
        /// Get the node usage data.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetNodeUsageData()
        {
            return _nodeUsageData;
        }
    }
}
