using MicroWish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Events
{
    public class ProductChangedEvent
    {
        public ProductModel Previous { get; set; }
        public ProductModel Current { get; set; }
    }
}
