//------------------------------------------------------------------------------
// <copyright file="ExceptionHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">nzeng</owner>
// Security review: nzeng 01-11-06
//------------------------------------------------------------------------------
namespace Microsoft.Hpc
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    public class RetryManager
    {
        public const int InfiniteRetries = -1;

        private RetryWaitTimer waitTimer;

        private int maxRetries;

        private int totalTimeLimit = Timeout.Infinite;

        private int retryCount = 0;

        private int totalWaitTime = 0;

        private int currentWaitTime = 0;

        public RetryManager(RetryWaitTimer waitTimer) : this(waitTimer, InfiniteRetries)
        {
        }

        public RetryManager(RetryWaitTimer waitTimer, int maxRetries) : this(waitTimer, maxRetries, Timeout.Infinite)
        {
        }

        public RetryManager(RetryWaitTimer waitTimer, int maxRetries, int totalTimeLimit)
        {
            if (waitTimer == null)
            {
                throw new ArgumentNullException(nameof(waitTimer));
            }

            this.waitTimer = waitTimer;

            this.SetMaxRetries(maxRetries);
            this.SetTotalTimeLimit(totalTimeLimit);
        }

        /// <summary>
        /// Gets the number of retries attempted thus far
        /// </summary>
        public int RetryCount => this.retryCount;

        /// <summary>
        /// Get the total spent waiting between retries
        /// </summary>
        public int ElaspsedWaitTime => this.totalWaitTime;

        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetryCount
        {
            get
            {
                return this.maxRetries;
            }
            set
            {
                this.SetMaxRetries(value);
            }
        }

        /// <summary>
        /// Gets or sets the total amount of time that may be spend waiting for retries.        
        /// </summary>
        public int TotalTimeLimit
        {
            get
            {
                return this.totalTimeLimit;
            }
            set
            {
                this.SetTotalTimeLimit(value);
            }
        }

        void SetMaxRetries(int n)
        {
            if (n < 0 && n != InfiniteRetries)
            {
                throw new ArgumentException("The maximum number of retries must be no less than zero, or InfiniteRetries");
            }

            this.maxRetries = n;
        }

        void SetTotalTimeLimit(int t)
        {
            if (t <= 0 && t != Timeout.Infinite)
            {
                throw new ArgumentException("The specified time must be greater than zero, or Timeout.Infinite");
            }

            this.totalTimeLimit = t;
        }

        /// <summary>
        /// Returns true if there are more retries left
        /// </summary>
        public bool HasAttemptsLeft
        {
            get
            {
                return (this.maxRetries == InfiniteRetries || this.retryCount < this.maxRetries)
                       && (this.totalTimeLimit == Timeout.Infinite || this.totalWaitTime < this.totalTimeLimit);
            }
        }

        /// <summary>
        /// Get the next wait time
        /// </summary>
        public int NextWaitTime
        {
            get
            {
                int waitTime = this.waitTimer.GetNextWaitTime(this.retryCount, this.currentWaitTime);
                if (this.totalTimeLimit != Timeout.Infinite && (this.totalWaitTime + waitTime > this.totalTimeLimit))
                {
                    waitTime = this.totalTimeLimit - this.totalWaitTime;
                }

                return waitTime;
            }
        }

        /// <summary>
        /// Increment the retry count and advance the total wait time without actually waiting
        /// </summary>
        public void SimulateNextAttempt()
        {
            this.WaitForNextAttempt(false, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Wait until the next retry by making the current thread sleep for the appropriate amount of time.
        /// May return immediately if the wait is zero.
        /// </summary>
        public void WaitForNextAttempt()
        {
            this.WaitForNextAttempt(true, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Increment the retry count and advance the total wait time without actually waiting
        /// </summary>
        public async Task AwaitSimulateNextAttempt()
        {
            await this.WaitForNextAttempt(false, CancellationToken.None).ConfigureAwait(false);
        }

        /// <summary>
        /// Wait until the next retry by making the current thread sleep for the appropriate amount of time.
        /// May return immediately if the wait is zero.
        /// </summary>
        public async Task AwaitForNextAttempt()
        {
            await this.AwaitForNextAttempt(CancellationToken.None).ConfigureAwait(false);
        }

        public async Task AwaitForNextAttempt(CancellationToken cancellationToken)
        {
            await this.WaitForNextAttempt(true, cancellationToken).ConfigureAwait(false);

        }

        async Task WaitForNextAttempt(bool doSleep, CancellationToken cancellationToken)
        {
            if (!this.HasAttemptsLeft)
            {
                throw new InvalidOperationException("There are no more retry attempts remaining");
            }

            this.currentWaitTime = this.NextWaitTime;
            this.retryCount++;

            Debug.Assert(this.currentWaitTime >= 0);
            if (this.currentWaitTime > 0)
            {
                if (doSleep)
                {
#if net40
                    await TaskEx.Delay(this.currentWaitTime, cancellationToken);
#else
                    await Task.Delay(this.currentWaitTime, cancellationToken).ConfigureAwait(false);
#endif
                }

                this.totalWaitTime += this.currentWaitTime;
            }
        }

        /// <summary>
        /// Resets the retry manager's retry count
        /// </summary>
        public void Reset()
        {
            this.retryCount = 0;
            this.totalWaitTime = 0;
            this.currentWaitTime = 0;
        }

        public async Task<T> InvokeWithRetryAsync<T>(
            Func<Task<T>> function,
            Func<Exception, bool> exPredicate,
            CancellationToken cancellationToken = default(CancellationToken),
            [CallerMemberName] string menberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            while (true)
            {
                try
                {
                    return await function().ConfigureAwait(false);
                }
                catch (Exception ex) when (!(ex is RetryCountExhaustException) && exPredicate(ex))
                {
                    if (this.HasAttemptsLeft)
                    {
                        Trace.TraceInformation(
                            "[RetryManager] {0}:{1} RetryCount: {2} \n Caller: {3} in {4}:line {5}",
                            ex.GetType(),
                            ex.Message,
                            this.RetryCount,
                            menberName,
                            sourceFilePath,
                            sourceLineNumber);
#if DEBUG
                        Trace.TraceInformation("[RetryManager] Exception: {0}", ex.ToString());
#endif
                        await this.AwaitForNextAttempt(cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        Trace.TraceError(
                            "[RetryManager] Execution failed with {0} retires. \nCaller: {1} in {2}:line {3} Exception:\n{4}",
                            this.RetryCount,
                            menberName,
                            sourceFilePath,
                            sourceLineNumber,
                            ex.ToString());
#if !NETCORE
                        if (HpcContext.NotRetryPreviousRetryFailure)
                        {
                            throw new RetryCountExhaustException(ex);
                        }
                        else
                        {
                            throw;
                        }
#else
                        throw;
#endif
                    }
                }
            }
        }

        public async Task InvokeWithRetryAsync(
            Func<Task> action,
            Func<Exception, bool> exPredicate,
            CancellationToken cancellationToken = default(CancellationToken),
            [CallerMemberName] string menberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            await this.InvokeWithRetryAsync<object>(
                    async () =>
                        {
                            await action().ConfigureAwait(false);
                            return null;
                        },
                    exPredicate,
                    cancellationToken,
                    menberName,
                    sourceFilePath,
                    sourceLineNumber)
                .ConfigureAwait(false);
        }

        public static async Task RetryOnceAsync(Action action, TimeSpan waitSpan, Func<Exception, bool> retryPredicate)
        {
            bool firstTry = true;
            while (true)
            {
                try
                {
                    action();
                    return;
                }
                catch (Exception ex) when (retryPredicate(ex) && firstTry)
                {
                    firstTry = false;
#if net40
                    await TaskEx.Delay(waitSpan).ConfigureAwait(false);
#else
                    await Task.Delay(waitSpan).ConfigureAwait(false);
#endif
                }
            }
        }
    }

    /// <summary>
    /// Defines how long a retry manager will wait between sub-sequent retries
    /// </summary>
    public abstract class RetryWaitTimer
    {
        internal abstract int GetNextWaitTime(int retryCount, int currentWaitTime);
    }

    /// <summary>
    /// Instantly returns without waiting
    /// </summary>
    public class InstantRetryTimer : RetryWaitTimer
    {
        internal override int GetNextWaitTime(int retryCount, int currentWaitTime)
        {
            return 0;
        }

        // This class should be a singleton
        private InstantRetryTimer() { }

        private static InstantRetryTimer instance = new InstantRetryTimer();
        public static InstantRetryTimer Instance
        {
            get { return instance; }
        }
    }

    /// <summary>
    /// Waits a constant time between subsequent retries
    /// </summary>
    public class PeriodicRetryTimer : RetryWaitTimer
    {
        private int period;

        public PeriodicRetryTimer(int period)
        {
            if (period < 0)
            {
                throw new ArgumentOutOfRangeException("period", "The period must be a non-negative integer (in milliseconds)");
            }

            this.period = period;
        }

        internal override int GetNextWaitTime(int retryCount, int currentWaitTime)
        {
            return this.period;
        }
    }

    /// <summary>
    /// A retry timer where wait time at retry n depends on the wait at retry n-1.
    /// </summary>
    public abstract class BoundedBackoffRetryTimer : RetryWaitTimer
    {
        private int initialWait;
        private int waitUpperBound;

        protected BoundedBackoffRetryTimer(int initialWait, int waitUpperBound)
        {
            if (initialWait <= 0)
            {
                throw new ArgumentOutOfRangeException("initialWait", "Initial value must be a positive integer (in milliseconds)");
            }

            if (waitUpperBound <= 0 && waitUpperBound != Timeout.Infinite)
            {
                throw new ArgumentOutOfRangeException("waitCap", "The wait cap must be greater than zero, or Timeout.Infinite");
            }

            this.initialWait = initialWait;
            this.waitUpperBound = waitUpperBound;
        }

        internal override int GetNextWaitTime(int retryCount, int currentWaitTime)
        {
            if (retryCount == 0)
            {
                return this.initialWait;
            }

            int nextWaitTime = this.GetBackOffValue(currentWaitTime);
            if (nextWaitTime < 0)
            {
                return 0;
            }

            if (this.waitUpperBound != Timeout.Infinite && nextWaitTime > this.waitUpperBound)
            {
                return this.waitUpperBound;
            }

            return nextWaitTime;
        }

        protected abstract int GetBackOffValue(int currentValue);
    }

    /// <summary>
    /// Wait times will increase exponentially
    /// </summary>
    public class ExponentialBackoffRetryTimer : BoundedBackoffRetryTimer
    {
        private double growthFactor;

        public ExponentialBackoffRetryTimer(int initialWait) : this(initialWait, Timeout.Infinite, 2) { }
        public ExponentialBackoffRetryTimer(int initialWait, int waitUpperBound) : this(initialWait, waitUpperBound, 2) { }

        public ExponentialBackoffRetryTimer(int initialWait, int waitUpperBound, double growthFactor)
            : base(initialWait, waitUpperBound)
        {
            if (growthFactor <= 0)
            {
                throw new ArgumentOutOfRangeException("growthFactor", "The growth factor must be a positive value");
            }

            this.growthFactor = growthFactor;
        }

        protected override int GetBackOffValue(int currentValue)
        {
            return (int)Math.Round(currentValue * this.growthFactor);
        }
    }

    /// <summary>
    /// Wait times will increase exponentially and also vary a bit randomly
    /// </summary>
    public class ExponentialRandomBackoffRetryTimer : ExponentialBackoffRetryTimer
    {
        private Random rand = null;
        public ExponentialRandomBackoffRetryTimer(int initialWait) : this(initialWait, Timeout.Infinite, 2) { }
        public ExponentialRandomBackoffRetryTimer(int initialWait, int waitUpperBound) : this(initialWait, waitUpperBound, 2) { }

        public ExponentialRandomBackoffRetryTimer(int initialWait, int waitUpperBound, double growthFactor)
            : base(initialWait, waitUpperBound, growthFactor)
        {
            this.rand = new Random();
        }

        protected override int GetBackOffValue(int currentValue)
        {
            return ((int)base.GetBackOffValue(currentValue)) + this.rand.Next(0, currentValue);
        }
    }

    /// <summary>
    /// Wait times will increase linearly
    /// </summary>
    public class LinearBackoffRetryTimer : BoundedBackoffRetryTimer
    {
        private int increment;

        public LinearBackoffRetryTimer(int initialWait) : this(initialWait, Timeout.Infinite, initialWait) { }
        public LinearBackoffRetryTimer(int initialWait, int waitUpperBound) : this(initialWait, waitUpperBound, initialWait) { }

        public LinearBackoffRetryTimer(int initialWait, int waitUpperBound, int increment)
            : base(initialWait, waitUpperBound)
        {
            this.increment = increment;
        }

        protected override int GetBackOffValue(int currentValue)
        {
            return currentValue + this.increment;
        }
    }

    public class RetryCountExhaustException : Exception
    {
        private const string RetryCountExhaustMessage = "Retry Count of RetryManager is exhausted.";

        public RetryCountExhaustException() : base()
        {
        }

        public RetryCountExhaustException(string message) : base(message)
        {
        }

        public RetryCountExhaustException(Exception ex) : base(RetryCountExhaustMessage, ex)
        {
        }
    }
}