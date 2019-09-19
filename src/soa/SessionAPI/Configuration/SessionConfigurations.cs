// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the session start information configuration section group
    /// </summary>
    public class SessionConfigurations : ConfigurationSectionGroup
    {
        /// <summary>
        /// Stores the name of the configuration section group
        /// </summary>
        private const string SessionStartInfoConfigurationName = "microsoft.Hpc.Session";

        /// <summary>
        /// Get the <see cref="SessionConfigurations"/>
        /// </summary>
        /// <param name="config">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        public static SessionConfigurations GetSectionGroup(System.Configuration.Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            return config.SectionGroups[SessionStartInfoConfigurationName] as SessionConfigurations;
        }


        /// <summary>
        /// Get a value indicates if is in debug mode
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ConfigurationProperty("debugMode", IsRequired = false)]
        public DebugModeSection DebugMode
        {
            get { return this.Sections["debugMode"] as DebugModeSection; }
        }
    }
}
