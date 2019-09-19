// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.


namespace Microsoft.Hpc.RESTServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
                var response = Request.CreateResponse<ServiceInfo>(HttpStatusCode.OK, serviceInfo);
                return response;
            }
            else
            {
                var response = Request.CreateResponse(HttpStatusCode.BadRequest);
                return response;
            }
        }


        [Route]
        [HttpDelete]
        public HttpResponseMessage Delete()
        {
            ServiceInfo.DeleteInfo();
            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }

}
