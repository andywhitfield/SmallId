using DotNetOpenAuth.OpenId.Provider.Behaviors;
using SmallId.Code;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmallId
{
    public class MvcApplication : HttpApplication
    {
        private static object behaviorInitializationSyncObject = new object();

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
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = string.Empty });
        }

        protected void Application_Start()
        {
            RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            InitializeBehaviors();
        }

        private static void InitializeBehaviors()
        {
            if (PpidGeneration.PpidIdentifierProvider != null)
            {
                return;
            }

            lock (behaviorInitializationSyncObject)
            {
                if (PpidGeneration.PpidIdentifierProvider == null)
                {
                    PpidGeneration.PpidIdentifierProvider = new AnonymousIdentifierProvider();
                    GsaIcamProfile.PpidIdentifierProvider = new AnonymousIdentifierProvider();
                }
            }
        }
    }
}