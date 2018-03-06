using System.Runtime.Serialization;

namespace MicroWish.Models
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
