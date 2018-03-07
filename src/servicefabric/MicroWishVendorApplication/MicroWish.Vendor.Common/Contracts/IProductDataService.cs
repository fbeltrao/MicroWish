using Microsoft.ServiceFabric.Services.Remoting;
using MicroWish.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Vendor.Contracts
{
    public interface IProductDataService : IService
    {
        Task<ProductModel> Create(ProductModel product);

        Task<ProductModel> Update(ProductModel product);

        Task<ProductModel> Get(Guid productId);

        Task<IEnumerable<ProductModel>> List(Guid? vendorId);        
    }
}
