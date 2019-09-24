// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    ///   <para>Represents the service registration information.</para>
    /// </summary>
    public sealed class ServiceRegistration : ConfigurationSectionGroup
    {
        const string ServiceConfigurationsName = "microsoft.Hpc.Session.ServiceRegistration";

        /// <summary>
        ///   <para>Gets the service registration section group from the specified configuration file.</para>
        /// </summary>
        /// <param name="config">
        ///   <para>A configuration object that represents a configuration file.</para>
        /// </param>
        /// <returns>
        ///   <para>An 
        /// 
        /// <see cref="ServiceRegistration" /> object that contains the service registration section of the configuration file.  
        /// </para>
        /// </returns>
        public static ServiceRegistration GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }
            return config.SectionGroups[ServiceConfigurationsName] as ServiceRegistration;
        }

        /// <summary>
        ///   <para>Gets the service section of the service registration configuration file.</para>
        /// </summary>
        /// <value>
        ///   <para>A <see cref="ServiceConfiguration" /> object that represents the service section.
        /// </para>
        /// </value>
        public ServiceConfiguration Service
        {
            get
            {
                return base.Sections["service"] as ServiceConfiguration;
            }
        }

        /// <summary>
        /// Get the service host configuration
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public HostConfiguration Host
        {
            get
            {
                return base.Sections["host"] as HostConfiguration;
            }
        }
    }
}
