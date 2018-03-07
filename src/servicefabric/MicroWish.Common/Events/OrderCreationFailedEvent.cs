using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Events
{
    [DataContract]
    public class OrderCreationFailedEvent
    {
        [DataMember]
        public Guid OrderId { get; set; }

        [DataMember]
        public string Reason { get; set; }
    }
}
