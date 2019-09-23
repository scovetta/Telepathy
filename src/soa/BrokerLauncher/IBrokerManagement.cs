// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Internal.BrokerLauncher
{
    using System.ServiceModel;

    /// <summary>
    /// Interface for BrokerManagement service
    /// </summary>
    [ServiceContract(Name = "IBrokerManagement", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    internal interface IBrokerManagement
    {
        /// <summary>
        /// Takes broker offline
        /// </summary>
        /// <param name="forced">Force sessions to end</param>
        [OperationContract]
        void StartOffline(bool force);

        /// <summary>
        /// Is the broker offline?
        /// </summary>
        /// <returns>True if is offline; Otherwise false</returns>
        [OperationContract]
        bool IsOffline();

        /// <summary>
        /// Takes broker online
        /// </summary>
        [OperationContract]
        void Online();
    }
}
