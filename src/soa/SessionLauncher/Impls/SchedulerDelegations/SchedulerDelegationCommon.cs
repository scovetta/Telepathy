// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Impls.SchedulerDelegations
{
    using System.Collections.Generic;

    using Microsoft.Telepathy.Session.Internal;

    internal static class SchedulerDelegationCommon
    {
        /// <summary>
        /// SOA related customized property names
        /// </summary>
        internal static List<string> CustomizedPropertyNames { get; } = new List<string>();

        /// <summary>
        /// customized property name maps to the env variable name
        /// </summary>
        internal static Dictionary<string, string> PropToEnvMapping { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Initializes custom job property ids
        /// </summary>
        static SchedulerDelegationCommon()
        {
            CustomizedPropertyNames.AddRange(
                new string[]
                    {
                        BrokerSettingsConstants.BrokerNode, BrokerSettingsConstants.Suspended, BrokerSettingsConstants.Durable, BrokerSettingsConstants.ShareSession, BrokerSettingsConstants.UseAad,
                        BrokerSettingsConstants.AadUserIdentity, BrokerSettingsConstants.Secure, BrokerSettingsConstants.TransportScheme, BrokerSettingsConstants.UseAzureQueue,
                        BrokerSettingsConstants.LocalUser, BrokerSettingsConstants.AllocationGrowLoadRatioThreshold, BrokerSettingsConstants.AllocationShrinkLoadRatioThreshold,
                        BrokerSettingsConstants.ClientIdleTimeout, BrokerSettingsConstants.SessionIdleTimeout, BrokerSettingsConstants.MessagesThrottleStartThreshold,
                        BrokerSettingsConstants.MessagesThrottleStopThreshold, BrokerSettingsConstants.ClientConnectionTimeout, BrokerSettingsConstants.Faulted, BrokerSettingsConstants.Calculated,
                        BrokerSettingsConstants.Calculating, BrokerSettingsConstants.PurgedTotal, BrokerSettingsConstants.PurgedProcessed, BrokerSettingsConstants.PurgedFaulted,
                        BrokerSettingsConstants.ServiceVersion, BrokerSettingsConstants.PersistVersion, BrokerSettingsConstants.ServiceConfigMaxMessageSize,
                        BrokerSettingsConstants.ServiceConfigOperationTimeout, BrokerSettingsConstants.SoaDiagTraceLevel, BrokerSettingsConstants.SoaDiagTraceCleanup,
                        BrokerSettingsConstants.MessageDetailsAvailable, BrokerSettingsConstants.Reemitted, BrokerSettingsConstants.DispatcherCapacityInGrowShrink
                    });

            PropToEnvMapping.Add("EndpointReference", "HPC_EndpointReference");
            PropToEnvMapping.Add("NumberOfCalls", "HPC_NumberOfCalls");
            PropToEnvMapping.Add("NumberOfOutstandingCalls", "HPC_NumberOfOutstandingCalls");
            PropToEnvMapping.Add("CallDuration", "HPC_CallDuration");
            PropToEnvMapping.Add("CallsPerSecond", "HPC_CallsPerSecond");
            PropToEnvMapping.Add(BrokerSettingsConstants.SoaDiagTraceLevel, Constant.TraceSwitchValue);
        }
    }
}