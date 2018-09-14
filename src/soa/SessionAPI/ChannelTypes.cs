//------------------------------------------------------------------------------
// <copyright file="Utility.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      SOA channel types
// </summary>
//------------------------------------------------------------------------------
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
