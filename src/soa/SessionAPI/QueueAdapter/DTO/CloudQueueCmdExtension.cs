// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.QueueAdapter.DTO
{
    public static class CloudQueueCmdExtension
    {
        public static ParameterUnpacker GetUnpacker(this CloudQueueCmdDto dto)
        {
            return new ParameterUnpacker(dto.Parameters);
        }
    }
}
