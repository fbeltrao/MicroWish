using MicroWish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Events
{
    [DataContract]
    public class OrderPaymentFailedEvent
    {
        public OrderPaymentFailedEvent()
        {
        }

        public OrderPaymentFailedEvent(OrderModel order)
        {
            this.Order = order;
        }

        [DataMember]
        public OrderModel Order { get; set; }
    }
}
