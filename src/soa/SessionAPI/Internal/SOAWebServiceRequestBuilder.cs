//------------------------------------------------------------------------------
// <copyright file="SOAWebServiceRequestBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//       Builder class to generate SOA web service web request
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Runtime.Serialization;
    using System.ServiceModel.Web;

    /// <summary>
    /// Builder class to generate SOA web service web request
    /// </summary>
    public static class SOAWebServiceRequestBuilder
    {
        /// <summary>
        /// Stores the dictionary contains ContentTypes for different message formats
        /// </summary>
        private static readonly Dictionary<WebMessageFormat, string> ContentTypes = new Dictionary<WebMessageFormat, string>(2)
        {
            {WebMessageFormat.Json,"application/json"},
            {WebMessageFormat.Xml,"application/xml"}
        };

        /// <summary>
        /// Stores the dictionary contains Accepts for different message formats
        /// </summary>
        private static readonly Dictionary<WebMessageFormat, string> Accepts = new Dictionary<WebMessageFormat, string>(2)
        {
            {WebMessageFormat.Json,"application/json"},
            {WebMessageFormat.Xml,"application/xml"}
        };

        /// <summary>
        /// Stores the default timeout
        /// </summary>
        private const int DefaultTimeout = int.MaxValue;

        /// <summary>
        /// TCP KeepAlive interval in milliseconds
        /// </summary>
        private const int KeepAliveInterval = 25000;

        /// <summary>
        /// Stores the content type of the http web request to send request
        /// </summary>
        private const string SendRequestWebRequestContentType = "application/octet-stream";

        /// <summary>
        /// Stores the Uri format of the http web request to close session
        /// </summary>
        private const string CloseSessionWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/Close";

        /// <summary>
        /// Stores the Uri format of the http web request to get service versions
        /// </summary>
        private const string GetServiceVersionsWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/SoaService/{2}/Versions";

        /// <summary>
        /// Stores the Uri format of the http web request to send request
        /// </summary>
        private const string SendRequestWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/batch/{3}?commit={4}";

        /// <summary>
        /// Stores the Uri format of the http web request to get status of a batch
        /// </summary>
        private const string GetBatchStatusWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/batch/{3}/Status";

        /// <summary>
        /// Stores the Uri format of the http web request to commit a batch
        /// </summary>
        private const string CommitBatchWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/batch/{3}/Commit";

        /// <summary>
        /// Stores the Uri format of the http web request to purge a batch
        /// </summary>
        private const string PurgeBatchWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/batch/{3}/Purge";

        /// <summary>
        /// Stores the uri template for create session
        /// </summary>
        private const string CreateSessionUriTemplate = "https://{0}/WindowsHPC/{1}/sessions/Create?durable={2}";

        /// <summary>
        /// Stores the Uri format of the http web request to send request
        /// </summary>
        private const string GetResponseWebRequestUriFormat = "https://{0}/WindowsHPC/{1}/session/{2}/batch/{3}/Response?action={4}&clientdata={5}&count={6}&reset={7}";

        /// <summary>
        /// Stores the Uri format of the http web request to attach session
        /// </summary>
        private const string AttachSessionUriTemplate = "https://{0}/WindowsHPC/{1}/session/{2}/Attach?durable={3}";

        /// <summary>
        /// Stores the Uri format of the http web request to get cluster name list
        /// </summary>
        private const string GetClusterUriTemplate = "https://{0}/WindowsHPC/Clusters";

        /// <summary>
        /// Stores the method of the http web request to close session
        /// </summary>
        private const string CloseSessionWebRequestMethod = "POST";

        /// <summary>
        /// Stores the method of the http web request to close session
        /// </summary>
        private const string AttachSessionWebRequestMethod = "GET";

        /// <summary>
        /// Stores the method of the http web request to get service versions
        /// </summary>
        private const string GetServiceVersionsWebRequestMethod = "GET";

        /// <summary>
        /// Stores the method of the http web request to send request
        /// </summary>
        private const string SendRequestWebRequestMethod = "POST";

        /// <summary>
        /// Stores the method of the http web request to create session
        /// </summary>
        private const string CreateSessionWebRequestMethod = "POST";

        /// <summary>
        /// Stores the method of the http web request to get response
        /// </summary>
        private const string GetResponseWebRequestMethod = "POST";

        /// <summary>
        /// Stores the method of the http web request to get status of a batch
        /// </summary>
        private const string GetBatchStatusWebRequestMethod = "GET";

        /// <summary>
        /// Stores the method of the http web request to commit a batch
        /// </summary>
        private const string CommitBatchWebRequestMethod = "POST";

        /// <summary>
        /// Stores the method of the http web request to purge a batch
        /// </summary>
        private const string PurgeBatchWebRequestMethod = "POST";

        /// <summary>
        /// stores the method of the http web request to get cluster name
        /// </summary>
        private const string GetClusterWebRequestMethod = "GET";

        /// <summary>
        /// Stores the ConnectionGroupName template of the http web request to get response
        /// </summary>
        private const string GetResponseConnectionGroupNameTemplate = "GetResponse_{0}";

        /// <summary>
        /// Use a unique name for the connection to SOA REST service
        /// </summary>
        private static readonly string SOAConnectionGroupName = Guid.NewGuid().ToString();

        /// <summary>
        /// Store the mapping table between Endpoint <-> cluster name
        /// </summary>
        private static Dictionary<string, string> clusterMapping = new Dictionary<string, string>();

        /// <summary>
        /// Generate the http web request to close session
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to close session</returns>
        public static HttpWebRequest GenerateCloseSessionWebRequest(string headnode, string sessionId, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(CloseSessionWebRequestUriFormat, headnode,
                GetClusterName(headnode, credential), sessionId));
            request.Method = CloseSessionWebRequestMethod;
            request.Credentials = credential;
            request.PreAuthenticate = true;
            request.ContentLength = 0;
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to get service versions
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="serviceName">indicating the service name</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to close session</returns>
        public static HttpWebRequest GenerateGetServiceVersionsWebRequest(string headnode, string serviceName, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(GetServiceVersionsWebRequestUriFormat, headnode,
                GetClusterName(headnode, credential), serviceName));
            request.Method = GetServiceVersionsWebRequestMethod;
            request.Credentials = credential;
            request.PreAuthenticate = true;
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to close session
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="durable">indicating whether to create a durable session</param>
        /// <param name="credential">indicating the credential</param>
        /// <param name="messageFormat">indicating the message format</param>
        /// <returns>returns the http web request to create session</returns>
        public static HttpWebRequest GenerateCreateSessionWebRequest(string headnode, bool durable, NetworkCredential credential, WebMessageFormat messageFormat)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(CreateSessionUriTemplate, headnode,
                GetClusterName(headnode, credential), durable));
            request.Credentials = credential;
            request.PreAuthenticate = true;
            request.Method = CreateSessionWebRequestMethod;
            request.ContentType = ContentTypes[messageFormat];
            request.Accept = ContentTypes[messageFormat];
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to send request
        /// </summary>
        /// <param name="brokerNode">indicating the broker node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="commit">indicating whether to commit the batch</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to send request</returns>
        public static HttpWebRequest GenerateSendRequestWebRequest(string brokerNode, int sessionId, string clientId, bool commit, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(SendRequestWebRequestUriFormat, brokerNode,
                GetClusterName(brokerNode, credential), sessionId, clientId, commit));
            request.PreAuthenticate = true;
            request.Method = SendRequestWebRequestMethod;
            request.Credentials = credential;
            request.ContentType = SendRequestWebRequestContentType;
            request.SendChunked = true;
            request.Timeout = DefaultTimeout;

            // Bug 16895: Do not override the default value (300 seconds)
            // of the ReadWriteTimeout property so that it would timeout
            // when reading message from the stream if connection was lost
            // Ref: http://msdn.microsoft.com/query/dev10.query?appId=Dev10IDEF1&l=EN-US&k=k(SYSTEM.NET.HTTPWEBREQUEST.READWRITETIMEOUT);k(TargetFrameworkMoniker-%22.NETFRAMEWORK%2cVERSION%3dV3.5%22);k(DevLang-CSHARP)&rd=true
            ////request.ReadWriteTimeout = DefaultTimeout;

            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to get status of a batch
        /// </summary>
        /// <param name="brokerNode">indicating the broker node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="credential">indicating the credential</param>
        /// <param name="format">indicating the web message format</param>
        /// <returns>returns the http web request to get status of a batch</returns>
        public static HttpWebRequest GenerateGetBatchStatusWebRequest(string brokerNode, int sessionId, string clientId, NetworkCredential credential, WebMessageFormat format)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(GetBatchStatusWebRequestUriFormat, brokerNode,
                GetClusterName(brokerNode, credential), sessionId, clientId));
            request.PreAuthenticate = true;
            request.Method = GetBatchStatusWebRequestMethod;
            request.Credentials = credential;
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Accept = Accepts[format];
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to commit a batch
        /// </summary>
        /// <param name="brokerNode">indicating the broker node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to commit a batch</returns>
        public static HttpWebRequest GenerateCommitBatchWebRequest(string brokerNode, int sessionId, string clientId, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(CommitBatchWebRequestUriFormat, brokerNode,
                GetClusterName(brokerNode, credential), sessionId, clientId));
            request.PreAuthenticate = true;
            request.Method = CommitBatchWebRequestMethod;
            request.Credentials = credential;
            request.Timeout = DefaultTimeout;
            request.ContentLength = 0;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to purge a batch
        /// </summary>
        /// <param name="brokerNode">indicating the broker node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to purge a batch</returns>
        public static HttpWebRequest GeneratePurgeBatchWebRequest(string brokerNode, int sessionId, string clientId, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(PurgeBatchWebRequestUriFormat, brokerNode,
                GetClusterName(brokerNode, credential), sessionId, clientId));
            request.PreAuthenticate = true;
            request.Method = PurgeBatchWebRequestMethod;
            request.Credentials = credential;
            request.Timeout = DefaultTimeout;
            request.ContentLength = 0;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to get response
        /// </summary>
        /// <param name="brokerNode">indicating the broker node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="clientId">indicating the client id</param>
        /// <param name="credential">indicating the credential</param>
        /// <param name="action">indicating the action</param>
        /// <param name="clientData">indicating the client data</param>
        /// <param name="count">indicating the count</param>
        /// <param name="startFromBegining">indicating a flag indicating whether client wants to start from begining</param>
        /// <returns>returns the http web request to get response</returns>
        public static HttpWebRequest GenerateGetResponseWebRequest(string brokerNode, int sessionId, string clientId, NetworkCredential credential, string action, string clientData, int count, bool startFromBegining)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(GetResponseWebRequestUriFormat, brokerNode,
                GetClusterName(brokerNode, credential), sessionId, clientId, Uri.EscapeDataString(action ?? String.Empty), Uri.EscapeDataString(clientData), count, startFromBegining));
            request.Method = GetResponseWebRequestMethod;
            request.PreAuthenticate = true;
            request.Credentials = credential;
            request.Timeout = DefaultTimeout;
            request.ContentLength = 0;

            // Bug 16895: Do not override the default value (300 seconds)
            // of the ReadWriteTimeout property so that it would timeout
            // when reading message from the stream if connection was lost
            // Ref: http://msdn.microsoft.com/query/dev10.query?appId=Dev10IDEF1&l=EN-US&k=k(SYSTEM.NET.HTTPWEBREQUEST.READWRITETIMEOUT);k(TargetFrameworkMoniker-%22.NETFRAMEWORK%2cVERSION%3dV3.5%22);k(DevLang-CSHARP)&rd=true
            ////request.ReadWriteTimeout = DefaultTimeout;

            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = String.Format(GetResponseConnectionGroupNameTemplate, clientData);
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        /// <summary>
        /// Generate the http web request to attach session
        /// </summary>
        /// <param name="headnode">indicating the head node</param>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="durable">indicating whether to attach to a durable session</param>
        /// <param name="credential">indicating the credential</param>
        /// <returns>returns the http web request to attach session</returns>
        public static HttpWebRequest GenerateAttachSessionWebRequest(string headnode, int sessionId, bool durable, NetworkCredential credential)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(AttachSessionUriTemplate, headnode,
                GetClusterName(headnode, credential), sessionId, durable));
            request.Method = AttachSessionWebRequestMethod;
            request.PreAuthenticate = true;
            request.Credentials = credential;
            request.Timeout = DefaultTimeout;
            request.ReadWriteTimeout = DefaultTimeout;
            request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
            request.ConnectionGroupName = SOAConnectionGroupName;
            request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);
            return request;
        }

        private static string GetClusterName(string headnode, NetworkCredential credential)
        {
            if (!clusterMapping.ContainsKey(headnode))
            {
                // request the service to get the cluster
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(String.Format(GetClusterUriTemplate, headnode));
                request.Credentials = credential;
                request.PreAuthenticate = true;
                request.Method = GetClusterWebRequestMethod;
                request.ContentType = ContentTypes[WebMessageFormat.Xml];
                request.Accept = ContentTypes[WebMessageFormat.Xml];
                request.Timeout = DefaultTimeout;
                request.ReadWriteTimeout = DefaultTimeout;
                request.Headers.Add(VersionConstants.APIVersionHeaderName, VersionConstants.V3SP3AzureGroupLabel);
                request.ConnectionGroupName = SOAConnectionGroupName;
                request.ServicePoint.SetTcpKeepAlive(true, KeepAliveInterval, KeepAliveInterval);

                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        var serializer = new DataContractSerializer(typeof(RestRow[]));
                        RestRow[] rows = (RestRow[])serializer.ReadObject(stream);
                        lock (clusterMapping)
                        {
                            clusterMapping[headnode] = rows[0].Props[0].Value;
                        }
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (HttpWebResponse)e.Response;
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            // we have logic to retry the credential if it is invalid.
                            // the caller of this method catches exception and closes the response.
                            throw;
                        }
                        else
                        {
                            response.Close();
                        }
                    }
