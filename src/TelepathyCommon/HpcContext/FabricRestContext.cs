using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelepathyCommon.HpcContext.Extensions;
using TelepathyCommon.Registry;

namespace TelepathyCommon.HpcContext
{
    internal class FabricRestContext : IFabricContext
    {
        #region Extracted data structure from Service Fabric libraries.

        //
        // Summary:
        //     Specifies the service kind.
        public enum ServiceKind
        {
            //
            // Summary:
            //     Invalid.
            Invalid = 0,
            //
            // Summary:
            //     Does not use Service Fabric to make its state highly available or reliable.
            Stateless = 1,
            //
            // Summary:
            //     Uses Service Fabric to make its state or part of its state highly available and
            //     reliable.
            Stateful = 2
        }

        //
        // Summary:
        //     Specifies the service partition status.
        public enum ServicePartitionStatus
        {
            //
            // Summary:
            //     This supports the Service Fabric infrastructure and is not meant to be used directly
            //     from your code.
            Invalid = 0,
            //
            // Summary:
            //     Partition is ready.
            //     For stateless services there is one up replica
            //     For stateful services the number of ready replicas is greater than or equal to
            //     the System.Fabric.Description.StatefulServiceDescription.MinReplicaSetSize
            Ready = 1,
            //
            // Summary:
            //     Partition is not ready. This is returned when none of the other states apply.
            NotReady = 2,
            //
            // Summary:
            //     Partition is in quorum loss. This means that less than MinReplicaSetSize number
            //     of replicas are participating in quorum.
            InQuorumLoss = 3,
            //
            // Summary:
            //     Partition is undergoing a reconfiguration.
            Reconfiguring = 4,
            //
            // Summary:
            //     Partition is getting deleted.
            Deleting = 5
        }

        //
        // Summary:
        //     Represents the health state.
        public enum HealthState
        {
            //
            // Summary:
            //     Indicates that the health state is invalid.
            Invalid = 0,
            //
            // Summary:
            //     Indicates that the health state is ok.
            Ok = 1,
            //
            // Summary:
            //     Indicates that the health state is warning. There may something wrong that requires
            //     investigation.
            Warning = 2,
            //
            // Summary:
            //     Indicates that the health state is error, there is something wrong that needs
            //     to be investigated.
            Error = 3,
            //
            // Summary:
            //     Indicates that the health state is unknown.
            Unknown = 65535
        }

        //
        // Summary:
        //     Indicates the type of partitioning scheme that is used.
        //
        // Remarks:
        //     System.Fabric.ServicePartitionKind defines the value of the System.Fabric.ServicePartitionInformation.Kind
        //     property of the System.Fabric.ServicePartitionInformation class.
        public enum ServicePartitionKind
        {
            //
            // Summary:
            //     Indicates the partition kind is invalid.
            Invalid = 0,
            //
            // Summary:
            //     Indicates that the partition is based on string names, and is a System.Fabric.SingletonPartitionInformation
            //     object, that was originally created via System.Fabric.Description.SingletonPartitionSchemeDescription.
            Singleton = 1,
            //
            // Summary:
            //     Indicates that the partition is based on Int64 key ranges, and is an System.Fabric.Int64RangePartitionInformation
            //     object that was originally created via System.Fabric.Description.UniformInt64RangePartitionSchemeDescription.
            Int64Range = 2,
            //
            // Summary:
            //     Indicates that the partition is based on string names, and is a System.Fabric.NamedPartitionInformation
            //     object, that was originally created via System.Fabric.Description.NamedPartitionSchemeDescription.
            Named = 3
        }

        public class Partitions
        {
            public string ContinuationToken { get; set; }

            public List<Partition2> Items { get; set; }
        }

