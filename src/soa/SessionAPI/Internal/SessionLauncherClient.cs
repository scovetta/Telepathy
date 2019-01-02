//------------------------------------------------------------------------------
// <copyright file="SessionLauncherClient.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Service client to connect the session launcher in headnode
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using Microsoft.Hpc.ServiceBroker;
    using System;
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ServiceModel.Channels;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;

    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Service client to connect the session launcher in headnode
    /// </summary>
    public class SessionLauncherClient : SessionLauncherClientBase
    {
        /// <summary>
        /// if the scheduler is running on IaaS
        /// </summary>
        private bool schedulerOnIaaS = false;

        /// <summary>
        /// Initializes a new instance of the SessionLauncherClient class.
        /// </summary>
        /// <param name="uri">the session launcher EPR</param>
        /// <param name="binding">indicating the binding</param>
        public SessionLauncherClient(Uri uri, Binding binding, bool useInternalChannel)
            : base(binding ?? GetBinding(useInternalChannel), GetEndpoint(uri, useInternalChannel))
        {
            if (!SoaHelper.IsOnAzure())
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
        }

        public SessionLauncherClient(string headnode, Binding binding, bool useInternalChannel)
            : this(Utility.GetSessionLauncher(headnode, binding, useInternalChannel), binding, useInternalChannel)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SessionLauncherClient class.
        /// </summary>
        /// <param name="startInfo">The session start info</param>
        /// <param name="binding">indicating the binding</param>
        public SessionLauncherClient(SessionStartInfo startInfo, Binding binding)
            : base(binding ?? startInfo.GetSessionLauncherBinding(), GetEndpoint(startInfo))
        {
            if (SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode))
            {
                this.schedulerOnIaaS = true;
            }

            if (startInfo.UseAad)
            {
            }
            else if ((startInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                if (this.schedulerOnIaaS || startInfo.UseWindowsClientCredential)
                {
                    string domainName;
                    string userName;
                    SoaHelper.ParseDomainUser(startInfo.Username, out domainName, out userName);
                    this.ClientCredentials.Windows.ClientCredential.Domain = domainName;
                    this.ClientCredentials.Windows.ClientCredential.UserName = userName;
                    this.ClientCredentials.Windows.ClientCredential.Password = startInfo.InternalPassword;
                }
                else if (startInfo.LocalUser)
                {
                    this.UseInternalAuthenticationAsync().GetAwaiter().GetResult();
                }
            }
            else if ((startInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (startInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.ClientCredentials.UserName.UserName = startInfo.Username;
                this.ClientCredentials.UserName.Password = startInfo.InternalPassword;
            }

            if (!SoaHelper.IsOnAzure() && !startInfo.UseAad)
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
        }

        /// <summary>
        /// Initializes a new instance of the SessionLauncherClient class.
        /// </summary>
        /// <param name="startInfo">The session attach info</param>
        /// <param name="binding">indicating the binding</param>
        public SessionLauncherClient(SessionAttachInfo attachInfo, Binding binding)
            : base(binding ?? attachInfo.GetSessionLauncherBinding(), GetEndpoint(attachInfo))
        {
            if (SoaHelper.IsSchedulerOnIaaS(attachInfo.Headnode))
            {
                this.schedulerOnIaaS = true;
            }

            if ((attachInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp || (attachInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                if (this.schedulerOnIaaS || attachInfo.UseWindowsClientCredential)
                {
                    string domainName;
                    string userName;
                    SoaHelper.ParseDomainUser(attachInfo.Username, out domainName, out userName);
                    this.ClientCredentials.Windows.ClientCredential.Domain = domainName;
                    this.ClientCredentials.Windows.ClientCredential.UserName = userName;
                    this.ClientCredentials.Windows.ClientCredential.Password = attachInfo.InternalPassword;
                }
                else if (attachInfo.UseAad)
                {

                }
                else if (attachInfo.LocalUser)
                {
                    this.UseInternalAuthenticationAsync().GetAwaiter().GetResult();
                }
            }
            else if ((attachInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
            {
                this.ClientCredentials.UserName.UserName = attachInfo.Username;
                this.ClientCredentials.UserName.Password = attachInfo.InternalPassword;
            }

            if (!SoaHelper.IsOnAzure() && !attachInfo.UseAad)
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }
        }

        /// <summary>
        /// Gets the endpoint prefix.
        /// </summary>
        public static string EndpointPrefix
        {
            get
            {
                return SessionLauncherClient.defaultEndpointPrefix;
            }
        }

        /// <summary>
        /// Get the https endpoint prefix
        /// </summary>
        public static string HttpsEndpointPrefix
        {
            get
            {
                return SessionLauncherClient.httpsEndpointPrefix;
            }
        }

        /// <summary>
        /// Get binding from configuration. If failed, fallback to default
        /// </summary>
        /// <returns>Binding Data</returns>
        private static Binding GetBinding(bool useInternalChannel)
        {
            // This file is shared by SessionAPI and BrokerLauncher
            // If for SessionAPI, it is necessary to check localbroker property for debug purpose
#if API
            if (LocalSession.LocalBroker)
            {
                return new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport);
            }
#endif
            if (useInternalChannel)
            {
                return BindingHelper.HardCodedNoAuthSessionLauncherNetTcpBinding;
            }
            else if (SoaHelper.IsCurrentUserLocal())
            {
                return BindingHelper.HardCodedInternalSessionLauncherNetTcpBinding;
            }
            else
            {
                return BindingHelper.HardCodedSessionLauncherNetTcpBinding;
            }
        }

        /// <summary>
        /// Create the endpoint of broker
        /// </summary>
        /// <param name="uri">Uri address</param>
        /// <returns>Endpoint Address</returns>
        private static EndpointAddress GetEndpoint(Uri uri, bool useInternalChannel)
        {
            // This file is shared by SessionAPI and BrokerLauncher
            // If for SessionAPI, it is necessary to check localbroker property for debug purpose
#if API
            if (LocalSession.LocalBroker)
            {
                return new EndpointAddress(uri);
            }
#endif
            // if (isLocalUser)
            // {
            //     string certThrumbprint = new NonHARegistry().GetSSLThumbprint().GetAwaiter().GetResult();
            //     return SoaHelper.CreateInternalCertEndpointAddress(uri, certThrumbprint);
            // }
            // 
            return SoaHelper.CreateEndpointAddress(uri, true, useInternalChannel);
        }

        private static EndpointAddress GetEndpoint(SessionInitInfoBase info) => GetEndpoint(new Uri(info.GetSessionLauncherAddressAsync().GetAwaiter().GetResult()), info.IsAadOrLocalUser);
    }
}
