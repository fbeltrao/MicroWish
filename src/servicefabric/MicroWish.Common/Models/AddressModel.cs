using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    [DataContract]
    public class AddressModel : ICloneable
    {
        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string ZipCode { get; set; }

        [DataMember]
        public string City { get; set; }

        public AddressModel()
        {
        }

        public AddressModel(AddressModel other)
        {
            this.Address = other.Address;
            this.ZipCode = other.ZipCode;
            this.City = other.City;
        }

        public virtual AddressModel Clone()
        {
            return new AddressModel(this);
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }
    }
}
