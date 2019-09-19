// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// The Broker Launcher Client
    /// </summary>
    public class BrokerLauncherClient : BrokerLauncherClientBase
    {
        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        /// <param name="binding">indicting the binding</param>
        public BrokerLauncherClient(SessionInitInfoBase info, Binding binding, Uri uri)
            : base(binding ?? info.GetBrokerBinding(), GetEndpoint(uri, info))
        {
            this.InitAzureOrAadOrCertAuth(info, info.Username, info.InternalPassword);
        }

        public BrokerLauncherClient(SessionInfoBase info, Binding binding, Uri uri)
            : base(binding ?? info.GetBrokerBinding(), GetEndpoint(uri, info))
        {
            this.InitAzureOrAadOrCertAuth(info, info.Username, info.InternalPassword);

        }

        private void InitAzureOrAadOrCertAuth(IConnectionInfo info, string username, string password)
        {
#if HPCPACK
            if (!SoaHelper.IsOnAzure() && !info.UseAad)
            {
                this.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
            }

#if !net40
            if (info.UseAad)
            {
                this.Endpoint.Behaviors.UseAadClientBehaviors(info.Headnode, username, password).GetAwaiter().GetResult();
                return; // If we use aad authentication, we don't care about if it is a local user anymore.
            }
#endif
            if (info.LocalUser)
            {
                // use certificate for cluster internal authentication
                this.UseInternalAuthenticationAsync().GetAwaiter().GetResult();
            }
#endif
        }

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        /// <param name="startInfo">The session start info</param>
        /// <param name="binding">indicting the binding</param>
        public BrokerLauncherClient(Uri uri, SessionStartInfo startInfo, Binding binding)
            : this(startInfo, binding, uri)
        {
            if (startInfo.UseAad)
            {
            }
            else if ((startInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                if (SoaHelper.IsSchedulerOnIaaS(startInfo.Headnode) || startInfo.UseWindowsClientCredential)
                {
                    string domainName;
                    string userName;
                    SoaHelper.ParseDomainUser(startInfo.Username, out domainName, out userName);
                    this.ClientCredentials.Windows.ClientCredential.Domain = domainName;
                    this.ClientCredentials.Windows.ClientCredential.UserName = userName;
                    this.ClientCredentials.Windows.ClientCredential.Password = startInfo.InternalPassword;
                }
            }
            else if ((startInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (startInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.ClientCredentials.UserName.UserName = startInfo.Username;
                this.ClientCredentials.UserName.Password = startInfo.InternalPassword;
            }

        }

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        /// <param name="startInfo">The session attach info</param>
        /// <param name="binding">indicting the binding</param>
        public BrokerLauncherClient(Uri uri, SessionAttachInfo attachInfo, Binding binding)
            : this(attachInfo, binding, uri)
        {
            if (attachInfo.UseAad)
            {
            }
            else if ((attachInfo.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                if (SoaHelper.IsSchedulerOnIaaS(attachInfo.Headnode) || attachInfo.UseWindowsClientCredential)
                {
                    string domainName;
                    string userName;
                    SoaHelper.ParseDomainUser(attachInfo.Username, out domainName, out userName);
                    this.ClientCredentials.Windows.ClientCredential.Domain = domainName;
                    this.ClientCredentials.Windows.ClientCredential.UserName = userName;
                    this.ClientCredentials.Windows.ClientCredential.Password = attachInfo.InternalPassword;
                }
            }
            else if ((attachInfo.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (attachInfo.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.ClientCredentials.UserName.UserName = attachInfo.Username;
                this.ClientCredentials.UserName.Password = attachInfo.InternalPassword;
            }
        }

        /// <summary>
        /// Initializes a new instance of the BrokerLauncherClient class.
        /// </summary>
        /// <param name="uri">The broker launcher EPR</param>
        /// <param name="info">The sesion info</param>
        /// <param name="binding">indicting the binding</param>
        public BrokerLauncherClient(Uri uri, SessionInfoBase info, Binding binding)
            : this(info, binding, uri)
        {
            if (info.UseAad)
            {
            }
            else if ((info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                SessionInfo sessionInfo = info as SessionInfo;
                if (SoaHelper.IsSchedulerOnIaaS(info.Headnode) || (sessionInfo != null && sessionInfo.UseWindowsClientCredential))
                {
                    string domainName;
                    string userName;
                    SoaHelper.ParseDomainUser(info.Username, out domainName, out userName);
                    this.ClientCredentials.Windows.ClientCredential.Domain = domainName;
                    this.ClientCredentials.Windows.ClientCredential.UserName = userName;
                    this.ClientCredentials.Windows.ClientCredential.Password = info.InternalPassword;
                }
            }
            else if ((info.TransportScheme & TransportScheme.Http) == TransportScheme.Http || (info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                this.ClientCredentials.UserName.UserName = info.Username;
                this.ClientCredentials.UserName.Password = info.InternalPassword;
            }
        }

        /// <summary>
        /// Create the endpoint of broker
        /// </summary>
        /// <param name="uri">Uri address</param>
        /// <returns>Endpoint Address</returns>
        private static EndpointAddress GetEndpoint(Uri uri, IConnectionInfo info)
        {
#if API
            if (LocalSession.LocalBroker)
            {
                return new EndpointAddress(uri);
            }
#endif
            // if (info.LocalUser)
            // {
            //     string certThrumbprint = new NonHARegistry().GetValueAsync<string>(HpcConstants.HpcFullKeyName, HpcConstants.SslThumbprint, CancellationToken.None, null).GetAwaiter().GetResult();
            //     return SoaHelper.CreateInternalCertEndpointAddress(uri, certThrumbprint);
            // }

            return SoaHelper.CreateEndpointAddress(uri,false, info.IsAadOrLocalUser);
        }
    }
}
