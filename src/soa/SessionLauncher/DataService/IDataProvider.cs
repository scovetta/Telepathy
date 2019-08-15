//------------------------------------------------------------------------------
// <copyright file="IDataProvider.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data provider interface definition
// </summary>
//------------------------------------------------------------------------------
#if HPCPACK

namespace Microsoft.Hpc.Scheduler.Session.Data.DataProvider
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    /// <summary>
    /// Data provider interface definition
    /// </summary>
    internal interface IDataProvider
    {
        /// <summary>
        /// Create a new data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <returns>information for accessing the data container</returns>
        DataClientInfo CreateDataContainer(string name);

        /// <summary>
        /// Open an existing data container
        /// </summary>
        /// <param name="name">name of the data container to be opened</param>
        /// <returns>information for accessing the data container</returns>
        DataClientInfo OpenDataContainer(string name);

        /// <summary>
        /// Delete a data container
        /// </summary>
        /// <param name="name">name of the data container to be deleted</param>
        void DeleteDataContainer(string name);

        /// <summary>
        /// Sets container attributes
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="attributes">attribute key and value pairs</param>
        /// <remarks>if attribute with the same key already exists, its value will be
        /// updated; otherwise, a new attribute is inserted. Valid characters for 
        /// attribute key and value are: 0~9, a~z</remarks>
        void SetDataContainerAttributes(string name, Dictionary<string, string> attributes);

        /// <summary>
        /// Gets container attributes
        /// </summary>
        /// <param name="name"> data container name</param>
        /// <returns>data container attribute key and value pairs</returns>
        Dictionary<string, string> GetDataContainerAttributes(string name);

        /// <summary>
        /// List all data containers
        /// </summary>
        /// <returns>List of all data containers</returns>
        IEnumerable<string> ListAllDataContainers();

        /// <summary>
        /// Set data container permissions
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userName">data container owner</param>
        /// <param name="allowedUsers">privileged users of the data container</param>
        void SetDataContainerPermissions(string name, string userName, string[] allowedUsers);

        /// <summary>
        /// Check if a user has specified permissions to a data container
        /// </summary>
        /// <param name="name">data container name</param>
        /// <param name="userIdentity">identity of the user to be checked</param>
        /// <param name="permissions">permissions to be checked</param>
        void CheckDataContainerPermissions(string name, WindowsIdentity userIdentity, DataPermissions permissions);
    }
}
#endif