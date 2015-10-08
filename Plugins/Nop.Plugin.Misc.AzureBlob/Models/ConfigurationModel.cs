using System.Collections.Generic;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.AzureBlob.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        
        [NopResourceDisplayName("AzureBlob.ConfigureModel.Account")]
        public string Account { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.Container")]
        public string Container { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.ContainerFoPictures")]
        public string ContainerForPictures { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.PictureStoreType")]
        public string PictureStoreType { get; set; }

        public List<string> PictureStoreTypeList { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.UseCDN")]
        public bool UseCDN { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.CDN")]
        public string CDN { get; set; }

        [NopResourceDisplayName("AzureBlob.ConfigureModel.DownloadStoreType")]
        public string DownloadStoreType { get; set; }
        public List<string> DownloadStoreTypeList { get; set; }
        [NopResourceDisplayName("AzureBlob.ConfigureModel.AccountKey")]
        public string AccountKey { get; set; }
        [NopResourceDisplayName("AzureBlob.ConfigureModel.AlwaysShowMainImage")]
        public bool AlwaysShowMainImage { get; set; }
        [NopResourceDisplayName("AzureBlob.ConfigureModel.CheckIfImageExist")]
        public bool CheckIfImageExist { get; set; }
    }
}
