//-----------------------------------------------------------------------
// <copyright file="DiagCleanerBase.cs" company="Microsoft">
//     Copyright   Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>
//   Base class of the soa diag cleaner.
// </summary>
//-----------------------------------------------------------------------
namespace Microsoft.Hpc.Scheduler.Session.Internal.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using Microsoft.Hpc.Scheduler.Session.Common;

    /// <summary>
    /// Cleanup the on-premise soa diag trace files periodically.
    /// </summary>
    internal abstract class DiagCleanerBase : DisposableObject
    {
        /// <summary>
        /// Group name in the regular expression.
        /// </summary>
        protected const string GroupName = "id";

        /// <summary>
        /// Regular expression of the trace folder name.
        /// </summary>
        protected static readonly Regex SessionFolderRegex = new Regex(@"^Session(?<id>\d+)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// Due time of the timer.
        /// </summary>
        private static readonly long DueTime = (long)TimeSpan.FromDays(1).TotalMilliseconds;

        /// <summary>
        /// Backoff span of the timer.
        /// </summary>
        private static readonly long Backoff = (long)TimeSpan.FromHours(1).TotalMilliseconds;

        /// <summary>
        /// Store the retry period for connection to scheduler delegation.
        /// </summary>
        private static readonly TimeSpan RetryPeriod = TimeSpan.FromHours(1);

        /// <summary>
        /// Backoff random of the timer.
        /// </summary>
        private readonly Random backoffRandom = new Random(DateTime.UtcNow.Millisecond);

        /// <summary>
        /// The timer for cleanup thread.
        /// </summary>
        private Timer timer;

        /// <summary>
        /// The set stores the valid session ids.
        /// </summary>
        private HashSet<int> sessionIds;

        /// <summary>
        /// Start the timer to cleanup trace.
        /// </summary>
        public void Start()
        {
            // start the cleaner immediately at first time
            this.timer = new Timer(this.TimerCallback, null, 0, -1);
        }

        /// <summary>
        /// Dispose the instance
        /// </summary>
        /// <param name="disposing">
        /// indicating whether it is disposing
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.timer != null)
                {
                    try
                    {
                        this.timer.Dispose();
                    }
                    catch
                    {
                        // ignore the exception, becasue the cleaner is closing.
                    }
                    finally
                    {
                        this.timer = null;
                    }
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Cleanup the trace folders and files.
        /// </summary>
        protected abstract void CleanupTrace();

        /// <summary>
        /// Check if the specified session is valid.
        /// </summary>
        /// <param name="sessionId">soa session id</param>
        /// <returns>id is valid or not</returns>
        protected bool IsSessionIdValid(int sessionId)
        {
            return this.sessionIds.Contains(sessionId);
        }

        /// <summary>
        /// Get the next due time for the timer.
        /// </summary>
        /// <returns>next due time</returns>
        private long GetNextDueTime()
        {
            // TODO: add a config file to specify the duetime and backoff time for testing
            //return (long)TimeSpan.FromSeconds(30).TotalMilliseconds;

            return DueTime + this.backoffRandom.Next(-(int)Backoff, (int)Backoff);
        }

        /// <summary>
        /// Callback method of the timer.
        /// </summary>
        /// <param name="state">callback state object (not used)</param>
        private async void TimerCallback(object state)
        {
            List<int> validIds = null;

            while (validIds == null)
            {
                try
                {
                    using (var client = new RetryableSchedulerAdapterClient())
                    {
                        validIds = await client.GetAllSessionId();

                        DiagTraceHelper.TraceVerbose(
                            "[DiagCleanerBase] TimerCallback: GetAllSessionId returns {0} Ids.", validIds.Count);
                    }
                }
                catch (Exception e)
                {
                    // the scheduler delegation may not be ready, so retry periodically
                    DiagTraceHelper.TraceError(
                        "[DiagCleanerBase] TimerCallback: Error happened when GetAllSessionId, {0}",
                        e);

                    Thread.Sleep(RetryPeriod);
                }
            }

            try
            {
                this.sessionIds = new HashSet<int>();

                foreach (int id in validIds)
                {
                    this.sessionIds.Add(id);
                }

                this.CleanupTrace();
            }
            catch (Exception e)
            {
                // swallow the exception avoid thread crash
                DiagTraceHelper.TraceError(
                    "[DiagCleanerBase] TimerCallback: Error happened when cleanup trace, {0}",
                    e);
            }
            finally
            {
                try
                {
                    this.timer.Change(this.GetNextDueTime(), -1);
                }
                catch (ObjectDisposedException)
                {
                    // swallow the exception if timer is disposed and set to null
                }
                catch (NullReferenceException)
                {
                    // swallow the exception if timer is disposed and set to null
                }
                catch (Exception e)
                {
                    // swallow the exception avoid thread crash
                    DiagTraceHelper.TraceError(
                        "[DiagCleanerBase] TimerCallback: Error happened when update timer, {0}",
                        e);
                }
            }
        }
    }
}
