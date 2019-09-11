//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Broker helper class
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.ServiceBroker.Common
{
    using Microsoft.Hpc.Scheduler.Session.Internal;
    using Microsoft.Hpc.Scheduler.Session.Utility;
    using System;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml;

    /// <summary>
    /// Broker helper class
    /// </summary>
    public static class Utility
    {
        // TODO: Refactor so that session launcher doesn't rely on this utility class (in ServiceBroker)
#if Broker
        /// <summary>
        /// Stores the callback to close channel
        /// </summary>
        private static AsyncCallback callbackToCloseChannel = new ThreadHelper<IAsyncResult>(new AsyncCallback(CallbackToCloseChannel)).CallbackRoot;
#else
        /// <summary>
        /// Stores the callback to close channel
        /// </summary>
        private static AsyncCallback callbackToCloseChannel = new AsyncCallback(CallbackToCloseChannel);
#endif

        /// <summary>
        /// Copy message header
        /// </summary>
        /// <param name="headerName">indicating the name of the header</param>
        /// <param name="headerNamespace">indicating the namespace of the header</param>
        /// <param name="source">indicating the source message header collection</param>
        /// <param name="destination">indicating the destination message header collection</param>
        public static void CopyMessageHeader(string headerName, string headerNamespace, MessageHeaders source, MessageHeaders destination)
        {
            int udIndex = source.FindHeader(headerName, headerNamespace);
            if (udIndex >= 0)
            {
                if (source.MessageVersion == destination.MessageVersion)
                {
                    // If the message version of source and destination matches, directly copy it from the source
                    destination.CopyHeaderFrom(source, udIndex);
                }
                else
                {
                    // add request message user data to response message header as an array of XmlNode.
                    XmlNode[] content = source.GetHeader<XmlNode[]>(udIndex);
                    destination.Add(MessageHeader.CreateHeader(headerName, headerNamespace, content));
                }
            }
        }

        /// <summary>
        /// Convert a message via different versions
        /// </summary>
        /// <param name="message">message to convert</param>
        /// <param name="version">required message version</param>
        /// <returns>converted message</returns>
        public static Message ConvertMessage(Message message, MessageVersion version)
        {
            return CSharpUsageUtility.SafeCreateDisposableObject<Message>(
                () => Message.CreateMessage(version, message.Headers.Action, message.GetReaderAtBodyContents()),
                (converted) => {
                    int index = message.Headers.FindHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS);
                    if (index >= 0)
                    {
                        
                        XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index);
                        string headerValue = reader.ReadInnerXml();
                        MessageHeader header = MessageHeader.CreateHeader(Constant.UserDataHeaderName, Constant.HpcHeaderNS, headerValue);
                        
                        converted.Headers.Add(header);
                    }

                    //add messageId header

                    index = message.Headers.FindHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS);
                    if (index >= 0)
                    {

                        XmlDictionaryReader reader = message.Headers.GetReaderAtHeader(index);
                        string headerValue = reader.ReadInnerXml();
                        MessageHeader header = MessageHeader.CreateHeader(Constant.MessageIdHeaderName, Constant.HpcHeaderNS, headerValue);

                        converted.Headers.Add(header);
                    }

                    PrepareAddressingHeaders(message, converted);

                }
            );
        }

        /// <summary>
        /// Prepare addressing headers when converting messages
        /// </summary>
        /// <param name="source">indicating the source message</param>
        /// <param name="destination">indicating the destination message</param>
        public static void PrepareAddressingHeaders(Message source, Message destination)
        {
            if (destination.Headers.MessageVersion.Addressing != AddressingVersion.None)
            {
                if (source.Headers.MessageVersion.Addressing != AddressingVersion.None)
                {
                    destination.Headers.MessageId = source.Headers.MessageId;
                    destination.Headers.RelatesTo = source.Headers.RelatesTo;
                    destination.Headers.ReplyTo = source.Headers.ReplyTo;
                    destination.Headers.To = source.Headers.To;
                    destination.Headers.FaultTo = source.Headers.FaultTo;
                    destination.Headers.From = source.Headers.From;
                }
                else
                {
                    UniqueId messageId = SoaHelper.GetMessageId(source);
                    if (messageId != null)
                    {
                        destination.Headers.MessageId = messageId;
                    }
                }
            }


        }

        /// <summary>
        /// Helper class to async close ICommunicationObject
        /// </summary>
        /// <param name="obj">indicating the object</param>
        public static void AsyncCloseICommunicationObject(ICommunicationObject obj)
        {
            try
            {
                if (obj == null)
                {
                    return;
                }

                if (obj.State == CommunicationState.Faulted)
                {
                    obj.Abort();
                }
                else
                {
                    obj.BeginClose(callbackToCloseChannel, obj);
                }
            }
            catch (Exception)
            {
                obj.Abort();
            }
        }

        /// <summary>
        /// Helper method to get guid message id from message
        /// </summary>
        /// <param name="message">indicating the message</param>
        /// <returns>returns the guid message id</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared in multiple projects.")]
        public static Guid GetMessageIdFromMessage(Message message)
        {
            Guid guid = Guid.Empty;
            UniqueId id = SoaHelper.GetMessageId(message);
            if (id != null)
            {
                id.TryGetGuid(out guid);
            }
            return guid;
        }

        /// <summary>
        /// Helper method to get guid message id from message
        /// </summary>
        /// <param name="message">indicating the message</param>
        /// <returns>returns the guid message id</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Shared in multiple projects.")]
        public static Guid GetMessageIdFromResponse(Message message)
        {
            Guid guid = Guid.Empty;
            if (message.Headers.RelatesTo != null)
            {
                message.Headers.RelatesTo.TryGetGuid(out guid);
            }
            return guid;
        }

        /// <summary>
        /// Returns a value indicating whether the caller is headnode's machine account
        /// </summary>
        /// <param name="caller">indicating the caller</param>
        /// <returns>returns whether the caller is head node's machine account</returns>
        public static bool IsCallingFromHeadNode(WindowsIdentity caller, string headNode)
        {
            // if this is calling from local
            if (caller.IsSystem)
            {
                return true;
            }

            // Check if it is headnode's machine account
            string nodeName;
            if (TryExtractComputerName(caller, out nodeName))
            {
                return String.Equals(nodeName, headNode, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        /// <summary>
        /// Extracts a computer name from a computer account identity
        /// </summary>
        private static bool TryExtractComputerName(WindowsIdentity identity, out string computerNamne)
        {
            string[] parts = identity.Name.Split('\\');

            int lastIndex = parts.Length - 1;
            if (parts[lastIndex][parts[lastIndex].Length - 1] == '$')
            {
                computerNamne = parts[lastIndex].Substring(0, parts[lastIndex].Length - 1);
                return true;
            }
            else
            {
                computerNamne = null;
                return false;
            }
        }

        /// <summary>
        /// Callback to close ICommunicationObject
        /// </summary>
        /// <param name="result">indicating the async result</param>
        private static void CallbackToCloseChannel(IAsyncResult result)
        {
            ICommunicationObject obj = (ICommunicationObject)result.AsyncState;
            try
            {
                obj.EndClose(result);
            }
            catch (Exception)
            {
                // Swallow the exception because we don't care this
                obj.Abort();
            }
        }

        /// <summary>
        /// Sets the Azure client cert for specified channel
        /// </summary>
        /// <param name="clientCredentials">Channel's client credentials</param>
        internal static void SetAzureClientCertificate(ClientCredentials clientCredentials)
        {
            clientCredentials.ClientCertificate.SetCertificate(
                                StoreLocation.LocalMachine,
                                StoreName.My,
                                X509FindType.FindBySubjectDistinguishedName,
                                Constant.HpcAzureProxyClientCertName);

            clientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
        }

        /// <summary>
        /// Sets the Java WSS4J server client cert for specified channel
        /// </summary>
        /// <param name="clientCredentials">Channel's client credentials</param>
        internal static void SetWssClientCertificate(ClientCredentials clientCredentials)
        {
            clientCredentials.ClientCertificate.SetCertificate(
                                StoreLocation.LocalMachine,
                                StoreName.My,
                                X509FindType.FindBySubjectDistinguishedName,
                                Constant.HpcWssClientCertName);

            clientCredentials.ServiceCertificate.SetDefaultCertificate(
                                StoreLocation.LocalMachine,
                                StoreName.My,
                                X509FindType.FindBySubjectDistinguishedName,
                                Constant.HpcWssServiceCertName);

            clientCredentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;
            clientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
        }
    }

}
