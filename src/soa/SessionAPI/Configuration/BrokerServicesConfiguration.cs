// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Configuration;

    /// <summary>
    ///   <para>Contains the configuration properties for the services section of the configuration file.</para>
    /// </summary>
    public sealed class BrokerServicesConfiguration : ConfigurationSection
    {
        const string BrokerServiceAddressesConfigurationName = "brokerServiceAddresses";

        ConfigurationProperty brokerServiceAddresses = new ConfigurationProperty(BrokerServiceAddressesConfigurationName, typeof(BaseAddressElementCollection));

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();


        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="BrokerServicesConfiguration" /> class.</para>
        /// </summary>
        public BrokerServicesConfiguration()
        {
            this.properties.Add(this.brokerServiceAddresses);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
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
            BaseAddressElementCollection baseAddresses = base[BrokerServiceAddressesConfigurationName] as BaseAddressElementCollection;
            return this.FindBaseAddress(baseAddresses, scheme);
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
