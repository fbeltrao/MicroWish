using MicroWish.Consumer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Client
{
    public interface ICatalogProductAPIClient
    {
        Task<ProductCatalogModel> Get(Guid productId);

        Task<ProductCatalogModel> Update(ProductCatalogModel product);

        Task<ProductCatalogModel> Create(ProductCatalogModel product);
    }
}
