//-------------------------------------------------------------------------------------------------
// <copyright file="NetworkInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// <summary>
//     network information include adapter name, ip address, mac address etc.
// </summary>
//-------------------------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Properties
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    [DataContract]
    public class NetworkInfo
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string IpAddress
        {
            get;
            set;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string MacAddress
        {
            get;
            set;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public bool SupportRDMA
        {
            get;
            set;
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <param name="name">
        ///   <para />
        /// </param>
        /// <param name="ip">
        ///   <para />
        /// </param>
        /// <param name="mac">
        ///   <para />
        /// </param>
        /// <param name="supportRDMA">
        ///   <para />
        /// </param>
        public NetworkInfo(string name, string ip, string mac, bool supportRDMA = false)
        {
            this.Name = name;
            this.IpAddress = ip;
            this.MacAddress = mac;
            this.SupportRDMA = supportRDMA;
        }
    }
}
