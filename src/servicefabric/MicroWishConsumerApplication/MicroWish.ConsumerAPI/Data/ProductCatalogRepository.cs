using MicroWish.Consumer.Models;
using MicroWish.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroWish.ConsumerAPI.Data
{
    public class ProductCatalogRepository : DocumentRepository<ProductCatalogModel>
    {
        public ProductCatalogRepository(Uri endpoint, string databaseId, string collectionName, string authKey) : base(endpoint, databaseId, collectionName, authKey)
        {
        }
    }
}
