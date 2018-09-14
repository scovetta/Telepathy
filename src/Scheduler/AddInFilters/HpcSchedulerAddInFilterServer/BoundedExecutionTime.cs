//------------------------------------------------------------------------------
// <copyright file="BoundedExecutionTime.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">daryls</owner>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Hpc.Scheduler.AddInFilter.HpcServer
{
    class MaxTimeCall
    {
        public class DoiItArgs
        {
            public Action<object> DoIt = null;
            public Exception SomethingBadHappened = null;
            public long CallCompleted = 0;

            public ManualResetEvent WaitOnThis = new ManualResetEvent(false);
        }

        public static void DoItThreadProc(object args)
        {
            DoiItArgs realArgs = args as DoiItArgs;

            if (null != realArgs)
            {
                try
                {
                    realArgs.DoIt(null);  // do whatever is needed doing
                }
                catch (Exception ex)
                {
                    realArgs.SomethingBadHappened = ex;  // pass back the exception to original thread
                }
                finally
                {
                    realArgs.CallCompleted = 1;
                    realArgs.WaitOnThis.Set();
                }
            }
        }

        // makes a call but on another thread.  any exceptions are
        // proxied back and thrown on calling thread
        public static void MakeACall(int maxMillSecs, string timeoutExceptionText, Action<object> doit)
        {
            DoiItArgs args = new DoiItArgs();
            
            args.DoIt = doit;

            Thread worker = new Thread(new ParameterizedThreadStart(DoItThreadProc));

            worker.IsBackground = true;
            worker.Start(args);  // go off and do work

            if (args.WaitOnThis.WaitOne(maxMillSecs, false)) // here we wait for the child thread to make desired call
            {
                if (null != args.SomethingBadHappened)
                {
                    throw new AddInFilterTimeBoundedCallException(args.SomethingBadHappened);  // here we throw so caller sees error
                }
            }
            else // it timed out
            {
                // leave here for this release, but ultimately, this should be removed and we'd leverage TPL to cancel the addin's task.
                worker.Abort();

                throw new TimeoutException(timeoutExceptionText);
            }
        }
    }
}
