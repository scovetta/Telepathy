// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.


namespace Microsoft.Telepathy.CcpServiceHost.Rest
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    [RoutePrefix("api/svchostserver")]
    public class SvcHostServerController : ApiController
    {
        [Route]
        [HttpGet]
        public ServiceInfo Get()
        {
            return SvcHostMgmtRestServer.Info;
        }

        [Route]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]ServiceInfo serviceInfo)
        {
            if (ServiceInfo.SaveInfo(serviceInfo))
            {
                var response = this.Request.CreateResponse<ServiceInfo>(HttpStatusCode.OK, serviceInfo);
                return response;
            }
            else
            {
                var response = this.Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }


        [Route]
        [HttpDelete]
        public HttpResponseMessage Delete()
        {
            ServiceInfo.DeleteInfo();
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }
    }

}
