// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.Hpc.AADAuthUtil;
    using Microsoft.Hpc.RuntimeTrace;
    using Microsoft.Hpc.Scheduler.Properties;
    using Microsoft.Hpc.Scheduler.Session.Internal.Common;

    /// <summary>
    /// the service job helper class.
    /// </summary>
    internal static class JobHelper
    {
        /// <summary>
        /// This method will only be internally called by session launcher,
        /// before the job is submitted. It combines the user's job properties and the
        /// items set in the service start info. The job properties will override the start info items.
        /// </summary>
        /// <param name="startInfo">the session start info</param>
        /// <param name="schedulerJob">service job</param>
        /// <param name="traceLevel">diag trace level</param>
        internal static void MakeJobProperties(SessionStartInfoContract startInfo, ISchedulerJob schedulerJob, string traceLevel)
        {
            Debug.Assert(startInfo != null,
                         "The session startInfo cannot be null.");

            Debug.Assert(!string.IsNullOrEmpty(startInfo.ServiceName),
                         "The service name in the session start info cannnot be null or empty.");

            schedulerJob.ServiceName = startInfo.ServiceName;
            schedulerJob.SetEnvironmentVariable("CCP_SERVICENAME", startInfo.ServiceName);
            if (startInfo.CanPreempt.HasValue)
            {
                schedulerJob.CanPreempt = startInfo.CanPreempt.Value;
            }

            if (!string.IsNullOrEmpty(startInfo.JobTemplate))
            {
                schedulerJob.SetJobTemplate(startInfo.JobTemplate);
            }

            if (startInfo.Priority.HasValue)
            {
                schedulerJob.Priority = (JobPriority)startInfo.Priority.Value;
            }

            if (startInfo.ExtendedPriority.HasValue)
            {
                schedulerJob.ExpandedPriority = startInfo.ExtendedPriority.Value;
            }

            schedulerJob.Progress = 0;

            // For max units
            if (startInfo.MaxUnits != null)
            {
                schedulerJob.MaximumNumberOfCores =
                    schedulerJob.MaximumNumberOfSockets =
                    schedulerJob.MaximumNumberOfNodes = startInfo.MaxUnits.Value;

                schedulerJob.AutoCalculateMax = false;
            }

            // For min units
            if (startInfo.MinUnits != null)
            {
                schedulerJob.MinimumNumberOfCores =
                    schedulerJob.MinimumNumberOfSockets =
                    schedulerJob.MinimumNumberOfNodes = startInfo.MinUnits.Value;

                schedulerJob.AutoCalculateMin = false;
            }

            // Should set UnitType after above resource count update
            if (startInfo.ResourceUnitType != null)
            {
                schedulerJob.UnitType = (JobUnitType)startInfo.ResourceUnitType.Value;
            }

            schedulerJob.Name = string.IsNullOrEmpty(startInfo.ServiceJobName) ?
                                string.Format("{0} - WCF service", startInfo.ServiceName) :
                                startInfo.ServiceJobName;

            if (!string.IsNullOrEmpty(startInfo.ServiceJobProject))
            {
                schedulerJob.Project = startInfo.ServiceJobProject;
            }

            if (!string.IsNullOrEmpty(startInfo.NodeGroupsStr))
            {
                string[] nodes = startInfo.NodeGroupsStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string node in nodes)
                {
                    schedulerJob.NodeGroups.Add(node);
                }
            }

            if (!string.IsNullOrEmpty(startInfo.RequestedNodesStr))
            {
                schedulerJob.RequestedNodes = new StringCollection(startInfo.RequestedNodesStr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (startInfo.Runtime >= 0)
            {
                schedulerJob.Runtime = startInfo.Runtime;
            }

            // start adding the broker settings.
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.ShareSession, startInfo.ShareSession.ToString());
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.UseAad, startInfo.UseAad.ToString());
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.Secure, startInfo.Secure.ToString());
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.TransportScheme, ((int)startInfo.TransportScheme).ToString());
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.UseAzureQueue, (startInfo.UseAzureQueue == true).ToString());
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.LocalUser, (startInfo.LocalUser == true).ToString());

            var context = HpcContext.Get();
            var principal = Thread.CurrentPrincipal;
            if (principal.IsHpcAadPrincipal(context))
            {
                string identity = principal.GenerateSecurityIdentifierFromAadPrincipal(context).Value + ";" + principal.Identity.Name;
                schedulerJob.SetCustomProperty(BrokerSettingsConstants.AadUserIdentity, identity);
            }

            // Save ServiceVersion if set
            if (startInfo.ServiceVersion != null)
            {
                schedulerJob.SetCustomProperty(BrokerSettingsConstants.ServiceVersion, startInfo.ServiceVersion.ToString());
            }

            string[] customPropNames = new string[]
            {
                BrokerSettingsConstants.AllocationGrowLoadRatioThreshold,
                BrokerSettingsConstants.AllocationShrinkLoadRatioThreshold,
                BrokerSettingsConstants.ClientIdleTimeout,
                BrokerSettingsConstants.SessionIdleTimeout,
                BrokerSettingsConstants.MessagesThrottleStartThreshold,
                BrokerSettingsConstants.MessagesThrottleStopThreshold,
                BrokerSettingsConstants.ClientConnectionTimeout,
                BrokerSettingsConstants.ServiceConfigMaxMessageSize,
                BrokerSettingsConstants.ServiceConfigOperationTimeout,
                BrokerSettingsConstants.DispatcherCapacityInGrowShrink
            };

            int?[] intNullableValues = new int?[]
            {
                startInfo.AllocationGrowLoadRatioThreshold,
                startInfo.AllocationShrinkLoadRatioThreshold,
                startInfo.ClientIdleTimeout,
                startInfo.SessionIdleTimeout,
                startInfo.MessagesThrottleStartThreshold,
                startInfo.MessagesThrottleStopThreshold,
                startInfo.ClientConnectionTimeout,
                startInfo.MaxMessageSize,
                startInfo.ServiceOperationTimeout,
                startInfo.DispatcherCapacityInGrowShrink
            };

            Debug.Assert(intNullableValues.Length == customPropNames.Length);

            for (int i = 0; i < customPropNames.Length; i++)
            {
                if (intNullableValues[i].HasValue)
                {
                    schedulerJob.SetCustomProperty(customPropNames[i], intNullableValues[i].Value.ToString());
                }
            }

            // add soa diag settings
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.SoaDiagTraceLevel, traceLevel);
            schedulerJob.SetCustomProperty(BrokerSettingsConstants.SoaDiagTraceCleanup, Boolean.FalseString);
        }

        /// <summary>
        /// Get value from a store property
        /// </summary>
        /// <param name="prop">indicate the store property</param>
        /// <param name="propId">the expected property Id</param>
        /// <param name="defaultValue">default value</param>
        /// <param name="callerName">caller name of this method, such as [JobMonitorEntry.GetMinAndMax]. it is only used in the log</param>
        /// <returns>return property value if it exists, otherwise return default value</returns>
        internal static object GetStorePropertyValue(StoreProperty prop, PropertyId propId, object defaultValue, string callerName)
        {
            if (prop.Id == propId && prop.Value != null)
            {
                return prop.Value;
            }
            else
            {
                TraceHelper.TraceEvent(
                    TraceEventType.Error,
                    "{0} Can't get valid value from store property {1}.", callerName, propId.Name);

                return defaultValue;
            }
        }


        /// <summary>
        /// Get customized properties of a specified job
        /// </summary>
        /// <param name="job">scheduler job</param>
        /// <param name="propNames">customized property names</param>
        /// <returns>dictionary of propname, propvalue</returns>
        internal static Dictionary<string, string> GetCustomizedProperties(ISchedulerJob job, params string[] propNames)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (NameValue pair in job.GetCustomProperties())
            {
                if (Array.Exists<string>(propNames,
                                         delegate(string name)
                                         {
                                             return pair.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
                                         }))
                {
                    dic.Add(pair.Name, pair.Value);
                }
            }

            return dic;
        }

        /// <summary>
        /// Get environment variables of a cluster
        /// </summary>
        /// <param name="scheduler">cluster scheduler</param>
        /// <returns>dictionary of env name, env value</returns>
        internal static Dictionary<string, string> GetEnvironmentVariables(IScheduler scheduler)
        {
            return ConvertNameValueCollection(scheduler.EnvironmentVariables);
        }

        /// <summary>
        /// Get cluster parameter
        /// </summary>
        /// <param name="scheduler">Connection to scheduler</param>
        /// <param name="name">Name of the param</param>
        /// <param name="defaultValue">Default value to use if param isnt set</param>
        /// <returns>Value of the param</returns>
        internal static string GetClusterParameterValue(IScheduler scheduler, string name, string defaultValue)
        {
            string ret = defaultValue;

            foreach (INameValue clusterParameter in scheduler.ClusterParameters)
            {
                if (0 == String.Compare(clusterParameter.Name, name, StringComparison.InvariantCultureIgnoreCase))
                {
                    ret = clusterParameter.Value;
                    break;
                }
            }

            return ret;
        }


        /// <summary>
        /// Get a specified env variable
        /// </summary>
        internal static string GetEnvironmentVariable(IScheduler scheduler, string envName)
        {
            string value = null;
            GetEnvironmentVariables(scheduler).TryGetValue(envName, out value);
            return value;
        }


        /// <summary>
        /// Get environment variables of a specified job
        /// </summary>
        /// <param name="job">scheduler job</param>
        /// <returns>dictionary of env name, env value</returns>
        internal static Dictionary<string, string> GetEnvironmentVariables(ISchedulerJob job)
        {
            return ConvertNameValueCollection(job.EnvironmentVariables);
        }


        /// <summary>
        /// Convert NameValue collection to dictionary
        /// </summary>
        /// <param name="collecion"></param>
        /// <returns></returns>
        internal static Dictionary<string, string> ConvertNameValueCollection(INameValueCollection collecion)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (NameValue pair in collecion)
            {
                dictionary.Add(pair.Name, pair.Value);
            }

            return dictionary;
        }


        /// <summary>
        /// Get service name of a specified job
        /// </summary>
        /// <param name="job">scheduler job</param>
        /// <returns>service name or null</returns>
        internal static string GetServiceName(ISchedulerJob job)
        {
            return (string)GetValue<string>(GetEnvironmentVariables(job), Constant.ServiceNameEnvVar);
        }

        /// <summary>
        /// Get value from dictionary
        /// </summary>
        /// <typeparam name="T">type of the value</typeparam>
        /// <param name="dictionary">soure dictionary</param>
        /// <param name="key">dictionary key</param>
        /// <returns>return null if key doesn't exist</returns>
        internal static object GetValue<T>(Dictionary<string, string> dictionary, string key)
        {
            string stringValue;
            if (dictionary.TryGetValue(key, out stringValue))
            {
                if (typeof(T) == typeof(string))
                {
                    return stringValue;
                }
                else if (typeof(T) == typeof(bool?))
                {
                    bool boolValue = false;
                    if (bool.TryParse(stringValue, out boolValue))
                    {
                        return new bool?(boolValue);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    throw new NotSupportedException(string.Format("The type {0} is not supported by method JobHelper.GetValue<T>.", typeof(T)));
                }
            }

            return null;
        }
    }
}
#endif