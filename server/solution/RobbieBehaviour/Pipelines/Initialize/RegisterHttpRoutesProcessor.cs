using Sitecore.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace RobbieBehaviour.Pipelines.Initialize
{
    public class RegisterHttpRoutesProcessor
    {
        public void Process(PipelineArgs args)
        {
            //GlobalConfiguration.Configure(Configure);
            //GlobalConfiguration.Configuration.Routes.MapHttpRoute(
            //    name: "IdentifyApi2", routeTemplate: "api/Identify/{action}", defaults: new {controller = "Identify");
            //GlobalConfiguration.Configuration.Routes.MapHttpRoute(
            //    name: "IdentifyApi", routeTemplate: "api/Identify");

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "IdentifyApi", "api/Identify/{action}", new { controller = "Identify" });
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "ProfileApi", "api/Profile/{action}", new { controller = "Profile" });
        }

        protected void Configure(HttpConfiguration configuration)
        {
            var routes = configuration.Routes;
            routes.MapHttpRoute("IdentifyApi", "sitecore/api/Identify/", new
            {
                controller = "Identify"
            });

            routes.MapHttpRoute("ProfileApi", "sitecore/api/Profile/", new
            {
                controller = "Profile"
            });
        }
    }
}
