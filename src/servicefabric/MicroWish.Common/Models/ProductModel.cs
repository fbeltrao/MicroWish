using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Models
{
    public class ProductModel
    {
        public Guid Id { get; set; }

        public Guid VendorId { get; set; }

        public string Name { get; set; }

        public decimal Price { get; set; }
    }
}
