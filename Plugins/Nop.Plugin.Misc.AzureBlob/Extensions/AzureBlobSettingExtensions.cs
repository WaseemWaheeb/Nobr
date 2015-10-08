using Nop.Plugin.Misc.AzureBlob.Services;

namespace Nop.Plugin.Misc.AzureBlob.Extensions
{
    public static class AzureBlobSettingExtensions
    {
        public static bool IsPictureStorageTypeAzure(this AzureBlobSetting setting)
        {
            return setting.PictureStoreType == StorePictureService.PICTURE_STORE_TYPE_AZURE;
        }

        public static bool IsPictureStorageTypeDataBase(this AzureBlobSetting setting)
        {
            return setting.PictureStoreType == StorePictureService.PICTURE_STORE_TYPE_DATABASE;
        }

        public static bool IsPictureStorageTypeFileSystem(this AzureBlobSetting setting)
        {
            return setting.PictureStoreType == StorePictureService.PICTURE_STORE_TYPE_FILESYSTEM;
        }

    }
}
