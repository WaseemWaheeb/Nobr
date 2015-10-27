using System.Configuration;
using System.Web.Configuration;
using System.Web.Routing;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.AzureBlob.Data;
using Nop.Plugin.Misc.AzureBlob.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.AzureBlob
{
    public class AzureBlobPlugin : BasePlugin, IMiscPlugin
    {
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly PictureFileObjectContext _pictureFileObjectContext;
        private readonly IDownloadService _downloadService;
        public AzureBlobPlugin(ISettingService settingService, IPictureService pictureService, PictureFileObjectContext pictureFileObjectContext, IDownloadService downloadService)
        {
            _settingService = settingService;
            _pictureService = pictureService;
            _pictureFileObjectContext = pictureFileObjectContext;
            _downloadService = downloadService;
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
            controllerName = "MiscAzureBlob";
            routeValues = new RouteValueDictionary() { { "Namespaces", "Nop.Plugin.Misc.AzureBlob.Controllers" }, { "area", null } };
        }


        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new AzureBlobSetting
            {
                ConnectionString = "",
                Account = "",
                AccountKey = "",
                Container = "",
                PictureStoreType = _pictureService.StoreInDb ? StorePictureService.PICTURE_STORE_TYPE_DATABASE : StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM,
                DownloadStoreType = StoreDownloadService.DOWNLOAD_STORE_TYPE_DATABASE,
                UseCDN = false,
                CDN = "",
                AlwaysShowMainImage = false,
                CheckIfImageExist = false
            };
            _settingService.SaveSetting(settings);
            _pictureFileObjectContext.Install();
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.ConnectionStrings.ConnectionStrings.Remove("Nop.Plugin.Misc.AzureBlob.ConnectionString");
            config.ConnectionStrings.ConnectionStrings.Add(new ConnectionStringSettings("Nop.Plugin.Misc.AzureBlob.ConnectionString", ""));
            config.Save();
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ConnectionStringName", "Connection String Name");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ConnectionStringName.Hint", "Enter Connection String Name");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Account", "Account Name");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.AccountKey", "Account Key");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.AccountKey.Hint", "Enter Account Key");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Account.Hint", "Enter Account name");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Container.Hint", "Enter Container name");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ButtonSave", "Save");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Azure", "Azure");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Database", "Database");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.FileSystem", "File system");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ContainerFoPictures", "Container for pictures");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ContainerFoPictures.Hint", "Enter Container for pictures"); 
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.Container", "Container for files");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.NOT", "NOTE: Don not forget to backup your database before changing this option");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.PictureStoreType", "Pictures are stored into...");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.PictureStoreType.Hint", "Chose pictures storage type");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.DownloadStoreType", "Files are stored into...");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.DownloadStoreType.Hint", "Chose files storage type");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ChangeStorage", "Change");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ChangeFilesStorage", "Files storage has been changed successfully.");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.ChangePicturesStorage", "Pictures storage has been changed successfully.");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.UseCDN", "UseCDN");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.UseCDN.Hint", "Enter CDN");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.CDN", "CDN");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.AddDefaultImages", "Add Default Images");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.AlwaysShowMainImage","Always show main image");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.AlwaysShowMainImage.Hint", "Please check this checkbox if you want to improve performance. Plugin will not generate thumbnails and will return main image");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.CheckIfImageExist", "Check if image exist");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.CheckIfImageExist.Hint", "Please uncheck this checkbox if you want to improve performance. Plugin will not check existence of images and image thumbnails");
            this.AddOrUpdatePluginLocaleResource("Admin.Configuration.ValidatePictureSettings", "Please enter and save a valid Azure settings(Account Name, Key, Container Name for pictures)");
            this.AddOrUpdatePluginLocaleResource("Admin.Configuration.ValidateDownloadSettings", "Please enter and save a valid Azure settings(Account Name, Key, Container Name for files)");
            this.AddOrUpdatePluginLocaleResource("AzureBlob.ConfigureModel.UseDevAccount", "UseDevAccount");

            base.Install();
        }
        
        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AzureBlobSetting>();
            var config = WebConfigurationManager.OpenWebConfiguration("~");
            config.ConnectionStrings.ConnectionStrings.Remove("Nop.Plugin.Misc.AzureBlob.ConnectionString");
            config.Save();
            _pictureService.StoreInDb = true;
            (_downloadService as StoreDownloadService).StoreType = StoreDownloadService.DOWNLOAD_STORE_TYPE_DATABASE;
            _pictureFileObjectContext.Uninstall();
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ConnectionStringName");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ConnectionStringName.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Account");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Account.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Container");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Container.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ButtonSave");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Azure");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.Database");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.FileSystem");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ContainerFoPictures");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ContainerFoPictures.Hint"); 
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.NOT");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.PictureStoreType");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.PictureStoreType.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.DownloadStoreType");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.DownloadStoreType.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ChangeStorage");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ChangeFilesStorage");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.ChangePicturesStorage");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.UseCDN");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.UseCDN.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.CDN");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.AccountKey");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.AccountKey.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.AddDefaultImages");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.AlwaysShowMainImage");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.AlwaysShowMainImage.Hint");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.CheckIfImageExist");
            this.DeletePluginLocaleResource("AzureBlob.ConfigureModel.CheckIfImageExist.Hint");

            base.Uninstall();
        }
    }
}
