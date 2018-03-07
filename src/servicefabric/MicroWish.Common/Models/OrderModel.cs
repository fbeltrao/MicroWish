using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MicroWish.Models
{
    [DataContract]
    public class OrderModel : ICloneable
    {
        [DataMember]

        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        public OrderState State { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime? SubmittedDate { get; set; }

        [DataMember]
        public DateTime LastUpdate { get; set; }

        [DataMember]
        public Guid CustomerId { get; set; }

        [DataMember]
        public AddressModel DeliveryAddress { get; set; }

        [DataMember]
        public decimal Total { get; set; }

        [DataMember]
        public List<OrderItemModel> Items { get; set; }

        [DataMember]
        public PaymentModel Payment { get; set; }

        public OrderModel()
        {
            this.State = OrderState.Created;
            this.Items = new List<OrderItemModel>();
        }

        public OrderModel(OrderModel other)
        {
            this.CreationDate = other.CreationDate;
            this.CustomerId = other.CustomerId;
            this.DeliveryAddress = other.DeliveryAddress;
            this.Id = other.Id;
            this.State = other.State;
            this.SubmittedDate = other.SubmittedDate;
            this.Total = other.Total;
            this.LastUpdate = other.LastUpdate;
            this.Items = new List<OrderItemModel>();
            this.Payment = other.Payment.Clone();
            if (other.Items != null)
                this.Items.AddRange(other.Items.Select(x => x.Clone()));
        }

        public OrderModel Clone()
        {
            return new OrderModel(this);
        }

        object ICloneable.Clone()
        {
            return new OrderModel(this);
        }
    }
}
