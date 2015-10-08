using Nop.Web.Framework.Themes;

namespace Nop.Plugin.Misc.AzureBlob
{
    public class CustomViewEngine : ThemeableRazorViewEngine
    {
     
        public CustomViewEngine()
        {
            ViewLocationFormats = new[] { "~/Plugins/Misc.AzureBlob/Views/{0}.cshtml" };
            PartialViewLocationFormats = new[] { "~/Plugins/Misc.AzureBlob/Views/{0}.cshtml", };
        }


    }
}
