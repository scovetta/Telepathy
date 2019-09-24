// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
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
        bool AuthenticateUser(string sessionId, WindowsIdentity identity);
    }
}
