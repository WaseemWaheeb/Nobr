using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sloppr.Api.Cart.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public int StoreId { get; set; }
        public int CartTypeId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}