// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.CcpServiceHost
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.ServiceModel;

    using Microsoft.Telepathy.Session;
    using Microsoft.Telepathy.Session.Interface;

    using TelepathyCommon;

    using RuntimeTraceHelper = Microsoft.Telepathy.RuntimeTrace.TraceHelper;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class HpcServiceHost : IHpcServiceHost
    {
        /// <summary>
        /// How long to wait for Exiting event to return
        /// </summary>
        int _cancelTaskGracePeriod = 0;

        /// <summary>
        /// sessionId
        /// </summary>
        string _sessionId = "0";

        private CcpServiceHostWrapper hostWrapper;

        /// <summary>
        /// Constructor for service host
        /// </summary>
        /// <param name="cancelTaskGracePeriod"></param>
        public HpcServiceHost(string sessionId, int cancelTaskGracePeriod, CcpServiceHostWrapper hostWrapper)
        {
            this._sessionId = sessionId;
            this._cancelTaskGracePeriod = cancelTaskGracePeriod;
            this.hostWrapper = hostWrapper;
        }

        /// <summary>
        /// Shutdowns down service host
        /// </summary>
        /// <param name="state"></param>
        public void Exit()
        {
            try
            {
                lock (this.hostWrapper.SyncObjOnExitingCalled)
                {
                    // No need to call OnExiting again if it is already called when Ctrl-B signal is received.
                    if (!this.hostWrapper.IsOnExitingCalled)
                    {
                        // Invoke user's Exiting event async with a timeout specified by TaskCancelGracePeriod cluster parameter.
                        Action<object> a = this.InvokeFireExitingEvent;
                        IAsyncResult ar = a.BeginInvoke(null, null, null);
                        if (ar.AsyncWaitHandle.WaitOne(this._cancelTaskGracePeriod, false))
                        {
                            a.EndInvoke(ar);
                        }

                        this.hostWrapper.IsOnExitingCalled = true;
                    }
                }
            }

            catch (Exception e)
            {
                RuntimeTraceHelper.TraceEvent(
                    this._sessionId,
                    TraceEventType.Warning,
                    "[HpcServiceHost]: Exception calling Exiting - {0}",
                    e);
            }

            finally
            {
                // Keep the exit code the same as before. If the host is cancelled by user or scheduler,
                // it exits with -1. If the host is closed by graceful shrink, it exits with 0.
                if (this.hostWrapper.ReceivedCancelEvent)
                {
                    Console.Out.WriteLine(StringTable.TaskCanceledOrPreempted);
                    Console.Out.Flush();
                    Environment.Exit(-1);
                }
                else
                {
                    Console.Out.WriteLine(StringTable.ServiceShutdownFromBrokerShrink);
                    Console.Out.Flush();
                    Environment.Exit(ErrorCode.Success);
                }
            }
        }

        /// <summary>
        /// Invokes private member FireExistingEvent in Session API via reflection so that FireExitingEvent doesnt need to be public
        /// </summary>
        private void InvokeFireExitingEvent(object sender)
        {
            RuntimeTraceHelper.RuntimeTrace.LogHostCanceled(this._sessionId);

            Type serviceContextType = typeof(ServiceContext);

            serviceContextType.InvokeMember(
                "FireExitingEvent",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                null,
                new object[] { sender },
                CultureInfo.CurrentCulture);
        }
    }
}
