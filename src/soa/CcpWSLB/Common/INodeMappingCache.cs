// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.ServiceBroker.Common
{
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;

    /// <summary>
    /// Interface for retrieving Azure node mapping data
    /// </summary>
    [ServiceContract(Name = "INodeMappingCache", Namespace = "http://hpc.microsoft.com/brokerlauncher/")]
    internal interface INodeMappingCache
    {
        [OperationContract]
        Dictionary<string, string> GetNodeMapping(bool fromCache);
     
        [OperationContract(AsyncPattern = true)]
        IAsyncResult BeginGetNodeMapping(bool fromCache, AsyncCallback callback, object asyncState);

        Dictionary<string, string> EndGetNodeMapping(IAsyncResult result);
    }
}