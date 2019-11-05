// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.ServiceProcess;
    using System.Threading;

    using CommandLine;

    using Microsoft.Hpc.Scheduler.Session.LauncherHostService;
    using Microsoft.Telepathy.Common.TelepathyContext;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.AzureBatch;
    using Microsoft.Telepathy.Internal.SessionLauncher.Impls.SessionLaunchers.Local;
    using Microsoft.Telepathy.RuntimeTrace;
    using Microsoft.Telepathy.Session.Internal;
    using Newtonsoft.Json;

    using Serilog;

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
            var log = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithMachineName().CreateLogger();

            Log.Logger = log;
            try
            {
                if (!ParseAndSetGlobalConfiguration(args))
                {
                    // Parsing error
                    return;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("Excepetion parsing and setting configuration - " + e);
                Log.CloseAndFlush();
                return;
            }

            if (SessionLauncherRuntimeConfiguration.ConfigureLogging)
            {
                Trace.TraceInformation("Log configuration for Session Launcher has done successfully.");
                Log.CloseAndFlush();
                return;
            }

            TelepathyContext.GetOrAdd();

            LauncherHostService host = null;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (SessionLauncherRuntimeConfiguration.AsConsole)
            {
                try
                {
                    host = new LauncherHostService();
                    host.OpenService().Wait();
                    Console.WriteLine("Press any key to exit...");
                    Thread.Sleep(-1);
                }
                finally
                {
                    if (host != null)
                    {
                        try
                        {
                            host.StopService();
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception stopping service - " + e);
                        }
                    }
                }
            }
            else
            {
                ServiceBase[] servicesToRun;
                servicesToRun = new ServiceBase[] { new LauncherHostService() };
                ServiceBase.Run(servicesToRun);
            }

            Log.CloseAndFlush();
        }

        /// <summary>
        /// event to trace the unhandled exception.
        /// </summary>
        /// <param name="sender">the sender.</param>
        /// <param name="e">the exception arguments.</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            TraceHelper.TraceEvent(TraceEventType.Critical, "[SessionLauncher] Unhandled {2} exception found in {0}: \n{1}", sender, e.ExceptionObject, e.IsTerminating ? "fatal" : string.Empty);
            Log.CloseAndFlush();
        }

        private static bool ParseAndSetGlobalConfiguration(string[] args)
        {
            void SetGlobalConfiguration(SessionLauncherStartOption option)
            {
                if (option.AsConsole)
                {
                    SessionLauncherRuntimeConfiguration.AsConsole = true;
                }

                if (option.ConfigureLogging)
                {
                    SessionLauncherRuntimeConfiguration.ConfigureLogging = true;
                    Trace.TraceInformation("Set configureLogging true");
                }

                if (!string.IsNullOrEmpty(option.JsonFilePath))
                {
                    string[] argsInJson = JSONFileParser.parse(option.JsonFilePath);
                    var parserResult = new Parser(
                        s =>
                            {
                                s.CaseSensitive = false;
                                s.HelpWriter = Console.Error;
                            }).ParseArguments<SessionLauncherStartOption>(argsInJson).WithParsed(SetGlobalConfiguration);
                    if (parserResult.Tag != ParserResultType.Parsed)
                    {
                        TraceHelper.TraceEvent(TraceEventType.Critical, "[SessionLauncher] Parse arguments error.");
                        throw new ArgumentException("Parse arguments error.");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(option.AzureBatchServiceUrl))
                    {
                        SessionLauncherRuntimeConfiguration.SchedulerType = SchedulerType.AzureBatch;
                        AzureBatchConfiguration.BatchServiceUrl = option.AzureBatchServiceUrl;
                        AzureBatchConfiguration.BatchAccountName = option.AzureBatchAccountName;
                        AzureBatchConfiguration.BatchAccountKey = option.AzureBatchAccountKey;
                        AzureBatchConfiguration.SoaBrokerStorageConnectionString = option.AzureBatchBrokerStorageConnectionString;
                        AzureBatchConfiguration.BrokerLauncherPath = option.BrokerLauncherExePath;
                    }
                    else if (!string.IsNullOrEmpty(option.HpcPackSchedulerAddress))
                    {
                        SessionLauncherRuntimeConfiguration.SchedulerType = SchedulerType.HpcPack;
                    }
                    else if (!string.IsNullOrEmpty(option.ServiceHostExePath))
                    {
                        SessionLauncherRuntimeConfiguration.SchedulerType = SchedulerType.Local;
                        LocalSessionConfiguration.BrokerLauncherExePath = option.BrokerLauncherExePath;
                        LocalSessionConfiguration.ServiceHostExePath = option.ServiceHostExePath;
                        LocalSessionConfiguration.ServiceRegistrationPath = option.ServiceRegistrationPath;
                        LocalSessionConfiguration.BrokerStorageConnectionString = option.LocalBrokerStorageConnectionString;
                    }

                    if (!string.IsNullOrEmpty(option.AzureBatchPoolName))
                    {
                        AzureBatchConfiguration.BatchPoolName = option.AzureBatchPoolName;
                    }

                    if (!string.IsNullOrEmpty(option.AzureBatchBrokerStorageConnectionString))
                    {
                        SessionLauncherRuntimeConfiguration.SessionLauncherStorageConnectionString = option.AzureBatchBrokerStorageConnectionString;
                    }

                    if (!string.IsNullOrEmpty(option.Logging))
                    {
                        try
                        {
                            LogHelper.SetLoggingConfig(option, $"{Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location)}/HpcSession.exe.config", "SessionLauncher");
                        }
                        catch (Exception e)
                        {
                            Trace.TraceError("Exception occurs when configure logging - " + e);
                        }
                    }
                }
            }

            var result = new Parser(
                s =>
                    {
                        s.CaseSensitive = false;
                        s.HelpWriter = Console.Error;
                    }).ParseArguments<SessionLauncherStartOption>(args).WithParsed(SetGlobalConfiguration);
            return result.Tag == ParserResultType.Parsed;
        }
    }
}