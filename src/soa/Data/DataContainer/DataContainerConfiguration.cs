//------------------------------------------------------------------------------
// <copyright file="DataContainerConfiguration.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data container configuration
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data.DataContainer
{
    using System;
    using System.Configuration;
    using Microsoft.Hpc.Scheduler.Session;

    internal class DataContainerConfiguration : ConfigurationSection
    {
        /// <summary>
        /// Configuration section name for data provider
        /// </summary>
        private const string DataContainerConfigurationSectionName = "Microsoft.Hpc.Scheduler.Session.Data";

        private const string FileShareBufferSizeInKiloBytesConfigName = "fileShareBufferSizeInKiloBytes";

        private const string DownloadBlobThreadCountConfigName = "downloadBlobThreadCount";

        private const string DownloadBlobMinBackoffInSecondsConfigName = "downloadBlobMinBackoffInSeconds";

        private const string DownloadBlobMaxBackoffInSecondsConfigName = "downloadBlobMaxBackoffInSeconds";

        private const string DownloadBlobRetryCountConfigName = "downloadBlobRetryCount";

        private const string DownloadBlobTimeoutInSecondsConfigName = "downloadBlobTimeoutInSeconds";

        private ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public DataContainerConfiguration()
        {
            // Default FileStream buffer size for file share data provider: 64k (optimized for large data)
            properties.Add(new ConfigurationProperty(FileShareBufferSizeInKiloBytesConfigName, typeof(int), 64));
            properties.Add(new ConfigurationProperty(DownloadBlobMinBackoffInSecondsConfigName, typeof(int), 3));
            properties.Add(new ConfigurationProperty(DownloadBlobMaxBackoffInSecondsConfigName, typeof(int), 90));
            properties.Add(new ConfigurationProperty(DownloadBlobRetryCountConfigName, typeof(int), 3));
            properties.Add(new ConfigurationProperty(DownloadBlobThreadCountConfigName, typeof(int), Environment.ProcessorCount * 8));
            properties.Add(new ConfigurationProperty(DownloadBlobTimeoutInSecondsConfigName, typeof(int), 300));
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get {  return properties; }
        }

        public int FileShareBufferSizeInKiloBytes
        {
            get { return (int)this[FileShareBufferSizeInKiloBytesConfigName]; }
        }

        public int DownloadBlobThreadCount
        {
            get { return (int)this[DownloadBlobThreadCountConfigName]; }
        }

        public int DownloadBlobMinBackOffInSeconds
        {
            get { return (int)this[DownloadBlobMinBackoffInSecondsConfigName]; }
        }

        public int DownloadBlobMaxBackOffInSeconds
        {
            get { return (int)this[DownloadBlobMaxBackoffInSecondsConfigName]; }
        }

        public int DownloadBlobRetryCount
        {
            get { return (int)this[DownloadBlobRetryCountConfigName]; }
        }

        public int DownloadBlobTimeoutInSeconds
        {
            get { return (int)this[DownloadBlobTimeoutInSecondsConfigName]; }
        }

        public static DataContainerConfiguration GetSection(Configuration config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            ConfigurationSection configSection = config.GetSection(DataContainerConfigurationSectionName);
            if (configSection != null)
            {
                return configSection as DataContainerConfiguration;
            }
            else
            {
                return new DataContainerConfiguration();
            }
        }

        public bool Validate(out string errorMessage)
        {
            errorMessage = null;
            if (FileShareBufferSizeInKiloBytes <= 0)
            {
                errorMessage = SR.InvalidFileShareBufferSize;
                return false;
            }

            if (DownloadBlobRetryCount <= 0)
            {
                errorMessage = SR.InvalidDownloadBlobThreadCount;
                return false;
            }

            if (DownloadBlobMinBackOffInSeconds <= 0)
            {
                errorMessage = SR.InvalidDownloadBlobMinBackoff;
                return false;
            }

            if (DownloadBlobMaxBackOffInSeconds < DownloadBlobMinBackOffInSeconds)
            {
                errorMessage = SR.InvalidDownloadBlobMaxBackoff;
                return false;
            }

            if (DownloadBlobRetryCount < 0)
            {
                errorMessage = SR.InvalidDownloadBlobRetryCount;
                return false;
            }

            if (DownloadBlobTimeoutInSeconds <= 0)
            {
                errorMessage = SR.InvalidDownloadBlobTimeout;
                return false;
            }

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
