using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Ajax_Minimal
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


            routes.MapRoute("map", "display/{ip}/{port}/{seconds}",
            new { controller = "Flight", action = "map" });

            routes.MapRoute("PositionOrLoad", "display/{s}/{num}",
            new { controller = "Flight", action = "PositionOrLoad" });

            routes.MapRoute("Save", "save/{ip}/{port}/{perSeconds}/{duration}/{file}",
            new { controller = "Flight", action = "Save" });

            routes.MapRoute(
                name: "Default",                                              
                url: "{controller}/{action}/{id}",                           
                defaults: new { controller = "Flight", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
