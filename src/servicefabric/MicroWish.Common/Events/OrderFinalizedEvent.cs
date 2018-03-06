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
    public class OrderFinalizedEvent
    {
        [DataMember]
        public OrderModel Order { get; set; }


        public OrderFinalizedEvent()
        {

        }

        public OrderFinalizedEvent(OrderModel order)
        {
            this.Order = order;
        }
    }
}
