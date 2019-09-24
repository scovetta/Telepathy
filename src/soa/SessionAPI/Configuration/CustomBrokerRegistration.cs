// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Configuration
{
    using System.Configuration;

    /// <summary>
    ///   <para />
    /// </summary>
    public class CustomBrokerRegistration : ConfigurationSection
    {
        /// <summary>
        /// Stores the environment variables configuration name
        /// </summary>
        private const string EnvVarsConfigurationName = "environmentVariables";

        /// <summary>
        /// Stores the executive attribute name
        /// </summary>
        private const string ExecutiveAttributeName = "executive";

        /// <summary>
        /// Stores the environment variables configuration
        /// </summary>
        private ConfigurationProperty envVariables = new ConfigurationProperty(EnvVarsConfigurationName, typeof(NameValueConfigurationCollection));

        /// <summary>
        /// Stores the configuration properties
        /// </summary>
        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// Constructor of <see cref="CustomBrokerRegistration"/>
        /// </summary>
        public CustomBrokerRegistration()
        {
            this.properties.Add(new ConfigurationProperty(ExecutiveAttributeName, typeof(string)));
            this.properties.Add(this.envVariables);
        }
        
        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }

        /// <summary>
        /// Get the executive attribute value
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string Executive
        {
            get { return (string)this[ExecutiveAttributeName]; }
        }

        /// <summary>
        /// Get environment variables
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public NameValueConfigurationCollection EnvironmentVariables
        {
            get
            {
                return this[EnvVarsConfigurationName] as NameValueConfigurationCollection;
            }
        }

        //NOTE: override this function to ignore unrecognized attribute in the configuration section.
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="value">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            return true;
        }

        //NOTE: override this function to ignore unrecognized element in the configuration section.
        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="elementName">
        ///   <para />
        /// </param>
        /// <param name="reader">
        ///   <para />
        /// </param>
        /// <returns>
        ///   <para />
        /// </returns>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            return true;
        }
    }
}
