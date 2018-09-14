using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    ///   <para>Defines the delegate that you need to implement when you 
    /// subscribe to the responses in a service-oriented architecture (SOA) client by calling the  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> method.</para> 
    /// </summary>
    /// <param name="response">
    ///   <para>A 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object that represents a response that the delegate receives and processes.</para>
    /// </param>
    /// <typeparam name="TMessage">
    ///   <para>The type of the response messages that you want the delegate to receive. You create TMessage 
    /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
    /// </typeparam>
    public delegate void BrokerResponseHandler<TMessage>(BrokerResponse<TMessage> response);
}
