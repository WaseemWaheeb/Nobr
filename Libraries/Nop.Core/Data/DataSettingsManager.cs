using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;

namespace Nop.Core.Data
{
    /// <summary>
    /// Manager of data settings (connection string)
    /// </summary>
    public partial class DataSettingsManager
    {
        /// <summary>
        /// Load settings
        /// </summary>
        /// <returns></returns>
        public virtual DataSettings LoadSettings()
        {
            try
            {
                System.Configuration.Configuration webConfig = WebConfigurationManager.OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath);
                return new DataSettings
                {
                    DataConnectionString = webConfig.ConnectionStrings.ConnectionStrings["DefaultConnection"].ConnectionString,
                    DataProvider = webConfig.ConnectionStrings.ConnectionStrings["DefaultConnection"].ProviderName
                };
            }
            catch (NullReferenceException)
            {
                return new DataSettings();
            }
        }

        /// <summary>
        /// Save settings to a file
        /// </summary>
        /// <param name="settings"></param>
        public virtual void SaveSettings(DataSettings settings)
        {
            if (null == settings) throw new ArgumentNullException("settings");

            System.Configuration.Configuration webConfig = WebConfigurationManager.OpenWebConfiguration(HttpRuntime.AppDomainAppVirtualPath);

            webConfig.ConnectionStrings.ConnectionStrings["DefaultConnection"].ConnectionString = settings.DataConnectionString;
            webConfig.ConnectionStrings.ConnectionStrings["DefaultConnection"].ProviderName = settings.DataProvider;

            webConfig.Save();
        }
    }
}