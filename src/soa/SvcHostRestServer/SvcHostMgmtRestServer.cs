// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Hpc.RESTServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Threading.Tasks;

    public class SvcHostMgmtRestServer : InternalRestServer
    {
        public static ServiceInfo Info = null; //new ServiceInfo();

        public SvcHostMgmtRestServer(string appRoot, int port) : base(appRoot, port, false)
        {
        }

        public SvcHostMgmtRestServer Initialize()
        {
            return this;
        }

        override
        public void Start()
        {
            this.Start<Startup>();
        }
    }
}