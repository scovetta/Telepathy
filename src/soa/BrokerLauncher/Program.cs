//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The main entry point for the application.
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session.Internal.LauncherHostService
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;

    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Internal.BrokerLauncher;
    using Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Hpc.SoaContext;

    /// <summary>
    /// Main entry point
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        private static void Main(string[] args)
        {
            // clusterconnectionstring could be a machine name (for single headnode) or a connection string
#if HPCPACK
            string clusterConnectionString = SoaHelper.GetSchedulerName(false);
            var context = HpcContext.GetOrAdd(clusterConnectionString, CancellationToken.None, true);
#else
            var context = new SoaContext();
#endif
            Trace.TraceInformation("Get diag trace enabled internal.");
            SoaDiagTraceHelper.IsDiagTraceEnabledInternal = (sessionId) =>
                {
                    try
                    {
                        using (ISchedulerHelper helper = SchedulerHelperFactory.GetSchedulerHelper(context))
                        {
                            return helper.IsDiagTraceEnabled(sessionId).GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("[SoaDiagTraceHelper] Failed to get IsDiagTraceEnabled property: {0}", e);
                        return false;
                    }
                };

            SetBrokerLauncherSettings(args, BrokerLauncherSettings.Default);

            TraceHelper.IsDiagTraceEnabled = SoaDiagTraceHelper.IsDiagTraceEnabled;

            LauncherHostService host = null;
            BrokerManagement brokerManagement = null;

            // richci : Run as a console application if user wants to debug (-D) or run in MSCS (-FAILOVER)
            if (args.Length > 0
                && (string.Compare(args[0], "-D", StringComparison.InvariantCultureIgnoreCase) == 0 || string.Compare(args[0], "-FAILOVER", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                try
                {
                    host = new LauncherHostService(true, context);

                    // This instance of HpcBroker is running as a failover generic application or in debug
                    // mode so startup the brokerManagement WCF service to accept management commands
                    brokerManagement = new BrokerManagement(host.BrokerLauncher);
                    brokerManagement.Open();

                    Console.WriteLine("Press any key to exit...");
                    Thread.Sleep(-1);
                }
                finally
                {
                    if (host != null)
                    {
                        try
                        {
                            host.Stop();
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception stopping HpcBroker service - " + e);
                        }
                    }

                    if (brokerManagement != null)
                    {
                        try
                        {
                            brokerManagement.Close();
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception closing broker managment WCF service - " + e);
                        }
                    }
                }
            }
            else
            {
                ServiceBase[] servicesToRun;
                servicesToRun = new ServiceBase[] { new LauncherHostService(context) };
                ServiceBase.Run(servicesToRun);
            }
        }

        // TODO: replace this with a industry strength level parsing package
        private static void SetBrokerLauncherSettings(string[] args, BrokerLauncherSettings settings)
        {
            if (args == null || args.Length == 0)
            {
                return;
            }

            for (int i = 0; i != args.Length; ++i)
            {
                void ThrowIfLastItem()
                {
                    if (i + 1 >= args.Length)
                    {
                        throw new InvalidOperationException();
                    }
                }

                switch (args[i])
                {
                    case "-CCP_SERVICEREGISTRATION_PATH":
                        ThrowIfLastItem();
                        settings.CCP_SERVICEREGISTRATION_PATH = args[i + 1];
                        break;
                    case "-AzureStorageConnectionString":
                        ThrowIfLastItem();
                        settings.AzureStorageConnectionString = args[i + 1];
                        break;
                    case "-EnableAzureStorageQueueEndpoint":
                        ThrowIfLastItem();
                        settings.EnableAzureStorageQueueEndpoint = bool.Parse(args[i + 1]);
                        break;
                    case "-SvcHostList":
                        ThrowIfLastItem();
                        var list = args[i + 1].Split(',');
                        var strc = new System.Collections.Specialized.StringCollection();
                        strc.AddRange(list);
                        settings.SvcHostList = strc;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}