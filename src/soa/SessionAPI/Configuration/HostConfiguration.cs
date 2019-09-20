// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System;
    using System.Configuration;

    /// <summary>
    /// The implementation of host configuration
    /// </summary>
    public sealed class HostConfiguration : ConfigurationSection
    {
        const string HostTypeConfiguratoinName = "hostType";
        const string ExeFileConfiguratoinName = "exeFileName";

        /// <summary>
        /// Enumerable represents host types
        /// </summary>
        public enum HostType
        {
            /// <summary>
            /// Host type not set
            /// </summary>
            None = 0,

            /// <summary>
            /// Standard host type
            /// </summary>
            Standard = 1,

            /// <summary>
            /// Customized host type
            /// </summary>
            Customize = 2
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// Constructor of <see cref="HostConfiguration"/>
        /// </summary>
        public HostConfiguration()
        {
            this.properties.Add(new ConfigurationProperty(HostTypeConfiguratoinName, typeof(HostType), HostType.Standard));
            this.properties.Add(new ConfigurationProperty(ExeFileConfiguratoinName, typeof(string), String.Empty));
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return this.properties;
            }
        }

        /// <summary>
        /// Get the exe file configuration path
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string Path
        {
            get
            {
                if (this.Type == HostType.Standard)
                    return null;

                return (string)this[ExeFileConfiguratoinName];
            }
        }

        /// <summary>
        /// Get the <see cref="HostType"/>
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public HostType Type
        {
            get
            {
                return (HostType)this[HostTypeConfiguratoinName];
            }
        }

        /// <summary>
        /// Do validation
        /// </summary>
        /// <param name="errorMessage">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Backward compatibility")]
        public bool Validate(out string errorMessage)
        {
            errorMessage = String.Empty;
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