        /// <summary>
        /// Class similar to Partition. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class Partition2
        {
            public ServiceKind ServiceKind { get; set; }
            public ServicePartitionInformation2 PartitionInformation { get; set; }
            public ServicePartitionStatus PartitionStatus { get; set; }
            public HealthState HealthState { get; set; }
            public long MinReplicaSetSize { get; set; }
            public long TargetReplicaSetSize { get; set; }
            public Epoch2 CurrentConfigurationEpoch { get; set; }
            public long InstanceCount { get; set; }
        }

        /// <summary>
        /// Class similar to ServicePartitionInformation. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class ServicePartitionInformation2
        {
            public ServicePartitionKind ServicePartitionKind { get; set; }
            public Guid Id { get; set; }
        }

        /// <summary>
        /// Class similar to Epoch. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class Epoch2
        {
            public string ConfigurationVersion { get; set; }
            public string DataLossVersion { get; set; }
        }

        //
        // Summary:
        //     This supports the Service Fabric infrastructure and is not meant to be used directly
        //     from your code.
        public enum ReplicaStatus
        {
            //
            // Summary:
            //     This supports the Service Fabric infrastructure and is not meant to be used directly
            //     from your code.
            Invalid = 0,
            //
            // Summary:
            //     This supports the Service Fabric infrastructure and is not meant to be used directly
            //     from your code.
            Down = 1,
            //
            // Summary:
            //     This supports the Service Fabric infrastructure and is not meant to be used directly
            //     from your code.
            Up = 2
        }

        //
        // Summary:
        //     Indicates the role of a stateful service replica.
        //
        // Remarks:
        //     Service Fabric requires different behaviors from a service replica depending
        //     on what role it currently performs.
        public enum ReplicaRole
        {
            //
            // Summary:
            //     Indicates the initial role that a replica is created in.
            Unknown = 0,
            //
            // Summary:
            //     Specifies that the replica has no responsibility in regard to the replica set.
            //
            // Remarks:
            //     When System.Fabric.IStatefulServiceReplica.ChangeRoleAsync(System.Fabric.ReplicaRole,System.Threading.CancellationToken)
            //     indicates this role, it is safe to delete any persistent state that is associated
            //     with this replica.
            None = 1,
            //
            // Summary:
            //     Refers to the replica in the set on which all read and write operations are complete
            //     in order to enforce strong consistency semantics. Read operations are handled
            //     directly by the Primary replica, while write operations must be acknowledged
            //     by a quorum of the replicas in the replica set. There can only be one Primary
            //     replica in a replica set at a time.
            Primary = 2,
            //
            // Summary:
            //     Refers to a replica in the set that receives a state transfer from the Primary
            //     replica to prepare for becoming an active Secondary replica. There can be multiple
            //     Idle Secondary replicas in a replica set at a time. Idle Secondary replicas do
            //     not count as a part of a write quorum.
            IdleSecondary = 3,
            //
            // Summary:
            //     Refers to a replica in the set that receives state updates from the Primary replica,
            //     applies them, and sends acknowledgements back. Secondary replicas must participate
            //     in the write quorum for a replica set. There can be multiple active Secondary
            //     replicas in a replica set at a time. The number of active Secondary replicas
            //     is configurable that the reliability subsystem should maintain.
            ActiveSecondary = 4
        }

        public class Replicas
        {
            public string ContinuationToken { get; set; }

            public List<Replica2> Items { get; set; }
        }

        /// <summary>
        /// Class similar to Replica. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class Replica2
        {
            public string ReplicaId { get; set; }
            public ReplicaRole ReplicaRole { get; set; }
            public ReplicaStatus ReplicaStatus { get; set; }
            public ServiceKind ServiceKind { get; set; }
            public HealthState HealthState { get; set; }
            public string NodeName { get; set; }
            public string Address { get; set; }
            public string LastInBuildDurationInSeconds { get; set; }
        }

