//------------------------------------------------------------------------------
// <copyright file="ISessionUserAuthenticator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides an interface to authenticate session user
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session
{
    using System.Security.Principal;

    /// <summary>
    /// Provides an interface to authenticate session user
    /// </summary>
    public interface ISessionUserAuthenticator
    {
        /// <summary>
        /// Authenticate the incoming user
        /// </summary>
        /// <param name="sessionId">indicating the session id</param>
        /// <param name="identity">indicating the windows identity</param>
        /// <returns>
        /// returns a flag indicating whether the authentication succeeded
        /// </returns>
        bool AuthenticateUser(int sessionId, WindowsIdentity identity);
    }
}
