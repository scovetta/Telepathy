namespace TelepathyCommon.Service
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.Threading;
    using System.Threading.Tasks;

    public class WcfReliableClient<T> : IDisposable
    {
        private readonly Func<Task<T>> createWcfProxy;

        private readonly CancellationToken token;

        private T channel;

        private CancellationTokenSource cts;

        private int disposedValue; // To detect redundant calls

        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public WcfReliableClient(Func<Task<T>> createWcfProxy, CancellationToken token)
        {
            this.createWcfProxy = createWcfProxy;
            this.cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            this.token = this.cts.Token;
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            this.Dispose(true);

            // Suppress finalization of this disposed instance.
            GC.SuppressFinalize(this);
        }

        public async Task InvokeOperationWithRetryAsync(Action<T> action)
        {
            await this.InvokeOperationWithRetryAsync(action, new RetryManager(InstantRetryTimer.Instance, 1)).ConfigureAwait(false);
        }

        public async Task InvokeOperationWithRetryAsync(Action<T> action, RetryManager retryManager)
        {
            await this.InvokeOperationWithRetryAsync<object>(
                t =>
                    {
                        action(t);
                        return null;
                    },
                retryManager).ConfigureAwait(false);
        }

        public async Task<TResult> InvokeOperationWithRetryAsync<TResult>(Func<T, TResult> func)
        {
            return await this.InvokeOperationWithRetryAsync(func, new RetryManager(InstantRetryTimer.Instance, 1)).ConfigureAwait(false);
        }

        public virtual async Task<TResult> InvokeOperationWithRetryAsync<TResult>(Func<T, TResult> func, RetryManager retryManager)
        {
            while (true)
            {
                try
                {
                    var proxy = await this.GetWcfProxyAsync().ConfigureAwait(false);
                    return func(proxy);
                }
                catch (Exception e) when (!(e is FaultException) && e is CommunicationException)
                {
                    Trace.TraceError(e.ToString());
                    if (retryManager.HasAttemptsLeft)
                    {
                        await retryManager.AwaitForNextAttempt(this.token).ConfigureAwait(false);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (0 == Interlocked.Exchange(ref this.disposedValue, 1))
            {
                if (disposing)
                {
                    this.cts?.Cancel();
                    this.cts?.Dispose();
                    this.cts = null;

                    this.semaphore?.Dispose();
                    this.semaphore = null;

                    WcfChannelModule.DisposeWcfProxy(this.channel);
                }
            }
        }

        protected async Task<T> GetWcfProxyAsync()
        {
            if (this.disposedValue == 1)
            {
                throw new ObjectDisposedException(nameof(WcfReliableClient<T>));
            }

            if (this.channel == null || !WcfChannelModule.CheckWcfProxyHealth(this.channel))
            {
                Trace.TraceInformation("WcfReliableClient.GetWcfProxyAsync wait for semaphore, channel is null {0}", this.channel == null);
#if net40
                this.semaphore?.Wait(this.token);
#else
                await (this.semaphore?.WaitAsync(this.token)).ConfigureAwait(false);
#endif
                try
                {
                    if (this.channel == null || !WcfChannelModule.CheckWcfProxyHealth(this.channel))
                    {
                        Trace.TraceInformation("WcfReliableClient.GetWcfProxyAsync Dispose the old channel, channel is null {0}", this.channel == null);
                        WcfChannelModule.DisposeWcfProxy(this.channel);
                        Trace.TraceInformation("WcfReliableClient.GetWcfProxyAsync create a new channel");
                        this.channel = await this.createWcfProxy().ConfigureAwait(false);
                        Trace.TraceInformation("WcfReliableClient.GetWcfProxyAsync created a new channel");
                        var proxy = (IClientChannel)this.channel;
                    }
                }
                finally
                {
                    this.semaphore?.Release();
                }
            }

            return this.channel;
        }
    }
}