        //
        // Summary:
        //     Specifies the node status.
        public enum NodeStatus
        {
            //
            // Summary:
            //     Invalid.
            Invalid = 0,
            //
            // Summary:
            //     Node is up.
            Up = 1,
            //
            // Summary:
            //     Node is down.
            Down = 2,
            //
            // Summary:
            //     Node is being enabled.
            Enabling = 3,
            //
            // Summary:
            //     Node is being disabled.
            Disabling = 4,
            //
            // Summary:
            //     Node is disabled.
            Disabled = 5,
            //
            // Summary:
            //     Node status is not known.
            Unknown = 6,
            //
            // Summary:
            //     Node is removed.
            Removed = 7
        }

        public class Nodes
        {
            public string ContinuationToken { get; set; }

            public List<NodeInformation> Items { get; set; }
        }

        /// <summary>
        /// Class with members that correspond to the JSON name/value pairs returned from the Get Nodes REST API call.
        /// </summary>
        public class NodeInformation
        {
            public string CodeVersion { get; set; }
            public string ConfigVersion { get; set; }
            public string FaultDomain { get; set; }
            public HealthState HealthState { get; set; }
            public IdObject Id { get; set; }
            public string InstanceId { get; set; }
            public string IpAddressOrFQDN { get; set; }
            public bool IsSeedNode { get; set; }
            public string Name { get; set; }
            public NodeDeactivationResult2 NodeDeactivationInfo { get; set; }
            public NodeStatus NodeStatus { get; set; }
            public string NodeUpTimeInSeconds { get; set; }
            public string Type { get; set; }
            public string UpgradeDomain { get; set; }
        }

        /// <summary>
        /// Class to access the Node Id. 
        /// The JSON string describes the Node Id as ' "Id" : { "Id" : "18c04a60c5c8e287f4f4337ba8642205" }, '.
        /// </summary>
        public class IdObject
        {
            public string Id { get; set; }
        }

