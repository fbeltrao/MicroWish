using System;

namespace MicroWish.Models
{
    public class OrderItemModel : ICloneable
    {

        public OrderItemModel()
        {

        }

        public OrderItemModel(OrderItemModel other)
        {
            this.VendorId = other.VendorId;
            this.ProductId = other.ProductId;
            this.Quantity = other.Quantity;
            this.UnitPrice = other.UnitPrice;
        }

        public Guid VendorId { get; set; }

        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Price { get; set; }

        public OrderItemModel Clone()
        {
            return new OrderItemModel(this);
        }

        object ICloneable.Clone()
        {
            return new OrderItemModel(this);
        }
    }
}
