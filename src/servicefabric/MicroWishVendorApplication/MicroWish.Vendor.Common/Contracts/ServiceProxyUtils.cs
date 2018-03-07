using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MicroWish.ServiceFabric;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Vendor.Contracts
{
    public static class ServiceProxyUtils
    {
        public static IProductDataService GetProductService(Guid guid)
        {
            var proxy = ServiceProxy.Create<IProductDataService>(new Uri("fabric:/MicroWishVendorApplication/MicroWish.Vendor.ProductDataService"), new ServicePartitionKey(guid.GetPartitionKey()));
            return proxy;
        }


        public static async Task<IEnumerable<IProductDataService>> GetProductServicePartitions(FabricClient fabricClient)
        {
            var serviceName = new Uri("fabric:/MicroWishVendorApplication/MicroWish.Vendor.ProductDataService");
            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            var services = new List<IProductDataService>();
            foreach (var partition in partitions)
            {
                var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;
                services.Add(ServiceProxy.Create<IProductDataService>(serviceName, new ServicePartitionKey(partitionInformation.LowKey)));
            }

            return services;
        }
    }
}