#if API
                    throw new SessionException(SOAFaultCode.ConnectToSchedulerFailure, "Fail to get the cluster name");
#else
                    throw new Exception("Fail to get the cluster name");
#endif
                }
                catch
                {
#if API
                    throw new SessionException(SOAFaultCode.ConnectToSchedulerFailure, "Fail to get the cluster name");
#else
                    throw new Exception("Fail to get the cluster name");
#endif
                }
            }

            return clusterMapping[headnode];
        }

        //TODO: refactor following private class and enum, should link them from scheduer side.
        //This class is public in source file: "private\Scheduler\WebService\HpcWebServiceCommon\HpcWebVersioning.cs", so can't link that file.
        //Session.dll can't refer the assembly: "Microsoft.Hpc.WebService.Common.dll", which is not strong named.
        private class VersionConstants
        {
            public static string APIVersionHeaderName = "api-version";
            public static string V3SP3AzureGroupLabel = "2011-11-01";
        }

        [Serializable]
        [DataContract(Name = "Property", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
        private class RestStoreProp
        {
            [DataMember()]
            public string Name;

            [DataMember()]
            public string Value;

            public RestStoreProp(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public RestStoreProp()
            {
            }
        }

        [Serializable]
        [DataContract(Name = "Object", Namespace = "http://schemas.microsoft.com/HPCS2008R2/common")]
        private class RestRow
        {
            [DataMember(Name = "Properties")]
            public RestStoreProp[] Props;

            public RestRow(RestStoreProp[] props)
            {
                Props = props;
            }

            public RestRow()
            {
            }
        }

    }
}
