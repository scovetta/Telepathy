//------------------------------------------------------------------------------
// <copyright file="BindingHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Helper class to build binding from configuration file
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    /// Helper class to build binding from configuration file
    /// </summary>
    internal static class BindingHelper
    {
        /// <summary>
        /// Stores the max connections for scheduler delegation binding, backend binding and service host throttling behavior
        /// </summary>
        public const int MaxConnections = 1000;

        /// <summary>
        /// Stores the max message size fo the scheduler delegation binding
        /// </summary>
        private const int MaxMessageSizeForSchedulerDelegationBinding = 400 * 1024 * 1024;

        /// <summary>
        /// Stores the max message size for NetNamedPipeBinding: 64M
        /// </summary>
        private const int MaxMessageSizeForNamedPipeBinding = 64 * 1024 * 1024;

        /// <summary>
        /// Stores the max message size for diag service
        /// </summary>
        private const long MaxMessageSizeForDiagService = long.MaxValue;

        /// <summary>
        /// Stores the default max concurrent calls
        /// </summary>
        private const int DefaultMaxConcurrentCalls = 1000;


#if Broker
        /// <summary>
        /// Stores the binding security mode mismatched reason
        /// </summary>
        private static readonly string BindingSecurityModeMismatched = Microsoft.Hpc.SvcBroker.SR.BindingSecurityModeMismatched;
#else
        /// <summary>
        /// Stores the binding security mode mismatched reason
        /// </summary>
        /// <remarks>The BindingHelper class is a shared class and do not have a related SR, so make the string here</remarks>
        private const string BindingSecurityModeMismatched = "Binding security mode is not matched.";
#endif

#if Broker
        /// <summary>
        /// Stores the cannot find custom binding configuration reason
        /// </summary>
        private static readonly string CannotFindCustomBindingConfiguration = Microsoft.Hpc.SvcBroker.SR.CannotFindCustomBindingConfiguration;
#else
        /// <summary>
        /// Stores the cannot find custom binding configuration reason
        /// </summary>
        private const string CannotFindCustomBindingConfiguration = "Cannot find custom binding in the configuration file.";
#endif

        /// <summary>
        /// Store the unsecure http broker binding configuration name
        /// </summary>
        private const string UnsecureHttpFrontEndBindingName = "Microsoft.Hpc.UnsecureHttpBrokerBinding";

        /// <summary>
        /// Store the secure http broker binding configuration name
        /// </summary>
        private const string SecureHttpFrontEndBindingName = "Microsoft.Hpc.SecureHttpBrokerBinding";

        /// <summary>
        /// Store the secure net.tcp broker binding configuration name
        /// </summary>
        private const string SecureNetTcpFrontEndBindingName = "Microsoft.Hpc.SecureNetTcpBrokerBinding"; 
        
        /// <summary>
        /// Store the internal secure net.tcp broker binding configuration name
        /// </summary>
        private const string InternalSecureNetTcpFrontEndBindingName = "Microsoft.Hpc.InternalSecureNetTcpBrokerBinding";
        
        /// <summary>
        /// Store the AAD net.tcp broker binding configuration name
        /// </summary>
        private const string AadNetTcpFrontEndBindingName = "Microsoft.Hpc.AadNetTcpBrokerBinding";

        /// <summary>
        /// Store the unsecure net.tcp broker binding configuration name
        /// </summary>
        private const string UnsecureNetTcpFrontEndBindingName = "Microsoft.Hpc.UnsecureNetTcpBrokerBinding";

        /// <summary>
        /// Store the secure NetHttps broker binding configuration name
        /// </summary>
        private const string SecureNetHttpsFrontEndBindingName = "Microsoft.Hpc.SecureNetHttpsBrokerBinding";

        /// <summary>
        /// Store the unsecure NetHttp broker binding configuration name
        /// </summary>
        private const string UnsecureNetHttpFrontEndBindingName = "Microsoft.Hpc.UnsecureNetHttpBrokerBinding";

        /// <summary>
        /// Store the secure custom broker binding configuration name
        /// </summary>
        private const string SecureCustomFrontEndBindingName = "Microsoft.Hpc.SecureCustomBrokerBinding";

        /// <summary>
        /// Store the unsecure custom broker binding configuration name
        /// </summary>
        private const string UnsecureCustomFrontEndBindingName = "Microsoft.Hpc.UnsecureCustomBrokerBinding";

        /// <summary>
        /// Store the back end binding configuration name
        /// </summary>
        private const string BackEndBindingName = "Microsoft.Hpc.BackEndBinding";

        /// <summary>
        /// Store the session and broker launcher http binding configuration name
        /// </summary>
        public const string LauncherHttpBindingName = "Microsoft.Hpc.LauncherHttpBinding";

        /// <summary>
        /// Store the session and broker launcher nettcp binding configuration name
        /// </summary>
        public const string LauncherNetTcpBindingName = "Microsoft.Hpc.LauncherNetTcpBinding";

        /// <summary>
        /// Store the session and broker launcher nethttp binding configuration name
        /// </summary>
        public const string LauncherNetHttpBindingName = "Microsoft.Hpc.LauncherNetHttpBinding";

        /// <summary>
        /// Stores the binding collection name for basic http binding
        /// </summary>
        private const string BasicHttpBindingCollectionName = "basicHttpBinding";

        /// <summary>
        /// Stores the binding collection name for net.tcp binding
        /// </summary>
        private const string NetTcpBindingCollectionName = "netTcpBinding";

        /// <summary>
        /// Stores the binding collection name for NetHttps binding
        /// </summary>
        private const string NetHttpsBindingCollectionName = "netHttpsBinding";

        /// <summary>
        /// Stores the binding collection name for NetHttp binding
        /// </summary>
        private const string NetHttpBindingCollectionName = "netHttpBinding";

        /// <summary>
        /// Stores the binding collection name for custom binding
        /// </summary>
        private const string CustomBindingCollectionName = "customBinding";

        /// <summary>
        /// Represents how an endpoint address is constructed
        /// <![CDATA[<scheme>://<wcfnetworkprefix>.<nodename>:<port>/<jobId>/<taskId>]]>
        /// </summary>
        private const string BaseAddrTemplate = "{4}://{0}:{1}/{2}/{3}";

        /// <summary>
        /// Stores net.tcp scheme
        /// </summary>
        public const string NetTcpScheme = "net.tcp";

        /// <summary>
        /// Stores http scheme
        /// </summary>
        public const string HttpScheme = "http";

        /// <summary>
        /// Stores https scheme
        /// </summary>
        public const string HttpsScheme = "https";

        /// <summary>
        /// Store the default secure basic http binding
        /// </summary>
        private static readonly BasicHttpBinding defaultSecureBasicHttpBinding;

        /// <summary>
        /// Store the default unsecure basic http binding
        /// </summary>
        private static readonly BasicHttpBinding defaultUnsecureBasicHttpBinding;

        /// <summary>
        /// Store the default secure net.tcp binding
        /// </summary>
        private static readonly NetTcpBinding defaultSecureNetTcpBinding;
        
        /// <summary>
        /// Store the default secure net.tcp binding with no authentication
        /// </summary>
        private static readonly NetTcpBinding defaultNoAuthSecureNetTcpBinding;

        /// <summary>
        /// Store the default internal secure net.tcp binding
        /// </summary>
        private static readonly NetTcpBinding defaultInternalSecureNetTcpBinding;

        /// <summary>
        /// Store the default unsecure net.tcp binding
        /// </summary>
        private static readonly NetTcpBinding defaultUnsecureNetTcpBinding;

        /// <summary>
        /// Stores the default backend binding
        /// </summary>
        private static readonly NetTcpBinding defaultBackEndBinding;

#if !net40
        /// <summary>
        /// Store the default secure NetHttps binding
        /// </summary>
        private static readonly NetHttpsBinding defaultSecureNetHttpsBinding;

        /// <summary>
        /// Store the default unsecure NetHttp binding
        /// </summary>
        private static readonly NetHttpBinding defaultUnsecureNetHttpBinding;

        /// <summary>
        /// Stores the hard coded binding for SessionLauncherNetHttpsBinding, BrokerLauncherNetHttpsBinding.
        /// </summary>
        private static readonly NetHttpsBinding hardCodedNetHttpsBinding;
#endif

#if WebAPI
        /// <summary>
        /// Stores the hard coded Web API service binding
        /// </summary>
        private static readonly WebHttpBinding hardCodedWebAPIServiceBinding;
#endif

        /// <summary>
        /// Stores the hard coded scheduler delegation binding
        /// </summary>
        private static readonly NetTcpBinding hardCodedSchedulerDelegationBinding;

        /// <summary>
        /// Stores the hard coded internal scheduler delegation binding
        /// </summary>
        private static readonly NetTcpBinding hardCodedInternalSchedulerDelegationBinding;

        /// <summary>
        /// Stores the hard coded broker management service binding
        /// </summary>
        private static readonly BasicHttpBinding hardCodedBrokerManagementServiceBinding;

        /// <summary>
        /// Stores the hard coded binding for soa diag service.
        /// </summary>
        private static readonly NetTcpBinding hardCodedDiagServiceBinding;

        /// <summary>
        /// Stores the hard coded binding for soa diag monitor service.
        /// </summary>
        private static readonly NetTcpBinding hardCodedDiagMonitorServiceBinding;

        /// <summary>
        /// Stores the hard coded binding for SessionLauncherNetTcpBinding, BrokerLauncherNetTcpBinding and ServiceDataNetTcpBinding.
        /// </summary>
        private static readonly NetTcpBinding hardCodedSecureNetTcpBinding;

        /// <summary>
        /// Stores the hard coded binding for internal SessionLauncherNetTcpBinding, BrokerLauncherNetTcpBinding and ServiceDataNetTcpBinding.
        /// </summary>
        private static readonly NetTcpBinding hardCodedInternalSecureNetTcpBinding;
        
        /// <summary>
        /// Stores the hard coded binding for AAD integrated SessionLauncherNetTcpBinding, BrokerLauncherNetTcpBinding and ServiceDataNetTcpBinding.
        /// </summary>
        private static readonly NetTcpBinding hardCodedNoAuthSecureNetTcpBinding;

        /// <summary>
        /// Stores the hard coded unsecure net tcp binding.
        /// </summary>
        private static readonly NetTcpBinding hardCodedUnsecureNetTcpBinding;

        /// <summary>
        /// Stores the hard coded binding for inter-process communication on the same machine
        /// </summary>
        private static readonly NetNamedPipeBinding hardCodedNamedPipeBinding;

        /// <summary>
        /// Stores the default operation timeout
        /// </summary>
        private static readonly TimeSpan defaultOperationTimeout = TimeSpan.FromMilliseconds(86400000);

        /// <summary>
        /// Stores the hard coded send timeout
        /// </summary>
        private static readonly TimeSpan hardCodedSendTimeout = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Stores the default close timeout
        /// </summary>
        private static readonly TimeSpan defaultCloseTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Stores the receive timeout for scheduler delegation
        /// </summary>
        private static readonly TimeSpan receiveTimeoutForSchedulerDelegation = TimeSpan.FromMinutes(30);

        /// <summary>
        /// Stores the reader quotas for diag service
        /// </summary>
        private static readonly XmlDictionaryReaderQuotas readerQuotasForDiagService = XmlDictionaryReaderQuotas.Max;

        /// <summary>
        /// Initializes static members of the BindingHelper class
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Needs to do some logic when initializing the default bindings.")]
        static BindingHelper()
        {
            defaultSecureBasicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.TransportWithMessageCredential);
            defaultSecureBasicHttpBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            defaultSecureBasicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            defaultSecureBasicHttpBinding.CloseTimeout = defaultCloseTimeout;
            
            defaultSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            defaultSecureNetTcpBinding.PortSharingEnabled = true;
            defaultSecureNetTcpBinding.CloseTimeout = defaultCloseTimeout;
            defaultSecureNetTcpBinding.MaxConnections = MaxConnections;

            defaultNoAuthSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            defaultNoAuthSecureNetTcpBinding.PortSharingEnabled = true;
            defaultNoAuthSecureNetTcpBinding.CloseTimeout = defaultCloseTimeout;
            defaultNoAuthSecureNetTcpBinding.MaxConnections = MaxConnections;
            defaultNoAuthSecureNetTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;

            defaultInternalSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            defaultInternalSecureNetTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            defaultInternalSecureNetTcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            defaultInternalSecureNetTcpBinding.PortSharingEnabled = true;
            defaultInternalSecureNetTcpBinding.CloseTimeout = defaultCloseTimeout;
            defaultInternalSecureNetTcpBinding.MaxConnections = MaxConnections;

            defaultUnsecureBasicHttpBinding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            defaultUnsecureBasicHttpBinding.CloseTimeout = defaultCloseTimeout;

            defaultUnsecureNetTcpBinding = new NetTcpBinding(SecurityMode.None);
            defaultUnsecureNetTcpBinding.PortSharingEnabled = true;
            defaultUnsecureNetTcpBinding.CloseTimeout = defaultCloseTimeout;
            defaultUnsecureNetTcpBinding.MaxConnections = MaxConnections;

            /*
            defaultBackEndBinding = new NetTcpBinding(SecurityMode.Transport);
            defaultBackEndBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            defaultBackEndBinding.Security.Transport.ProtectionLevel = ProtectionLevel.None;
            defaultBackEndBinding.ReceiveTimeout = defaultOperationTimeout;
            defaultBackEndBinding.SendTimeout = defaultOperationTimeout;
            defaultBackEndBinding.MaxConnections = MaxConnections;
            */

            defaultBackEndBinding = defaultUnsecureNetTcpBinding;

#if !net40
            defaultSecureNetHttpsBinding = new NetHttpsBinding(BasicHttpsSecurityMode.TransportWithMessageCredential);
            defaultSecureNetHttpsBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            defaultSecureNetHttpsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            defaultSecureNetHttpsBinding.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
            defaultSecureNetHttpsBinding.CloseTimeout = defaultCloseTimeout;

            defaultUnsecureNetHttpBinding = new NetHttpBinding(BasicHttpSecurityMode.None);
            defaultUnsecureNetHttpBinding.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
            defaultUnsecureNetHttpBinding.CloseTimeout = defaultCloseTimeout;

            hardCodedNetHttpsBinding = new NetHttpsBinding(BasicHttpsSecurityMode.TransportWithMessageCredential);
            hardCodedNetHttpsBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            hardCodedNetHttpsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            hardCodedNetHttpsBinding.WebSocketSettings.TransportUsage = WebSocketTransportUsage.Always;
#endif
            
            hardCodedSchedulerDelegationBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedSchedulerDelegationBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            hardCodedSchedulerDelegationBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedSchedulerDelegationBinding.ReceiveTimeout = receiveTimeoutForSchedulerDelegation;
            hardCodedSchedulerDelegationBinding.MaxConnections = MaxConnections;
            hardCodedSchedulerDelegationBinding.MaxBufferPoolSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedSchedulerDelegationBinding.MaxBufferSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedSchedulerDelegationBinding.MaxReceivedMessageSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedSchedulerDelegationBinding.ReaderQuotas.MaxStringContentLength = MaxMessageSizeForSchedulerDelegationBinding;

            hardCodedInternalSchedulerDelegationBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedInternalSchedulerDelegationBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hardCodedInternalSchedulerDelegationBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedInternalSchedulerDelegationBinding.ReceiveTimeout = receiveTimeoutForSchedulerDelegation;
            hardCodedInternalSchedulerDelegationBinding.MaxConnections = MaxConnections;
            hardCodedInternalSchedulerDelegationBinding.MaxBufferPoolSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedInternalSchedulerDelegationBinding.MaxBufferSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedInternalSchedulerDelegationBinding.MaxReceivedMessageSize = MaxMessageSizeForSchedulerDelegationBinding;
            hardCodedInternalSchedulerDelegationBinding.ReaderQuotas.MaxStringContentLength = MaxMessageSizeForSchedulerDelegationBinding;

            hardCodedBrokerManagementServiceBinding = new BasicHttpBinding();
            hardCodedBrokerManagementServiceBinding.CloseTimeout = TimeSpan.FromSeconds(1);

            hardCodedDiagServiceBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedDiagServiceBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            hardCodedDiagServiceBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedDiagServiceBinding.ReaderQuotas = readerQuotasForDiagService;
            hardCodedDiagServiceBinding.MaxBufferSize = int.MaxValue;
            hardCodedDiagServiceBinding.MaxBufferPoolSize = MaxMessageSizeForDiagService;
            hardCodedDiagServiceBinding.MaxReceivedMessageSize = MaxMessageSizeForDiagService;
            hardCodedDiagServiceBinding.MaxConnections = MaxConnections;
            hardCodedDiagServiceBinding.ReceiveTimeout = TimeSpan.MaxValue;
            hardCodedDiagServiceBinding.SendTimeout = TimeSpan.MaxValue;
            hardCodedDiagServiceBinding.TransferMode = TransferMode.StreamedResponse;

            hardCodedDiagMonitorServiceBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedDiagMonitorServiceBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            hardCodedDiagMonitorServiceBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedDiagMonitorServiceBinding.MaxConnections = MaxConnections;
            hardCodedDiagMonitorServiceBinding.MaxReceivedMessageSize = long.MaxValue;
            hardCodedDiagMonitorServiceBinding.MaxBufferPoolSize = long.MaxValue;
            hardCodedDiagMonitorServiceBinding.MaxBufferSize = int.MaxValue;
            hardCodedDiagMonitorServiceBinding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
            hardCodedDiagMonitorServiceBinding.TransferMode = TransferMode.StreamedResponse;
            hardCodedDiagMonitorServiceBinding.SendTimeout = TimeSpan.FromDays(1);
            hardCodedDiagMonitorServiceBinding.ReceiveTimeout = TimeSpan.FromDays(1);

            hardCodedSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedSecureNetTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            hardCodedSecureNetTcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedSecureNetTcpBinding.MaxConnections = MaxConnections;
            hardCodedSecureNetTcpBinding.ListenBacklog = MaxConnections;

            hardCodedInternalSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedInternalSecureNetTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hardCodedInternalSecureNetTcpBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedInternalSecureNetTcpBinding.MaxConnections = MaxConnections;
            hardCodedInternalSecureNetTcpBinding.ListenBacklog = MaxConnections;

            hardCodedNoAuthSecureNetTcpBinding = new NetTcpBinding(SecurityMode.Transport);
            hardCodedNoAuthSecureNetTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.None;
            hardCodedNoAuthSecureNetTcpBinding.MaxConnections = MaxConnections;
            hardCodedNoAuthSecureNetTcpBinding.ListenBacklog = MaxConnections;

            hardCodedUnsecureNetTcpBinding = new NetTcpBinding(SecurityMode.None);
            hardCodedUnsecureNetTcpBinding.MaxConnections = MaxConnections;

            hardCodedNamedPipeBinding = new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport);
            hardCodedNamedPipeBinding.Security.Transport.ProtectionLevel = ProtectionLevel.EncryptAndSign;
            hardCodedNamedPipeBinding.ReceiveTimeout = defaultOperationTimeout;
            hardCodedNamedPipeBinding.SendTimeout = defaultOperationTimeout;
            hardCodedNamedPipeBinding.MaxConnections = MaxConnections;
            hardCodedNamedPipeBinding.MaxBufferPoolSize = MaxMessageSizeForNamedPipeBinding;
            hardCodedNamedPipeBinding.MaxBufferSize = MaxMessageSizeForNamedPipeBinding;
            hardCodedNamedPipeBinding.MaxReceivedMessageSize = MaxMessageSizeForNamedPipeBinding;
            hardCodedNamedPipeBinding.ReaderQuotas.MaxStringContentLength = MaxMessageSizeForNamedPipeBinding;
            hardCodedNamedPipeBinding.ReaderQuotas.MaxArrayLength = MaxMessageSizeForNamedPipeBinding;

