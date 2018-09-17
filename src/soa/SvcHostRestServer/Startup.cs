
namespace Microsoft.Hpc.RESTServiceModel
{

    using System.Web.Http;
    using global::Owin;

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.MessageHandlers.Add(new RequestValidationHandler());
            appBuilder.UseWebApi(config);
        }
    }
}