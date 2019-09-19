// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;

    /// <summary>
    ///   <para>Specifies the behavior of the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property.</para>
    /// </summary>
    /// <remarks>
    ///   <para>When the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.EnableIsLastResponseProperty" />, the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> will return 
    /// True when the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response. The 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will hold the last response until the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method is called.</para>
    ///   <para>When the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.Behaviors" /> property is set to 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClientBehaviors.None" />, the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object will return the response immediately without the need to call the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.EndRequests()" /> method. However, the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property will always return 
    /// False, even when the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object contains the last response.</para>
    /// </remarks>
    [Flags]
    public enum BrokerClientBehaviors
    {
        /// <summary>
        ///   <para>Disables the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property.</para>
        /// </summary>
        None = 0x0,

        /// <summary>
        ///   <para>Enables the <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}.IsLastResponse" /> property.</para>
        /// </summary>
        EnableIsLastResponseProperty = 0x1
    }
}
