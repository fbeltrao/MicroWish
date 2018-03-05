using MicroWish.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Customer.Contract.Models
{
    [DataContract]
    public class CustomerAddressModel : AddressModel
    {
        public CustomerAddressModel()
        {
        }

        public CustomerAddressModel(CustomerAddressModel other) : base(other)
        {
            this.IsActual = other.IsActual;
        }

        [DataMember]
        public bool IsActual { get; set; }


        public override AddressModel Clone()
        {
            return new CustomerAddressModel(this);
        }
    }
}