        //
        // Summary:
        //     Describes the reason why the node is being deactivated.
        //
        // Remarks:
        //     The System.Fabric.NodeDeactivationIntent enumeration is provided as a part of
        //     the System.Fabric.FabricClient.ClusterManagementClient.DeactivateNodeAsync(System.String,System.Fabric.NodeDeactivationIntent)
        //     method.
        //     Service Fabric uses this information to take the correct actions at the node
        //     to provide a graceful shutdown of the node. The intents have a general progression
        //     or severity.
        //     A deactivation that is started with one intent can be increased to subsequent
        //     higher levels of intent. The general order of this progression is: Pause, Restart,
        //     Stop, ForceStop.
        public enum NodeDeactivationIntent
        {
            //
            // Summary:
            //     Indicates that a deactivation intent is invalid. This value is not used.
            Invalid = 0,
            //
            // Summary:
            //     Indicates that the node should be paused.
            //
            // Remarks:
            //     When this intent is used, Service Fabric prevents changes to the specified node.
            //     No new replicas are placed on the node, and existing replicas are not moved or
            //     shut down.
            //     The System.Fabric.NodeDeactivationIntent.Pause intent is useful when one or more
            //     replicas on a node encounter issues and that node has to be isolated for further
            //     investigation
            //     This investigation could include accessing the remote machine to investigate
            //     such activities as reviewing local logs, taking memory dumps, and observing other
            //     information.
            //     The purpose of this mode is to attempt to preserve the node so that additional
            //     debugging can be performed under the same conditions that existed when the error
            //     occurred.
            //     Note that specifying this mode does not guarantee that all changes to the node
            //     can be prevented.
            //     For example, replicas on the node might crash after the intent to pause the node
            //     has been received.
            //     As another example, failures in another location in the cluster might cause a
            //     Secondary replica on the node to be promoted to the Primary replica.
            //     In this mode, Service Fabric will disable Placement and Resource Balancing on
            //     the target node
            //     In addition Safety Checks (see System.Fabric.SafetyCheckKind) will be performed
            //     by Service Fabric
            Pause = 1,
            //
            // Summary:
            //     Indicates that the intent is for the node to be restarted after a short period
            //     of time. Service Fabric does not restart the node - this action is done outside
            //     of Service Fabric.
            //
            // Remarks:
            //     A node might be shut down, for example, to perform an OS update or a Service
            //     Fabric code update.
            //     In this mode, Service Fabric prevents new replicas from being placed on the node.
            //     Additionally, Service Fabric takes the following actions:
            //     Disable Placement and Resource balancing on the target node
            //     Performs safety checks. The System.Fabric.SafetyCheckKind.WaitForPrimaryPlacement
            //     safety check is not performed for this intent.
            //     Close all replicas and instances running on the node.
            //     NOTE: Once replicas and instances are closed, Service Fabric will reactively
            //     create replacements for replicas of stateful volatile services and stateless
            //     services.
            //     For Persisted replicas on the node, new replicas are not be built, because the
            //     intention is to restart this node and to recover the persistent state after the
            //     restart. The replicas are opened once the node is activated.
            Restart = 2,
            //
            // Summary:
            //     Indicates that the intent is to reimage the node. Service Fabric does not reimage
            //     the node - this action is done outside of Service Fabric.
            //
            // Remarks:
            //     When Service Fabric receives this intent, it ensures that:
            //     In this mode, Service Fabric prevents new replicas from being placed on the node.
            //     Additionally, Service Fabric takes the following actions:
            //     Disable Placement and Resource balancing on the target node
            //     Move all Up replicas out of the node.
            //     For stateless instances this implies creating another instance on another node
            //     For replicas of stateful services a replacement replica is built on another node
            //     (if there is sufficient capacity in the cluster)
            //     If the replica is a primary, some other active secondary of the partition is
            //     made the primary prior to creating the replacement
            //     Stateful replicas on the node receive notifications to clean up their state and
            //     close.
            //     Performs a subset of safety checks that ensure that as a result of taking this
            //     node down no data loss can occur.
            RemoveData = 3,
            //
            // Summary:
            //     Indicates that the node is being decommissioned and is not expected to return.
            //     Service Fabric does not decommission the node - this action is done outside of
            //     Service Fabric.
            //
            // Remarks:
            //     When Service Fabric receives this intent, it ensures that:
            //     In this mode, Service Fabric prevents new replicas from being placed on the node.
            //     Additionally, Service Fabric takes the following actions:
            //     Disable Placement and Resource balancing on the target node
            //     Move all Up replicas out of the node.
            //     For stateless instances this implies creating another instance on another node
            //     For replicas of stateful services a replacement replica is built on another node
            //     (if there is sufficient capacity in the cluster)
            //     If the replica is a primary, some other active secondary of the partition is
            //     made the primary prior to creating the replacement
            //     Stateful replicas on the node receive notifications to clean up their state and
            //     close.
            //     Performs a subset of safety checks that ensure that as a result of taking this
            //     node down no data loss can occur.
            RemoveNode = 4
        }

        //
        // Summary:
        //     Specified the status for a node deactivation task.
        public enum NodeDeactivationStatus
        {
            //
            // Summary:
            //     No status is associated with the task.
            None = 0,
            //
            // Summary:
            //     Safety checks are in progress for the task.
            SafetyCheckInProgress = 1,
            //
            // Summary:
            //     All the safety checks have been completed for the task.
            SafetyCheckComplete = 2,
            //
            // Summary:
            //     The task is completed.
            Completed = 3
        }

        /// <summary>
        /// Class similar to NodeDeactivationResult. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class NodeDeactivationResult2
        {
            public NodeDeactivationIntent NodeDeactivationIntent { get; set; }
            public NodeDeactivationStatus NodeDeactivationStatus { get; set; }
            public List<NodeDeactivationTask2> NodeDeactivationTask { get; set; }
        }

        /// <summary>
        /// Class similar to NodeDeactivationTaskId2. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class NodeDeactivationTask2
        {
            public NodeDeactivationIntent NodeDeactivationIntent { get; set; }
            public NodeDeactivationTaskId2 NodeDeactivationTaskId { get; set; }
        }

