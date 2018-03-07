using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    [DataContract]
    public class NewOrderItemModel
    {
        [DataMember]
        public Guid ProductId { get; set; }

        [DataMember]
        public int Quantity { get; set; }
    }
}
