using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.Eventing;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Hpc.Azure.Common;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Linq;


namespace Microsoft.Hpc.Azure.Common
{
    abstract internal class AzureTraceBase
    {
        TraceSource _trace;

        /// <summary>
        /// This is the trace event Id, from the class TracingEventId in AzureNaming.cs
        /// </summary>
        internal abstract int TraceEvtId
        {
            get;
        }

        /// <summary>
        /// This is the name of the trace source. Also the configuration switch name should be FacilityString + "Tracing";
        /// </summary>
        internal abstract string FacilityString
        {
            get;
        }

        /// <summary>
        /// Format the trace message and attach necessary information
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        internal abstract string FormatMessage(string msg);

        string _configName;

        const SourceLevels DefaultTracelevel = SourceLevels.Verbose;

        internal AzureTraceBase()
        {
            _configName = "Microsoft.Hpc.Azure." + FacilityString + "Tracing";

            _trace = new TraceSource(FacilityString);
            _trace.Switch = new SourceSwitch(_configName);
            _trace.Switch.Level = DefaultTracelevel;
            
            RoleEnvironment.Changed += new EventHandler<RoleEnvironmentChangedEventArgs>(RoleEnvironment_Changed);

            RefreshTraceSwitch();
        }

        internal void RefreshTraceSwitch()
        {
            string srcLevelStr = RoleEnvironment.GetConfigurationSettingValue(_trace.Switch.DisplayName);
            if(string.IsNullOrEmpty(srcLevelStr))
            {
                return;
            }

            try
            {
                _trace.Switch.Level = (SourceLevels)Enum.Parse(typeof(SourceLevels), srcLevelStr);
            }
            catch { }
        }

        void RoleEnvironment_Changed(object sender, RoleEnvironmentChangedEventArgs e)
        {
            if (e.Changes.Any(change => change is RoleEnvironmentConfigurationSettingChange &&
                string.Compare(((RoleEnvironmentConfigurationSettingChange)change).ConfigurationSettingName, _configName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                RefreshTraceSwitch();
            }
        }

        internal void TraceErrorInternal(string format, params object[] args)
        {
            _trace.TraceEvent(TraceEventType.Error, TraceEvtId, FormatMessage(format), args);
            _trace.Flush();
        }

        internal void TraceInformationInternal(string format, params object[] args)
        {
            _trace.TraceEvent(TraceEventType.Information, TraceEvtId, FormatMessage(format), args);
            _trace.Flush();
        }

        internal void TraceWarningInternal(string format, params object[] args)
        {
            _trace.TraceEvent(TraceEventType.Warning, TraceEvtId, FormatMessage(format), args);
            _trace.Flush();
        }

        internal void TraceVerboseInternal(string format, params object[] args)
        {
            _trace.TraceEvent(TraceEventType.Verbose, TraceEvtId, FormatMessage(format), args);
            _trace.Flush();
        }

        //The message based trace methods without format strings

        internal void TraceErrorInternal(string message)
        {
            _trace.TraceEvent(TraceEventType.Error, TraceEvtId, message);
            _trace.Flush();
        }

        internal void TraceInformationInternal(string message)
        {
            _trace.TraceEvent(TraceEventType.Information, TraceEvtId, message);
            _trace.Flush();
        }

        internal void TraceWarningInternal(string message)
        {
            _trace.TraceEvent(TraceEventType.Warning, TraceEvtId, message);
            _trace.Flush();
        }

        internal void TraceVerboseInternal(string message)
        {
            _trace.TraceEvent(TraceEventType.Verbose, TraceEvtId, message);
            _trace.Flush();
        }
    }

    internal abstract class WorkerTraceBase : AzureTraceBase
    {
        static string _nodeIp = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[SchedulerEndpointNames.NodeManagerService].IPEndpoint.Address.ToString();

        internal override string FormatMessage(string msg)
        {
            string logicalName = Environment.GetEnvironmentVariable("LogicalName");
            if (string.IsNullOrEmpty(logicalName))
            {
                logicalName = "Unbound";
            }

            StringBuilder bldr = new StringBuilder();
            bldr.Append("[" + FacilityString + "] ");
            bldr.Append("NodeName=");
            bldr.Append(logicalName);
            bldr.Append(" PhysicalIP=");
            bldr.Append(_nodeIp);
            bldr.Append(" ");
            bldr.Append(msg);

            return bldr.ToString();
        }
    }

    internal abstract class ProxyTraceBase : AzureTraceBase
    {
        static string _proxyIp = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints[SchedulerEndpointNames.ProxyServiceEndpoint].IPEndpoint.Address.ToString();

        internal override string FormatMessage(string msg)
        {
            StringBuilder bldr = new StringBuilder();
            bldr.Append("[" + FacilityString + "] ");
            bldr.Append("ProxyIP=");
            bldr.Append(_proxyIp);
            bldr.Append(" ");
            bldr.Append(msg);

            return bldr.ToString();
        }
    }
}
