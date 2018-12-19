using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Hpc.Scheduler.Session
{
    /// <summary>
    ///   <para>Defines the delegate that you need to implement when you subscribe to 
    /// the responses in a service-oriented architecture (SOA) client by calling the versions of the  
    /// 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}.SetResponseHandler(Microsoft.Hpc.Scheduler.Session.BrokerResponseHandler{System.Object})" /> method that include a <paramref name="state" /> parameter.</para> 
    /// </summary>
    /// <param name="response">
    ///   <para>A 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerResponse{T}" /> object that represents a response that the delegate receives and processes.</para>
    /// </param>
    /// <param name="state">
    ///   <para>A state object that the delegate receives each time it is called. You can use this object to pass the instance of the 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.BrokerClient{T}" /> class or other state information to the delegate.</para>
    /// </param>
    /// <typeparam name="TMessage">
    ///   <para>The type of the response messages that you want the delegate to receive. You create a TMessage 
    /// type by adding a service reference to the Visual Studio project for the client application or by running the svcutil tool.</para>
    /// </typeparam>
    public delegate void BrokerResponseStateHandler<TMessage>(BrokerResponse<TMessage> response, object state);
}
