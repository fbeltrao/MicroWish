using MicroWish.Consumer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Commands
{
    [DataContract]
    public class VerifyOrderPaymentCommand
    {
        public VerifyOrderPaymentCommand()
        {

        }

        public VerifyOrderPaymentCommand(OrderModel order)
        {
            this.Order = order;
        }

        [DataMember]
        public OrderModel Order { get; set; }
    }
}
