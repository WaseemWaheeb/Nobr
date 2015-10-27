using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;
using System.Web.Mvc;
using Nop.Admin.Extensions;
using Nop.Admin.Models.Settings;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Plugin.Misc.AzureBlob.Models;
using Nop.Plugin.Misc.AzureBlob.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.UI;

namespace Nop.Plugin.Misc.AzureBlob.Controllers
{
    public class MiscAzureBlobController : Controller
    {
        #region Fileds
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IDownloadService _downloadService;
        private readonly BlobService _blobService;
        #endregion

        #region Ctor
        public MiscAzureBlobController(ILocalizationService localizationService,
            ICustomerActivityService customerActivityService, IWorkContext workContext, IStoreService storeService,
            ISettingService settingService, IPermissionService permissiionService,
            IPictureService pictureService, IDownloadService downloadService,
           BlobService blobService)
        {
            _settingService = settingService;
            _pictureService = pictureService;
            _storeService = storeService;
            _permissionService = permissiionService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _downloadService = downloadService;
            _blobService = blobService;
        }
        #endregion

        #region Config Actions 
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
            var model = new ConfigurationModel
            {
                Account = azureBlobSetting.Account,
                AccountKey = azureBlobSetting.AccountKey,
                UseDevAccount = azureBlobSetting.UseDevAccount,
                ContainerForPictures = azureBlobSetting.ContainerForPictures,
                Container = azureBlobSetting.Container,
                UseCDN = azureBlobSetting.UseCDN,
                CDN = azureBlobSetting.CDN,
                PictureStoreType = azureBlobSetting.PictureStoreType,
                DownloadStoreType = azureBlobSetting.DownloadStoreType,
                AlwaysShowMainImage = azureBlobSetting.AlwaysShowMainImage,
                CheckIfImageExist = azureBlobSetting.CheckIfImageExist,
            };
            //todo:Factory 
            var validatePictureSettings = _blobService.ValidatePictureSettings();
            if (String.IsNullOrEmpty(validatePictureSettings))
            {
                model.PictureStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_AZURE,
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE,
                    StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM
                };
            }
            else
            {
                AddNotification(NotifyType.Error, validatePictureSettings, true);
                model.PictureStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE,
                    StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM
                };
            }
            //todo:Factory 
            var validateDownloadSettings = _blobService.ValidateDownloadSettings();
            if (String.IsNullOrEmpty(validateDownloadSettings))
            {
                model.DownloadStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_AZURE,
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE
                };
            }
            else
            {
                AddNotification(NotifyType.Error, validateDownloadSettings, true);
                model.DownloadStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE
                };
            }

            return View("~/Plugins/Misc.AzureBlob/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [FormValueRequired("save-configure")]
        public ActionResult Configure(ConfigurationModel model)
        {
            
            if (!ModelState.IsValid)
                return Configure();
            var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();
            azureBlobSetting.Container = model.Container;
            azureBlobSetting.ContainerForPictures = model.ContainerForPictures;
            azureBlobSetting.AccountKey = model.AccountKey;
            azureBlobSetting.UseDevAccount = model.UseDevAccount;
            azureBlobSetting.ConnectionString = AzureConnectionStringFactory.Create(model.Account, model.AccountKey, model.UseDevAccount);
            azureBlobSetting.Account = model.Account;
            azureBlobSetting.UseCDN = model.UseCDN;
            azureBlobSetting.CDN = model.CDN;
            azureBlobSetting.CheckIfImageExist = model.CheckIfImageExist;
            azureBlobSetting.AlwaysShowMainImage = model.AlwaysShowMainImage;
            _settingService.SaveSetting(azureBlobSetting);
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.ConnectionStrings.ConnectionStrings.Remove("Nop.Plugin.Misc.AzureBlob.ConnectionString");
            config.ConnectionStrings.ConnectionStrings.Add(
                new ConnectionStringSettings("Nop.Plugin.Misc.AzureBlob.ConnectionString",
                    azureBlobSetting.ConnectionString));
            config.Save();
            (_pictureService as StorePictureService).StoreType = model.PictureStoreType;
            (_downloadService as StoreDownloadService).StoreType = model.DownloadStoreType;
            _customerActivityService.InsertActivity("EditSettings",
                            _localizationService.GetResource("ActivityLog.EditSettings"));
            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-pictures-storage")]
        public ActionResult ChangePictureStorage(ConfigurationModel model)
        {
            (_pictureService as StorePictureService).StoreType = model.PictureStoreType;

            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));
            SuccessNotification(_localizationService.GetResource("AzureBlob.ConfigureModel.ChangePicturesStorage"));

            return Configure();
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("add-default-images")]
        public ActionResult AddDefaultImages(ConfigurationModel model)
        {
            (_pictureService as StorePictureService).CopyDefaultFiles();

            return Configure();
        }
        
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("change-files-storage")]
        public ActionResult ChangeFilesStorage(ConfigurationModel model)
        {
            (_downloadService as StoreDownloadService).StoreType = model.DownloadStoreType;
            
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));
            SuccessNotification(_localizationService.GetResource("AzureBlob.ConfigureModel.ChangeFilesStorage"));
            return Configure();
        }


        #endregion

        #region MediaMethods
        public ActionResult Change()
        {
            return PartialView("~/Plugins/Misc.AzureBlob/Views/Change.cshtml");
        }

        public ActionResult ChangeInformation(MediaModel model)
        {
            (_pictureService as StorePictureService).StoreType = model.PictureStoreType;
            _customerActivityService.InsertActivity("EditSettings",
                _localizationService.GetResource("ActivityLog.EditSettings"));
            

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            return RedirectToAction("Media");
        }

        public ActionResult Media()
        {
            
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            var model = mediaSettings.ToModel();
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.AvatarPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.AvatarPictureSize, storeScope);
                model.ProductThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.ProductThumbPictureSize, storeScope);
                model.ProductDetailsPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.ProductDetailsPictureSize, storeScope);
                model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore =
                    _settingService.SettingExists(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                        storeScope);
                model.AssociatedProductPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.AssociatedProductPictureSize, storeScope);
                model.CategoryThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.CategoryThumbPictureSize, storeScope);
                model.ManufacturerThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.ManufacturerThumbPictureSize, storeScope);
                model.CartThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.CartThumbPictureSize, storeScope);
                model.MiniCartThumbPictureSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.MiniCartThumbPictureSize, storeScope);
                model.MaximumImageSize_OverrideForStore = _settingService.SettingExists(mediaSettings,
                    x => x.MaximumImageSize, storeScope);
            }
            model.PicturesStoredIntoDatabase = true;
            var azureBlobSetting = _settingService.LoadSetting<AzureBlobSetting>();

            ViewBag.DownloadStoreType = azureBlobSetting.DownloadStoreType;

            var validatePictureSettings = _blobService.ValidatePictureSettings();
            if (String.IsNullOrEmpty(validatePictureSettings))
            {
                ViewBag.PictureStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_AZURE,
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE,
                    StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM
                };
            }
            else
            {
                AddNotification(NotifyType.Error, validatePictureSettings, true);
                ViewBag.PictureStoreTypeList = new List<string>
                {
                    StorePictureService.PICTURE_STORE_TYPE_DATABASE,
                    StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM
                };
            }
            return View("~/Plugins/Misc.AzureBlob/Views/Media.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Media(MediaSettingsModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            //load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var mediaSettings = _settingService.LoadSetting<MediaSettings>(storeScope);
            mediaSettings = model.ToEntity(mediaSettings);

            if (model.AvatarPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.AvatarPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.AvatarPictureSize, storeScope);

            if (model.ProductThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSize, storeScope);

            if (model.ProductDetailsPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductDetailsPictureSize, storeScope);

            if (model.ProductThumbPictureSizeOnProductDetailsPage_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ProductThumbPictureSizeOnProductDetailsPage,
                    storeScope);

            if (model.AssociatedProductPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.AssociatedProductPictureSize, storeScope);

            if (model.CategoryThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.CategoryThumbPictureSize, storeScope);

            if (model.ManufacturerThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.ManufacturerThumbPictureSize, storeScope);

            if (model.CartThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.CartThumbPictureSize, storeScope);

            if (model.MiniCartThumbPictureSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.MiniCartThumbPictureSize, storeScope);

            if (model.MaximumImageSize_OverrideForStore || storeScope == 0)
                _settingService.SaveSetting(mediaSettings, x => x.MaximumImageSize, storeScope, false);
            else if (storeScope > 0)
                _settingService.DeleteSetting(mediaSettings, x => x.MaximumImageSize, storeScope);

            _settingService.ClearCache();

            _customerActivityService.InsertActivity("EditSettings",
                _localizationService.GetResource("ActivityLog.EditSettings"));
            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));
            return RedirectToAction("Media");
        }

        protected virtual void SuccessNotification(string message, bool persistForTheNextRequest = true)
        {
            AddNotification(NotifyType.Success, message, persistForTheNextRequest);
        }

        protected virtual void AddNotification(NotifyType type, string message, bool persistForTheNextRequest)
        {
            string dataKey = string.Format("nop.notifications.{0}", type);
            if (persistForTheNextRequest)
            {
                if (TempData[dataKey] == null)
                    TempData[dataKey] = new List<string>();
                ((List<string>)TempData[dataKey]).Add(message);
            }
            else
            {
                if (ViewData[dataKey] == null)
                    ViewData[dataKey] = new List<string>();
                ((List<string>)ViewData[dataKey]).Add(message);
            }
        }

        protected ActionResult AccessDeniedView()
        {
            //return new HttpUnauthorizedResult();
            return RedirectToAction("AccessDenied", "Security", new { pageUrl = this.Request.RawUrl });
        }

        public virtual int GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if (storeService.GetAllStores().Count < 2)
                return 0;


            var storeId =
                workContext.CurrentCustomer.GetAttribute<int>(
                    SystemCustomerAttributeNames.AdminAreaStoreScopeConfiguration);
            var store = storeService.GetStoreById(storeId);
            return store != null ? store.Id : 0;
        }
        #endregion


    }
}
