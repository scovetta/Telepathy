// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.ServiceModel.Configuration;

namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    /// <summary>
    ///   <para>Contains the configuration properties for the services section of the configuration file.</para>
    /// </summary>
    public sealed class BrokerServicesConfiguration : ConfigurationSection
    {
        const string BrokerServiceAddressesConfiguratoinName = "brokerServiceAddresses";

        ConfigurationProperty brokerServiceAddresses = new ConfigurationProperty(BrokerServiceAddressesConfiguratoinName, typeof(BaseAddressElementCollection));

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();


        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Session.Configuration.BrokerServicesConfiguration" /> class.</para>
        /// </summary>
        public BrokerServicesConfiguration()
        {
            properties.Add(brokerServiceAddresses);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        ///   <para>Gets the base address for the specified transport scheme.</para>
        /// </summary>
        /// <param name="scheme">
        ///   <para>The transport scheme. The possible values are http, https, and net.tcp.</para>
        /// </param>
        /// <returns>
        ///   <para>An <see cref="System.Uri" /> object that contains the base address.</para>
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Backward compatibility")]
        public Uri GetBrokerBaseAddress(string scheme)
        {
            BaseAddressElementCollection baseAddresses = base[BrokerServiceAddressesConfiguratoinName] as BaseAddressElementCollection;
            return FindBaseAddress(baseAddresses, scheme);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Backward compatibility")]
        Uri FindBaseAddress(BaseAddressElementCollection baseAddresses, string scheme)
        {
            if (baseAddresses != null)
            {
                foreach (BaseAddressElement baseAddress in baseAddresses)
                {
                    Uri uri = new Uri(baseAddress.BaseAddress);
                    if (uri.Scheme.Equals(scheme, StringComparison.OrdinalIgnoreCase))
                    {
                        return uri;
                    }
                }
            }

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "May add validatation in the future")]
        internal bool Validate(out string errorMessage)
        {
            errorMessage = string.Empty;
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
