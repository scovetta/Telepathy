// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Interface
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///   <para>Defines values that indicate the state of a <see cref="BrokerClient{TContract}" /> object.</para>
    /// </summary>
    [Serializable]
    [DataContract(Namespace = "http://hpc.microsoft.com")]
    public enum BrokerClientStatus
    {
        /// <summary>
        ///   <para>Indicates that the <see cref="BrokerClient{TContract}" /> object is in an unknown state.</para>
        /// </summary>
        [EnumMember]
        Unknown = 0,

        /// <summary>
        ///   <para>Indicates that the 
        /// <see cref="BrokerClient{TContract}" /> object is newly created and ready to process requests.</para>
        /// </summary>
        [EnumMember]
        Ready,

        /// <summary>
        ///   <para>Indicates that the <see cref="BrokerClient{TContract}" /> object is processing requests.</para>
        /// </summary>
        [EnumMember]
        Processing,

        /// <summary>
        ///   <para>Indicates that the <see cref="BrokerClient{TContract}" /> object is finished processing requests.</para>
        /// </summary>
        [EnumMember]
        Finished,
    }
}
