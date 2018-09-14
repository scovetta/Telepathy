//--------------------------------------------------------------------------
// <copyright file="AzureFeatures.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     This is a common module for Azure features.
// </summary>
//--------------------------------------------------------------------------

namespace Microsoft.Hpc.Azure.Common
{
    [System.Flags]
    internal enum AzureFeatures : int
    {
        None = 0x0,
        RemoteDesktopForwarder = 0x1,
        AzureConnect = 0x2,
        AzureVNet = 0x4,
    }
}
