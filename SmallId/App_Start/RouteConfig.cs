using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmallId
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "User identities",
                "user/{id}/{action}",
                new { controller = "User", action = "Identity", id = string.Empty, anon = false });

            routes.MapRoute(
                "PPID identifiers",
                "anon",
                new { controller = "User", action = "Identity", id = string.Empty, anon = true });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
