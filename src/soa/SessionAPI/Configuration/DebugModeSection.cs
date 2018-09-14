//------------------------------------------------------------------------------
// <copyright file="DebugModeSection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents the debugMode configuration section
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents the debugMode configuration section
    /// </summary>
    public class DebugModeSection : ConfigurationSection
    {
        /// <summary>
        /// Get the EPR collection
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ConfigurationProperty("eprList", IsDefaultCollection = false, IsRequired = true)]
        public EprCollection EprCollection
        {
            get { return this["eprList"] as EprCollection; }
        }

        /// <summary>
        /// Get a value indicates if debugMode is enabled
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ConfigurationProperty("enabled", IsRequired = true)]
        public bool Enabled
        {
            get { return Convert.ToBoolean(this["enabled"]); }
        }
    }
}
