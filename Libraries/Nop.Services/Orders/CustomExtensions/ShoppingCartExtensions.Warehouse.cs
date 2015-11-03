using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Localization;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Represents a shopping cart
    /// </summary>
    public static class ShoppingCartExtensions_Warehouse
    {
        public static IEnumerable<ShoppingCartItem> LimitPerWarehouse(this IEnumerable<ShoppingCartItem> cart, int warehouseId)
        {
            //simply replace the following code with "return cart"
            //if you want to share shopping carts between warehouses

            return cart.Where(x => x.WarehouseId == warehouseId);
        }
    }
}
