using EF6AspNetWebApi.Data;
using EF6AspNetWebApi.Web.Filters;
using EF6AspNetWebApi.Web.Handlers;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Routing;

namespace EF6AspNetWebApi.Web
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            using (var context = new ExampleContext())
            {
                ExampleContextMetadata.Build(context);
            }

            GlobalConfiguration.Configure(config =>
            {
                //If this is an internal system it shouldn't hurt to let the consumers have a better idea of what's happening
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

                config.MapHttpAttributeRoutes();

                config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

                config.Filters.Add(new ItemNotFoundExceptionFilterAttribute());
                config.Filters.Add(new PatchCommandResultFilterAttribute());
                config.MessageHandlers.Add(new VersionHandler());
                config.MessageHandlers.Add(new ExposeHeadersHandler());
                config.MessageHandlers.Add(new ODataResultHandler());

                config.Services.Replace(typeof(IHttpControllerActivator), new ServiceActivator(config));
            });

            RouteTable.Routes.Ignore("api/{*anything}");
            RouteTable.Routes.Ignore("wwwroot/{*anything}");
            RouteTable.Routes.MapPageRoute("AnythingNonApi", "{*url}", "~/wwwroot/index.html");
        }
    }
}
