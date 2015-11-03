using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
//using Nop.Data;
using Nop.Services.Media;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc;
using Sloppr.Nop.Plugins.ShopByWarehouse.Controllers;

namespace Sloppr.Nop.Plugins.ShopByWarehouse
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            builder.RegisterType<WarehouseShoppingCartController>().As<ShoppingCartController>();
            builder.RegisterType<WarehouseCheckoutController>().As<CheckoutController>();
        }

        public int Order
        {
            get { return 99; }
        }
    }
}
