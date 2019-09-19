// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using TelepathyCommon;

namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Net;
    using System.Security.Authentication;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading.Tasks;

    using Microsoft.Hpc.Scheduler.Session.Configuration;
    using Microsoft.Hpc.Scheduler.Session.Interface;

    /// <summary>
    /// Utility functions
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Returns whether SOAP message is broker's EOM message
        /// </summary>
        /// <param name="message">SOAP Message</param>
        /// <returns>Whether SOAP message is broker's EOM message</returns>
        internal static bool IsEOM(Message message)
        {
            return 0 == String.Compare(message.Headers.Action, Constant.EndOfMessageAction, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Logs error
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="args">Arguments into error message</param>
        internal static void LogError(string message, params object[] args)
        {
        }

        /// <summary>
        /// Throws ArgumentNullException if arg is null
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        internal static void ThrowIfNull(object arg, string name)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Throw ArgumentException is arg is string.Empty
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        internal static void ThrowIfEmpty(string arg, string name)
        {
            if (arg.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, SR.ArgumentEmptyString, name), name);
            }
        }

        /// <summary>
        /// Throws ArgumentNullException if arg is null or empty
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        internal static void ThrowIfNullOrEmpty(string arg, string name)
        {
            ThrowIfNullOrEmpty(arg, name, String.Format(CultureInfo.CurrentCulture, SR.ArgumentEmptyString, name));
        }

        /// <summary>
        /// Throws ArgumentNullException if arg is null or empty
        /// </summary>
        /// <param name="arg">Argument to check</param>
        /// <param name="name">Name of argument to check</param>
        /// <param name="message">Error message</param>
        internal static void ThrowIfNullOrEmpty(string arg, string name, string message)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }

            if (String.IsNullOrEmpty(arg))
            {
                throw new ArgumentException(message, name);
            }
        }

        /// <summary>
        /// Throws ArgumentException if specified len is too large
        /// </summary>
        /// <param name="len">Length to check</param>
        /// <param name="name">Name of argument</param>
        /// <param name="maxLen">Maxium length</param>
        /// <param name="message">Error message</param>
        /// <param name="args">Error message arguments</param>
        internal static void ThrowIfTooLong(int len, string name, int maxLen, string message, params object[] args)
        {
            if (len > maxLen)
            {
                throw new ArgumentException(String.Format(message, args), name);
            }
        }

        internal static void ThrowIfInvalidTimeout(int timeoutMilliseconds, string name)
        {
            ThrowIfInvalid(timeoutMilliseconds > 0 || timeoutMilliseconds == System.Threading.Timeout.Infinite, name);
        }

        internal static void ThrowIfInvalid(bool valid, string name)
        {
            ThrowIfInvalid(valid, name, String.Empty);
        }

        internal static void ThrowIfInvalid(bool valid, string name, string msg)
        {
            if (!valid)
            {
                throw new ArgumentException(msg, name);
            }
        }

        /// <summary>
        /// safe close a comunication object.
        /// </summary>
        /// <param name="communicateObj">the communication object.</param>
        internal static void SafeCloseCommunicateObject(ICommunicationObject communicateObj)
        {
            // -1 for timeout (2nd param) means use binding's CloseTimeout value
            SafeCloseCommunicateObject(communicateObj, -1);
        }

        /// <summary>
        /// safe close a comunication object.
        /// </summary>
        /// <param name="communicateObj">the communication object.</param>
        /// <param name="timeoutInMS">how long to wait for close. -1 means to use binding's CloseTimeout value</param>
        internal static void SafeCloseCommunicateObject(ICommunicationObject communicateObj, int timeoutInMS)
        {
            System.Diagnostics.Debug.Assert(communicateObj != null, "the communicateObj should not be null.");

            try
            {
                if (communicateObj.State == CommunicationState.Faulted)
                {
                    communicateObj.Abort();
                }
                else
                {
                    if (timeoutInMS >= 0)
                    {
                        communicateObj.Close(new TimeSpan(0, 0, 0, 0, timeoutInMS));
                    }
                    else
                    {
                        communicateObj.Close();
                    }
                }
            }
            catch (Exception e)
            {
                SessionBase.TraceSource.TraceInformation(e.ToString());
                communicateObj.Abort();
            }
        }

        /// <summary>
        /// Build a network credential by username and password
        /// </summary>
        /// <param name="username">indicating the username</param>
        /// <param name="password">indicating the password</param>
        /// <returns>returns the network credential</returns>
        internal static NetworkCredential BuildNetworkCredential(string username, string password)
        {
            string[] domainAndUserName = username.Split('\\');
            NetworkCredential credential = domainAndUserName.Length == 2 ?
                new NetworkCredential(domainAndUserName[1], password, domainAndUserName[0]) :
                new NetworkCredential(username, password);
            return credential;
        }

        /// <summary>
        /// Check if already hits the retry limit
        /// </summary>
        /// <param name="retry">retry count</param>
        /// <param name="askForCredential">ask for credential or not in current iteration</param>
        /// <param name="askForCredentialTimes">times of asking credential</param>
        /// <returns>can retry or not</returns>
        internal static bool CanRetry(int retry, bool askForCredential, int askForCredentialTimes)
        {
            return ((retry < SessionBase.MaxRetryCount) || (askForCredential && (askForCredentialTimes < SessionBase.MaxRetryCount)));
        }

        /// <summary>
        /// Handle instances of WebException, try to convert to SessionException and throw
        /// </summary>
        /// <param name="e">indicating the web exception</param>
        internal static void HandleWebException(WebException e)
        {
            Exception ex = WebAPIUtility.ConvertWebException(e);
            FaultException<SessionFault> fe = ex as FaultException<SessionFault>;
            if (fe != null)
            {
                throw Utility.TranslateFaultException(fe);
            }
            else
            {
                throw ex;
            }
        }

        internal static Exception TranslateFaultException(FaultException<SessionFault> innerException)
        {
            Exception exception = null;
            if (innerException != null)
            {
                string errorMessage = innerException.Reason.GetMatchingTranslation().Text;
                switch (innerException.Detail.Code)
                {
                    case (int)SOAFaultCode.NoAvailableBrokerNodes:
                        errorMessage = SR.NoBrokerNodeFound;
                        break;
                    case (int)SOAFaultCode.StorageSpaceNotSufficient:
                        errorMessage = SR.StorageSpaceNotSufficient;
                        break;
                    case (int)SOAFaultCode.OperationTimeout:
                        exception = new TimeoutException(innerException.Detail.Reason);
                        break;
                    case (int)SOAFaultCode.AccessDenied_Broker:
                    case (int)SOAFaultCode.AccessDenied_BrokerLauncher:
                    case (int)SOAFaultCode.AccessDenied_BrokerQueue:
                    case (int)SOAFaultCode.AuthenticationFailure:
                        exception = new AuthenticationException(GenerateErrorMessage(innerException.Detail, errorMessage), innerException);
                        break;
                    case (int)SOAFaultCode.ConfigFile_Invalid:
                        exception = new ConfigurationErrorsException(GenerateErrorMessage(innerException.Detail, errorMessage), innerException);
                        break;
                    default:
                        // If no such string is found, null is returned
                        errorMessage = GenerateErrorMessage(innerException.Detail, errorMessage);
                        break;
                }

                if (exception == null)
                {
                    exception = new SessionException(innerException.Detail.Code, errorMessage);
                }
            }

            return exception;
        }

        private static string GenerateErrorMessage(SessionFault sessionFault, string defaultMessage)
        {
            string message = SR.ResourceManager.GetString(SOAFaultCode.GetFaultCodeName(sessionFault.Code), CultureInfo.CurrentCulture);
            if (message != null)
            {
                if (sessionFault.Context == null || sessionFault.Context.Length == 0)
                {
                    return message;
                }
                else
                {
                    object[] objArr = new object[sessionFault.Context.Length];
                    for (int i = 0; i < objArr.Length; i++)
                    {
                        objArr[i] = sessionFault.Context[i];
                    }

                    return String.Format(CultureInfo.CurrentCulture, message, objArr);
                }
            }
            else
            {
                return defaultMessage;
            }
        }

        private const string SessionLauncherNetPipeAddress = "net.pipe://localhost/SessionLauncher";

        /// <summary>
        /// Get the endpoint Uri for session launcher
        /// </summary>
        /// <param name="headnode">the headnode name</param>
        /// <param name="binding">indicating the binding</param>
        /// <param name="useInternalChannel">If connect to Session Launcher via internal channel</param>
        /// <returns>the SessionLauncher EPR</returns>
        public static Uri GetSessionLauncher(string headnode, Binding binding, bool useInternalChannel)
        {
            if (LocalSession.LocalBroker)
            {
                return new Uri(SessionLauncherNetPipeAddress);
            }
            else
            {
                return new Uri((useInternalChannel || SoaHelper.IsCurrentUserLocal()) ? SoaHelper.GetSessionLauncherInternalAddress(headnode, binding) : SoaHelper.GetSessionLauncherAddress(headnode, binding));
            }
        }

        public static async Task<Uri> GetSessionLauncherAsync(SessionInitInfoBase info, Binding binding)
        {
            string headnode = await info.ResolveHeadnodeMachineAsync().ConfigureAwait(false);
            bool isAadUser = info.IsAadOrLocalUser;
            return GetSessionLauncher(headnode, binding, isAadUser);
        }

        /// <summary>
        /// Returns whether type is from Microsoft.Hpc.Scheduler.Session
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        // TODO: remove this
        public static bool IsHpcSessionType(Type t)
        {
            //return (t.Assembly == typeof(Session).Assembly);
            return true;
        }

        /// <summary>
        /// Build session info from data contract
        /// </summary>
        /// <param name="contract">indicating the session info data contract</param>
        /// <returns>returns session info instance</returns>
        public static SessionInfo BuildSessionInfoFromDataContract(SessionInfoContract contract)
        {
            SessionInfo info = new SessionInfo();
            info.BrokerEpr = contract.BrokerEpr;
            info.BrokerLauncherEpr = contract.BrokerLauncherEpr;
            info.ClientBrokerHeartbeatInterval = contract.ClientBrokerHeartbeatInterval;
            info.ClientBrokerHeartbeatRetryCount = contract.ClientBrokerHeartbeatRetryCount;
            info.ControllerEpr = contract.ControllerEpr;
            info.Durable = contract.Durable;
            info.Id = contract.Id;
            info.JobState = contract.JobState;
            info.ResponseEpr = contract.ResponseEpr;
            info.Secure = contract.Secure;
            info.ServerVersion = contract.ServerVersion;
            info.ServiceOperationTimeout = contract.ServiceOperationTimeout;
            info.ServiceVersion = contract.ServiceVersion;
            info.SessionACL = contract.SessionACL;
            info.SessionOwner = contract.SessionOwner;
            info.TransportScheme = contract.TransportScheme;
            info.UseInprocessBroker = contract.UseInprocessBroker;
            info.UseAad = contract.UseAad;
            return info;
        }

        /// <summary>
        /// Try get epr list from application configuration file
        /// returns null if no such configuration section found or enabled is set to false
        /// </summary>
        /// <returns>returns the epr list if debug mode is enabled</returns>
        public static string[] TryGetEprList()
        {
            ExeConfigurationFileMap map = new ExeConfigurationFileMap();
            map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            Configuration config = null;
            RetryManager.RetryOnceAsync(
                    () => config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None),
                    TimeSpan.FromSeconds(1),
                    ex => ex is ConfigurationErrorsException)
                .GetAwaiter()
                .GetResult();
            Debug.Assert(config != null, "Configuration is not opened properly.");
            SessionConfigurations sessionConfig = SessionConfigurations.GetSectionGroup(config);
            if (sessionConfig != null && sessionConfig.DebugMode != null)
            {
                if (sessionConfig.DebugMode.Enabled)
                {
                    List<string> eprList = new List<string>();
                    foreach (object element in sessionConfig.DebugMode.EprCollection)
                    {
                        EprElement epr = element as EprElement;
                        if (epr == null)
                        {
                            continue;
                        }

                        eprList.Add(epr.Epr);
                    }

                    return eprList.ToArray();
                }
            }

            return null;
        }
    }
}
