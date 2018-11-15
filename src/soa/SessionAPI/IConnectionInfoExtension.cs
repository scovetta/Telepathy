//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Extension methods of IConnectionInfo
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Collections.Generic;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.ServiceBroker;

    /// <summary>
    /// Extension methods of IConnectionInfo
    /// </summary>
    public static class ConnectionInfoExtension
    {
        /// <summary>
        /// Get default session launcher Net.Tcp binding
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        public static Binding DefaultSessionLauncherNetTcpBinding(this IConnectionInfo connectionInfo)
        {
            if (connectionInfo.UseAad)
            {
                return BindingHelper.HardCodedNoAuthSessionLauncherNetTcpBinding;
            }
            else if (connectionInfo.LocalUser)
            {
                return BindingHelper.HardCodedInternalSessionLauncherNetTcpBinding;
            }
            else
            {
                return BindingHelper.HardCodedSessionLauncherNetTcpBinding;
            }
        }

        /// <summary>
        /// Get default broker launcher Net.Tcp binding
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        public static Binding DefaultBrokerLauncherNetTcpBinding(this IConnectionInfo connectionInfo)
        {
            if (connectionInfo.UseAad)
            {
                return BindingHelper.HardCodedNoAuthBrokerLauncherNetTcpBinding;
            }
            else if (connectionInfo.LocalUser)
            {
                return BindingHelper.HardCodedInternalBrokerLauncherNetTcpBinding;
            }
            else
            {
                return BindingHelper.HardCodedBrokerLauncherNetTcpBinding;
            }
        }

        /// <summary>
        /// Get broker node binding
        /// </summary>
        /// <param name="connectionInfo"></param>
        /// <returns></returns>
        public static Binding GetBrokerBinding(this IConnectionInfo connectionInfo)
        {
#if HPCPACK
            var scheme = connectionInfo.TransportScheme;
#if API
            if (LocalSession.LocalBroker)
            {
                return new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport);
            }
#endif
            if ((scheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                Binding binding;
                try
                {
                    binding = new NetTcpBinding(BindingHelper.LauncherNetTcpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = connectionInfo.DefaultBrokerLauncherNetTcpBinding();
                }
                return binding;
            }
#if !net40
            else if ((scheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                Binding binding;
                try
                {
                    binding = new NetTcpBinding(BindingHelper.LauncherNetHttpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = BindingHelper.HardCodedBrokerLauncherNetHttpsBinding;
                }
                return binding;
            }
#endif
            else if ((scheme & TransportScheme.Http) == TransportScheme.Http)
            {
                Binding binding;
                try
                {
                    binding = new BasicHttpBinding(BindingHelper.LauncherHttpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = BindingHelper.HardCodedBrokerLauncherHttpsBinding;
                }
                return binding;
            }
            else if ((scheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return BindingHelper.HardCodedBrokerLauncherNetTcpBinding;
            }
            else
            {
                return BindingHelper.HardCodedBrokerLauncherNetTcpBinding;
            }
#else
            return BindingHelper.HardCodedUnSecureNetTcpBinding;
#endif
        }

        /// <summary>
        /// Get session launcher binding
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static Binding GetSessionLauncherBinding(this IConnectionInfo info)
        {
#if API
            if (LocalSession.LocalBroker)
            {
                return new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport);
            }
#endif
            if ((info.TransportScheme & TransportScheme.NetTcp) == TransportScheme.NetTcp)
            {
                Binding binding;
                try
                {
                    binding = new NetTcpBinding(BindingHelper.LauncherNetTcpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = info.DefaultSessionLauncherNetTcpBinding();
                }
                return binding;
            }
#if !net40
            else if ((info.TransportScheme & TransportScheme.NetHttp) == TransportScheme.NetHttp)
            {
                Binding binding;
                try
                {
                    binding = new NetTcpBinding(BindingHelper.LauncherNetHttpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = BindingHelper.HardCodedSessionLauncherNetHttpsBinding;
                }
                return binding;
            }
#endif
            else if ((info.TransportScheme & TransportScheme.Http) == TransportScheme.Http)
            {
                Binding binding;
                try
                {
                    binding = new BasicHttpBinding(BindingHelper.LauncherHttpBindingName);
                }
                catch (KeyNotFoundException)
                {
                    binding = BindingHelper.HardCodedSessionLauncherHttpsBinding;
                }
                return binding;
            }
            else if ((info.TransportScheme & TransportScheme.Custom) == TransportScheme.Custom)
            {
                return info.DefaultSessionLauncherNetTcpBinding();
            }
            else
            {
                return info.DefaultSessionLauncherNetTcpBinding();
            }
        }

        /// <summary>
        /// Get the channel type
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static ChannelTypes GetChannelType(this IConnectionInfo info)
        {
            if (info.UseAad)
            {
                return ChannelTypes.AzureAD;
            }
            else if (info.LocalUser)
            {
                return ChannelTypes.Certificate;
            }
            else
            {
                return ChannelTypes.LocalAD;
            }
        }
    }
}

