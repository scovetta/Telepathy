// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.ServiceProcess;
    using System.Threading;

    using CommandLine;

    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Internal.LauncherHostService;
    using Microsoft.Hpc.ServiceBroker;
    using Microsoft.Telepathy.RuntimeTrace;

    using Serilog;

    using TelepathyCommon.HpcContext;

    /// <summary>
    /// Main entry point
    /// </summary>
    internal static class Program
    {
        private const string AzureBatchNodeListEnvVarName = "AZ_BATCH_NODE_LIST";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        private static void Main(string[] args)
        {
            var log = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithMachineName().CreateLogger();
            Log.Logger = log;

            if (!ParseAndSetBrokerLauncherSettings(args, BrokerLauncherSettings.Default))
            {
                // parsing failed
                return;
            }

            // clusterconnectionstring could be a machine name (for single headnode) or a connection string
            ITelepathyContext context;
            string clusterConnectionString = SoaHelper.GetSchedulerName();
            context = TelepathyContext.GetOrAdd(clusterConnectionString);

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

            TraceHelper.IsDiagTraceEnabled = SoaDiagTraceHelper.IsDiagTraceEnabled;

            LauncherHostService host = null;
            BrokerManagement brokerManagement = null;

            // richci : Run as a console application if user wants to debug (-D) or run in MSCS (-FAILOVER)
            if (BrokerLauncherSettings.Default.AsConsole)
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

            Log.CloseAndFlush();

        }

        private static bool ParseAndSetBrokerLauncherSettings(string[] args, BrokerLauncherSettings settings)
        {
            void SetBrokerLauncherSettings(StartOption option)
            {
                if (option.AsConsole)
                {
                    settings.AsConsole = true;
                    Trace.TraceInformation("Starting as console");
                }

                if (bool.TryParse(option.EnableAzureStorageQueueEndpoint, out var res))
                {
                    settings.EnableAzureStorageQueueEndpoint = res;
                    Trace.TraceInformation($"{nameof(settings.EnableAzureStorageQueueEndpoint)} set to {res}.");
                }

                if (!option.ReadSvcHostFromEnv && option.SvcHostList != null && option.SvcHostList.Any())
                {
                    var collection = new StringCollection();
                    collection.AddRange(option.SvcHostList.ToArray());
                    settings.SvcHostList = collection;
                    Trace.TraceInformation($"{nameof(settings.SvcHostList)} set to {string.Join(",", option.SvcHostList)}.");
                }

                if (option.ReadSvcHostFromEnv)
                {
                    var nodeListVar = Environment.GetEnvironmentVariable(AzureBatchNodeListEnvVarName);
                    string[] nodes = nodeListVar?.Split(';');
                    if (nodes == null || !nodes.Any())
                    {
                        throw new ArgumentException($"Environment {AzureBatchNodeListEnvVarName} is empty");
                    }

                    var collection = new StringCollection();
                    collection.AddRange(nodes);
                    settings.SvcHostList = collection;
                    Trace.TraceInformation($"{nameof(settings.SvcHostList)} set to {string.Join(",", nodes)} from env var {AzureBatchNodeListEnvVarName}={nodeListVar}.");

                }

                if (!string.IsNullOrEmpty(option.ServiceRegistrationPath))
                {
                    settings.CCP_SERVICEREGISTRATION_PATH = option.ServiceRegistrationPath;
                    Trace.TraceInformation($"{nameof(settings.CCP_SERVICEREGISTRATION_PATH)} set to {option.ServiceRegistrationPath}.");

                }

                if (!string.IsNullOrEmpty(option.AzureStorageConnectionString))
                {
                    settings.AzureStorageConnectionString = option.AzureStorageConnectionString;
                    Trace.TraceInformation($"{nameof(settings.AzureStorageConnectionString)} changed by cmd args.");
                }

                if (!string.IsNullOrEmpty(option.SessionAddress))
                {
                    settings.SessionAddress = option.SessionAddress;
                    Trace.TraceInformation($"{nameof(settings.SessionAddress)} set to {option.SessionAddress}.");
                }
            }

            var result = new Parser(s =>
                {
                    s.CaseSensitive = false;
                    s.HelpWriter = Console.Error;
                }).ParseArguments<StartOption>(args).WithParsed(SetBrokerLauncherSettings);
            return result.Tag == ParserResultType.Parsed;
        }
    }
}