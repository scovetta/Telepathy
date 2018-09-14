//------------------------------------------------------------------------------
// <copyright file="DataClientInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Data client info definition
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Data
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Data client info  definition
    /// </summary>
    [DataContract(Namespace = "http://hpc.microsoft.com/session/data")]
    [Serializable]
    public class DataClientInfo
    {
        /// <summary>
        /// Primary data path
        /// </summary>
        [DataMember]
        public string PrimaryDataPath
        {
            get;
            set;
        }

        /// <summary>
        /// Secondary data path
        /// </summary>
        [DataMember]
        public string SecondaryDataPath
        {
            get;
            set;
        }
    }
}