#if WebAPI
            // FIXME: Fix security mode
            hardCodedWebAPIServiceBinding = new WebHttpBinding(WebHttpSecurityMode.Transport);
            hardCodedWebAPIServiceBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;
            hardCodedWebAPIServiceBinding.MaxReceivedMessageSize = int.MaxValue;
            hardCodedWebAPIServiceBinding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;
            hardCodedWebAPIServiceBinding.TransferMode = TransferMode.Streamed;
            hardCodedWebAPIServiceBinding.SendTimeout = TimeSpan.MaxValue;
            hardCodedWebAPIServiceBinding.ReceiveTimeout = TimeSpan.MaxValue;
#endif
        }

        /// <summary>
        /// Gets the hard coded broker management service binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static Binding HardCodedBrokerManagementServiceBinding
        {
            get { return BindingHelper.hardCodedBrokerManagementServiceBinding; }
        }

        /// <summary>
        /// Gets the hard coded broker launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedBrokerLauncherNetTcpBinding
        {
            get { return hardCodedSecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded internal broker launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedInternalBrokerLauncherNetTcpBinding
        {
            get { return hardCodedInternalSecureNetTcpBinding; }
        }
        
        /// <summary>
        /// Gets the hard coded no auth broker launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedNoAuthBrokerLauncherNetTcpBinding
        {
            get { return hardCodedNoAuthSecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded broker launcher http binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static BasicHttpBinding HardCodedBrokerLauncherHttpsBinding
        {
            get { return defaultSecureBasicHttpBinding; }
        }

        /// <summary>
        /// Gets the hard coded session launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedSessionLauncherNetTcpBinding
        {
            get { return hardCodedSecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded no auth session launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedNoAuthSessionLauncherNetTcpBinding
        {
            get { return hardCodedNoAuthSecureNetTcpBinding; }
        }

#if !net40
        /// <summary>
        /// Gets the hard coded session launcher net https binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetHttpsBinding HardCodedSessionLauncherNetHttpsBinding
        {
            get { return hardCodedNetHttpsBinding; }
        }

        /// <summary>
        /// Gets the hard coded broker launcher net https binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetHttpsBinding HardCodedBrokerLauncherNetHttpsBinding
        {
            get { return hardCodedNetHttpsBinding; }
        }
#endif

        /// <summary>
        /// Gets the hard coded internal session launcher net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedInternalSessionLauncherNetTcpBinding
        {
            get { return hardCodedInternalSecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded session launcher http binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static BasicHttpBinding HardCodedSessionLauncherHttpsBinding
        {
            get { return defaultSecureBasicHttpBinding; }
        }

        /// <summary>
        /// Gets the hard coded scheduler delegation binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedSchedulerDelegationBinding
        {
            get { return BindingHelper.hardCodedSchedulerDelegationBinding; }
        }

        /// <summary>
        /// Gets the hard coded internal scheduler delegation binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedInternalSchedulerDelegationBinding
        {
            get { return BindingHelper.hardCodedInternalSchedulerDelegationBinding; }
        }

        /// <summary>
        /// Gets the hard coded data service binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedDataServiceNetTcpBinding
        {
            // Don't cache this binding, because client may change its target to on-premise cluster or Azure cluster.
            get { return hardCodedSecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded unsecure data service binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedUnsecureDataServiceNetTcpBinding
        {
            // Don't cache this binding, because client may change its target to on-premise cluster or Azure cluster.
            get { return hardCodedUnsecureNetTcpBinding; }
        }

        /// <summary>
        /// Gets the hard coded data service https binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static BasicHttpBinding HardCodedDataServiceHttpsBinding
        {
            get { return defaultSecureBasicHttpBinding; }
        }


        /// <summary>
        /// Gets the hard coded NetNamedPipeBinding for inter-process communication on the same machine
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetNamedPipeBinding HardCodedNamedPipeBinding
        {
            // Don't cache this binding, because client may change its target to on-premise cluster or Azure cluster.
            get { return hardCodedNamedPipeBinding; }
        }

        /// <summary>
        /// Gets the hard coded diag service net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedDiagServiceNetTcpBinding
        {
            get { return hardCodedDiagServiceBinding; }
        }

        /// <summary>
        /// Gets the hard coded diag service net tcp binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static NetTcpBinding HardCodedDiagMonitorServiceNetTcpBinding
        {
            get { return hardCodedDiagMonitorServiceBinding; }
        }

        public static NetTcpBinding HardCodedUnSecureNetTcpBinding
        {
            get { return defaultUnsecureNetTcpBinding; }
        }

#if WebAPI
        /// <summary>
        /// Gets the hard coded Web API service binding
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static WebHttpBinding HardCodedWebAPIServiceBinding
        {
            get { return BindingHelper.hardCodedWebAPIServiceBinding; }
        }
#endif

        /// <summary>
        /// Get the backend binding from the configuration
        /// When on Azure, it changes the backend binding to un-secure mode.
        /// </summary>
        /// <param name="isSecure">indicate if the binding is secure</param>
        /// <returns>binding object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static Binding GetBackEndBinding(out bool isSecure)
        {
            NetTcpBinding netTcpBinding;
            isSecure = true;
            try
            {
                netTcpBinding = new NetTcpBinding(BackEndBindingName);
                isSecure = netTcpBinding.Security.Mode != SecurityMode.None;
            }
            catch (KeyNotFoundException)
            {
                // Try http binding
                try
                {
                    BasicHttpBinding basicHttpBining = new BasicHttpBinding(BackEndBindingName);
                    isSecure = basicHttpBining.Security.Mode != BasicHttpSecurityMode.None;
                    return basicHttpBining;
                }
                catch (KeyNotFoundException)
                {
                    netTcpBinding = defaultBackEndBinding;
                    isSecure = defaultBackEndBinding.Security.Mode != SecurityMode.None;
                }
            }

            // Use non-secure connection on Azure. In the burst mode, the Azure broker proxy
            // runs under system account. It can't use secure connection with service host,
            // which runs under user account.
            if (SoaHelper.IsOnAzure())
            {
                netTcpBinding.Security.Mode = SecurityMode.None;
            }

            return netTcpBinding;
        }

        /// <summary>
        /// Get the backend binding from the configuration.
        /// When on Azure, it changes the backend binding to un-secure mode.
        /// </summary>
        /// <param name="bindings">indicating the bindings section</param>
        /// <returns>binding object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static Binding GetBackEndBinding(BindingsSection bindings)
        {
            NetTcpBinding netTcpBinding;

            if (bindings.NetTcpBinding.Bindings.ContainsKey(BackEndBindingName))
            {
                // Try to find backend binding in net.tcp bindings
                netTcpBinding = new NetTcpBinding();
                bindings.NetTcpBinding.Bindings[BackEndBindingName].ApplyConfiguration(netTcpBinding);
            }
            else if (bindings.BasicHttpBinding.Bindings.ContainsKey(BackEndBindingName))
            {
                // Try to find backend binding in basic http bindings
                BasicHttpBinding binding = new BasicHttpBinding();
                bindings.BasicHttpBinding.Bindings[BackEndBindingName].ApplyConfiguration(binding);
                return binding;
            }
            else if (bindings.CustomBinding.Bindings.ContainsKey(BackEndBindingName))
            {
                CustomBinding binding = new CustomBinding();
                bindings.CustomBinding.Bindings[BackEndBindingName].ApplyConfiguration(binding);
                return binding;
            }
            else
            {
                netTcpBinding = defaultBackEndBinding;
            }

            // Use non-secure connection on Azure. In the burst mode, the Azure broker proxy
            // runs under system account. It can't use secure connection with service host,
            // which runs under user account.
            if (SoaHelper.IsOnAzure())
            {
                netTcpBinding.Security.Mode = SecurityMode.None;
            }

            return netTcpBinding;
        }

        /// <summary>
        /// Apply the default throttling behavior to a service host
        /// </summary>
        /// <param name="host">indicating the service host</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static void ApplyDefaultThrottlingBehavior(ServiceHost host)
        {
            ApplyDefaultThrottlingBehavior(host, DefaultMaxConcurrentCalls);
        }

        /// <summary>
        /// Apply the default throttling behavior to a service host
        /// </summary>
        /// <param name="host">indicating the service host</param>
        /// <param name="maxConcurrentCalls">indicating the maxConcurrentCall</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static void ApplyDefaultThrottlingBehavior(ServiceHost host, int maxConcurrentCalls)
        {
            ServiceThrottlingBehavior stb = host.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            if (stb == null)
            {
                stb = new ServiceThrottlingBehavior();
                stb.MaxConcurrentCalls = maxConcurrentCalls;
                stb.MaxConcurrentInstances = BindingHelper.MaxConnections;
                stb.MaxConcurrentSessions = BindingHelper.MaxConnections;
                host.Description.Behaviors.Add(stb);
            }
            else
            {
                stb.MaxConcurrentCalls = maxConcurrentCalls;
                stb.MaxConcurrentInstances = BindingHelper.MaxConnections;
                stb.MaxConcurrentSessions = BindingHelper.MaxConnections;
            }
        }

        /// <summary>
        /// Generates the binding from the configuration
        /// </summary>
        /// <param name="transportScheme">indicate the transport scheme</param>
        /// <param name="secure">indicate whether secure is required</param>
        /// <returns>binding object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static Binding GetBinding(TransportScheme transportScheme, bool secure)
        {
            Binding binding = null;
            if (transportScheme == TransportScheme.Http)
            {
                try
                {
                    binding = new BasicHttpBinding(secure ? SecureHttpFrontEndBindingName : UnsecureHttpFrontEndBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = secure ? defaultSecureBasicHttpBinding : defaultUnsecureBasicHttpBinding;
                }

                // Check the binding if it is secure
                if (secure && ((binding as BasicHttpBinding).Security.Mode == BasicHttpSecurityMode.None))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
            else if (transportScheme == TransportScheme.NetTcp)
            {
                try
                {
                    binding = new NetTcpBinding(secure ? SecureNetTcpFrontEndBindingName : UnsecureNetTcpFrontEndBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = secure ? defaultSecureNetTcpBinding : defaultUnsecureNetTcpBinding;
                }

                // Check the binding if it is secure
                if (secure && ((binding as NetTcpBinding).Security.Mode == SecurityMode.None))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
#if !net40
            else if (transportScheme == TransportScheme.NetHttp)
            {
                try
                {
                    if (secure)
                    {
                        binding = new NetHttpsBinding(SecureNetHttpsFrontEndBindingName);
                    }
                    else
                    {
                        binding = new NetHttpBinding(UnsecureNetHttpFrontEndBindingName);
                    }
                }
                catch (KeyNotFoundException)
                {
                    if (secure)
                    {
                        binding = defaultSecureNetHttpsBinding;
                    }
                    else
                    {
                        binding = defaultUnsecureNetHttpBinding;
                    }
                }
                
                // Check the binding if it is secure
                if (secure && binding is NetHttpBinding)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
#endif
            else if (transportScheme == (TransportScheme)0x8)
            {
                binding = new NetNamedPipeBinding(secure ? NetNamedPipeSecurityMode.Transport : NetNamedPipeSecurityMode.None);
            }
            else if (transportScheme == TransportScheme.Custom)
            {
                // TODO: Shall we catch the KeyNotFoundException and rethrow another exception indicating that the configuration name is not found?
                try
                {
                    binding = new CustomBinding(secure ? SecureCustomFrontEndBindingName : UnsecureCustomFrontEndBindingName);
                }
                catch (KeyNotFoundException)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CannotFindCustomBindingConfiguration, CannotFindCustomBindingConfiguration);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("transportScheme");
            }

            return binding;
        }

        /// <summary>
        /// Generates the binding from the configuration
        /// </summary>
        /// <param name="transportScheme">indicate the transport scheme</param>
        /// <param name="secure">indicate whether secure is required</param>
        /// <param name="bindings">indicating the bindings section</param>
        /// <returns>binding object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This class is shared in multi projects.")]
        public static Binding GetBinding(TransportScheme transportScheme, bool secure, BindingsSection bindings, bool? useInternalBinding = false, bool useAad = false)
        {
            Binding binding = null;
            if (transportScheme == TransportScheme.Http)
            {
                try
                {
                    binding = new BasicHttpBinding();
                    BasicHttpBindingCollectionElement element = (BasicHttpBindingCollectionElement)bindings[BasicHttpBindingCollectionName];
                    element.Bindings[secure ? SecureHttpFrontEndBindingName : UnsecureHttpFrontEndBindingName].ApplyConfiguration(binding);
                }
                catch (KeyNotFoundException)
                {
                    binding = secure ? defaultSecureBasicHttpBinding : defaultUnsecureBasicHttpBinding;
                }

                // Check the binding if it is secure
                if (secure && ((binding as BasicHttpBinding).Security.Mode == BasicHttpSecurityMode.None))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
            else if (transportScheme == TransportScheme.NetTcp)
            {
                try
                {
                    binding = new NetTcpBinding();
                    NetTcpBindingCollectionElement element = (NetTcpBindingCollectionElement)bindings[NetTcpBindingCollectionName];
                    string bindingName;
                    if (useAad)
                    {
                        bindingName = AadNetTcpFrontEndBindingName;
                    }
                    else if (useInternalBinding.HasValue && useInternalBinding.Value)
                    {
                        bindingName = InternalSecureNetTcpFrontEndBindingName;
                    }
                    else if (secure)
                    {
                        bindingName = SecureNetTcpFrontEndBindingName;
                    }
                    else
                    {
                        bindingName = UnsecureNetTcpFrontEndBindingName;
                    }

                    element.Bindings[bindingName].ApplyConfiguration(binding);
                }
                catch (KeyNotFoundException)
                {
                    if (useAad)
                    {
                        binding = defaultNoAuthSecureNetTcpBinding;
                    }
                    else if (useInternalBinding.HasValue && useInternalBinding.Value)
                    {
                        binding = defaultInternalSecureNetTcpBinding;
                    }
                    else if (secure)
                    {
                        binding = defaultSecureNetTcpBinding;
                    }
                    else
                    {
                        binding = defaultUnsecureNetTcpBinding;
                    }
                }

                // Check the binding if it is secure
                if (secure && ((binding as NetTcpBinding).Security.Mode == SecurityMode.None))
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
#if !net40
            else if (transportScheme == TransportScheme.NetHttp)
            {
                try
                {
                    if (secure)
                    {
                        binding = new NetHttpsBinding();
                        NetHttpsBindingCollectionElement element = (NetHttpsBindingCollectionElement)bindings[NetHttpsBindingCollectionName];
                        element.Bindings[SecureNetHttpsFrontEndBindingName].ApplyConfiguration(binding);
                    }
                    else
                    {
                        binding = new NetHttpBinding();
                        NetHttpBindingCollectionElement element = (NetHttpBindingCollectionElement)bindings[NetHttpBindingCollectionName];
                        element.Bindings[UnsecureNetHttpFrontEndBindingName].ApplyConfiguration(binding);
                    }
                }
                catch (KeyNotFoundException)
                {
                    if (secure)
                    {
                        binding = defaultSecureNetHttpsBinding;
                    }
                    else
                    {
                        binding = defaultUnsecureNetHttpBinding;
                    }
                }

                // Check the binding if it is secure
                if (secure && binding is NetHttpBinding)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_BindingSecurityModeMismatched, BindingSecurityModeMismatched);
                }
            }
#endif
            else if (transportScheme == (TransportScheme)0x8)
            {
                binding = new NetNamedPipeBinding(secure ? NetNamedPipeSecurityMode.Transport : NetNamedPipeSecurityMode.None);
            }
            else if (transportScheme == TransportScheme.Custom)
            {
                // TODO: Shall we catch the KeyNotFoundException and rethrow another exception indicating that the configuration name is not found?
                try
                {
                    binding = new CustomBinding();
                    CustomBindingCollectionElement element = (CustomBindingCollectionElement)bindings[CustomBindingCollectionName];
                    element.Bindings[secure ? SecureCustomFrontEndBindingName : UnsecureCustomFrontEndBindingName].ApplyConfiguration(binding);
                }
                catch (KeyNotFoundException)
                {
                    ThrowHelper.ThrowSessionFault(SOAFaultCode.Broker_CannotFindCustomBindingConfiguration, CannotFindCustomBindingConfiguration);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("transportScheme");
            }

            return binding;
        }

        /// <summary>
        /// Applies the maxMessageSize to the binding
        /// </summary>
        /// <param name="binding"></param>
        /// <param name="maxMessageSize"></param>
        public static void ApplyMaxMessageSize(Binding binding, long maxMessageSize)
        {
            // If the maxMessageSize is less than or equal to zero return
            if (maxMessageSize <= 0)
            {
                return;
            }

            // Prepare serializer quotas
            XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
            quotas.MaxBytesPerRead = XmlDictionaryReaderQuotas.Max.MaxBytesPerRead;
            quotas.MaxDepth = XmlDictionaryReaderQuotas.Max.MaxDepth;
            quotas.MaxNameTableCharCount = XmlDictionaryReaderQuotas.Max.MaxNameTableCharCount;
            quotas.MaxStringContentLength = Convert.ToInt32(maxMessageSize);
            quotas.MaxArrayLength = Convert.ToInt32(maxMessageSize);

            // Set binding information based on type

            if (binding is NetTcpBinding)
            {
                NetTcpBinding netTcpBinding = binding as NetTcpBinding;

                netTcpBinding.MaxBufferSize = Convert.ToInt32(maxMessageSize);
                netTcpBinding.MaxReceivedMessageSize = maxMessageSize;
                netTcpBinding.ReaderQuotas = quotas;
            }
            else if (binding is BasicHttpBinding)
            {
                BasicHttpBinding basicHttpBinding = binding as BasicHttpBinding;

                basicHttpBinding.MaxBufferSize = Convert.ToInt32(maxMessageSize);
                basicHttpBinding.MaxReceivedMessageSize = maxMessageSize;
                basicHttpBinding.ReaderQuotas = quotas;
            }
#if !net40
            else if (binding is NetHttpsBinding)
            {
                NetHttpsBinding netHttpsBinding = binding as NetHttpsBinding;
                netHttpsBinding.MaxBufferSize = Convert.ToInt32(maxMessageSize);
                netHttpsBinding.MaxReceivedMessageSize = maxMessageSize;
                netHttpsBinding.ReaderQuotas = quotas;
            }
            else if (binding is NetHttpBinding)
            {
                NetHttpBinding netHttpBinding = binding as NetHttpBinding;
                netHttpBinding.MaxBufferSize = Convert.ToInt32(maxMessageSize);
                netHttpBinding.MaxReceivedMessageSize = maxMessageSize;
                netHttpBinding.ReaderQuotas = quotas;
            }
#endif
#if WebAPI
            else if (binding is WebHttpBinding)
            {
                WebHttpBinding webHttpBinding = binding as WebHttpBinding;
                webHttpBinding.MaxReceivedMessageSize = maxMessageSize;
                webHttpBinding.ReaderQuotas = quotas;
            }
#endif
        }

        /// <summary>
        /// Apply service operation timeout on a binding
        /// </summary>
        /// <param name="binding">indicating the binding</param>
        /// <param name="serviceOperationTimeout">indicating the service operation timeout</param>
        public static void ApplyServiceOperationTimeoutOnBinding(Binding binding, int serviceOperationTimeout)
        {
            binding.ReceiveTimeout = TimeSpan.FromMilliseconds(serviceOperationTimeout);
            binding.SendTimeout = hardCodedSendTimeout;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hostnameWithPrefix"></param>
        /// <param name="jobId"></param>
        /// <param name="taskId"></param>
        /// <param name="port"></param>
        /// <param name="isHttp"></param>
        /// <returns></returns>
        public static string GenerateServiceHostEndpointAddress(string hostnameWithPrefix, int jobId, int taskId, int port, bool isHttp)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                BaseAddrTemplate,
                hostnameWithPrefix,
                port,
                jobId,
                taskId,
                isHttp ? HttpScheme : NetTcpScheme);
        }
    }
}
