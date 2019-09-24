// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session
{
    using System;

    using Microsoft.Telepathy.Session.Internal;

    /// <summary>
    ///   <para>Specifies the behavior of the <see cref="BrokerResponse{TMessage}.IsLastResponse" /> property.</para>
    /// </summary>
    /// <remarks>
    ///   <para>When the 
    /// <see cref="BrokerClient{TContract}.Behaviors" /> property is set to 
    /// <see cref="EnableIsLastResponseProperty" />, the 
    /// <see cref="BrokerResponse{TMessage}.IsLastResponse" /> will return 
    /// True when the 
    /// <see cref="BrokerResponse{TMessage}" /> object contains the last response. The 
    /// <see cref="BrokerResponse{TMessage}" /> object will hold the last response until the 
    /// <see cref="BrokerClient{TContract}.EndRequests()" /> method is called.</para>
    ///   <para>When the 
    /// <see cref="BrokerClient{TContract}.Behaviors" /> property is set to 
    /// <see cref="None" />, the 
    /// <see cref="BrokerResponse{TMessage}" /> object will return the response immediately without the need to call the 
    /// <see cref="BrokerClient{TContract}.EndRequests()" /> method. However, the 
    /// <see cref="BrokerResponse{TMessage}.IsLastResponse" /> property will always return 
    /// False, even when the 
    /// <see cref="BrokerResponse{TMessage}" /> object contains the last response.</para>
    /// </remarks>
    [Flags]
    public enum BrokerClientBehaviors
    {
        /// <summary>
        ///   <para>Disables the <see cref="BrokerResponse{TMessage}.IsLastResponse" /> property.</para>
        /// </summary>
        None = 0x0,

        /// <summary>
        ///   <para>Enables the <see cref="BrokerResponse{TMessage}.IsLastResponse" /> property.</para>
        /// </summary>
        EnableIsLastResponseProperty = 0x1
    }
}
