//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The main entry point for the application.
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.BrokerShim
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.ServiceModel;
    using System.Threading;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Session.Interface;
    using Microsoft.Hpc.ServiceBroker;

    using Serilog;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// It is a format of the date time string. It does not include '/' or
        /// ':', because it is used as a part of cosmos log file name.
        /// </summary>
        private const string DataTimeStringFormat = "HHmmss";

        /// <summary>
        /// Stores the trace instance
        /// </summary>
        private static RuntimeTraceWrapper trace;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">indicating arguments</param>
        private static int Main(string[] args)
        {
            var log = new LoggerConfiguration().ReadFrom.AppSettings().Enrich.WithMachineName().CreateLogger();
            Log.Logger = log;

            //SingletonRegistry.Initialize(SingletonRegistry.RegistryMode.WindowsNonHA);
#if HPCPACK
            WinServiceHpcContextModule.GetOrAddWinServiceHpcContextFromEnv().GetAADClientAppIdAsync().FireAndForget(); // cache AAD AppId now.
#endif
            // improve http performance for Azure storage queue traffic
            ServicePointManager.DefaultConnectionLimit = 1000;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.UseNagleAlgorithm = false;

            int pid = Process.GetCurrentProcess().Id;

            trace = TraceHelper.RuntimeTrace;

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            ManualResetEvent exitWaitHandle = new ManualResetEvent(false);

            // ThreadPoolMonitor.StartOnlyInDebug();

            try
            {
                Uri brokerManagementServiceAddress = new Uri(SoaHelper.GetBrokerManagementServiceAddress(pid));

                ServiceHost host;

                try
                {
                    BrokerManagementService instance = new BrokerManagementService(exitWaitHandle);

                    trace.LogBrokerWorkerMessage(pid,
                        string.Format("[Main] Try open broker management service at {0}.", brokerManagementServiceAddress.ToString()));

                    host = new ServiceHost(instance, brokerManagementServiceAddress);
                    host.CloseTimeout = TimeSpan.FromSeconds(1);
                    host.AddServiceEndpoint(typeof(IBrokerManagementService), BindingHelper.HardCodedBrokerManagementServiceBinding, String.Empty);
                    host.Open();

                    trace.LogBrokerWorkerMessage(pid, "[Main] Open broker management service succeeded.");
                }
                catch (Exception e)
                {
                    trace.LogBrokerWorkerUnexpectedlyExit(pid,
                        string.Format("[Main] Failed to open broker management service: {0}", e));

                    return (int)BrokerShimExitCode.FailedOpenServiceHost;
                }

                bool createdNew;
                EventWaitHandle initializeWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, String.Format(Constant.InitializationWaitHandleNameFormat, pid), out createdNew);
                if (createdNew && !BrokerWorkerSetting.Default.Debug)
                {
                    trace.LogBrokerWorkerUnexpectedlyExit(pid,
                        "[Main] Initialize wait handle has not been created by the broker launcher.");

                    return (int)BrokerShimExitCode.InitializeWaitHandleNotExist;
                }

                if (!initializeWaitHandle.Set())
                {
                    trace.LogBrokerWorkerUnexpectedlyExit(pid,
                        "[Main] Failed to set the initialize wait handle.");

                    return (int)BrokerShimExitCode.FailedToSetInitializeWaitHandle;
                }

                // Wait for exit
                exitWaitHandle.WaitOne();

                try
                {
                    // Make sure server is terminated correctly and the client is getting notified
                    // Swallow exception and ignore any failure because we don't want broker to retry
                    host.Close();
                }
                catch (Exception ex1)
                {
                    trace.LogBrokerWorkerMessage(
                        pid,
                        string.Format(CultureInfo.InvariantCulture, "[Program].Main: Exception {0}", ex1));

                    trace.LogBrokerWorkerMessage(pid,
                        "[Main] Failed to close the ServiceHost.");

                    try
                    {
                        host.Abort();
                    }
                    catch (Exception ex)
                    {
                        trace.LogBrokerWorkerMessage(
                            pid,
                            string.Format(CultureInfo.InvariantCulture, "[Program].Main: Exception {0}", ex));

                        trace.LogBrokerWorkerMessage(pid,
                            "[Main] Failed to abort the ServiceHost.");
                    }
                }

                return (int)BrokerShimExitCode.Success;
            }
            catch (Exception e)
            {
                trace.LogBrokerWorkerUnexpectedlyExit(pid, String.Format("[Main] Exception thrown to ROOT: {0}", e));
                throw;
            }
            finally
            {
                trace.LogBrokerWorkerMessage(pid, "[Main] Process exit.");
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Write trace for unhandled exception
        /// </summary>
        /// <param name="sender">indicating the sender</param>
        /// <param name="e">indicating the event arguments</param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            int pid = Process.GetCurrentProcess().Id;
            trace.LogBrokerWorkerUnexpectedlyExit(pid, String.Format("[Main] Unhandled exception: {0}", e.ExceptionObject));
        }
    }
}
