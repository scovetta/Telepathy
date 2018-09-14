//------------------------------------------------------------------------------
// <copyright file="DataServerInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data server info definition
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data server info  definition
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/session/data")]
    [Serializable]
    public class DataServerInfo
    {
        [DataMember]
        public string AddressInfo;

        /// <summary>
        /// Create a new DataServerInfo instance
        /// </summary>
        public DataServerInfo(string strDataServerInfo)
        {
            this.AddressInfo = strDataServerInfo;
        }
    }
}
