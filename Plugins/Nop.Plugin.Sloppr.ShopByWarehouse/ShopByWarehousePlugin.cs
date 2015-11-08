using System.Configuration;
using System.Web.Configuration;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Sloppr.ShopByWarehouse
{
    public class ShopByWarehousePlugin : BasePlugin, IMiscPlugin
    {
        public ShopByWarehousePlugin()
        {
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ShopByWarehouse";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Sloppr.ShopByWarehouse.Controllers" }, { "area", null } };
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            base.Install();
        }
        
        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            base.Uninstall();
        }
    }
}
