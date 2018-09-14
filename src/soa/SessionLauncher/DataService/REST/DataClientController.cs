namespace Microsoft.Hpc.Scheduler.Session.Internal.SessionLauncher.DataService.REST
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Security;
    using System.ServiceModel;
    using System.Web.Http;

    using Microsoft.Hpc.Scheduler.Session.Data;
    using Microsoft.Hpc.Scheduler.Session.Data.DTO;
    using Microsoft.Hpc.Scheduler.Session.Data.Internal;

    [RoutePrefix("api/data-client")]
    public class DataClientController : ApiController
    {
        private static DataService ServiceInstance => DataServiceRestServer.DataServiceInstance;

        [HttpPost]
        [Route("open-by-secret")]
        public HttpResponseMessage OpenDataClientBySecret([FromBody] OpenDataClientBySecretParams param)
        {
            if (param == null)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid parameters");
            }

            try
            {
                var clientInfo = ServiceInstance.OpenDataClientBySecret(param.DataClientId, param.JobId, param.JobSecret);
                return this.Request.CreateResponse(HttpStatusCode.OK, clientInfo);
            }
            catch (SecurityException)
            {
                System.Diagnostics.Trace.TraceWarning(
                    $"[{nameof(DataClientController)}].{nameof(OpenDataClientBySecret)} Authentication failed for DataClient {param.DataClientId} and job {param.JobId}");
                return this.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch (FaultException<DataFault> ex)
            {
                System.Diagnostics.Trace.TraceError($"[{nameof(DataClientController)}].{nameof(OpenDataClientBySecret)} DataFault happened: {ex.ToString()}");
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"[{nameof(DataClientController)}].{nameof(OpenDataClientBySecret)} Exception happened: {ex.ToString()}");
                throw;
            }
        }
    }
}
