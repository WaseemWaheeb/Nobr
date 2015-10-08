using Nop.Web.Framework.Mvc.Routes;
using System.Web.Routing;
using System.Web.Mvc;


namespace Nop.Plugin.Misc.AzureBlob
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Misc.AzureBlob.Configure",
                 "Plugins/AzureBlobConfigure/Configure",
                 new { controller = "MiscAzureBlob", action = "Configure" },
                 new[] { "Nop.Plugin.Misc.AzureBlob.Controllers" }
            );

            RouteBase r =
                 routes.MapRoute("Plugin.Misc.AzureBlob.Media",
                "Admin/Setting/Media",
                 new { controller = "MiscAzureBlob", action = "Media", area = "admin" },
                 new[] { "Nop.Plugin.Misc.AzureBlob.Controllers" });
            routes.Remove(r);
            routes.Insert(0, r);

            ViewEngines.Engines.Insert(0, new CustomViewEngine());
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
