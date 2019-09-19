// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal.LauncherHostService
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IO;
    using System.Security.Principal;
    using System.ServiceProcess;
    using Microsoft.Hpc;

    internal static class NetTcpPortSharingService
    {
        public const string ServiceKey = "NetTcpPortSharing";
        private const string AdminValue = "S-1-5-20";
        private const string UserValue = "S-1-5-32-545";
        private const string AllowAccountsTag = "AllowAccounts";
        private const string ServiceModelTag = "system.serviceModel.activation";
        private const string NetTcpTag = "net.tcp";
        private const string SecurityIdentifierTag = "SecurityIdentifier";

        private static List<string> AllowAccounts = null;

        /// <summary>
        /// Initialised the required configuration for the 
        /// service when the cluster is running application
        /// integrated jobs.
        /// </summary>
        public static void SetRequiredConfigurationForApplicationIntegration()
        {
            // the service must be enabled.
            ServiceNativeMethods.SERVICE_STARTTYPE startType = ServiceHelpers.GetServiceStartType(ServiceKey);

            if (startType == ServiceNativeMethods.SERVICE_STARTTYPE.SERVICE_DISABLED)
            {
                ServiceHelpers.SetServiceStartType(ServiceKey, ServiceNativeMethods.SERVICE_STARTTYPE.SERVICE_DEMAND_START);
            }

            AllowAccounts = new List<string>();
            // the admins and users groups should be granted 
            // permission to create endpoints.
            AllowAccounts.Add(NetTcpPortSharingService.AdminValue);
            AllowAccounts.Add(NetTcpPortSharingService.UserValue);
            Update();
        }

        private static void Restart()
        {
            Stack<ServiceController> controllersToStop = new Stack<ServiceController>();
            Stack<ServiceController> controllersToStart = new Stack<ServiceController>();
            Queue<ServiceController> controllersToProcess = new Queue<ServiceController>();
            try
            {
                //work out the set of services we need to stop.
                ServiceController service = new ServiceController(ServiceKey);

                controllersToProcess.Enqueue(service);
                while (controllersToProcess.Count > 0)
                {
                    ServiceController serviceToProcess = controllersToProcess.Dequeue();
                    if (serviceToProcess.Status != ServiceControllerStatus.Stopped)
                    {
                        foreach (ServiceController dependentService in serviceToProcess.DependentServices)
                        {
                            controllersToProcess.Enqueue(dependentService);
                        }
                        controllersToStop.Push(serviceToProcess);
                    }
                }

                if (controllersToStop.Count > 0)
                {
                    //Need to restart the service since service already started
                    //now stop them
                    while (controllersToStop.Count > 0)
                    {
                        ServiceController serviceToStop = controllersToStop.Pop();
                        if (serviceToStop.Status != ServiceControllerStatus.Stopped)
                        {
                            controllersToStart.Push(serviceToStop);
                            serviceToStop.Stop();
                            serviceToStop.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 2, 0));
                        }
                        else
                        {
                            serviceToStop.Dispose();
                        }
                    }

                    //now start them again.
                    while (controllersToStart.Count > 0)
                    {
                        ServiceController serviceToStart = controllersToStart.Pop();
                        try
                        {
                            serviceToStart.Start();
                            serviceToStart.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 2, 0));
                        }
                        finally
                        {
                            serviceToStart.Dispose();
                        }
                    }
                }
                else
                {
                    // service is not start, just start the service
                    service.Start();
                }
            }
            finally
            {
                //clean up all the controllers.
                foreach (ServiceController controller in controllersToStop)
                {
                    controller.Dispose();
                }

                foreach (ServiceController controller in controllersToStart)
                {
                    controller.Dispose();
                }
                foreach (ServiceController controller in controllersToProcess)
                {
                    controller.Dispose();
                }
            }
        }

        /// <summary>
        /// Will bring the service configuration in line
        /// with the model.
        /// </summary>
        /// <param name="session"></param>
        /// <param name="currentConfiguration"></param>
        private static void Update()
        {
           //update the configuration to match the sids in the file.
            string cfgFile = getSMSvcHostConfigFilePath();
            string cfgFile64 = getSMSvcHostConfigFilePath64();

            if (File.Exists(cfgFile64))
            {
                Configuration c = ConfigurationManager.OpenExeConfiguration(cfgFile64);
                ReadConfiguration(c);
                WriteConfiguration(c);
                //session.Information(Resources.NetTcpPortSharingService_UpdatingConfig, c.FilePath);
                c.Save();
            }
            else
            {
                if (File.Exists(cfgFile))
                {
                    Configuration c = ConfigurationManager.OpenExeConfiguration(cfgFile);
                    ReadConfiguration(c);
                    WriteConfiguration(c);
                    //session.Information(Resources.NetTcpPortSharingService_UpdatingConfig, c.FilePath);
                    c.Save();
                }
            }

            Restart();
        }

        /// <summary>
        /// Walks through the config file extracting the data we need for the model.
        /// </summary>
        /// <param name="tcpHostingConfiguration"></param>
        private static void ReadConfiguration(Configuration tcpHostingConfiguration)
        {
            ConfigurationSectionGroup sg = tcpHostingConfiguration.SectionGroups[ServiceModelTag];
            ConfigurationSection cs = sg.Sections[NetTcpTag];

            //avoid taking a dependency on System.ServiceModel,
            //use reflection to get the property collections we need.
            System.Reflection.PropertyInfo pi = cs.GetType().GetProperty(AllowAccountsTag);
            ConfigurationElementCollection allowedAccounts = (ConfigurationElementCollection)pi.GetValue(cs, null);

            //enumerates over System.ServiceModel.Activation.Configuration.SecurityIdentifierElement
            List<String> currentSids = new List<string>();
            foreach (ConfigurationElement securityIdentiferElement in allowedAccounts)
            {
                SecurityIdentifier sid = (SecurityIdentifier) securityIdentiferElement.GetType().GetProperty(SecurityIdentifierTag).GetValue(securityIdentiferElement, null);
                if (!currentSids.Contains(sid.ToString()))
                {
                    currentSids.Add(sid.ToString());
                }
            }

            foreach (string sid in currentSids)
            {
                if (!AllowAccounts.Contains(sid))
                    AllowAccounts.Add(sid);
            }
        }

        /// <summary>
        /// Updates the configuration file to match the model
        /// </summary>
        /// <param name="tcpHostingConfiguration">The configuration file to update</param>
        public static void WriteConfiguration(Configuration tcpHostingConfiguration)
        {
            ConfigurationSectionGroup sg = tcpHostingConfiguration.SectionGroups[ServiceModelTag];
            ConfigurationSection cs = sg.Sections[NetTcpTag];

            //avoid taking a dependency on System.ServiceModel,
            //use reflection to get the property collections we need.
            System.Reflection.PropertyInfo pi = cs.GetType().GetProperty(AllowAccountsTag);
            ConfigurationElementCollection allowedAccounts = (ConfigurationElementCollection)pi.GetValue(cs, null);

            //enumerates over System.ServiceModel.Activation.Configuration.SecurityIdentifierElement
            List<String> currentSids = new List<string>();
            foreach (ConfigurationElement securityIdentiferElement in allowedAccounts)
            {
                SecurityIdentifier sid = (SecurityIdentifier)securityIdentiferElement.GetType().GetProperty(SecurityIdentifierTag).GetValue(securityIdentiferElement, null);
                if (!currentSids.Contains(sid.ToString()))
                {
                    currentSids.Add(sid.ToString());
                }
            }

            //now add the sids that are missing
            //add, contains, remove
            SecurityIdentifierCollectionHelperClass helper = new SecurityIdentifierCollectionHelperClass(allowedAccounts);
            foreach(String sid in AllowAccounts)
            {
                if (!currentSids.Contains(sid))
                {
                    helper.Add(sid);
                }
                else
                {
                    currentSids.Remove(sid);
                }
            }

            foreach(String sid in currentSids)
            {
                helper.Remove(sid);
            }
        }

        /// <summary>
        /// Gets the path to the service on a 32 bit platform
        /// </summary>
        /// <returns></returns>
        private static string getSMSvcHostConfigFilePath()
        {
            return Path.Combine(
                getNetFrameworkRoot(),
                @"Framework\v3.0\Windows Communication Foundation\SMSvcHost.exe");
        }

        /// <summary>
        /// Gets the path to the service on a 64 bit platform
        /// </summary>
        /// <returns></returns>
        private static string getSMSvcHostConfigFilePath64()
        {
            return Path.Combine(
                getNetFrameworkRoot(),
                @"Framework64\v3.0\Windows Communication Foundation\SMSvcHost.exe");
        }

        private static string getNetFrameworkRoot()
        {
            return Path.Combine(Environment.GetEnvironmentVariable("windir"), "Microsoft.NET");
        }

        /// <summary>
        /// Reflection based wrapper that provides access to the 
        /// System.ServiceModel SecurityIdentifierCollection
        /// </summary>
        private class SecurityIdentifierCollectionHelperClass
        {
            private ConfigurationElementCollection target;

            public SecurityIdentifierCollectionHelperClass(ConfigurationElementCollection target)
            {
                this.target = target;
            }

            public bool Contains(string sid)
            {
                return (bool) this.target.GetType().GetMethod("ContainsKey").Invoke(this.target, new object[] { sid });
            }

            public void Add(string sid)
            {
                object value = this.CreateSecurityElement(sid);
                this.target.GetType().GetMethod("Add").Invoke(this.target, new object[] { value });
            }

            public void Remove(string sid)
            {
                object value = this.CreateSecurityElement(sid);
                this.target.GetType().GetMethod("Remove").Invoke(this.target, new object[] { value });
            }

            private Object CreateSecurityElement(string sid)
            {
                System.Security.Principal.SecurityIdentifier identifier = new System.Security.Principal.SecurityIdentifier(sid);
                Type elementType = this.target.GetType().Assembly.GetType("System.ServiceModel.Activation.Configuration.SecurityIdentifierElement");
                System.Reflection.ConstructorInfo constructor = elementType.GetConstructor(new Type[] { typeof(System.Security.Principal.SecurityIdentifier) });
                return constructor.Invoke(new object[] { identifier });
            }
        }
    }
}
