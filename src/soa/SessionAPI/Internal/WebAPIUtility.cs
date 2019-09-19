// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.Xml;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Utility class for Web API related staff
    /// </summary>
    public static class WebAPIUtility
    {
        static DataContractSerializer FaultSerializer = new DataContractSerializer(typeof(HpcWebServiceFault));

        /// <summary>
        /// Try to convert an instance of WebException into an instance of FaultException[SessionFault] or ArgumentException
        /// </summary>
        /// <param name="exception">indicating the instance of WebException to be converted</param>
        /// <returns>
        /// returns the generated exception if convertion succeeded, returns the incoming WebException instance if convertion
        /// failed
        /// </returns>
        public static Exception ConvertWebException(WebException exception)
        {
            HttpWebResponse response = exception.Response as HttpWebResponse;

            if (response == null)
            {
                return exception;
            }

            using (response)
            {
                HpcWebServiceFault serviceFault = null;

                try
                {
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(response.GetResponseStream(), XmlDictionaryReaderQuotas.Max))
                    {
                        serviceFault = (HpcWebServiceFault)FaultSerializer.ReadObject(reader);
                    }
                }
                catch
                {
                    if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        // Forbidden error may occur when attempt to GetClusterName, the WebException doesn't contain HpcWebServiceFault.
                        // So convert that exception to SessionException
                        return new AuthenticationException(SR.WebAPI_AccessDenied);
                    }
                    else
                    {
                        // Swallow exception if the response stream is not a WCF message
                        // Just return the exception immediately
                        return exception;
                    }
                }

                try
                {
                    if (serviceFault.Code == SOAFaultCode.ArgumentError)
                    {
                        // This is ArgumentException thrown by the server, fetch error message and rebuild the exception
                        return new ArgumentException(serviceFault.Message);
                    }
                    else
                    {
                        string[] values;

                        if (serviceFault.Values != null)
                        {
                            values = new string[serviceFault.Values.Length];

                            int i = 0;

                            foreach (KeyValuePair<string, string> s in serviceFault.Values)
                            {
                                values[i++] = s.Value;
                            }
                        }
                        else
                        {
                            values = new string[0];
                        }

                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            return new AuthenticationException(string.Format(serviceFault.Message, values));
                        }
                        else
                        {
                            SessionFault sessionFault =
                                new SessionFault(serviceFault.Code, serviceFault.Message, values);

                            return new FaultException<SessionFault>(sessionFault, serviceFault.Message);
                        }
                    }
                }
                catch
                {
                    // If the detail is not an instance of SessionFault, just return the exception itself
                }
            }

            return exception;
        }

        // /// <summary>
        // /// Convert an instance of the SessionStartInfoContract to WebSessionStartInfo
        // /// </summary>
        // /// <param name="startInfo">indicating the session start info instance to be converted</param>
        // /// <returns>returns the session start info</returns>
        // public static WebSessionStartInfo ToWebSessionStartInfo(SessionStartInfoContract startInfo)
        // {
        //     WebSessionStartInfo result = new WebSessionStartInfo();
        //     result.AdminJobForHostInDiag = startInfo.AdminJobForHostInDiag;
        //     result.AllocationGrowLoadRatioThreshold = startInfo.AllocationGrowLoadRatioThreshold;
        //     result.AllocationShrinkLoadRatioThreshold = startInfo.AllocationShrinkLoadRatioThreshold;
        //     result.CanPreempt = startInfo.CanPreempt;
        //     result.ClientConnectionTimeout = startInfo.ClientConnectionTimeout;
        //     result.ClientIdleTimeout = startInfo.ClientIdleTimeout;
        //     result.DiagnosticBrokerNode = startInfo.DiagnosticBrokerNode;
        //     result.Environments = startInfo.Environments;
        //     result.ExtendedPriority = startInfo.ExtendedPriority;
        //     result.JobTemplate = startInfo.JobTemplate;
        //     result.MaxMessageSize = startInfo.MaxMessageSize;
        //     result.MaxUnits = startInfo.MaxUnits;
        //     result.MessagesThrottleStartThreshold = startInfo.MessagesThrottleStartThreshold;
        //     result.MessagesThrottleStopThreshold = startInfo.MessagesThrottleStopThreshold;
        //     result.MinUnits = startInfo.MinUnits;
        //     result.NodeGroupsStr = startInfo.NodeGroupsStr;
        //     result.Password = startInfo.Password;
        //     result.Priority = startInfo.Priority;
        //     result.RequestedNodesStr = startInfo.RequestedNodesStr;
        //     result.ResourceUnitType = startInfo.ResourceUnitType;
        //     result.Runtime = startInfo.Runtime;
        //     result.ServiceJobName = startInfo.ServiceJobName;
        //     result.ServiceJobProject = startInfo.ServiceJobProject;
        //     result.ServiceName = startInfo.ServiceName;
        //     result.ServiceOperationTimeout = startInfo.ServiceOperationTimeout;
        //     result.ServiceVersion = startInfo.ServiceVersion;
        //     result.SessionIdleTimeout = startInfo.SessionIdleTimeout;
        //     result.ShareSession = startInfo.ShareSession;
        //     result.Username = startInfo.Username;
        //     result.UseSessionPool = startInfo.UseSessionPool;
        //     return result;
        // }
    }

    [DataContract(Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
    [Serializable]
    class HpcWebServiceFault
    {
        public HpcWebServiceFault(int code, string message, params KeyValuePair<string, string>[] values)
        {
            Code = code;
            Message = message;
            Values = values;
        }

        /// <summary>
        /// Gets the fault code.
        /// </summary>
        [DataMember]
        public int Code
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the fault reason.
        /// </summary>
        [DataMember]
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the fault context.
        /// </summary>
        [DataMember]
        public KeyValuePair<string, string>[] Values
        {
            get;
            set;
        }
    }
}
