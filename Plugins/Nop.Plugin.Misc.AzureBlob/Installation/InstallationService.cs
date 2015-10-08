using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Installation;

namespace Nop.Plugin.Misc.AzureBlob.Installation
{
    public class AzureBlobInstallationService : SqlFileInstallationService
    {
        private readonly IWebHelper _webHelper2;
        public AzureBlobInstallationService(IRepository<Language> languageRepository, IRepository<Customer> customerRepository, IRepository<Store> storeRepository, IDbContext dbContext, IWebHelper webHelper)
            : base(languageRepository, customerRepository, storeRepository, dbContext, webHelper)
        {
            _webHelper2 = webHelper;
        }

        public virtual void InstallData()
        {
            ExecuteSqlFile(_webHelper2.MapPath("~/Plugins/Misc.AzureBlob/sql/create_script.sql"));
        }

    }
}
