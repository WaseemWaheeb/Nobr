using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sloppr.Api.Cart.Models
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public string StoreId { get; set; }
        public string CartTypeId { get; set; }
        public string CustomerId { get; set; }
        public string ProductId { get; set; }
    }
}