        //
        // Summary:
        //     Specifies the different types of node deactivation tasks.
        public enum NodeDeactivationTaskType
        {
            //
            // Summary:
            //     Invalid task type.
            Invalid = 0,
            //
            // Summary:
            //     Specifies the task created by the Azure MR.
            Infrastructure = 1,
            //
            // Summary:
            //     Specifies the task that was created by the Repair Manager service.
            Repair = 2,
            //
            // Summary:
            //     Specifies that the task was created by calling the public API.
            Client = 3
        }

        /// <summary>
        /// Class similar to NodeDeactivationTaskId2. Designed for use with JavaScriptSerializer.
        /// </summary>
        public class NodeDeactivationTaskId2
        {
            public string Id { get; set; }
            public NodeDeactivationTaskType NodeDeactivationTaskType { get; set; }
        }

        #endregion

        private const string GetPartitionsUri = "{0}/Applications/{1}/$/GetServices/{1}/{2}/$/GetPartitions?api-version=2.0";
        private const string GetPartitionsUriWithContinueToken = "{0}/Applications/{1}/$/GetServices/{1}/{2}/$/GetPartitions?api-version=2.0&ContinuationToken={3}";
        private const string GetReplicasUri = "{0}/Applications/{1}/$/GetServices/{1}/{2}/$/GetPartitions/{3}/$/GetReplicas?api-version=2.0";
        private const string GetReplicasUriWithContinueToken = "{0}/Applications/{1}/$/GetServices/{1}/{2}/$/GetPartitions/{3}/$/GetReplicas?api-version=2.0&ContinuationToken={4}";
        private const string GetNodeUri = "{0}/Nodes/{1}?api-version=1.0";
        private const string GetNodesUri = "{0}/Nodes?api-version=2.0";
        private const string GetNodesUriWithContinueToken = "{0}/Nodes?api-version=2.0&ContinuationToken={1}";

        internal FabricRestContext(EndpointsConnectionString gatewayString)
        {
            this.ConnectionString = gatewayString;
        }

        public EndpointsConnectionString ConnectionString { get; private set; }
        
        public IRegistry Registry
        {
            get
            {
                return new NonHARegistry();
            }
        }

