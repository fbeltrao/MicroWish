using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using MicroWish.Common.Client;
using MicroWish.Common.Models;
using MicroWish.Order.Contract.Client;
using MicroWish.Order.Contract.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Query;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroWish.Order.Business.Client
{
    public class ReliableCollectionOrderServiceClient : IOrderServiceClient
    {
        private const int MaxQueryRetryCount = 20;
        private readonly Uri serviceUri;
        private readonly FabricClient fabricClient;
        private readonly HttpCommunicationClientFactory communicationFactory;
        private readonly TimeSpan backoffQueryDelay;

        public ReliableCollectionOrderServiceClient(FabricClient fabricClient)
        {
            backoffQueryDelay = TimeSpan.FromSeconds(3);

            this.serviceUri = new Uri(FabricRuntime.GetActivationContext().ApplicationName + "/MicroWish.Order.API");
            this.fabricClient = fabricClient;
            communicationFactory = new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fabricClient));

        }
       
        /// <summary>
        /// Gets the partition key which serves the specified word.
        /// Note that the sample only accepts Int64 partition scheme. 
        /// </summary>
        /// <param name="word">The word that needs to be mapped to a service partition key.</param>
        /// <returns>A long representing the partition key.</returns>
        private static long GetPartitionKey(Guid guid)
        {
            var first = guid.ToString().ToUpperInvariant().First();
            var offset = first - '0';
            if (offset <= 9)
            {
                return offset;
            }

            return first - 'A' + 10;
        }


        public async Task<OrderModel> Book(Guid orderId, AddressModel deliveryAddress)
        {
            // Determine the partition key that should handle the request
            long partitionKey = GetPartitionKey(orderId);

            ServicePartitionClient<HttpCommunicationClient> partitionClient
                = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri, new ServicePartitionKey(partitionKey));


            var orderModel = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var uri = client.Url.ToString() + "/api/order/" + orderId.ToString() + "/book";
                var payload = JsonConvert.SerializeObject(deliveryAddress);
                var res = await client.HttpClient.PostAsync(uri, new StringContent(payload, Encoding.UTF8, "application/json"));
                if (res.IsSuccessStatusCode)
                {
                    var responsePayload = await res.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrderModel>(responsePayload);
                }

                return null;
            });

            return orderModel;
        }


        public async Task<OrderModel> CreateOrder(Guid orderId, Guid customerId, IEnumerable<OrderItemModel> items)
        {
            // Determine the partition key that should handle the request
            long partitionKey = GetPartitionKey(orderId);

            ServicePartitionClient<HttpCommunicationClient> partitionClient
                = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri, new ServicePartitionKey(partitionKey));


            var orderModel = await partitionClient.InvokeWithRetryAsync(async (client) =>
            {
                var uri = client.Url.ToString() + "/api/order/" + orderId.ToString() + "?customerId=" + customerId.ToString();
                var payload = JsonConvert.SerializeObject(items);
                var res = await client.HttpClient.PostAsync(uri, new StringContent(payload, Encoding.UTF8, "application/json"));
                if (res.IsSuccessStatusCode)
                {
                    var responsePayload = await res.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<OrderModel>(responsePayload);
                }

                return null;
            });

            return orderModel;
        }


        public async Task<OrderModel> Get(Guid orderId)
        {
            try
            {
                var partitionKey = GetPartitionKey(orderId);
                ServicePartitionClient<HttpCommunicationClient> partitionClient
                    = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri, new ServicePartitionKey(partitionKey));

                return await partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                    {
                        var uri = client.Url.ToString() + "/api/order/" + orderId.ToString();

                        HttpResponseMessage response = await client.HttpClient.GetAsync(uri);
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<OrderModel>(content);
                    });
            }
            catch (Exception ex)
            {
                // Sample code: print exception
                //ServiceEventSource.Current.OperationFailed(ex.Message, "Count - run web request");
                throw;
            }
            
        }

        public async Task<IEnumerable<OrderModel>> GetPending()
        {
            var pendingOrders = new List<OrderModel>();

            try
            {

                var partitions = await GetServicePartitionKeysAsync();

                foreach (var partition in partitions)
                {
                    var partitionClient = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri, new ServicePartitionKey(partition.LowKey));
                    var localPendingOrders = await partitionClient.InvokeWithRetryAsync(
                       async (client) =>
                       {
                           var uri = client.Url.ToString() + "/api/order/pending";

                           HttpResponseMessage response = await client.HttpClient.GetAsync(uri);
                           string content = await response.Content.ReadAsStringAsync();
                           return JsonConvert.DeserializeObject<IEnumerable<OrderModel>>(content);
                       
                       });

                    if (localPendingOrders != null)
                        pendingOrders.AddRange(localPendingOrders);
                }

                return pendingOrders;

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Returns a list of service partition clients pointing to one key in each of the WordCount service partitions.
        /// The returned representative key is the min key served by each partition.
        /// </summary>
        /// <returns>The service partition clients pointing at a key in each of the WordCount service partitions.</returns>
        private async Task<IList<Int64RangePartitionInformation>> GetServicePartitionKeysAsync()
        {
            for (int i = 0; i < MaxQueryRetryCount; i++)
            {
                try
                {
                    // Get the list of partitions up and running in the service.
                    ServicePartitionList partitionList = await fabricClient.QueryManager.GetPartitionListAsync(serviceUri);

                    // For each partition, build a service partition client used to resolve the low key served by the partition.
                    IList<Int64RangePartitionInformation> partitionKeys = new List<Int64RangePartitionInformation>(partitionList.Count);
                    foreach (Partition partition in partitionList)
                    {
                        Int64RangePartitionInformation partitionInfo = partition.PartitionInformation as Int64RangePartitionInformation;
                        if (partitionInfo == null)
                        {
                            throw new InvalidOperationException(
                                string.Format(
                                    "The service {0} should have a uniform Int64 partition. Instead: {1}",
                                    serviceUri.ToString(),
                                    partition.PartitionInformation.Kind));
                        }

                        partitionKeys.Add(partitionInfo);
                    }

                    return partitionKeys;
                }
                catch (FabricTransientException ex)
                {
                    //ServiceEventSource.Current.OperationFailed(ex.Message, "create representative partition clients");
                    if (i == MaxQueryRetryCount - 1)
                    {
                        throw;
                    }
                }

                await Task.Delay(backoffQueryDelay);
            }

            throw new TimeoutException("Retry timeout is exhausted and creating representative partition clients wasn't successful");
        }
    



    }
}
