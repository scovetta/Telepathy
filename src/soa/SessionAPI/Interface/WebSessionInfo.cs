//-----------------------------------------------------------------------
// <copyright file="WebSessionInfo.cs" company="Microsoft">
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
    /// Data contract for result of creating/attaching session
    /// </summary>
    public class WebSessionInfo : SessionInfoBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override int Id { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string BrokerNode { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override int ServiceOperationTimeout { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public long MaxMessageSize { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override Version ServiceVersion { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public bool IsDurable { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override bool Secure
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override TransportScheme TransportScheme
        {
            get
            {
                return TransportScheme.WebAPI;
            }
            set
            {
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public override bool UseInprocessBroker
        {
            get
            {
                return false;
            }
            set
            {
            }
        }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public NetworkCredential Credential { get; set; }

        /// <summary>
        ///   <para />
        /// </summary>
        /// <value>
        ///   <para />
        /// </value>
        public string HeadNode { get; set; }
    }
}