        public async Task<string> ResolveSingletonServicePrimaryAsync(string serviceName, CancellationToken token)
        {
            List<Exception> exceptions = new List<Exception>();

            foreach (var ep in this.ConnectionString.EndPoints)
            {
                try
                {
                    var uriBase = $"http://{ep}";
                    using (HttpClient client = new HttpClient())
                    {
                        List<Partition2> partitionList = new List<Partition2>();
                        var partions = await this.GetRestResponse<Partitions>(client, token, GetPartitionsUri, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName).ConfigureAwait(false);
                        partitionList.AddRange(partions.Items);
                        while (!string.IsNullOrEmpty(partions.ContinuationToken))
                        {
                            partions = await this.GetRestResponse<Partitions>(client, token, GetPartitionsUriWithContinueToken, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, partions.ContinuationToken).ConfigureAwait(false);
                            partitionList.AddRange(partions.Items);
                        }

                        var partion = partitionList.Single(p => p.PartitionInformation.ServicePartitionKind == ServicePartitionKind.Singleton);

                        List<Replica2> replicaList = new List<Replica2>();
                        var replicas = await this.GetRestResponse<Replicas>(client, token, GetReplicasUri, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, partion.PartitionInformation.Id).ConfigureAwait(false);
                        replicaList.AddRange(replicas.Items);
                        while (!string.IsNullOrEmpty(replicas.ContinuationToken))
                        {
                            replicas = await this.GetRestResponse<Replicas>(client, token, GetReplicasUriWithContinueToken, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, replicas.ContinuationToken).ConfigureAwait(false);
                            replicaList.AddRange(replicas.Items);
                        }

                        var replica = replicaList.Single(r => r.ReplicaRole == ReplicaRole.Primary);
                        var node = await this.GetRestResponse<NodeInformation>(client, token, GetNodeUri, uriBase, replica.NodeName).ConfigureAwait(false);
                        return node.IpAddressOrFQDN;
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    continue;
                }
            }

            throw new AggregateException(exceptions);
        }

        private async Task<T> GetRestResponse<T>(HttpClient client, CancellationToken token, string format, params object[] args)
        {
            var response = await client.GetAsync(string.Format(format, args), token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsAsync<T>(token).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> ResolveStatelessServiceNodesAsync(string serviceName, CancellationToken token)
        {
            return await (await Task.WhenAny<IEnumerable<string>>(this.ConnectionString.EndPoints.Select(async ep =>
                {
                    var uriBase = $"http://{ep}";
                    var nodeList = new List<string>();
                    using (HttpClient client = new HttpClient())
                    {
                        List<Partition2> partitionList = new List<Partition2>();
                        var partions = await this.GetRestResponse<Partitions>(client, token, GetPartitionsUri, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName).ConfigureAwait(false);
                        partitionList.AddRange(partions.Items);
                        while (!string.IsNullOrEmpty(partions.ContinuationToken))
                        {
                            partions = await this.GetRestResponse<Partitions>(client, token, GetPartitionsUriWithContinueToken, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, partions.ContinuationToken).ConfigureAwait(false);
                            partitionList.AddRange(partions.Items);
                        }

                        var nodeInfos = new List<NodeInformation>();
                        var nodes = await this.GetRestResponse<Nodes>(client, token, GetNodesUri, uriBase).ConfigureAwait(false);
                        nodeInfos.AddRange(nodes.Items);
                        while (!string.IsNullOrEmpty(nodes.ContinuationToken))
                        {
                            nodes = await this.GetRestResponse<Nodes>(client, token, GetNodesUriWithContinueToken, uriBase, nodes.ContinuationToken).ConfigureAwait(false);
                            nodeInfos.AddRange(nodes.Items);
                        }

                        foreach (var partition in partitionList)
                        {
                            List<string> replicaList = new List<string>();
                            var replicas = await this.GetRestResponse<Replicas>(client, token, GetReplicasUri, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, partition.PartitionInformation.Id).ConfigureAwait(false);
                            replicaList.AddRange(replicas.Items.Select(r => r.NodeName));
                            while (!string.IsNullOrEmpty(replicas.ContinuationToken))
                            {
                                replicas = await this.GetRestResponse<Replicas>(client, token, GetReplicasUriWithContinueToken, uriBase, ServiceResolverExtension.HpcApplicationName, serviceName, replicas.ContinuationToken).ConfigureAwait(false);
                                replicaList.AddRange(replicas.Items.Select(r => r.NodeName));
                            }

                            nodeList.AddRange(nodeInfos.Where(n => replicaList.Contains(n.Name)).Select(n => n.IpAddressOrFQDN));
                        }

                        return nodeList.Distinct(StringComparer.InvariantCultureIgnoreCase);
                    }
                })).ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<IEnumerable<string>> GetNodesAsync(CancellationToken token)
        {
            return await (await Task.WhenAny<IEnumerable<string>>(this.ConnectionString.EndPoints.Select(async ep =>
                {
                    var uriBase = $"http://{ep}";
                    var nodeList = new List<string>();
                    using (HttpClient client = new HttpClient())
                    {
                        var nodes = await this.GetRestResponse<Nodes>(client, token, GetNodesUri, uriBase).ConfigureAwait(false);
                        nodeList.AddRange(nodes.Items.Select(n => n.IpAddressOrFQDN));
                        while (!string.IsNullOrEmpty(nodes.ContinuationToken))
                        {
                            nodes = await this.GetRestResponse<Nodes>(client, token, GetNodesUriWithContinueToken, uriBase, nodes.ContinuationToken).ConfigureAwait(false);
                            nodeList.AddRange(nodes.Items.Select(n => n.IpAddressOrFQDN));
                        }

                        return nodeList.Distinct(StringComparer.InvariantCultureIgnoreCase);
                    }
                })).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }
}
