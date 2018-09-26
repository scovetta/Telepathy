//------------------------------------------------------------------------------
// <copyright file="PopupBasherSectionHandler.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      configuration section handler for popup basher
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Excel
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    /// <summary>
    ///   <para>Main configuration section handler.</para>
    /// </summary>
    public class PopupBasherSectionHandler : IConfigurationSectionHandler
    {
        /// <summary>
        ///   <para>Initializes a new instance of the PopupBasherSectionHandler class.</para>
        /// </summary>
        public PopupBasherSectionHandler() 
        { 
        }

        /// <summary>
        ///   <para>Interface method which creates configuration object from the xml in the config file.</para>
        /// </summary>
        /// <param name="parent">
        ///   <para>Parent configuration tag representation.</para>
        /// </param>
        /// <param name="configContext">
        ///   <para>Context under which the configuration should be parsed.</para>
        /// </param>
        /// <param name="section">
        ///   <para>The section of the XML which is to be parsed.</para>
        /// </param>
        /// <returns>
        ///   <para>PopupBasherConfiguration object representing the XML-specified configuration.</para>
        /// </returns>
        object IConfigurationSectionHandler.Create(object parent, object configContext, XmlNode section)
        {
            return this.Create(parent, configContext, section);
        }

        /// <summary>
        ///   <para>Creates the popup basher configuration object from the xml in the config file.</para>
        /// </summary>
        /// <param name="parent">
        ///   <para>Parent configuration tag representation.</para>
        /// </param>
        /// <param name="configContext">
        ///   <para>Context under which the configuration should be parsed.</para>
        /// </param>
        /// <param name="section">
        ///   <para>The section of the XML which is to be parsed.</para>
        /// </param>
        /// <returns>
        ///   <para>PopupBasherConfiguration object representing the XML-specified configuration.</para>
        /// </returns>
        protected object Create(object parent, object configContext, XmlNode section)
        {
            XmlSerializer serializer = null;
            serializer = new XmlSerializer(typeof(PopupBasherConfiguration));

            // now try for deserialization--remember, each of the config property classes is decorated with xml deserialization 
            // attributes which will "automatically" put elements/attributes in class fields
            PopupBasherConfiguration config = (PopupBasherConfiguration)serializer.Deserialize(new XmlNodeReader(section));

            return config;
        }
    }
}
