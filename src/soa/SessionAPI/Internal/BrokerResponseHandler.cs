// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Session.Internal
{
    /// <summary>
    ///   <para>Defines the delegate that you need to implement when you 
    /// subscribe to the responses in a service-oriented architecture (SOA) client by calling the  
    /// 
    /// <see cref="BrokerClient{TContract}.SetResponseHandler(Microsoft.Telepathy.Session.Internal.BrokerResponseHandler{object})" /> method.</para> 
    /// </summary>
    /// <param name="response">
    ///   <para>A 
    /// <see cref="BrokerResponse{TMessage}" /> object that represents a response that the delegate receives and processes.</para>
    /// </param>
    /// <typeparam name="TMessage">
    ///   <para>The type of the response messages that you want the delegate to receive. You create TMessage 
    /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
    /// </typeparam>
    public delegate void BrokerResponseHandler<TMessage>(BrokerResponse<TMessage> response);
}
