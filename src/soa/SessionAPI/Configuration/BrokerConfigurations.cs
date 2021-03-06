// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    ///   <para>Contains the properties used to access the different sections of a session’s configuration file.</para>
    /// </summary>
    /// <remarks>
    ///   <para>Use the 
    /// 
    /// <see cref="GetSectionGroup" /> static method to load a  
    /// <see cref="BrokerConfigurations" /> object with the session’s configuration file.</para>
    /// </remarks>
    public class BrokerConfigurations : ConfigurationSectionGroup
    {
        /// <summary>
        /// Stores the broker configuration name
        /// </summary>
        private const string BrokerConfigurationsName = "microsoft.Hpc.Broker";

        /// <summary>
        /// Stores the updated monitor config
        /// </summary>
        private BrokerMonitorConfiguration monitor;

        /// <summary>
        /// Stores the updated services config
        /// </summary>
        private BrokerServicesConfiguration services;

        /// <summary>
        /// Stores the updated load balancing config
        /// </summary>
        private LoadBalancingConfiguration loadBalancing;

        /// <summary>
        /// Stores the custom broker configuration
        /// </summary>
        private CustomBrokerRegistration customBroker;

        /// <summary>
        /// Get or set the custom broker registration
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public CustomBrokerRegistration CustomBroker
        {
            get
            {
                if (this.customBroker == null)
                {
                    return this.Sections["customBroker"] as CustomBrokerRegistration;
                }

                return this.customBroker;
            }

            set
            {
                this.customBroker = value;
            }
        }

        /// <summary>
        ///   <para>Retrieves the monitor section of the configuration file.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="BrokerMonitorConfiguration" /> object that contains the properties of the monitor section of the configuration file.  
        /// </para>
        /// </value>
        public BrokerMonitorConfiguration Monitor
        {
            get
            {
                if (this.monitor == null)
                {
                    return this.Sections["monitor"] as BrokerMonitorConfiguration;
                }

                return this.monitor;
            }

            set
            {
                this.monitor = value;
            }
        }

        /// <summary>
        ///   <para>Retrieves the services section of the configuration file.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="BrokerServicesConfiguration" /> object that contains the properties of the services section of the configuration file.  
        /// </para>
        /// </value>
        public BrokerServicesConfiguration Services
        {
            get
            {
                if (this.services == null)
                {
                    return this.Sections["services"] as BrokerServicesConfiguration;
                }

                return this.services;
            }

            set
            {
                this.services = value;
            }
        }

        /// <summary>
        ///   <para>Retrieves the loadBalancing section of the configuration file.</para>
        /// </summary>
        /// <value>
        ///   <para>A 
        /// 
        /// <see cref="LoadBalancingConfiguration" /> object that contains the properties of the loadBalancing section of the configuration file.  
        /// </para>
        /// </value>
        public LoadBalancingConfiguration LoadBalancing
        {
            get
            {
                if (this.loadBalancing == null)
                {
                    return this.Sections["loadBalancing"] as LoadBalancingConfiguration;
                }

                return this.loadBalancing;
            }

            set
            {
                this.loadBalancing = value;
            }
        }

        /// <summary>
        ///   <para>Gets the configuration section group from the specified configuration file.</para>
        /// </summary>
        /// <param name="config">
        ///   <para>A configuration object that represents a configuration file.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="BrokerConfigurations" /> object that contains the sections of the session’s configuration file.  
        /// </para>
        /// </returns>
        public static BrokerConfigurations GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            return config.SectionGroups[BrokerConfigurationsName] as BrokerConfigurations;
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
            if (this.Monitor == null)
            {
                errorMessage = SR.MonitorConfigurationMissing;
                return false;
            }

            if (!this.Monitor.Validate(out errorMessage))
            {
                return false;
            }

            if (this.Services == null)
            {
                errorMessage = SR.ServicesConfigurationMissing;
                return false;
            }

            if (!this.Services.Validate(out errorMessage))
            {
                return false;
            }

            if (this.LoadBalancing == null)
            {
                errorMessage = SR.LoadBalancingConfigurationMissing;
                return false;
            }

            return this.LoadBalancing.Validate(out errorMessage);
        }
    }
}
