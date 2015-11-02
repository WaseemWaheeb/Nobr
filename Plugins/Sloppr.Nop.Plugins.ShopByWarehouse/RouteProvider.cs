using Nop.Web.Framework.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;
using Nop.Web.Framework.Localization;

namespace Sloppr.Nop.Plugins.ShopByWarehouse
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            var defaultShoppingCartRoute = routes["ShoppingCart"];
            routes.Remove(defaultShoppingCartRoute);

            routes.MapLocalizedRoute(
                name: "ShoppingCart",
                url: "Cart",
                defaults: new { controller = "WarehouseShoppingCart", action = "Cart" },
                namespaces: new[] { "Sloppr.Nop.Plugins.ShopByWarehouse.Controllers" }
            );

            var a = routes["ShoppingCart"];
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
