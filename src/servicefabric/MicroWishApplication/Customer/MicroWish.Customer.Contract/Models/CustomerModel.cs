using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Customer.Contract.Models
{
    [DataContract]
    public class CustomerModel : ICloneable
    {
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime LastUpdate { get; set; }


        [DataMember]
        public List<CustomerAddressModel> Addresses { get; set; }

        public CustomerModel()
        {
            this.Addresses = new List<CustomerAddressModel>();
        }

        public CustomerModel(CustomerModel other)
        {
            this.CreationDate = other.CreationDate;
            this.FirstName = other.FirstName;
            this.LastName = other.LastName;
            this.Id = other.Id;
            this.Email = other.Email;
            this.LastUpdate = other.LastUpdate;
            this.Addresses = new List<CustomerAddressModel>();
            if (other.Addresses != null)
                this.Addresses.AddRange(other.Addresses.Select(x => (CustomerAddressModel)x.Clone()));
        }

        public CustomerModel Clone()
        {
            return new CustomerModel(this);
        }

        object ICloneable.Clone()
        {
            return new CustomerModel(this);
        }
    }
}
