//------------------------------------------------------------------------------
// <copyright file="EprElement.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Represents the configuration element for epr
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    using System.Configuration;

    /// <summary>
    /// Represents the configuration element for EPR
    /// </summary>
    public class EprElement : ConfigurationSection
    {
        /// <summary>
        /// Get the EPR string
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [ConfigurationProperty("epr", IsRequired = true)]
        public string Epr
        {
            get { return (string)this["epr"]; }
        }
    }
}
