using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Hpc.Scheduler.Properties;

namespace Microsoft.Hpc.Scheduler.Store
{
    public class JobMessageObject : IClusterStoreObject
    {
        SchedulerStoreSvc _owner;
        int _id;
        int _parentJobId;
        
        internal JobMessageObject(SchedulerStoreSvc owner, int id, int parentJobId)
        {
            _owner = owner;
            _id = id;
            _parentJobId = parentJobId;
        }
            
        
        public int Id
        {
            get { return _id; }
        }

        public PropertyRow GetProps(params PropertyId[] propertyIds)
        {
            return _owner.GetPropsFromServer(ObjectType.JobMessage, _id, propertyIds);
        }

        public PropertyRow GetPropsByName(params string[] propertyNames)
        {
            return GetProps(PropertyLookup.JobMessage.PropertyIdsFromNames(propertyNames));
        }

        public PropertyRow GetAllProps()
        {
            return GetProps();
        }

        public void SetProps(params StoreProperty[] properties)
        {
            _owner.SetPropsOnServer(ObjectType.JobMessage, _id, properties);
        }

        public void PersistToXml(XmlWriter writer, XmlExportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RestoreFromXml(XmlReader reader, XmlImportOptions flags)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    /// <summary>
    /// Additional codes for job messages
    /// </summary>
    public static class JobMessageCode
    {
        #region Job warning messages
        
        public const int Unknown = 0;


        const int JobWarning_Start = 1000;

        public const int JobWarning_NodesExcluded = JobWarning_Start + 1;

        const int JobWarning_End = JobWarning_Start + 1000;
        
        #endregion

        
    }

    public static class JobMessageHelper
    {
        const string ErrorMessageResourcePrefix = "JobErrorMessage_";
        const string GenericMessageResourcePrefix = "GenericJobMessage_";
        const string HeaderResourcePrefix  = "JobMessageHeader_";
        const string HeaderResourceSuffixSingular = "_Singular";
        const string HeaderResourceSuffixPlural = "_Plural";

        static Dictionary<int, string> _errorCodeMessageMap;
        static Dictionary<int, string> _genericCodeMessageMap;
        static Dictionary<JobMessageType, string[]> _messageTypeHeaderMap;

        static Dictionary<int, string> BuildCodeMessageMap(Type container, string resourcePrefix)
        {
            Dictionary<int, string> map = new Dictionary<int, string>();
            foreach (FieldInfo field in container.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                object valueObj = field.GetValue(null);
                if (valueObj == null || !(valueObj is int))
                    continue;

                int value = (int)valueObj;

                if (!map.ContainsKey(value))
                {
                    string message = string.Empty;
                    try
                    {
                        message = SR.ResourceManager.GetString(resourcePrefix + field.Name, SR.Culture);
                    }
                    catch { }

                    if (message != null && !message.Trim().Equals(string.Empty))
                    {
                        map.Add(value, message);
                    }
                }
            }
            return map;

        }

        static JobMessageHelper()
        {
            _errorCodeMessageMap = BuildCodeMessageMap(typeof(ErrorCode), ErrorMessageResourcePrefix);
            _genericCodeMessageMap = BuildCodeMessageMap(typeof(JobMessageCode), GenericMessageResourcePrefix);

            // Enumerate through all error codes, but save only those found among StoreAPI resources
            // Enumerate through message types, populate the 
            _messageTypeHeaderMap = new Dictionary<JobMessageType, string[]>();
            foreach (JobMessageType messageType in Enum.GetValues(typeof(JobMessageType)))
            {
                string[] headerPrototypes = new string[2];

                try
                {
                    headerPrototypes[0] = SR.ResourceManager.GetString(
                        HeaderResourcePrefix + messageType.ToString() + HeaderResourceSuffixSingular, SR.Culture);
                }
                catch
                {
                    Debug.Assert(false, "Singular header prototype missing for message type " + messageType);
                    headerPrototypes[0] = string.Empty;
                }

                try
                {
                    headerPrototypes[1] = SR.ResourceManager.GetString(
                        HeaderResourcePrefix + messageType.ToString() + HeaderResourceSuffixPlural, SR.Culture);                
                }
                catch
                {
                    Debug.Assert(false, "Plural header prototype missing for message type " + messageType);
                    headerPrototypes[1] = string.Empty;
                }

                _messageTypeHeaderMap[messageType] = headerPrototypes;
            }
        }


        static string GetHeaderFormat(JobMessageType messageType, int count)
        {
            int index = (count == 1 ? 0 : 1);
            return _messageTypeHeaderMap[messageType][index];
        }

        static string GetTaskErrorText(int errorCode, string errorParams)
        { 
            // First, check our internal message table
            string message;
            if (_errorCodeMessageMap.TryGetValue(errorCode, out message))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    return message;
                }
            }

            // Now, see if we can get the error message from among the general error message list
            string defaultErrorMessage = ErrorCode.ToString(errorCode, errorParams);
            Debug.Assert(!string.IsNullOrEmpty(defaultErrorMessage), "Unable to format a message for task error code " + errorCode);
            return defaultErrorMessage;
        }

