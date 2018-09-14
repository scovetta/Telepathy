//------------------------------------------------------------------------------
// <copyright file="LoadBalancingConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents the load balancing configuration section group
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para>Contains the configuration properties for the load balancing section of the configuration file.</para>
    /// </summary>
    public class LoadBalancingConfiguration : ConfigurationSection
    {
        const string MessagesResendLimitConfigurationName = "messageResendLimit";
        const string MultiEmissionDelayTimeConfigurationName = "multiEmissionDelayTime";
        const string ServiceOperationTimeoutConfigurationName = "serviceOperationTimeout";
        const string EndpointNotFoundRetryCountLimitConfigurationName = "endpointNotFoundRetryCountLimit";
        const string EndpointNotFoundRetryPeriodConfigurationName = "endpointNotFoundRetryPeriod";
        const string ServiceRequestPrefetchCountConfigurationName = "serviceRequestPrefetchCount";
        const string MaxConnectionCountPerAzureProxyConfigurationName = "maxConnectionCountPerAzureProxy";
        const string DispatcherCapacityInGrowShrinkConfigurationName = "dispatcherCapacityInGrowShrink";

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// table to save configurations changed by the process
        /// </summary>
        Dictionary<string, object> updatedValues = new Dictionary<string, object>();

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Session.Configuration.LoadBalancingConfiguration" /> class.</para>
        /// </summary>
        public LoadBalancingConfiguration()
        {
            properties.Add(new ConfigurationProperty(MessagesResendLimitConfigurationName, typeof(int), 3));
            properties.Add(new ConfigurationProperty(MultiEmissionDelayTimeConfigurationName, typeof(int), -1));
            properties.Add(new ConfigurationProperty(ServiceOperationTimeoutConfigurationName, typeof(int), Constant.DefaultServiceOperationTimeout));
            properties.Add(new ConfigurationProperty(EndpointNotFoundRetryCountLimitConfigurationName, typeof(int), 10));

            //default EndpointNotFoundRetryPeriod: 5 minutes
            properties.Add(new ConfigurationProperty(EndpointNotFoundRetryPeriodConfigurationName, typeof(int), 5 * 60 * 1000));

            properties.Add(new ConfigurationProperty(ServiceRequestPrefetchCountConfigurationName, typeof(int), 1));

            // According to the test, 64 connections per deployment is too many, it often leads to the failure when open client.
            // Change it to 16, customers can set this in the load balancing configuration based on the network situation.
            properties.Add(new ConfigurationProperty(MaxConnectionCountPerAzureProxyConfigurationName, typeof(int), 16));

            properties.Add(new ConfigurationProperty(DispatcherCapacityInGrowShrinkConfigurationName, typeof(int), 0));
        }

        /// <summary>
        ///   <para>Represents a collection of configuration-element properties.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// <see cref="System.Configuration.ConfigurationPropertyCollection" /> object that contains a collection of ConfigurationProperty objects.</para>
        /// </value>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        /// Get the service request prefetch count 
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ServiceRequestPrefetchCount
        {
            get { return (int)this[ServiceRequestPrefetchCountConfigurationName]; }
        }

        /// <summary>
        /// Get or set endpoint not found retry count limit
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int EndpointNotFoundRetryCountLimit
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(EndpointNotFoundRetryCountLimitConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[EndpointNotFoundRetryCountLimitConfigurationName];
            }

            set
            {
                updatedValues[EndpointNotFoundRetryCountLimitConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set endpoint not found retry period
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int EndpointNotFoundRetryPeriod
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(EndpointNotFoundRetryPeriodConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[EndpointNotFoundRetryPeriodConfigurationName];
            }

            set
            {
                updatedValues[EndpointNotFoundRetryPeriodConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set multi-emission delay time
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int MultiEmissionDelayTime
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(MultiEmissionDelayTimeConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[MultiEmissionDelayTimeConfigurationName];
            }

            set
            {
                updatedValues[MultiEmissionDelayTimeConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The number of times that the broker will resend a message.</para>
        /// </summary>
        /// <value>
        ///   <para>The number of times that the broker will resend a message. The default is 3.</para>
        /// </value>
        /// <remarks>
        ///   <para>The message is discarded if it cannot be delivered within the limit.</para>
        /// </remarks>
        public int MessageResendLimit
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(MessagesResendLimitConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[MessagesResendLimitConfigurationName];
            }

            set
            {
                updatedValues[MessagesResendLimitConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>The length of time that the broker waits for the service to finish processing the message.</para>
        /// </summary>
        /// <value>
        ///   <para>The length of time, in milliseconds, that the broker waits for 
        /// the service to finish processing the message. The default is 86,400,000 milliseconds (24 hours).</para>
        /// </value>
        public int ServiceOperationTimeout
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(ServiceOperationTimeoutConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[ServiceOperationTimeoutConfigurationName];
            }

            set
            {
                updatedValues[ServiceOperationTimeoutConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set max connection count per Azure proxy
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int MaxConnectionCountPerAzureProxy
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(MaxConnectionCountPerAzureProxyConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[MaxConnectionCountPerAzureProxyConfigurationName];
            }

            set
            {
                updatedValues[MaxConnectionCountPerAzureProxyConfigurationName] = value;
            }
        }


        /// <summary>
        ///   <para>The dispatcher capacity considered in grow and shrink</para>
        /// </summary>
        /// <value>
        ///   <para>The capacity number. The default is zero, which means the capacity is auto calculated by the number of cores allocated.</para>
        /// </value>
        /// <remarks>
        ///   <para>Specify one if the resource type of the SOA job is node or socket, and the grow according to node or socket is expected.</para>
        /// </remarks>
        public int DispatcherCapacityInGrowShrink
        {
            get
            {
                object value;
                if (updatedValues.TryGetValue(DispatcherCapacityInGrowShrinkConfigurationName, out value))
                {
                    return (int)value;
                }

                return (int)this[DispatcherCapacityInGrowShrinkConfigurationName];
            }

            set
            {
                updatedValues[DispatcherCapacityInGrowShrinkConfigurationName] = value;
            }
        }

        /// <summary>
        ///   <para>Validates the contents of the configuration file.</para>
        /// </summary>
        /// <param name="errorMessage">
        ///   <para>If 
        /// <see cref=out "Microsoft.Hpc.Scheduler.Session.Configuration.LoadBalancingConfiguration.Validate(System.String)" /> returns false, this parameter contains the validation error.</para> 
        /// </param>
        /// <returns>
        ///   <para>Is true if the file validates; otherwise, false.</para>
        /// </returns>
        public bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (MessageResendLimit < 0)
            {
                errorMessage = SR.MessageRetryLimitNotNegative;
                return false;
            }

            if (ServiceOperationTimeout <= 0)
            {
                errorMessage = SR.InvalidServiceOperationTimeout;
                return false;
            }

            if (EndpointNotFoundRetryCountLimit < 0)
            {
                errorMessage = SR.EndpointNotFoundRetryCountLimitNotNegative;
                return false;
            }

            if (EndpointNotFoundRetryPeriod <= 0)
            {
                errorMessage = SR.InvalidEndpointNotFoundRetryPeriod;
                return false;
            }

            if (ServiceRequestPrefetchCount < 0)
            {
                errorMessage = SR.InvalidServiceRequestPrefetchCount;
                return false;
            }

            if (MaxConnectionCountPerAzureProxy <= 0)
            {
                errorMessage = SR.InvalidMaxConnectionCountPerAzureProxy;
                return false;
            }

            if (DispatcherCapacityInGrowShrink < 0)
            {
                errorMessage = SR.DispatcherCapacityInGrowShrinkNonNegative;
                return false;
            }

            return true;
        }

        //NOTE: override this function to ignore unrecognized attribute in the configuration section.
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return true;
        }

        //NOTE: override this function to ignore unrecognized element in the configuration section.
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="elementName">
        ///   <para />
        /// </param>
        /// <param name="reader">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            return true;
        }
    }
}
