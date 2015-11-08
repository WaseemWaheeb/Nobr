using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Sloppr.ShopByWarehouse
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            //var defaultShoppingCartRoute = routes["ShoppingCart"];
            //routes.Remove(defaultShoppingCartRoute);

            //routes.MapLocalizedRoute(
            //    name: "ShoppingCart",
            //    url: "Cart",
            //    defaults: new { controller = "WarehouseShoppingCart", action = "Cart" },
            //    namespaces: new[] { "Nop.Plugin.Sloppr.ShopByWarehouse.Controllers" }
            //);
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
