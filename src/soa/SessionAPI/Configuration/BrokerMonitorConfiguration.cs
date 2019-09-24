// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;

    /// <summary>
    ///   <para>Contains the configuration properties for the monitor section of the configuration file.</para>
    /// </summary>
    public sealed class BrokerMonitorConfiguration : ConfigurationSection
    {
        const string LoadSamplingIntervalConfigurationName = "loadSamplingInterval";
        const string AllocationAdjustIntervalConfigurationName = "allocationAdjustInterval";
        const string ClientIdleTimeoutConfigurationName = "clientIdleTimeout";
        const string ClientConnectionTimeoutConfigurationName = "clientConnectionTimeout";
        const string SessionIdleTimeoutConfigurationName = "sessionIdleTimeout";
        const string StatusUpdateIntervalConfigurationName = "statusUpdateInterval";
        const string MessageThrottleStartThresholdConfigurationName = "messageThrottleStartThreshold";
        const string MessageThrottleStopThresholdConfigurationName = "messageThrottleStopThreshold";
        const string ClientBrokerHeartbeatIntervalConfigurationName = "clientBrokerHeartbeatInterval";
        const string ClientBrokerHeartbeatRetryCountConfigurationName = "clientBrokerHeartbeatRetryCount";

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// table to save configurations changed by the process
        /// </summary>
        Dictionary<string, object> updatedValues = new Dictionary<string, object>();

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="BrokerMonitorConfiguration" /> class.</para>
        /// </summary>
        public BrokerMonitorConfiguration()
        {
            this.properties.Add(new ConfigurationProperty(LoadSamplingIntervalConfigurationName, typeof(int), 1000));
            this.properties.Add(new ConfigurationProperty(AllocationAdjustIntervalConfigurationName, typeof(int), 5 * 1000));
            this.properties.Add(new ConfigurationProperty(ClientIdleTimeoutConfigurationName, typeof(int), 5 * 60 * 1000));
            this.properties.Add(new ConfigurationProperty(ClientConnectionTimeoutConfigurationName, typeof(int), 5 * 60 * 1000));
            this.properties.Add(new ConfigurationProperty(SessionIdleTimeoutConfigurationName, typeof(int), 5 * 60 * 1000));
            this.properties.Add(new ConfigurationProperty(StatusUpdateIntervalConfigurationName, typeof(int), 3 * 1000));
            this.properties.Add(new ConfigurationProperty(MessageThrottleStartThresholdConfigurationName, typeof(int), 4096));
            this.properties.Add(new ConfigurationProperty(MessageThrottleStopThresholdConfigurationName, typeof(int), 3072));
            this.properties.Add(new ConfigurationProperty(ClientBrokerHeartbeatIntervalConfigurationName, typeof(int), 20000));
            this.properties.Add(new ConfigurationProperty(ClientBrokerHeartbeatRetryCountConfigurationName, typeof(int), 3));
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        ///   <para>The interval at which the broker checks the load capacity of the service.</para>
        /// </summary>
        /// <value>
        ///   <para>The load sampling interval, in milliseconds. The default is 1,000 milliseconds (one second).</para>
        /// </value>
        public int LoadSamplingInterval
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(LoadSamplingIntervalConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[LoadSamplingIntervalConfigurationName];
            }

            set
            {
                this.updatedValues[LoadSamplingIntervalConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The interval at which you want to grow or shrink the capacity of the services.</para>
        /// </summary>
        /// <value>
        ///   <para>The length of the interval in milliseconds that elapses between successive checks that the broker makes 
        /// of the load ratios for the session to determine whether to grow or shrink capacity. The default is 60,000 milliseconds.</para>
        /// </value>
        public int AllocationAdjustInterval
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(AllocationAdjustIntervalConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[AllocationAdjustIntervalConfigurationName];
            }

            set
            {
                this.updatedValues[AllocationAdjustIntervalConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The amount of time that the client can go without sending requests to the web-service.</para>
        /// </summary>
        /// <value>
        ///   <para>The amount of time, in milliseconds, that the client 
        /// can go without sending requests to the web-service. The default is five minutes.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the idle timeout period is exceeded, the session is terminated.</para>
        ///   <para>You must cast the value to an integer. If the value is null (means that the value 
        /// has not been set and is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        public int ClientIdleTimeout
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(ClientIdleTimeoutConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[ClientIdleTimeoutConfigurationName];
            }

            set
            {
                this.updatedValues[ClientIdleTimeoutConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The time in which the client must connect to the web-service after creating the session.</para>
        /// </summary>
        /// <value>
        ///   <para>The time, in milliseconds, in which the client must connect to the web-service. The default is five minutes.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the client does not connect within the timeout period, 
        /// the session broker is terminated if no other clients are using the broker.</para>
        ///   <para>You must cast the value to an integer. If the value is null (means that the value 
        /// has not been set and is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        public int ClientConnectionTimeout
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(ClientConnectionTimeoutConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[ClientConnectionTimeoutConfigurationName];
            }

            set
            {
                this.updatedValues[ClientConnectionTimeoutConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The amount of time that the broker waits for a client to connect after all previous client sessions ended.</para>
        /// </summary>
        /// <value>
        ///   <para>The amount of time, in milliseconds, that the broker waits for a client to connect. The default is zero.</para>
        /// </value>
        /// <remarks>
        ///   <para>If the timeout period is exceeded, the broker ends. This property is useful only for shared sessions.</para>
        /// </remarks>
        public int SessionIdleTimeout
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(SessionIdleTimeoutConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[SessionIdleTimeoutConfigurationName];
            }

            set
            {
                this.updatedValues[SessionIdleTimeoutConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The interval at which the broker updates service-specific job properties in the scheduler. </para>
        /// </summary>
        /// <value>
        ///   <para>The update interval, in milliseconds. The default is 15,000 milliseconds (five seconds). </para>
        /// </value>
        /// <remarks>
        ///   <para>The service job contains several properties that are specific to services (For 
        /// example, EndpointAddresses, CallDuration, and CallsPerSecond). The service-specific properties are updated each time the interval expires. </para>
        /// </remarks>
        public int StatusUpdateInterval
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(StatusUpdateIntervalConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[StatusUpdateIntervalConfigurationName];
            }

            set
            {
                this.updatedValues[StatusUpdateIntervalConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The upper threshold at which the broker stops receiving messages from the clients.</para>
        /// </summary>
        /// <value>
        ///   <para>The upper threshold of queued messages. The default is 5,120 messages.</para>
        /// </value>
        /// <remarks>
        ///   <para>You must cast the value to an integer. If the value is null (means that the value 
        /// has not been set and is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        public int MessageThrottleStartThreshold
        {
            get
            {
                return (int)this[MessageThrottleStartThresholdConfigurationName];
            }

            set
            {
                this[MessageThrottleStartThresholdConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The lower threshold at which the broker begins receiving messages from the clients.</para>
        /// </summary>
        /// <value>
        ///   <para>The lower threshold of queued messages. The default is 3,840 messages.</para>
        /// </value>
        /// <remarks>
        ///   <para>You must cast the value to an integer. If the value is null (means that the value 
        /// has not been set and is using the default value set in the configuration file), the cast raises an exception.</para>
        /// </remarks>
        public int MessageThrottleStopThreshold
        {
            get
            {
                //if not defined use 3/4 of throttle limit
                if (this[MessageThrottleStopThresholdConfigurationName] == null)
                {
                    return (this.MessageThrottleStartThreshold * 3) / 4;
                }

                return (int)this[MessageThrottleStopThresholdConfigurationName];
            }

            set
            {
                this[MessageThrottleStopThresholdConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set the client broker heart beat interval
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ClientBrokerHeartbeatInterval
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(ClientBrokerHeartbeatIntervalConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[ClientBrokerHeartbeatIntervalConfigurationName];
            }

            set
            {
                this.updatedValues[ClientBrokerHeartbeatIntervalConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set the client broker heart beat retry count
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ClientBrokerHeartbeatRetryCount
        {
            get
            {
                object value;
                if (this.updatedValues.TryGetValue(ClientBrokerHeartbeatRetryCountConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[ClientBrokerHeartbeatRetryCountConfigurationName];
            }

            set
            {
                this.updatedValues[ClientBrokerHeartbeatRetryCountConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>Validates the contents of the configuration file.</para>
        /// </summary>
        /// <param name="errorMessage">
        ///   <para>If 
        /// 
        /// <see cref="Validate" /> returns false, this parameter contains the validation error.</para> 
        /// </param>
        /// <returns>
        ///   <para>Is true if the file validates; otherwise, false.</para>
        /// </returns>
        public bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (this.LoadSamplingInterval <= 0)
            {
                errorMessage = SR.LoadSamplingIntervalPositive;
                return false;
            }

            if (this.ClientIdleTimeout < 0)
            {
                errorMessage = SR.ClientIdleTimeoutNotNegative;
                return false;
            }

            if (this.SessionIdleTimeout < 0)
            {
                errorMessage = SR.SessionIdleTimeoutNotNegative;
                return false;
            }

            if (this.StatusUpdateInterval <= 0)
            {
                errorMessage = SR.StatusUpdateIntervalPositive;
                return false;
            }

            if (this.MessageThrottleStartThreshold <= this.MessageThrottleStopThreshold)
            {
                errorMessage = SR.MessageThrottleStartGreaterStop;
                return false;
            }

            if (this.MessageThrottleStopThreshold < 0)
            {
                errorMessage = SR.MessageThrottleStopThresholdPositive;
                return false;
            }

            if (this.ClientBrokerHeartbeatInterval < 0)
            {
                errorMessage = SR.ClientBrokerHeartbeatIntervalPositive;
                return false;
            }

            if (this.ClientBrokerHeartbeatRetryCount < 0)
            {
                errorMessage = SR.ClientBrokerHeartbeatRetryCountPositive;
                return false;
            }

            return true;
        }

        //NOTE: override this function to ignore unrecognized attribute in the configuration section.
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return true;
        }

        //NOTE: override this function to ignore unrecognized element in the configuration section.
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            return true;
        }
    }
}

