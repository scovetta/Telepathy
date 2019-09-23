// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.ServiceModel.Configuration;
    using System.Text;

    using Microsoft.Telepathy.Common;
    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Common;
    using Microsoft.Telepathy.Session.Configuration;
    using Microsoft.Telepathy.Session.Exceptions;
    using Microsoft.Telepathy.Session.Interface;

    /// <summary>
    /// Helper class to load the configuration
    /// </summary>
    internal static class ConfigurationHelper
    {
        /// <summary>
        /// The min Allocation adjust time.
        /// </summary>
        private const int MinAllocationAdjustTime = 3000;

        /// <summary>
        /// Stores the bindings section name
        /// </summary>
        private const string BindingsSectionName = "system.serviceModel/bindings";

        /// <summary>
        /// Store the default broker configuration
        /// </summary>
        private static readonly BrokerConfigurations defaultBrokerConfiguration;

        /// <summary>
        /// Initializes static members of the ConfigurationHelper class
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Need to initialize properties")]
        static ConfigurationHelper()
        {
            defaultBrokerConfiguration = new BrokerConfigurations();
            InitMonitorConfig();
            InitServicesConfig();
            InitLoadBalancingConfig();
        }

        /// <summary>
        /// Load the configuration from the configuration file
        /// If no configuration is found or some part is missing, we will fill the blank with default value
        /// </summary>
        /// <param name="brokerSettings">indicate the broker settings from the session start info, this settings will override the settings load from the configuration file</param>
        /// <param name="brokerInfo">indicating the broker info</param>
        /// <param name="brokerConfig">out the broker configurations</param>
        /// <param name="serviceConfig">out the service configurations</param>
        /// <param name="bindings">output the bindings</param>
        public static void LoadConfiguration(SessionStartInfoContract brokerSettings, BrokerStartInfo brokerInfo, out BrokerConfigurations brokerConfig, out ServiceConfiguration serviceConfig, out BindingsSection bindings)
        {
            // Init config file
            string filename = brokerInfo.ConfigurationFile;
            BrokerTracing.TraceVerbose("[ConfigurationHelper] LoadConfiguration. Step 1: Load configuration file name: {0}", filename);

            brokerConfig = null;
            serviceConfig = null;
            bindings = null;

            try
            {
                ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                map.ExeConfigFilename = filename;
                Configuration config = null;
                RetryManager.RetryOnceAsync(
                        () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                        TimeSpan.FromSeconds(1),
                        ex => ex is ConfigurationErrorsException)
                    .GetAwaiter()
                    .GetResult();
                brokerConfig = BrokerConfigurations.GetSectionGroup(config);
                serviceConfig = ServiceRegistration.GetSectionGroup(config).Service;
                bindings = (BindingsSection)config.GetSection(BindingsSectionName);
            }
            catch (ConfigurationErrorsException e)
            {
                ThrowHelper.ThrowSessionFault(SOAFaultCode.ConfigFile_Invalid,
                                              "{0}",
                                              e.ToString());
            }

            if (brokerConfig == null)
            {
                // Set the default value
                brokerConfig = defaultBrokerConfiguration;
                BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[ConfigurationHelper] Didn't find the broker config from the configuration file, use the default configuration");
            }
            else
            {
                // Set the default configuration if the very section is not found
                if (brokerConfig.Monitor == null)
                {
                    brokerConfig.Monitor = defaultBrokerConfiguration.Monitor;
                    BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[ConfigurationHelper] Didn't find the monitor config from the configuration file, use the default configuration");
                }

                if (brokerConfig.Services == null)
                {
                    brokerConfig.Services = defaultBrokerConfiguration.Services;
                    BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[ConfigurationHelper] Didn't find the services config from the configuration file, use the default configuration");
                }

                if (brokerConfig.LoadBalancing == null)
                {
                    brokerConfig.LoadBalancing = defaultBrokerConfiguration.LoadBalancing;
                    BrokerTracing.TraceEvent(TraceEventType.Information, 0, "[ConfigurationHelper] Didn't find the load balancing config from the configuration file, use the default configuration");
                }
            }

            BrokerTracing.TraceVerbose("[ConfigurationHelper] LoadConfiguration. Step 2: Load broker config and service config succeeded.");

            if (brokerConfig.Monitor.AllocationAdjustInterval < MinAllocationAdjustTime && brokerConfig.Monitor.AllocationAdjustInterval != System.Threading.Timeout.Infinite)
            {
                brokerConfig.Monitor.AllocationAdjustInterval = MinAllocationAdjustTime;
            }

            // Update the broker config using the session start info
            if (brokerSettings.ClientIdleTimeout.HasValue)
            {
                brokerConfig.Monitor.ClientIdleTimeout = brokerSettings.ClientIdleTimeout.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default ClientIdleTimeout to {0}", brokerConfig.Monitor.ClientIdleTimeout);
            }

            if (brokerSettings.ClientConnectionTimeout.HasValue)
            {
                brokerConfig.Monitor.ClientConnectionTimeout = brokerSettings.ClientConnectionTimeout.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default ClientConnectionTimeout to {0}", brokerConfig.Monitor.ClientConnectionTimeout);
            }

            if (brokerSettings.SessionIdleTimeout.HasValue)
            {
                brokerConfig.Monitor.SessionIdleTimeout = brokerSettings.SessionIdleTimeout.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default SessionIdleTimeout to {0}", brokerConfig.Monitor.SessionIdleTimeout);
            }

            if (brokerSettings.MessagesThrottleStartThreshold.HasValue)
            {
                brokerConfig.Monitor.MessageThrottleStartThreshold = brokerSettings.MessagesThrottleStartThreshold.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default MessageThrottleStartThreshold to {0}", brokerConfig.Monitor.MessageThrottleStartThreshold);
            }

            if (brokerSettings.MessagesThrottleStopThreshold.HasValue)
            {
                brokerConfig.Monitor.MessageThrottleStopThreshold = brokerSettings.MessagesThrottleStopThreshold.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default MessageThrottleStopThreshold to {0}", brokerConfig.Monitor.MessageThrottleStopThreshold);
            }

            if (brokerSettings.ClientBrokerHeartbeatRetryCount.HasValue)
            {
                brokerConfig.Monitor.ClientBrokerHeartbeatRetryCount = brokerSettings.ClientBrokerHeartbeatRetryCount.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default ClientBrokerHeartbeatRetryCount to {0}", brokerConfig.Monitor.ClientBrokerHeartbeatRetryCount);
            }

            if (brokerSettings.ClientBrokerHeartbeatInterval.HasValue)
            {
                brokerConfig.Monitor.ClientBrokerHeartbeatInterval = brokerSettings.ClientBrokerHeartbeatInterval.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default ClientBrokerHeartbeatInterval to {0}", brokerConfig.Monitor.ClientBrokerHeartbeatInterval);
            }

            if (brokerSettings.ServiceOperationTimeout.HasValue)
            {
                brokerConfig.LoadBalancing.ServiceOperationTimeout = brokerSettings.ServiceOperationTimeout.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default ServiceOperationTimeout to {0}", brokerConfig.LoadBalancing.ServiceOperationTimeout);
            }

            if (brokerSettings.DispatcherCapacityInGrowShrink.HasValue)
            {
                brokerConfig.LoadBalancing.DispatcherCapacityInGrowShrink = brokerSettings.DispatcherCapacityInGrowShrink.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default DispatcherCapacityInGrowShrink to {0}", brokerConfig.LoadBalancing.DispatcherCapacityInGrowShrink);
            }

            if (brokerSettings.MaxMessageSize.HasValue)
            {
                serviceConfig.MaxMessageSize = brokerSettings.MaxMessageSize.Value;
                BrokerTracing.TraceEvent(TraceEventType.Verbose, 0, "[ConfigurationHelper] Modified default MaxMessageSize to {0}", serviceConfig.MaxMessageSize);
            }

            BrokerTracing.TraceVerbose("[ConfigurationHelper] LoadConfiguration. Step 3: Override broker settings using session start info succeeded.");

            // Validate the config section
            string configError;
            bool validateSucceeded;
            try
            {
                validateSucceeded = brokerConfig.Validate(out configError);
            }
            catch (ConfigurationErrorsException e)
            {
                validateSucceeded = false;
                configError = e.Message;
            }

            if (!validateSucceeded)
            {
                BrokerTracing.TraceEvent(TraceEventType.Error, 0, "[ConfigurationHelper] Invalid broker configuration section. Error {0}", configError);
                ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_InvalidConfiguration, configError);
            }

            BrokerTracing.TraceVerbose("[ConfigurationHelper] LoadConfiguration. Step 4: Validate broker configuration succeeded.");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Monitor]");
            BrokerTracing.WriteProperties(sb, brokerConfig.Monitor, 3, typeof(int), typeof(string));
            sb.AppendLine("[BaseAddress]");
            sb.AppendFormat("   Http = {0}\n", brokerConfig.Services.GetBrokerBaseAddress("http"));
            sb.AppendFormat("   Https = {0}\n", brokerConfig.Services.GetBrokerBaseAddress("https"));
            sb.AppendFormat("   NetTcp = {0}\n", brokerConfig.Services.GetBrokerBaseAddress("net.tcp"));
            sb.AppendLine("[LoadBalancing]");
            BrokerTracing.WriteProperties(sb, brokerConfig.LoadBalancing, 3, typeof(int), typeof(string));
            BrokerTracing.TraceVerbose("[ConfigurationHelper] BrokerConfiguration: \n{0}", sb.ToString());
            sb = new StringBuilder();
            sb.AppendLine("[Service]");
            BrokerTracing.WriteProperties(sb, serviceConfig, 3, typeof(int), typeof(string));
            BrokerTracing.TraceVerbose("[ConfigurationHelper] ServiceConfiguration: \n{0}", sb.ToString());
        }

        /// <summary>
        /// Initializes the load balancing configuration
        /// </summary>
        private static void InitLoadBalancingConfig()
        {
            LoadBalancingConfiguration loadBalancing = new LoadBalancingConfiguration();
            loadBalancing.MessageResendLimit = 3;
            loadBalancing.ServiceOperationTimeout = 86400000;
            defaultBrokerConfiguration.LoadBalancing = loadBalancing;
        }

        /// <summary>
        /// Initializes the services configuration
        /// </summary>
        private static void InitServicesConfig()
        {
            BrokerServicesConfiguration services = new BrokerServicesConfiguration();
            defaultBrokerConfiguration.Services = services;
        }

        /// <summary>
        /// Initializes the monitor configuration
        /// </summary>
        private static void InitMonitorConfig()
        {
            BrokerMonitorConfiguration monitor = new BrokerMonitorConfiguration();
            defaultBrokerConfiguration.Monitor = monitor;
        }
    }
}
