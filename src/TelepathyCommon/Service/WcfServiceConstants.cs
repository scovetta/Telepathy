namespace Microsoft.Hpc
{
    public class WcfServiceConstants
    {
        // name of service to connect to client or other service
        public const string SchedulerStoreServiceName = "SchedulerStoreService";
        public const string SchedulerStoreInternalServiceName = "SchedulerStoreServiceInternal";

        // name of service to connect to HpcNodeManager on compute node
        public const string SchedulerListenerServiceName = "CCPSchedulerListener.remote";

        public const string SdmServiceName = "Sdm";
        public const string SdmInternalServiceName = "SdmInternal";

        public const string DiagnosticsStoreServiceName = "DiagnosticsStoreService.remote";
        public const string DiagnosticsStoreInternalServiceName = "DiagnosticsStoreServiceInternal.remote";

        public const string NetTcpUriFormat = "net.tcp://{0}:{1}/{2}";
        public const string HttpsUriFormat = "https://{0}:{1}/{2}";

        public const int ManagementWcfChannelPort = 6730;
        public const int SdmWcfChannelPort = 9893;

        public const string ClusterNodeServiceName = "IClusterNode";
        public const string ClusterManagerServiceName = "IClusterManager";
        public const string SchedulerNodeServiceName = "SchedulerNodeService";
        public const string ClusterNodeInternalServiceName = "IClusterNodeInternal";
        public const string ClusterManagerInternalServiceName = "IClusterManagerInternal";
        public const string SchedulerNodeInternalServiceName = "SchedulerNodeServiceInternal";

        public const string InternalServicePath = "Internal";
        public const string LocalhostPath = "localhost";
    }
}
