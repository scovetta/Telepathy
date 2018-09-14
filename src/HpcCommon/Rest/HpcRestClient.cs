namespace Microsoft.Hpc.Rest
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class HpcRestClient
    {
        protected abstract HttpClient RestClient { get; }

        protected HpcRestClient(IHpcContext context)
        {
            this.Context = context;
        }

        protected HpcRestClient(string restNode)
        {
            this.RestNode = restNode;
        }

        protected string RestNode { get; }

        protected IHpcContext Context { get; set; }

        protected abstract string RestClientName { get; }

        protected abstract string ApiRoot { get; }

        protected virtual string AppRoot => HpcConstants.FrontEndAppRoot;

        /// <summary> Get route string of Rest API. </summary>
        /// <param name="routeTemplate"> Route to Rest API </param>
        /// <param name="parameters"> Parameters to rest API, include optional parameters </param>
        /// <returns> Full API route string </returns>
        protected string GetApiRoute(string routeTemplate, params string[] parameters) 
            => this.AppRoot.TrimEnd('/') + '/' + this.ApiRoot.TrimEnd('/') + "/" + string.Format(routeTemplate, parameters);

        protected async Task<HttpResponseMessage> GetHttpApiCallAsync(CancellationToken cancellationToken, bool ensureSuccessStatusCode, string apiRoute)
        {
            var node = await this.ResolveRestNodeAsync().ConfigureAwait(false);
            Trace.TraceInformation("[{0}] Calling GET {1} on {2}.", this.RestClientName, apiRoute, node);
            var resp = await this.RestClient.GetHttpApiCallAsync(node, apiRoute, cancellationToken, RetryableException, ensureSuccessStatusCode).ConfigureAwait(false);
            if(!resp.IsSuccessStatusCode)
            {
                Trace.TraceWarning("[{0}] GET {1} on {2} Error: {3} (Code: {4}).", this.RestClientName, apiRoute, node, resp.ReasonPhrase, resp.StatusCode);
            }

            return resp;
        }

        protected async Task<HttpResponseMessage> PostHttpApiCallAsync<T>(CancellationToken cancellationToken, bool ensureSuccessStatusCode, string apiRoute, T value)
        {
            var node = await this.ResolveRestNodeAsync().ConfigureAwait(false);
            Trace.TraceInformation("[{0}] Calling POST {1} on {2}.", this.RestClientName, apiRoute, node);
            var resp = await this.RestClient.PostHttpApiCallAsync(node, apiRoute, value, cancellationToken, RetryableException, ensureSuccessStatusCode).ConfigureAwait(false);
            if (!resp.IsSuccessStatusCode)
            {
                Trace.TraceWarning("[{0}] POST {1} on {2} Error: {3} (Code: {4}).", this.RestClientName, apiRoute, node, resp.ReasonPhrase, resp.StatusCode);
            }

            return resp;
        }

        protected async Task<T> GetHttpApiCallAsync<T>(CancellationToken cancellationToken, string routeString, params string[] parameters)
        {
            string apiRoute = this.GetApiRoute(routeString, parameters);
            var res = await this.GetHttpApiCallAsync(cancellationToken, true, apiRoute).ConfigureAwait(false);
#if net40
            var content = await res.Content.ReadAsAsync<T>().ConfigureAwait(false);
#else
            var content = await res.Content.ReadAsAsync<T>(cancellationToken).ConfigureAwait(false);
#endif
            string traceContent;
            if (content != null)
            {
                string contentStr = content.ToString();
                int newLinePos = contentStr.IndexOf(Environment.NewLine, StringComparison.InvariantCulture);
                if (newLinePos == -1)
                {
                    // Single line content
                    traceContent = contentStr;
                }
                else
                {
                    traceContent = contentStr.Substring(0, newLinePos) + "...";
                }
            }
            else
            {
                traceContent = string.Empty;
            }

            Trace.TraceInformation("[{0}] GET {1} returned {2}.", this.RestClientName, apiRoute, traceContent);
            return content;
        }

        protected async Task<TOut> PostHttpApiCallAsync<TOut, TIn>(CancellationToken cancellationToken, string routeString, TIn value, params string[] parameters)
        {
            string apiRoute = this.GetApiRoute(routeString, parameters);
            var res = await this.PostHttpApiCallAsync(cancellationToken, true, apiRoute, value).ConfigureAwait(false);
#if net40
            var content = await res.Content.ReadAsAsync<TOut>().ConfigureAwait(false);
#else
            var content = await res.Content.ReadAsAsync<TOut>(cancellationToken).ConfigureAwait(false);
#endif
            Trace.TraceInformation("[{0}] POST {1} returned {2}.", this.RestClientName, apiRoute, content);
            return content;
        }

        protected virtual async Task<string> ResolveRestNodeAsync()
        {
            string node;
            if (string.IsNullOrEmpty(this.RestNode))
            {
                node = await this.Context.ResolveFrontendNodeAsync().ConfigureAwait(false) + ':' + HpcConstants.HpcFrontendServicePort;
            }
            else
            {
                node = this.RestNode;
                if (node.Contains(','))
                {
                    var nodes = node.Split(',').Select(n => n.Trim());
                    node = nodes.ElementAt((new Random()).Next(nodes.Count()));
                }

                node = node + ':' + HpcConstants.HpcFrontendServicePort;
            }

            return node;
        }

        protected virtual bool RetryableException(Exception ex) => !(ex is TaskCanceledException || ex is OperationCanceledException);

        protected static HttpClient DefaultHttpClientFactory() => new HttpClient(new HttpClientHandler { Proxy = null, UseProxy = false });
    }
}