// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Reflection;
    using System.IO;

    public static class LocalSession
    {
        static private bool LoadBrokerSession;

        public static bool LocalBroker
        {
            get
            {
                return LoadBrokerSession;
            }
        }

        public static void InitLocalSession(SessionStartInfo startInfo)
        {
            Debug.Assert(startInfo.TransportScheme == (TransportScheme)0x8);
            Debug.Assert(startInfo != null);

            if (startInfo.TransportScheme != (TransportScheme)0x8)
            {
                throw new NotSupportedException(SR.LocalBrokerOnlySupportNetTcp);
            }

            if (startInfo.ShareSession)
            {
                throw new NotSupportedException(SR.LocalBrokerOnlySupportNonShared);
            }

            LoadImplementations(startInfo.Headnode);

            return;
        }

        static private void LoadImplementations(string headnode)
        {
            if (!LoadBrokerSession)
            {
                AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

                ServiceHost sessionHost = new ServiceHost(GetSessionLauncherObect(headnode), new Uri("net.pipe://localhost/SessionLauncher"));
                sessionHost.AddServiceEndpoint("Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.ISessionLauncher",
                    new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), "");
                sessionHost.Open();

                ServiceHost brokerHost = new ServiceHost(GetBrokerLauncherObect(headnode), new Uri("net.pipe://localhost/BrokerLauncher"));
                brokerHost.AddServiceEndpoint("Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.IBrokerLauncher",
                    new NetNamedPipeBinding(NetNamedPipeSecurityMode.Transport), "");
                brokerHost.Open();

                LoadBrokerSession = true;
            }

        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.StartsWith("Microsoft.Hpc"))
            {
                string[] parts = args.Name.Split(',');

                if (parts.Length == 0)
                    return null;

                string file;
                string path = Environment.GetEnvironmentVariable(Constant.HomePathEnvVar);
                if (!string.IsNullOrEmpty(path))
                {
                    file = Path.Combine(path, "bin/" + parts[0] + ".dll");
                    if (File.Exists(file))
                    {
                        return Assembly.LoadFile(file);
                    }
                }
            }

            return null;
        }

        private static object GetSessionLauncherObect(string headnode)
        {
            Assembly assembly = Assembly.Load("Microsoft.Hpc.Scheduler.SessionLauncher");
            return assembly.CreateInstance("Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.SessionLauncher",
                true, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic
                , null, new object[] { headnode }, null, null);
        }

        private static object GetBrokerLauncherObect(string headnode)
        {
            Assembly assembly = Assembly.Load("Microsoft.Hpc.Scheduler.BrokerLauncher");
            return assembly.CreateInstance("Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher.BrokerLauncher",
                 true, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { headnode }, null, null);
        }
    }
}
