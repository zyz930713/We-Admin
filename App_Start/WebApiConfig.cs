using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Wenba.App_Start;

namespace Wenba
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            //利用反射，修改DefaultHttpContollerSelector.ControllerSuffix为“ApiController”
            var suffix = typeof(DefaultHttpControllerSelector).GetField("ControllerSuffix", BindingFlags.Static | BindingFlags.Public);
            if (suffix != null) suffix.SetValue(null, "ApiController");
            
            // Web API routes
            config.MapHttpAttributeRoutes();

            //config.Routes.MapHttpRoute(
            //    name: "GetOpenId",
            //    routeTemplate: "api/{controller}/{action}/{code}"
            //    //defaults: new { name = RouteParameter.Optional, password = RouteParameter.Optional }
            //);

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            var jsonFormatter = new JsonMediaTypeFormatter();
            config.Services.Replace(typeof(IContentNegotiator), new JsonContentNegotiator(jsonFormatter));
        }
    }
}
