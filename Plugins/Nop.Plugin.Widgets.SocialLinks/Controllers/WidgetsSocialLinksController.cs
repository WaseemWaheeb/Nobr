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
        private readonly string _viewFolderPath = "~/Plugins/Widgets.SocialLinks/Views/WidgetsSocialLinks";

        private readonly ISettingService _settingService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        public WidgetsSocialLinksController(StoreInformationSettings storeInformationSettings, IStoreService storeService, IWorkContext workContext, ISettingService settingService)
        {
            _storeInformationSettings = storeInformationSettings;
            _storeService = storeService;
            _workContext = workContext;
            _settingService = settingService;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel()
            {
                FacebookUrl = _storeInformationSettings.FacebookLink,
                TwitterUrl = _storeInformationSettings.TwitterLink
            };

            return View(string.Format("{0}/{1}", _viewFolderPath, "Configure.cshtml"), model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var storeInformationSettings = _settingService.LoadSetting<StoreInformationSettings>(storeScope);

            storeInformationSettings.FacebookLink = model.FacebookUrl;
            storeInformationSettings.TwitterLink = model.TwitterUrl;

            if (!string.IsNullOrWhiteSpace(storeInformationSettings.FacebookLink))
                _settingService.SaveSetting(storeInformationSettings, x => x.FacebookLink, storeScope, false);
            else
                _settingService.DeleteSetting(storeInformationSettings, x => x.FacebookLink, storeScope);

            if (!string.IsNullOrWhiteSpace(storeInformationSettings.TwitterLink))
                _settingService.SaveSetting(storeInformationSettings, x => x.TwitterLink, storeScope, false);
            else
                _settingService.DeleteSetting(storeInformationSettings, x => x.TwitterLink, storeScope);

            _settingService.ClearCache();

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

            return View(string.Format("{0}/{1}", _viewFolderPath, "PublicInfo.cshtml"), model);
        }
    }
}