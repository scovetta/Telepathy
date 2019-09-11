//------------------------------------------------------------------------------
// <copyright file="SimulateFailure.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Simulates failures in broker
// </summary>
//------------------------------------------------------------------------------

using Microsoft.Hpc.RuntimeTrace;
using System;
using System.Configuration;
using System.Diagnostics;

namespace Microsoft.Hpc.ServiceBroker
{
    public static class SimulateFailure
    {
        static volatile string operationToFail;
        static volatile int stepToFailOn = 1;

        const string simulateFailureTestingRegKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\HPC";
        const string simulateFailureTestingRegValue = @"EnableSimulatedBrokerFailures";

        /// <summary>
        /// Loads simulate failure configuration
        /// operationToFail setting can be 'Method' or 'Method.Step' where step specifies
        /// which failure in a method to trigger if there are more than one defined.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Initializing static fields here is actually more efficient")]
        static SimulateFailure()
        {
            try
            {
                string operationToFailSetting = ConfigurationManager.AppSettings["operationToFail"];

                if (!String.IsNullOrEmpty(operationToFailSetting))
                {
                    // Get the registry value that enables the failures
                    object onOff = Microsoft.Win32.Registry.GetValue(simulateFailureTestingRegKey, simulateFailureTestingRegValue, 0);

                    if ((onOff is int) && ((int) onOff != 0))
                    {
                        string[] operationToFailSettings = operationToFailSetting.Split('.');

                        if (operationToFailSettings.Length > 0)
                        {
                            operationToFail = operationToFailSettings[0];
                        }

                        if (operationToFailSettings.Length > 1)
                        {
                            int stepToFailOnSetting;
                            if (!Int32.TryParse(operationToFailSettings[1], out stepToFailOnSetting))
                            {
                                stepToFailOn = 1;
                            }
                            else
                            {
                                stepToFailOn = stepToFailOnSetting;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                operationToFail = null;
                TraceHelper.TraceEvent(TraceEventType.Warning, "[SimulateFailure].SimulateFailure: Exception {0}", ex);
            }
        }

        /// <summary>
        /// Fails the process if the specified failure is enabled
        /// </summary>
        /// <param name="step">Step in the metod to fail</param>
        [Conditional("DEBUG")]
        static public void FailOperation(int step)
        {
// #if _ADD_SIMULATED_FAILURES
            if (!String.IsNullOrEmpty(operationToFail) && (stepToFailOn == step))
            {
                StackFrame stackframe = new StackFrame(1);

                if (0 == String.Compare(stackframe.GetMethod().Name, operationToFail, StringComparison.InvariantCultureIgnoreCase))
                {
                     // Disable failures so they dont occur when service restarts
                    Microsoft.Win32.Registry.SetValue(simulateFailureTestingRegKey, simulateFailureTestingRegValue, 0);

                    Process process = Process.GetCurrentProcess();
                    process.Kill();
                }
            }
// #endif
        }
    }
}