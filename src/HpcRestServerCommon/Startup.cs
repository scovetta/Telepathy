namespace Microsoft.Hpc
{
    using System.Web.Http;

    using global::Owin;

    public class Startup
    {
        public static void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes(new InheritanceDirectRouteProvider());

            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional });

            appBuilder.UseClientCertificateAuthentication(new DefaultClientCertificateValidator());

            appBuilder.UseWebApi(config);
        }
    }
}
