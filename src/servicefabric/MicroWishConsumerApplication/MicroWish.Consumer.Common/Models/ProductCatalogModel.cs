using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Models
{
    public class ProductCatalogModel : ICloneable
    {
        [JsonProperty("id")]
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public Guid VendorId { get; set; }

        [DataMember]
        public string VendorName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public decimal? PreviousPrice { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public bool Enabled { get; set; } = true;

        [DataMember]
        public bool MembersOnly { get; set; }

        [DataMember]
        public DateTime? LastUpdate { get; set; }

        public ProductCatalogModel()
        {
        }

        public ProductCatalogModel(ProductCatalogModel other)
        {
            this.Id = other.Id;
            this.VendorId = other.VendorId;
            this.Name = other.Name;
            this.Price = other.Price;
            this.CreationDate = other.CreationDate;
            this.LastUpdate = other.LastUpdate;
            this.Enabled = other.Enabled;
            this.MembersOnly = other.MembersOnly;
            this.PreviousPrice = other.PreviousPrice;
            this.VendorName = other.VendorName;
        }

        public ProductCatalogModel Clone() => new ProductCatalogModel(this);

        object ICloneable.Clone() => new ProductCatalogModel(this);
    }
}
