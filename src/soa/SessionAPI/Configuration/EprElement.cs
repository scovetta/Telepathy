// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
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
