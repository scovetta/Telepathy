// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Microsoft.Telepathy.Common.Rest.Server
{
    using System.Collections.Generic;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    public class InheritanceDirectRouteProvider : DefaultDirectRouteProvider
    {
        protected override IReadOnlyList<IDirectRouteFactory> GetActionRouteFactories(HttpActionDescriptor actionDescriptor)
        {
            return actionDescriptor.GetCustomAttributes<IDirectRouteFactory>(true);
        }
    }
}