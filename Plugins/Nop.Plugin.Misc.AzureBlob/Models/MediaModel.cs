using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.AzureBlob.Models
{
    public class MediaModel : BaseNopModel
    {
        [NopResourceDisplayName("AzureBlob.ConfigureModel.PictureStoreType")]
        public string PictureStoreType { get; set; }
    }
}
