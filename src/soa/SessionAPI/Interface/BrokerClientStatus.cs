//-----------------------------------------------------------------------
// <copyright file="BrokerClientStatus.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>Enum for broker client status</summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    ///   <para>Defines values that indicate the state of a <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object.</para>
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com")]
    public enum BrokerClientStatus
    {
        /// <summary>
        ///   <para>Indicates that the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object is in an unknown state.</para>
        /// </summary>
        [EnumMember]
        Unknown = 0,

        /// <summary>
        ///   <para>Indicates that the 
        /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object is newly created and ready to process requests.</para>
        /// </summary>
        [EnumMember]
        Ready,

        /// <summary>
        ///   <para>Indicates that the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object is processing requests.</para>
        /// </summary>
        [EnumMember]
        Processing,

        /// <summary>
        ///   <para>Indicates that the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> object is finished processing requests.</para>
        /// </summary>
        [EnumMember]
        Finished,
    }
}
