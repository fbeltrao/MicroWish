using MicroWish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Commands
{
    [DataContract]
    public class CreateOrderCommand
    {
        [DataMember]
        public Guid OrderId { get; set; }

        [DataMember]
        public Guid CustomerId { get; set; }

        [DataMember]
        public List<OrderItemModel> Items { get; set; }

        [DataMember]
        public AddressModel DeliveryAddress { get; set; }

        [DataMember]
        public PendingPaymentModel Payment { get; set; }
    }
}
