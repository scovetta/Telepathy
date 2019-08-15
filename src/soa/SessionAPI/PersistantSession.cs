//------------------------------------------------------------------------------
// <copyright file="PersistantSession.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      The implementation of the Persistant Session Class
// </summary>
//------------------------------------------------------------------------------

namespace Microsoft.Hpc.Scheduler.Session
{
    using System;
    using System.ServiceModel.Channels;

    /// <summary>
    ///   <para>Represents a durable session that binds a client application 
    /// to a service that supports the service-oriented architecture (SOA) programming model  
    /// that is based on the Windows Communication Foundation (WCF). A durable 
    /// session is a session that can recover from hardware or software failure.</para> 
    /// </summary>
    /// <remarks>
    ///   <para>You must dispose of this object when you finish using it. You can do this by calling the 
    /// <see cref="DurableSession.CreateSession(Microsoft.Hpc.Scheduler.Session.SessionStartInfo)" /> or 
    /// <see cref="Microsoft.Hpc.Scheduler.Session.Session.EndCreateSession(System.IAsyncResult)" /> method within a 
    /// <see href="http://go.microsoft.com/fwlink/?LinkID=177731">using Statement</see> (http://go.microsoft.com/fwlink/?LinkID=177731) in 
    /// C#, or by calling the  
    /// <see cref="Microsoft.Hpc.Scheduler.Session.SessionBase.Dispose" /> method.</para>
    /// </remarks>
    public class DurableSession : SessionBase
    {
        /// <summary>
        /// Initializes a new instance of the PersistantSession class
        /// </summary>
        /// <param name="info">Session Info</param>
        /// <param name="headnode">the head node machine name.</param>
        /// <param name="binding">indicating the binding</param>
        public DurableSession(SessionInfoBase info, string headnode, Binding binding)
            : base(info, headnode, binding)
        {
        }

        public static DurableSession CreateSession(SessionStartInfo info)
        {
            throw new NotImplementedException();
        }
    }
}
