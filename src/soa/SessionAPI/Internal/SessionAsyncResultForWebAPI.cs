//------------------------------------------------------------------------------
// <copyright file="SessionAsyncResultForWebAPI.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//      Provides async result for create session through web API
// </summary>
//------------------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    /// <summary>
    /// Provides async result for create session through web API
    /// </summary>
    internal class SessionAsyncResultForWebAPI : SessionAsyncResultBase
    {
        /// <summary>
        /// Initializes a new instance of the SessionAsyncResultForWebAPI class
        /// </summary>
        /// <param name="startInfo">indicating the session start information</param>
        /// <param name="callback">indicating the callback</param>
        /// <param name="state">indicating the async state</param>
        public SessionAsyncResultForWebAPI(SessionStartInfo startInfo, AsyncCallback callback, object state)
            : base(startInfo, callback, state)
        {
            ThreadPool.QueueUserWorkItem(this.CreateSessionThreadProc);
        }

        /// <summary>
        /// Create session in another thread
        /// </summary>
        /// <param name="state">indicating the state</param>
        private void CreateSessionThreadProc(object state)
        {
            try
            {
                this.MarkFinish(null, V3Session.CreateSessionAsync(this.StartInfo).GetAwaiter().GetResult());
            }
            catch (Exception e)
            {
                this.MarkFinish(e);
            }
        }

        /// <summary>
        /// Override cancel method as we don't support to cancel create session for Web API
        /// </summary>
        public override void Cancel()
        {
            throw new NotSupportedException(SR.WebAPI_NotSupportCancelCreateSession);
        }

        /// <summary>
        /// Need to do nothing when failed to create session
        /// </summary>
        protected override Task CleanupOnFailure()
        {
#if net40
            return TaskEx.FromResult(1);
#else
            return Task.FromResult(1);
#endif
        }
    }
}
