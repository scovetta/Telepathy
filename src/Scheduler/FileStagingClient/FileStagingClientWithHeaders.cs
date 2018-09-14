//--------------------------------------------------------------------------
// <copyright file="FileStagingClientWithHeaders.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     Provides structure on top of a contained FileStagingClient object
//     that writes headers to the client before it is made available by
//     a public property. This is useful for the implementation of the API.
// </summary>
//--------------------------------------------------------------------------
namespace Microsoft.Hpc.Azure.FileStaging
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides structure on top of a contained FileStagingClient object
    /// that writes headers to the client before it is made available by
    /// a public property. This is useful for the implementation of the API.
    /// </summary>
    internal class FileStagingClientWithHeaders : IDisposable
    {
        /// <summary>
        /// This class uses a FileStagingClient for communication
        /// </summary>
        private FileStagingClient internalClient;

        /// <summary>
        /// A scope needs to be defined so that we can write headers
        /// </summary>
        private OperationContextScope scope;

        public static FileStagingClientWithHeaders CreateInstance(string connectionString, string targetNode, string path)
        {
            return CreateInstance(connectionString, targetNode, path, CancellationToken.None);
        }

        public static FileStagingClientWithHeaders CreateInstance(string connectionString, string targetNode, string path, CancellationToken token)
        {
            FileStagingClientWithHeaders instance = null;

            try
            {
                EndpointsConnectionString endpoints;
                if (!EndpointsConnectionString.TryParseConnectionString(connectionString, out endpoints))
                {
                    Trace.TraceWarning($"The connectionString {connectionString} cannot be parsed, loading from environment variables or windows registry.");

                    endpoints = EndpointsConnectionString.LoadFromEnvVarsOrWindowsRegistry();
                }

                var context = HpcContext.GetOrAdd(endpoints, token);

                instance = new FileStagingClientWithHeaders();
                instance.Initialize(context, targetNode, path, true, string.IsNullOrWhiteSpace(connectionString));

                return instance;
            }
            catch (Exception ex)
            {
                instance?.Dispose();
                Trace.TraceError("Failed to create FileStagingClientWithHeaders instance, ex {0}.", ex);
                throw;
            }
        }


        /// <summary>
        /// Initializes a new instance of the FileStagingClientWithHeaders class.
        /// This instance is used to make operation calls to a specific head node
        /// </summary>
        /// <param name="headNode">head node name</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        public FileStagingClientWithHeaders()
        {
        }

        /// <summary>
        /// Gets a handle to the client used to send messages
        /// </summary>
        public FileStagingClient Client
        {
            get { return this.internalClient; }
        }

        /// <summary>
        /// Adds headers needed to open a file to the current message context
        /// </summary>
        /// <param name="mode">file mode that specifies how the file is opened</param>
        /// <param name="position">position in the file where opeation is performed</param>
        /// <param name="backward">a flag indicating whether position is counted backwards from end of file or not.
        /// true if position is counted backwards from end of file, false otherwise</param>
        /// <param name="lines">a flag indicating whether "position" is number of bytes or nr number of lines.
        /// true if "position" is number of lines, false if "position" is number of bytes</param>
        /// <param name="encoding">encoding used to read source file</param>
        public void AddFileTransferHeaders(FileMode mode, long position, bool backward, bool lines, string encoding)
        {
            MessageHeader<FileMode> fileModeHeader = new MessageHeader<FileMode>(mode);
            MessageHeader untypedFileModeHeader = fileModeHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderMode, FileStagingCommon.WcfHeaderNamespace);
            OperationContext.Current.OutgoingMessageHeaders.Add(untypedFileModeHeader);

            MessageHeader<long> positionHeader = new MessageHeader<long>(position);
            MessageHeader untypedPositionHeader = positionHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderPosition, FileStagingCommon.WcfHeaderNamespace);
            OperationContext.Current.OutgoingMessageHeaders.Add(untypedPositionHeader);

            if (backward)
            {
                MessageHeader<bool> backwardHeader = new MessageHeader<bool>(backward);
                MessageHeader untypedBackwardHeader = backwardHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderBackward, FileStagingCommon.WcfHeaderNamespace);
                OperationContext.Current.OutgoingMessageHeaders.Add(untypedBackwardHeader);
            }

            if (lines)
            {
                MessageHeader<bool> linesHeader = new MessageHeader<bool>(lines);
                MessageHeader untypedLinesHeader = linesHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderLines, FileStagingCommon.WcfHeaderNamespace);
                OperationContext.Current.OutgoingMessageHeaders.Add(untypedLinesHeader);
            }

            if (!string.IsNullOrEmpty(encoding))
            {
                MessageHeader<string> linesHeader = new MessageHeader<string>(encoding);
                MessageHeader untypedEncodingHeader = linesHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderEncoding, FileStagingCommon.WcfHeaderNamespace);
                OperationContext.Current.OutgoingMessageHeaders.Add(untypedEncodingHeader);
            }
        }

        /// <summary>
        /// Disposes the client by disposing the scope and closing the connection
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.scope?.Dispose();
                this.scope = null;
            }
            catch (Exception ex)
            {
                // Ignore exceptions thrown by disposing the scope; it wouldn't really affect the behavior of the API but
                // it might indicate a larger failure somewhere else
                Trace.TraceWarning("Unable to dispose of operation context scope. {0}", ex);
            }

            this.internalClient?.Close();
            this.internalClient = null;
        }

        /// <summary>
        /// Initializes data members of the FileStagingClientWithHeaders instance
        /// </summary>
        /// <param name="context">the hpc context</param>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        /// <param name="secure">if the communication should be secured</param>
        /// <param name="connectToLocal">true if the connection should be to local node</param>
        private void Initialize(IHpcContext context, string targetNode, string path, bool secure, bool connectToLocal)
        {
            NetTcpBinding binding = secure ? FileStagingCommon.GetSecureFileStagingBinding() : FileStagingCommon.GetFileStagingBinding();
            EndpointAddress endpoint = null;

            if (connectToLocal)
            {
                endpoint = FileStagingCommon.GetFileStagingEndpoint();
            }
            else
            {
                var headNode = context.ResolveSchedulerNodeAsync().GetAwaiter().GetResult();
                endpoint = FileStagingCommon.GetFileStagingEndpointOnHeadNode(headNode);
            }

            this.internalClient = new FileStagingClient(binding, endpoint);

            // Enter a new scope for the outgoing message, and write the required headers.
            this.SetupScopeAndHeaders(targetNode, path);
        }

        /// <summary>
        /// Initializes a scope for setting message headers, then sets the default headers
        /// </summary>
        /// <param name="targetNode">target node name</param>
        /// <param name="path">file path on target node</param>
        private void SetupScopeAndHeaders(string targetNode, string path)
        {
            this.scope = new OperationContextScope(this.internalClient.InnerChannel);

            MessageHeader<string> targetNodeHeader = new MessageHeader<string>(targetNode);
            MessageHeader untypedTargetNodeHeader = targetNodeHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderTargetNode, FileStagingCommon.WcfHeaderNamespace);
            OperationContext.Current.OutgoingMessageHeaders.Add(untypedTargetNodeHeader);

            MessageHeader<string> namespaceHeader = new MessageHeader<string>(path);
            MessageHeader untypedNamespaceHeader = namespaceHeader.GetUntypedHeader(FileStagingCommon.WcfHeaderPath, FileStagingCommon.WcfHeaderNamespace);
            OperationContext.Current.OutgoingMessageHeaders.Add(untypedNamespaceHeader);
        }
    }
}
