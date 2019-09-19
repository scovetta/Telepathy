// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session.QueueAdapter.DTO
{
    using System;

    public class CloudQueueCmdDto
    {
        public CloudQueueCmdDto()
        {
        }

        public CloudQueueCmdDto(string cmdName, params object[] parameters) : this(Guid.NewGuid().ToString(), cmdName, parameters)
        {
        }

        public CloudQueueCmdDto(string requestId, string cmdName, params object[] parameters)
        {
            this.RequestId = requestId;
            this.CmdName = cmdName;
            this.Parameters = parameters;
            this.Version = 1;
        }

        public string RequestId { get; set; }

        public string CmdName { get; set; }

        public object[] Parameters { get; set; }

        public int Version { get; set; }
    }
}