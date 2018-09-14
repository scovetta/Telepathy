//-----------------------------------------------------------------------
// <copyright file="HpcAzureTraceListener.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Implement trace listener take TextWriterTraceListener for rollover listener</summary>
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace Microsoft.Hpc.Azure.Common
{
    /// <summary>
    /// RollingListener's implemented by TextWriterTraceListener
    /// </summary>
    /// <remarks>
    /// This is reflected source from Microsoft.WindowsAzure.GuestAgent.ContainerStateMachine.WaAppAgentTraceListener 
    /// in Microsoft.WindowsAzure.GuestAgent.ContainerStateMachine.dll
    /// </remarks>
    public class HpcAzureTraceListener : RollingListener
    {
        // Methods
        public HpcAzureTraceListener(string initializeData)
            : base(GetTraceFilePath(initializeData))
        {
        }

        protected override void CloseTraceListener(TraceListener listener)
        {
            listener.Dispose();
        }

        private static string GetTraceFilePath(string traceFileName)
        {
            return Environment.ExpandEnvironmentVariables(traceFileName);
        }

        protected override TraceListener OpenTraceListener(string path)
        {
            TraceListener listener =  new TextWriterTraceListener(path);
            listener.TraceOutputOptions = this.TraceOutputOptions;
            listener.IndentLevel = this.IndentLevel;
            listener.IndentSize = this.IndentSize;
            return listener;
        }
    }
}


