// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.Server
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Microsoft.Telepathy.Session.QueueAdapter.DTO;
    using Microsoft.Telepathy.Session.QueueAdapter.Interface;

    public abstract class CloudQueueWatcherBase
    {
        protected IQueueListener<CloudQueueCmdDto> QueueListener { get; set; }

        protected IQueueWriter<CloudQueueResponseDto> QueueWriter { get; set; }

        private readonly Dictionary<string, Func<CloudQueueCmdDto, Task>> cmdNameToDelegate = new Dictionary<string, Func<CloudQueueCmdDto, Task>>();

        protected CloudQueueWatcherBase()
        {
        }

        protected CloudQueueWatcherBase(IQueueListener<CloudQueueCmdDto> listener, IQueueWriter<CloudQueueResponseDto> writer)
        {
            this.QueueListener = listener;
            this.QueueWriter = writer;
            this.QueueListener.MessageReceivedCallback = this.InvokeInstanceMethodFromCmdObj;
        }

        protected async Task InvokeInstanceMethodFromCmdObj(CloudQueueCmdDto cmdObj)
        {
            if (cmdObj == null)
            {
                throw new ArgumentNullException(nameof(cmdObj));
            }

            if (string.IsNullOrEmpty(cmdObj.CmdName))
            {
                throw new InvalidOperationException($"{nameof(cmdObj.CmdName)} is null or empty string.");
            }

            if (this.cmdNameToDelegate.TryGetValue(cmdObj.CmdName, out var del))
            {
                await del(cmdObj);
            }
            else
            {
                throw new InvalidOperationException($"Unknown CmdName {cmdObj.CmdName}");
            }
        }

        protected void RegisterCmdDelegate(string cmdName, Func<CloudQueueCmdDto, Task> del)
        {
            this.cmdNameToDelegate[cmdName] = del;
        }

        protected Task CreateAndSendResponse(string requestId, string cmdName, object response)
        {
            var ans = new CloudQueueResponseDto(requestId, cmdName, response);
            return this.QueueWriter.WriteAsync(ans);
        }

        protected Task CreateAndSendResponse(CloudQueueCmdDto cmdObj, object response)
        {
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, response);
        }

        protected Task CreateAndSendEmptyResponse(CloudQueueCmdDto cmdObj)
        {
            return this.CreateAndSendResponse(cmdObj.RequestId, cmdObj.CmdName, string.Empty);
        }
    }
}