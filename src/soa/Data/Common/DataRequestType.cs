//------------------------------------------------------------------------------
// <copyright file="DataRequestType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      DataRequest types
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    /// <summary>
    /// Enumeration of supported DataRequest types
    /// </summary>
    public enum DataRequestType
    {
        /// <summary>
        /// Create a DataClient
        /// </summary>
        Create  = 1,

        /// <summary>
        /// Open a DataClient
        /// </summary>
        Open    = 2,

        /// <summary>
        /// Delete a DataClient
        /// </summary>
        Delete  = 3,
    }
}
