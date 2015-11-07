using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Security;
using Nop.Web.Framework.Security.Captcha;
using Sloppr.Nop.Plugins.Invoice.Models;

namespace Sloppr.Nop.Plugins.Invoice.Controllers
{
    public class InvoiceController : BasePluginController
    {
        private readonly string _viewFolderPath = "~/Plugins/Sloppr.Nop.Plugins.Invoice/Views/Invoice";
        
        private readonly ISettingService _settingService;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        public InvoiceController(StoreInformationSettings storeInformationSettings, IStoreService storeService, IWorkContext workContext, ISettingService settingService)
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
            return View(string.Format("{0}/{1}", _viewFolderPath, "Configure.cshtml"));
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
            return View(string.Format("{0}/{1}", _viewFolderPath, "PublicInfo.cshtml"));
        }
    }
}
