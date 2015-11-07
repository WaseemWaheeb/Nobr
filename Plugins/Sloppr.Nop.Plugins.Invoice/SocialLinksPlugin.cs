using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Widgets.NivoSlider
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class SocialLinksPlugin : BasePlugin, IWidgetPlugin
    {
        public SocialLinksPlugin()
        {
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string> { "top_header_social" };
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
            controllerName = "WidgetsSocialLinks";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Widgets.SocialLinks.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetDisplayWidgetRoute(string widgetZone, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsSocialLinks";
            routeValues = new RouteValueDictionary
            {
                {"Namespaces", "Nop.Plugin.Widgets.SocialLinks.Controllers"},
                {"area", null},
                {"widgetZone", widgetZone}
            };
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
            base.Uninstall();
        }
    }
}