        // Copied, with a few modifications, from ErrorCode.cs
        static string GetGenericMessageText(int messageCode, string messageParams)
        {
            string messageTemplate;
            if (!_genericCodeMessageMap.TryGetValue(messageCode, out messageTemplate))
            {
                return string.Empty;
            }

            messageParams = messageParams ?? string.Empty;
            string[] arguments = messageParams.Split(new string[] { ErrorCode.ArgumentSeparator }, StringSplitOptions.None);
            
            try
            {
                return string.Format(SR.Culture, messageTemplate, arguments);
            }
            catch (FormatException)
            {
                return string.Empty;
            }
        }

        public static string GetMessageText(PropertyRow row)
        {
            // First, ensure that all the necessary arguments are there
            JobMessageType messageType = PropertyUtil.GetRequiredValueFromPropRow<JobMessageType>(row, JobMessagePropertyIds.MessageType);
            int messageCode = PropertyUtil.GetRequiredValueFromPropRow<int>(row, JobMessagePropertyIds.MessageCode);            

            int messageCount = PropertyUtil.GetRequiredValueFromPropRow<int>(row, JobMessagePropertyIds.MessageCount);
            int messageSubCode = PropertyUtil.GetValueFromPropRow<int>(row, JobMessagePropertyIds.MessageSubCode, 0);
            string extraText = PropertyUtil.GetValueFromPropRow<string>(row, JobMessagePropertyIds.ExtraText, string.Empty);

            string headerFormat = GetHeaderFormat(messageType, messageCount);
            if (messageCode == ErrorCode.Execution_TaskFinishedDuringExecution || messageCode == ErrorCode.Operation_FinishedByUser)
            {
                headerFormat = GetHeaderFormat(JobMessageType.JobWarning, messageCount);
            }

            Debug.Assert(headerFormat != null);

            switch (messageType)
            {
                case JobMessageType.JobPendingReason:
                    PendingReason.ReasonCode reasonCode = (PendingReason.ReasonCode)messageCode;
                    return String.Format(headerFormat, PendingReason.ToString(reasonCode));

                case JobMessageType.JobFailure:
                case JobMessageType.JobCancelation:
                    return String.Format(headerFormat, ErrorCode.ToString(messageCode, extraText));

                case JobMessageType.TaskNodeError:
                    string nodeName = extraText;
                    return String.Format(headerFormat, GetTaskErrorText(messageCode,""), nodeName, messageCount);

                case JobMessageType.TaskExecutionError:
                    int exitCode = messageSubCode;
                    return String.Format(headerFormat, exitCode, messageCount);

                case JobMessageType.TaskFailure:
                case JobMessageType.TaskCancelation:
                    return String.Format(headerFormat, GetTaskErrorText(messageCode,""), messageCount);

                case JobMessageType.TaskValidationError:
                    return String.Format(headerFormat, messageCount);

                case JobMessageType.JobWarning:
                    return String.Format(headerFormat, GetGenericMessageText(messageCode, extraText));

                default:
                    Debug.Assert(false, "Don't know how to handle message type " + messageType);
                    return string.Empty;
            }
        }
    }
}
