// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    /// <summary>
    /// Exit code for broker shim
    /// </summary>
    public enum BrokerShimExitCode : int
    {
        /// <summary>
        /// Exit code for force exit
        /// </summary>
        ForceExit = -1,

        /// <summary>
        /// Exit code for success exit
        /// </summary>
        Success = 0,

        /// <summary>
        /// Exit code for invalid argument
        /// </summary>
        InvalidArgument = 1,

        /// <summary>
        /// Exit code for failed to open service host
        /// </summary>
        FailedOpenServiceHost = 2,

        /// <summary>
        /// Exit code for initialize wait handle does not exist
        /// </summary>
        InitializeWaitHandleNotExist = 3,

        /// <summary>
        /// Exit code for failed to set initialize wait handle
        /// </summary>
        FailedToSetInitializeWaitHandle = 4,
    }
}
