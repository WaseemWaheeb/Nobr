using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Widgets.SocialLinks.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
    }
}