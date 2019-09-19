// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Configuration;
using Microsoft.Hpc.Scheduler.Session.Internal;

namespace Microsoft.Hpc.Scheduler.Session.Configuration
{
    /// <summary>
    ///   <para>Defines values that represent the possible service architectures for a service.</para>
    /// </summary>
    /// <remarks>
    ///   <para>To use this enumeration in Visual Basic Scripting Edition (VBScript), you 
    /// need to use the numeric values for the enumeration members or create constants that  
    /// correspond to those members and set them equal to the numeric values. The 
    /// following code example shows how to create and set constants for this enumeration in VBScript.</para> 
    ///   <code language="vbs">const X86 = 1
    /// const X64 = 2</code>
    /// </remarks>
    public enum ServiceArch
    {
        /// <summary>
        ///   <para>The service architecture is not set. This enumeration member represents a value of 0.</para>
        /// </summary>
        None = 0,

        /// <summary>
        ///   <para>The service is a 32-bit binary. This enumeration member represents a value of 1.</para>
        /// </summary>
        X86 = 1,

        /// <summary>
        ///   <para>The service is a 64-bit binary. This enumeration member represents a value of 2.</para>
        /// </summary>
        X64 = 2,
    }

    /// <summary>
    ///   <para>Represents the parts of the service configuration file.</para>
    /// </summary>
    public sealed class ServiceConfiguration : ConfigurationSection
    {
        const string AssemblyConfigurationName = "assembly";
        const string ContractConfigurationName = "contract";
        const string TypeConfigurationName = "type";
        const string IncludeExceptionDetailInFaultsConfigurationName = "includeExceptionDetailInFaults";
        const string MaxConcurrentCallsConfigurationName = "maxConcurrentCalls";
        const string ServiceInitializationTimeoutConfigurationName = "serviceInitializationTimeout";
        const string ServiceHostIdleTimeoutConfigurationName = "serviceHostIdleTimeout";
        const string ServiceHangTimeoutConfigurationName = "serviceHangTimeout";
        const string EnableMessageLevelPreemptionConfigurationName = "enableMessageLevelPreemption";
        const string ArchitectureConfigurationName = "architecture";
        const string EnvVarsConfigurationName = "environmentVariables";
        const string StdErrorConfigurationName = "stdError";
        const string MaxMessageSizeConfigurationName = "maxMessageSize";
        const string MaxSessionPoolSizeConfigurationName = "maxSessionPoolSize";
        const string PrepareNodeCommandLineConfigurationName = "prepareNodeCommandLine";
        const string ReleaseNodeCommandLineConfigurationName = "releaseNodeCommandLine";
        const string SoaDiagTraceLevelConfigurationName = "soaDiagTraceLevel";

        const int defaultServiceInitializationTimeout = 60 * 1000;  // 60 seconds
        const int defaultServiceHostIdleTimeout = 60 * 60 * 1000;  // 60 minutes
        const int defaultServiceHangTimeout = -1;  // Timeout.Infinite


        string _fullAssemblyPath;

