namespace TelepathyCommon.Service
{
    public class WcfServiceConstants
    {
        public const string ClusterManagerInternalServiceName = "IClusterManagerInternal";

        public const string ClusterManagerServiceName = "IClusterManager";

        public const string ClusterNodeInternalServiceName = "IClusterNodeInternal";

        public const string ClusterNodeServiceName = "IClusterNode";

        public const string DiagnosticsStoreInternalServiceName = "DiagnosticsStoreServiceInternal.remote";

        public const string DiagnosticsStoreServiceName = "DiagnosticsStoreService.remote";

        public const string HttpsUriFormat = "https://{0}:{1}/{2}";

        public const string InternalServicePath = "Internal";

        public const string LocalhostPath = "localhost";

        public const int ManagementWcfChannelPort = 6730;

        public const string NetTcpUriFormat = "net.tcp://{0}:{1}/{2}";

        // name of service to connect to HpcNodeManager on compute node
        public const string SchedulerListenerServiceName = "CCPSchedulerListener.remote";

        public const string SchedulerNodeInternalServiceName = "SchedulerNodeServiceInternal";

        public const string SchedulerNodeServiceName = "SchedulerNodeService";

        public const string SchedulerStoreInternalServiceName = "SchedulerStoreServiceInternal";

        // name of service to connect to client or other service
        public const string SchedulerStoreServiceName = "SchedulerStoreService";

        public const string SdmInternalServiceName = "SdmInternal";

        public const string SdmServiceName = "Sdm";

        public const int SdmWcfChannelPort = 9893;
    }
}