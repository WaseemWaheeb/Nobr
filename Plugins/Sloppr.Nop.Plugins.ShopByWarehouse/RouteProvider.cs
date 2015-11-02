using Nop.Web.Framework.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;

namespace Sloppr.Nop.Plugins.ShopByWarehouse
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Sloppr.Nop.Plugins.ShopByWarehouse.Cart",
                 "cart",
                 new { controller = "ShoppingCart", action = "Cart" },
                 new[] { "Sloppr.Nop.Plugins.ShopByWarehouse.Controllers" }
            );

            //RouteBase r =
            //     routes.MapRoute("Plugin.Misc.AzureBlob.Media",
            //    "Admin/Setting/Media",
            //     new { controller = "MiscAzureBlob", action = "Media", area = "admin" },
            //     new[] { "Nop.Plugin.Misc.AzureBlob.Controllers" });
            //routes.Remove(r);
            //routes.Insert(0, r);

            //ViewEngines.Engines.Insert(0, new CustomViewEngine());
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