        ConfigurationProperty envVariables = new ConfigurationProperty(EnvVarsConfigurationName, typeof(NameValueConfigurationCollection));

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        ///   <para>Initializes a new instance of the <see cref="Microsoft.Hpc.Scheduler.Session.Configuration.ServiceConfiguration" /> class.</para>
        /// </summary>
        public ServiceConfiguration()
        {
            properties.Add(new ConfigurationProperty(AssemblyConfigurationName, typeof(string), ""));
            properties.Add(new ConfigurationProperty(ContractConfigurationName, typeof(string), ""));
            properties.Add(new ConfigurationProperty(TypeConfigurationName, typeof(string), ""));
            properties.Add(new ConfigurationProperty(IncludeExceptionDetailInFaultsConfigurationName, typeof(bool), false));
            properties.Add(new ConfigurationProperty(MaxConcurrentCallsConfigurationName, typeof(int), 0));
            properties.Add(new ConfigurationProperty(ServiceInitializationTimeoutConfigurationName, typeof(int), defaultServiceInitializationTimeout));
            properties.Add(new ConfigurationProperty(ServiceHostIdleTimeoutConfigurationName, typeof(int), defaultServiceHostIdleTimeout));
            properties.Add(new ConfigurationProperty(ServiceHangTimeoutConfigurationName, typeof(int), defaultServiceHangTimeout));
            properties.Add(new ConfigurationProperty(EnableMessageLevelPreemptionConfigurationName, typeof(bool), true));
            properties.Add(new ConfigurationProperty(ArchitectureConfigurationName, typeof(string), "X64"));
            properties.Add(new ConfigurationProperty(StdErrorConfigurationName, typeof(string), String.Empty));
            properties.Add(new ConfigurationProperty(MaxMessageSizeConfigurationName, typeof(int), Constant.DefaultMaxMessageSize));
            properties.Add(new ConfigurationProperty(MaxSessionPoolSizeConfigurationName, typeof(int), Constant.DefaultMaxSessionPoolSize));
            properties.Add(new ConfigurationProperty(PrepareNodeCommandLineConfigurationName, typeof(string), String.Empty));
            properties.Add(new ConfigurationProperty(ReleaseNodeCommandLineConfigurationName, typeof(string), String.Empty));
            properties.Add(new ConfigurationProperty(SoaDiagTraceLevelConfigurationName, typeof(string), String.Empty));
            properties.Add(envVariables);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        ///   <para>The full path to the service DLL.</para>
        /// </summary>
        /// <value>
        ///   <para>The full path to the service DLL.</para>
        /// </value>
        public string AssemblyPath
        {
            get
            {
                if (_fullAssemblyPath == null)
                {
                    _fullAssemblyPath = this[AssemblyConfigurationName] as string;
                }

                return _fullAssemblyPath;
            }
        }

        /// <summary>
        ///   <para>The interface of the service (WCF contract).</para>
        /// </summary>
        /// <value>
        ///   <para>The interface of the service (WCF contract).</para>
        /// </value>
        public string ContractType
        {
            get
            {
                return this[ContractConfigurationName] as string;
            }
        }

        /// <summary>
        ///   <para>The class that implements the WCF contract.</para>
        /// </summary>
        /// <value>
        ///   <para>The class that implements the WCF contract.</para>
        /// </value>
        public string ServiceType
        {
            get
            {
                return this[TypeConfigurationName] as string;
            }
        }

        /// <summary>
        /// Get a value indicates if include exception detail in faults
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool IncludeExceptionDetailInFaults
        {
            get
            {
                return (bool)this[IncludeExceptionDetailInFaultsConfigurationName];
            }
        }

        /// <summary>
        /// Get the max concurrent calls
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int MaxConcurrentCalls
        {
            get
            {
                return (int)this[MaxConcurrentCallsConfigurationName];
            }
        }

        /// <summary>
        /// Get or set the max message size
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int MaxMessageSize
        {
            get
            {
                return (int)this[MaxMessageSizeConfigurationName];
            }

            set
            {
                this[MaxMessageSizeConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get or set the max session pool size
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int MaxSessionPoolSize
        {
            get
            {
                return (int)this[MaxSessionPoolSizeConfigurationName];
            }

            set
            {
                this[MaxSessionPoolSizeConfigurationName] = value;
            }
        }

        /// <summary>
        /// Get the service initialization timeout
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public int ServiceInitializationTimeout
        {
            get
            {
                return (int)this[ServiceInitializationTimeoutConfigurationName];
            }
        }

        /// <summary>
        /// Get the service host idle timeout
        /// </summary>
        public int ServiceHostIdleTimeout
        {
            get
            {
                return (int)this[ServiceHostIdleTimeoutConfigurationName];
            }
        }

        /// <summary>
        /// Get the service hang timeout
        /// </summary>
        public int ServiceHangTimeout
        {
            get
            {
                return (int)this[ServiceHangTimeoutConfigurationName];
            }
        }

        /// <summary>
        /// Get the value indicates if enable message level preemption
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool EnableMessageLevelPreemption
        {
            get
            {
                return (bool)this[EnableMessageLevelPreemptionConfigurationName];
            }
        }

        /// <summary>
        ///   <para>The architecture on which your service can run.</para>
        /// </summary>
        /// <value>
        ///   <para>For possible values, see the   <see cref="Microsoft.Hpc.Scheduler.Session.Configuration.ServiceArch" /> enumeration.</para>
        /// </value>
        public ServiceArch Architecture
        {
            get
            {
                try
                {
                    return (ServiceArch)Enum.Parse(
                                                typeof(ServiceArch),
                                                this[ArchitectureConfigurationName] as string,
                                                true);
                }
                catch (ArgumentException)
                {
                    // Use x64 as the default value if invalid architecture name is found
                    return ServiceArch.X64;
                }
            }
        }

        /// <summary>
        ///   <para>The environment variables that the service uses.</para>
        /// </summary>
        /// <value>
        ///   <para>An 
        /// 
        /// <see cref="System.Configuration.NameValueConfigurationCollection" /> object that contains a collection of name/value pairs that define the environment variables that the service uses.</para> 
        /// </value>
        public NameValueConfigurationCollection EnvironmentVariables
        {
            get
            {
                return this[EnvVarsConfigurationName] as NameValueConfigurationCollection;
            }
        }

        /// <summary>
        /// Get the stderr configuration
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string StdError
        {
            get
            {
                return this[StdErrorConfigurationName] as string;
            }
        }

        /// <summary>
        /// Get the prepare node command line configuration
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string PrepareNodeCommandLine
        {
            get
            {
                return this[PrepareNodeCommandLineConfigurationName] as string;
            }
        }

        /// <summary>
        /// Get the release node command line configuration
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string ReleaseNodeCommandLine
        {
            get
            {
                return this[ReleaseNodeCommandLineConfigurationName] as string;
            }
        }

        /// <summary>
        /// Get or set the SOA diagnostic trace level
        /// </summary>
        /// <value>
        ///   <para>Returns <see cref="System.String" />.</para>
        /// </value>
        public string SoaDiagTraceLevel
        {
            get
            {
                return this[SoaDiagTraceLevelConfigurationName] as string;
            }

            set
            {
                this[SoaDiagTraceLevelConfigurationName] = value;
            }
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
