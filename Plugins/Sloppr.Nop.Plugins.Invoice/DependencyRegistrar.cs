using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Media;
using Nop.Web.Framework.Mvc;
using Nop.Plugins.Sloppr.Invoice.Services;

namespace Nop.Plugins.Sloppr.Invoice
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<Nop.Plugins.Sloppr.Invoice.Services.PdfService>().As<Nop.Services.Common.PdfService>().InstancePerRequest();
        }

        public int Order
        {
            get { return 1; }
        }
    }
}
