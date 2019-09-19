// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    /// Indicate the authentication type a channel uses
    /// </summary>
    public enum ChannelTypes
    {
        LocalAD,
        AzureAD,
        Certificate
    }
}
