using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.AzureBlob
{
    public class AzureBlobSetting : ISettings
    {
        public string ConnectionString { get; set; }
        public string Account { get; set; }
        public string Container { get; set; }
        public string ContainerForPictures { get; set; }
        public bool UseCDN { get; set; }
        public string CDN { get; set; }
        public string PictureStoreType { get; set; }
        public string DownloadStoreType { get; set; }
        public string AccountKey { get; set; }
        public string DefaultImageName { get; set; }
        public string DefaultAvatarImageName { get; set; }
        public bool AlwaysShowMainImage { get; set; }
        public bool CheckIfImageExist { get; set; }
    }
}
