using System.Collections.Generic;
using System.IO;
using System.Web.Routing;
using Nop.Core;
using Nop.Core.Plugins;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Sloppr.Nop.Plugins.Invoice
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class InvoicePlugin : BasePlugin, IMiscPlugin
    {
        public InvoicePlugin()
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
            controllerName = "Invoice";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Sloppr.Nop.Plugins.Invoice.Controllers" }, { "area", null } };
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
