using MicroWish.Data;
using MicroWish.Models;
using System;

namespace MicroWish.Vendor.API.Data
{
    public class VendorRepository : DocumentRepository<VendorModel>
    {
        public VendorRepository(Uri endpoint, string databaseId, string collectionName, string authKey) : base(endpoint, databaseId, collectionName, authKey)
        {
        }
    }
}
