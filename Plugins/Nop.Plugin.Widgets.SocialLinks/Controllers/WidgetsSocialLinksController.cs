using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Plugin.Widgets.SocialLinks.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Widgets.SocialLinks.Controllers
{
    public class WidgetsSocialLinksController : BasePluginController
    {
        private StoreInformationSettings _storeInformationSettings;

        public WidgetsSocialLinksController(StoreInformationSettings storeInformationSettings)
        {
            _storeInformationSettings = storeInformationSettings;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            return View("~/Plugins/Widgets.SocialLinks/Views/WidgetsSocialLinks/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone, object additionalData = null)
        {
            var model = new PublicInfoModel()
            {
                FacebookUrl = _storeInformationSettings.FacebookLink,
                TwitterUrl = _storeInformationSettings.TwitterLink
            };

            return View("~/Plugins/Widgets.SocialLinks/Views/WidgetsSocialLinks/PublicInfo.cshtml", model);
        }
    }
}