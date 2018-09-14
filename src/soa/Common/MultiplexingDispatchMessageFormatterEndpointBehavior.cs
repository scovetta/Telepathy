//-----------------------------------------------------------------------
// <copyright file="MultiplexingDispatchMessageFormatterEndpointBehavior.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//    Provides an implementation of IEndpointBehavior to provide a 
//    multiplexing dispatch message formatter which supports both XML and
//    JSON messages.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Common
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Web;

    /// <summary>
    /// Provides an implementation of IEndpointBehavior to provide a multiplexing dispatch message 
    /// formatter which supports both XML and JSON messages
    /// </summary>
    /// <remarks>
    /// Only need to overried GetReplyDispatchFormatter as .Net 3.5 already support multiplexing on
    /// request messages.
    /// </remarks>
    internal class MultiplexingDispatchMessageFormatterEndpointBehavior : WebHttpBehavior
    {
        /// <summary>
        /// Override the GetReplyDispatchFormatter to return an instance of MultiplexingDispatchMessageFormatter
        /// which supports both XML and JSON messages
        /// </summary>
        /// <param name="operationDescription">indicating the operation description</param>
        /// <param name="endpoint">indicating the service endpoint</param>
        /// <returns>returns the message formatter</returns>
        protected override IDispatchMessageFormatter GetReplyDispatchFormatter(OperationDescription operationDescription, ServiceEndpoint endpoint)
        {
            var webGet = operationDescription.Behaviors.Find<WebGetAttribute>();
            var webInvoke = operationDescription.Behaviors.Find<WebInvokeAttribute>();
            if (webGet == null && webInvoke == null)
            {
                // The operation does not have a WebGet attribute and WebInvoke attribute.
                // This means this operation might not be used for REST service. So leave
                // it as it is and just return what the base class returns.
                return base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }

            IDispatchMessageFormatter xmlFormatter = null;
            IDispatchMessageFormatter jsonFormatter = null;
            WebMessageFormat defaultFormat;

            // Set the response format to XML to get the message formatter for xml messages,
            // then set the response format to Json to get the message formatter for json 
            // messages. Then, return an instance of the MultiplexingDispatchMessageFormatter
            // class which contains both message formatters as inner formatters.
            if (webGet != null)
            {
                defaultFormat = webGet.ResponseFormat;
                webGet.ResponseFormat = WebMessageFormat.Xml;
                xmlFormatter = base.GetReplyDispatchFormatter(operationDescription, endpoint);
                webGet.ResponseFormat = WebMessageFormat.Json;
                jsonFormatter = base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }
            else
            {
                // webInvoke!=null
                defaultFormat = webInvoke.ResponseFormat;
                webInvoke.ResponseFormat = WebMessageFormat.Xml;
                xmlFormatter = base.GetReplyDispatchFormatter(operationDescription, endpoint);
                webInvoke.ResponseFormat = WebMessageFormat.Json;
                jsonFormatter = base.GetReplyDispatchFormatter(operationDescription, endpoint);
            }

            return new MultiplexingDispatchMessageFormatter(jsonFormatter, xmlFormatter, defaultFormat);
        }

        /// <summary>
        /// Provides a multiplexing dispatch message formatter
        /// This formatter contains two inner message formatters to support both XML and JSON messages.
        /// This formatter chooses message formatter by the content of HTTP header "Content-Type" and 
        /// "Accpet".
        /// </summary>
        private class MultiplexingDispatchMessageFormatter : IDispatchMessageFormatter
        {
            /// <summary>
            /// Stores the Json content type
            /// </summary>
            private const string JsonContentType = "application/json";

            /// <summary>
            /// Stores the XML content type
            /// </summary>
            private const string XmlContentType = "application/xml";

            /// <summary>
            /// Stores the default message format
            /// </summary>
            private WebMessageFormat defaultFormat;

            /// <summary>
            /// Stores the json message formatter
            /// </summary>
            private IDispatchMessageFormatter jsonMessageFormatter;

            /// <summary>
            /// Stores the xml message formatter
            /// </summary>
            private IDispatchMessageFormatter xmlMessageFormatter;

            /// <summary>
            /// Initializes a new instance of the MultiplexingDispatchMessageFormatter class
            /// </summary>
            /// <param name="jsonMessageFormatter">indicating the json message formatter</param>
            /// <param name="xmlMessageFormatter">indicating the xml message formatter</param>
            /// <param name="defaultFormat">indicating the default format</param>
            public MultiplexingDispatchMessageFormatter(IDispatchMessageFormatter jsonMessageFormatter, IDispatchMessageFormatter xmlMessageFormatter, WebMessageFormat defaultFormat)
            {
                if (jsonMessageFormatter == null)
                {
                    throw new ArgumentNullException("jsonMessageFormatter");
                }

                if (xmlMessageFormatter == null)
                {
                    throw new ArgumentNullException("xmlMessageFormatter");
                }

                this.jsonMessageFormatter = jsonMessageFormatter;
                this.xmlMessageFormatter = xmlMessageFormatter;
                this.defaultFormat = defaultFormat;
            }

            /// <summary>
            /// Choose an inner formatter to deserialize the request message
            /// </summary>
            /// <param name="message">indicating the message</param>
            /// <param name="parameters">indicating the parameters</param>
            public void DeserializeRequest(Message message, object[] parameters)
            {
                this.GetFormatterByContentType(WebOperationContext.Current.IncomingRequest.ContentType).DeserializeRequest(message, parameters);
            }

            /// <summary>
            /// Choose an inner formatter to serialize the reply message
            /// </summary>
            /// <param name="messageVersion">indicating the message version</param>
            /// <param name="parameters">indicating the parameters</param>
            /// <param name="result">indicating the result</param>
            /// <returns>returns the serialized reply message</returns>
            public Message SerializeReply(MessageVersion messageVersion, object[] parameters, object result)
            {
                return this.GetFormatterByContentType(WebOperationContext.Current.OutgoingResponse.ContentType).SerializeReply(messageVersion, parameters, result);
            }

            /// <summary>
            /// Get the inner message formatter by the content type
            /// </summary>
            /// <param name="contentType">indicating the content type</param>
            /// <returns>returns the chosen message formatter</returns>
            private IDispatchMessageFormatter GetFormatterByContentType(string contentType)
            {
                string lowerContentType = contentType == null ? String.Empty : contentType.ToLowerInvariant();
                if (lowerContentType.Contains(JsonContentType))
                {
                    return this.jsonMessageFormatter;
                }
                else if (lowerContentType.Contains(XmlContentType))
                {
                    return this.xmlMessageFormatter;
                }

                switch (this.defaultFormat)
                {
                    case WebMessageFormat.Xml:
                        return this.xmlMessageFormatter;
                    default:    // case WebMessageFormat.Json
                        return this.jsonMessageFormatter;
                }
            }
        }
    }
}
