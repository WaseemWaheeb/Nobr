using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.SocialLinks.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [DisplayName("Facebook page URL")]
        public string FacebookUrl { get; set; }

        [DisplayName("Twitter page URL")]
        public string TwitterUrl { get; set; }
    }
}