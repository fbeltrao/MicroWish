using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    [DataContract]
    public class VendorModel : ICloneable
    {
        [DataMember]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public AddressModel Address { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime? LastUpdate { get; set; }

        public VendorModel()
        {

        }

        public VendorModel(VendorModel other)
        {
            this.Id = other.Id;
            this.Name = other.Name;
            this.Address = other.Address?.Clone();
            this.CreationDate = other.CreationDate;
            this.LastUpdate = other.LastUpdate;
        }

        public VendorModel Clone() => new VendorModel(this);

        object ICloneable.Clone() => new VendorModel(this);
    }
}
