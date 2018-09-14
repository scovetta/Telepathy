//-----------------------------------------------------------------------
// <copyright file="WebSessionInfoContract.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Data contract for result of creating/attaching session</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Net;
    using System.Runtime.Serialization;
    using Microsoft.Hpc.Scheduler.Session.Internal;

    /// <summary>
    ///   <para />
    /// </summary>
    [Serializable]
    [DataContract(Name = "WebSessionInfo", Namespace = "http://hpc.microsoft.com/")]
    public class WebSessionInfoContract
    {
        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public string BrokerNode { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public int ServiceOperationTimeout { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public long MaxMessageSize { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public Version ServiceVersion { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        [DataMember]
        public bool IsDurable { get; set; }
    }
}
