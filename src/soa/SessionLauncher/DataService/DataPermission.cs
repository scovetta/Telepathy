//------------------------------------------------------------------------------
// <copyright file="DataPermission.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data permissions
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;

    /// <summary>
    /// Enumeration of supported data permissions
    /// </summary>
    [Flags]
    public enum DataPermissions
    {
        /// <summary>
        /// No permission
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Read a DataClient
        /// </summary>
        Read = 0x01,

        /// <summary>
        /// Write to a DataClient
        /// </summary>
        Write = 0x02,

        /// <summary>
        /// Set attribute of a DataClient
        /// </summary>
        SetAttribute = 0x04,

        /// <summary>
        /// Delete a DataClient
        /// </summary>
        Delete  = 0x08,
    }
}
