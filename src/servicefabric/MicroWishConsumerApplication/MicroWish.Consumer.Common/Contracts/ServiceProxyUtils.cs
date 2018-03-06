using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MicroWish.Consumer.ServiceFabric;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Contracts
{
    public static class ServiceProxyUtils
    {
        public static IOrderDataService GetOrderService(Guid guid)
        {
            var proxy = ServiceProxy.Create<IOrderDataService>(new Uri("fabric:/MicroWishConsumerApplication/MicroWish.Consumer.OrderDataService"), new ServicePartitionKey(guid.GetPartitionKey()));
            return proxy;
        }


        public static async Task<IEnumerable<IOrderDataService>> GetOrderServicePartitions(FabricClient fabricClient)
        {
            var serviceName = new Uri("fabric:/MicroWishConsumerApplication/MicroWish.Consumer.OrderDataService");
            var partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            var services = new List<IOrderDataService>();
            foreach (var partition in partitions)
            {                
                var partitionInformation = (Int64RangePartitionInformation)partition.PartitionInformation;
                services.Add(ServiceProxy.Create<IOrderDataService>(serviceName, new ServicePartitionKey(partitionInformation.LowKey)));
            }

            return services;
        }
    }
}
