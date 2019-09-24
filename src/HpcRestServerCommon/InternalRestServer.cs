// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Rest.Server
{
    using System;
    using System.Diagnostics;

    using Microsoft.Owin.Hosting;

    public abstract class InternalRestServer
    {
        private readonly string httpsUri;

        private IDisposable restService;

        protected InternalRestServer(string appRoot, int port, bool isHttps=true)
        {
            if (isHttps)
                this.httpsUri = string.Format("https://*:{0}/{1}", port, appRoot);
            else
                this.httpsUri = string.Format("http://*:{0}/{1}", port, appRoot);
        }

        public virtual void Start()
        {
            this.Start<Startup>();
        }

        protected virtual void Start<T>()
        {
            this.Stop();
            Trace.TraceInformation("[REST] Start internal REST service on {0}", this.httpsUri);
            this.restService = WebApp.Start<T>(this.httpsUri);
        }

        public virtual void Stop()
        {
            if (this.restService != null)
            {
                Trace.TraceInformation("[REST] Stop internal REST service on {0}", this.httpsUri);
                this.restService.Dispose();
                this.restService = null;
            }
        }
    }
}
