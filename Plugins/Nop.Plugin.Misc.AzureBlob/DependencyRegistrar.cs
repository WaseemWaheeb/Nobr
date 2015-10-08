using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.AzureBlob.Data;
using Nop.Plugin.Misc.AzureBlob.Domain;
using Nop.Plugin.Misc.AzureBlob.Installation;
using Nop.Plugin.Misc.AzureBlob.Services;
using Nop.Services.Media;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.AzureBlob
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_azure_blob_storage";

        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<PictureFileService>().As<IPictureFileService>().InstancePerLifetimeScope();

            //data context
            this.RegisterPluginDataContext<PictureFileObjectContext>(builder, CONTEXT_NAME);

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PictureFile>>()
                .As<IRepository<PictureFile>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME))
                .InstancePerLifetimeScope();
            builder.RegisterType<PictureFileService>().As<IPictureFileService>().InstancePerHttpRequest();
            builder.RegisterType<AzureBlobInstallationService>().As<AzureBlobInstallationService>().InstancePerHttpRequest();
            builder.RegisterType<BlobService>().As<BlobService>().InstancePerHttpRequest();
            builder.RegisterType<StoreDownloadService>().As<IDownloadService>().InstancePerHttpRequest();
            builder.RegisterType<StorePictureService>().As<IPictureService>().InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
