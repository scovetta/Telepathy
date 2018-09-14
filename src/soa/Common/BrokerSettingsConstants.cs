//----------------------------------------------------------------------------------------------------------------------------------------
// <copyright file="BrokerSettingsConstants.cs" company="Microsoft">
//     Copyright(C) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>the constants definition for the broker settings.</summary>
//-----------------------------------------------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Common
{
    /// <summary>
    /// the constants definition for the broker settings.
    /// </summary>
    internal static class BrokerSettingsConstants
    {
        /// <summary>
        /// the durable
        /// </summary>
        public const string Durable = "HPC_Durable";

        /// <summary>
        /// the share session setting.
        /// </summary>
        public const string ShareSession = "HPC_ShareSession";
        
        /// <summary>
        /// the use AAD setting.
        /// </summary>
        public const string UseAad = "HPC_UseAad";

        /// <summary>
        /// the use AAD setting.
        /// </summary>
        public const string AadUserIdentity = "HPC_AadUserIdentity";

        /// <summary>
        /// the secure setting.
        /// </summary>
        public const string Secure = "HPC_SessionStartInfo_Secure";

        /// <summary>
        /// the transport schema setting.
        /// </summary>
        public const string TransportScheme = "HPC_SessionStartInfo_TransportScheme";

        /// <summary>
        /// whether message details is available
        /// </summary>
        public const string MessageDetailsAvailable = "HPC_MessageDetailsAvailable";

        /// <summary>
        /// the session is setting.
        /// </summary>
        public const string SessionId = "HPC_SessionId";

        /// <summary>
        /// the AllocationGrowLoadRatioThreshold setting.
        /// </summary>
        public const string AllocationGrowLoadRatioThreshold = "HPC_AllocationGrowLoadRatioThreshold";

        /// <summary>
        /// the AllocationShrinkLoadRatioThreshold setting.
        /// </summary>
        public const string AllocationShrinkLoadRatioThreshold = "HPC_AllocationShrinkLoadRatioThreshold";

        /// <summary>
        /// the ClientIdleTimeout setting.
        /// </summary>
        public const string ClientIdleTimeout = "HPC_ClientIdleTimeout";

        /// <summary>
        /// the SessionIdleTimeout setting.
        /// </summary>
        public const string SessionIdleTimeout = "HPC_SessionIdleTimeout";

        /// <summary>
        /// the MessagesThrottleStartThreshold setting.
        /// </summary>
        public const string MessagesThrottleStartThreshold = "HPC_MessagesThrottleStartThreshold";

        /// <summary>
        /// the MessagesThrottleStopThreshold setting.
        /// </summary>
        public const string MessagesThrottleStopThreshold = "HPC_MessagesThrottleStopThreshold";

        /// <summary>
        /// the broker node machine name
        /// </summary>
        public const string BrokerNode = "HPC_BrokerNode";

        /// <summary>
        /// the broker is suspended
        /// </summary>
        public const string Suspended = "HPC_Suspended";

        /// <summary>
        /// the ClientConnectionTimeout setting
        /// </summary>
        public const string ClientConnectionTimeout = "HPC_ClientConnectionTimeout";

        /// <summary>
        /// Job property id name for Faulted
        /// </summary>
        public const string Faulted = "HPC_Faulted";

        /// <summary>
        /// Job property id name for Calculated
        /// </summary>
        public const string Calculated = "HPC_Calculated";

        /// <summary>
        /// Job property id name for Calculating
        /// </summary>
        public const string Calculating = "HPC_Calculating";

        /// <summary>
        /// Job property id name for Reemitted
        /// </summary>
        public const string Reemitted = "HPC_Reemitted";

        /// <summary>
        /// Job property id name for PurgedTotal
        /// </summary>
        public const string PurgedTotal = "HPC_PurgedTotal";

        /// <summary>
        /// Job property id name for PurgedProcessed
        /// </summary>
        public const string PurgedProcessed = "HPC_PurgedProcessed";

        /// <summary>
        /// Job property id name for PurgedFaulted
        /// </summary>
        public const string PurgedFaulted = "HPC_PurgedFaulted";

        /// <summary>
        /// Version of the service
        /// </summary>
        public const string ServiceVersion = "HPC_ServiceVersion";

        /// <summary>
        /// Version of persistent message schema
        /// </summary>
        public const string PersistVersion = "HPC_PersistVersion";

        /// <summary>
        /// Job property id name for MaxMessageSize
        /// </summary>
        public const string ServiceConfigMaxMessageSize = "HPC_SERVICE_MAXMESSAGESIZE";

        /// <summary>
        /// Job property id name for ServiceOperationTimout
        /// </summary>
        public const string ServiceConfigOperationTimeout = "HPC_SERVICE_SERVICEOPERATIONTIMEOUT";

        /// <summary>
        /// Job property id name for SoaDiagTraceLevel flag.
        /// </summary>
        public const string SoaDiagTraceLevel = "HPC_SoaDiagTraceLevel";

        /// <summary>
        /// Job property id name for SoaDiagTraceCleanup flag (true/false).
        /// </summary>
        public const string SoaDiagTraceCleanup = "HPC_SoaDiagTraceCleanup";

        /// <summary>
        /// Job property id name for UseAzureQueue flag (true/false).
        /// </summary>
        public const string UseAzureQueue = "HPC_UseAzureQueue";

        /// <summary>
        /// Job property id name for DispatcherCapacityInGrowShrink.
        /// </summary>
        public const string DispatcherCapacityInGrowShrink = "HPC_DispatcherCapacityInGrowShrink";

        /// <summary>
        /// Job property id name for LocalUser flag (true/false).
        /// </summary>
        public const string LocalUser = "HPC_LocalUser";
    }
}
