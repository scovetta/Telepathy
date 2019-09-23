// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Helper class for manipulating configuration
    /// </summary>
    internal static class ConfigurationHelper
    {
        /// <summary>
        /// the name of the HpcServiceHosting trace soure
        /// </summary>
        private const string HpcServiceHostingSourceName = "Microsoft.Hpc.HpcServiceHosting";

        /// <summary>
        /// the name of the DiagnosticsElement in config file
        /// </summary>
        private const string DiagnosticsElement = "system.diagnostics";

        /// <summary>
        /// the name of the SourcesElement in config file
        /// </summary>
        private const string SourcesElement = "sources";

        /// <summary>
        /// the name of the SwitchesElement in config file
        /// </summary>
        private const string SwitchesElement = "switches";

        /// <summary>
        /// the name of the NameAttribute in config file
        /// </summary>
        private const string NameAttribute = "name";

        /// <summary>
        /// the name of the ValueAttribute in config file
        /// </summary>
        private const string ValueAttribute = "value";

        /// <summary>
        /// the name of the SwitchNameAttribute in config file
        /// </summary>
        private const string SwitchNameAttribute = "switchName";

        /// <summary>
        /// the name of the SwitchValueAttribute in config file
        /// </summary>
        private const string SwitchValueAttribute = "switchValue";

        /// <summary>
        /// Get trace switch value from the specified configuration
        /// </summary>
        /// <param name="config">configuration of the targeted service</param>
        /// <returns>switch value</returns>
        public static string GetTraceSwitchValue(Configuration config)
        {
            string switchValue = string.Empty;
            ConfigurationSection diagnostics = config.GetSection(DiagnosticsElement);
            ConfigurationElementCollection sources = diagnostics.ElementInformation.Properties[SourcesElement].Value as ConfigurationElementCollection;
            foreach (ConfigurationElement source in sources)
            {
                string name = source.ElementInformation.Properties[NameAttribute].Value as string;

                // search the trace source by name
                if (HpcServiceHostingSourceName == name)
                {
                    string switchName = source.ElementInformation.Properties[SwitchNameAttribute].Value as string;
                    if (switchName == null)
                    {
                        switchValue = source.ElementInformation.Properties[SwitchValueAttribute].Value as string;
                    }
                    else
                    {
                        // search the switch by the switchName
                        ConfigurationElementCollection switches = diagnostics.ElementInformation.Properties[SwitchesElement].Value as ConfigurationElementCollection;
                        foreach (ConfigurationElement switchElement in switches)
                        {
                            string switchElementName = switchElement.ElementInformation.Properties[NameAttribute].Value as string;
                            if (string.Equals(switchName, switchElementName, StringComparison.CurrentCulture))
                            {
                                switchValue = switchElement.ElementInformation.Properties[ValueAttribute].Value as string;
                                break;
                            }
                        }
                    }

                    break;
                }
            }

            return switchValue;
        }
    }
}
