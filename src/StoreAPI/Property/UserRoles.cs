//------------------------------------------------------------------------------
// <copyright file="AuthenticationUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Definition for public facing User Roles.
// </summary>
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;


namespace Microsoft.Hpc.Scheduler.Properties
{
    /// <summary>
    ///   <para />
    /// </summary>
    [ComVisible(true)]
    [Flags]
    [GuidAttribute(ComGuids.GuidUserRoles)]
    public enum UserRoles
    {
        /// <summary>
        ///   <para />
        /// </summary>
        AccessDenied = 0,
        /// <summary>
        ///   <para />
        /// </summary>
        User = 1,
        /// <summary>
        ///   <para />
        /// </summary>
        Administrator = 2,
        /// <summary>
        ///   <para />
        /// </summary>
        JobAdministrator = 4,
        /// <summary>
        ///   <para />
        /// </summary>
        JobOperator = 8
    }
}

