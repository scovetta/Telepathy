// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.SessionLauncher.Utils
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading;

    /// <summary>
    /// Helpers used by Windows service which have HAController service controllers
    /// </summary>
    public static class ServiceControllerHelpers
    {
        /// <summary>
        /// Asynchronously monitors the scheduler service
        /// </summary>
        public static void MonitorHAControllerStopAsync(string source)
        {
            ThreadPool.QueueUserWorkItem(MonitorHAControllerStop, source);
        }

        /// <summary>
        /// Threadproc for monitoring the scheduler service
        /// </summary>
        /// <param name="state"></param>
        private static void MonitorHAControllerStop(object source)
        {
            try
            {
                using (ServiceController controller = new ServiceController(HpcServiceNames.HpcHAController))
                {
                    controller.WaitForStatus(ServiceControllerStatus.Stopped);
                }
            }

            catch (Exception e)
            {
                EventLog.WriteEntry((string) source, String.Format("Failed to monitor HAController. Exiting service - {0}", e), EventLogEntryType.Error);
            }

            Environment.Exit(-1);
        }
    }
}
