namespace Microsoft.Hpc.RESTServiceModel
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    internal class RequestValidationHandler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            //TODO
            //have not considered security in this version, so delete this validation part.
            /*string version = null;
            IEnumerable<string> values;
            if (request.Headers.TryGetValues("x-hpc-version", out values))
            {
                version = values.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(version))
            {
                return request.CreateResponse(HttpStatusCode.BadRequest);
            }*/

            // TODO: validate Whether version support
            return await base.SendAsync(request, cancellationToken);
        }
    }
}