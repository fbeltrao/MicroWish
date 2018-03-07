using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using MicroWish.Client;
using MicroWish.Consumer.Models;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Consumer.Client
{
    public class CatalogProductAPIClient : ICatalogProductAPIClient
    {
        private readonly FabricClient fabricClient;
        private const int MaxQueryRetryCount = 20;
        private readonly Uri serviceUri;
        private readonly HttpCommunicationClientFactory communicationFactory;
        private readonly TimeSpan backoffQueryDelay;


        public CatalogProductAPIClient(FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            backoffQueryDelay = TimeSpan.FromSeconds(3);

            this.serviceUri = new Uri("fabric:/MicroWishConsumerApplication/MicroWish.ConsumerAPI");
            communicationFactory = new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fabricClient));

        }

        public async Task<ProductCatalogModel> Get(Guid productId)
        {
            try
            {
                ServicePartitionClient<HttpCommunicationClient> partitionClient = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri);

                return await partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                    {
                        var uri = client.Url.ToString() + "api/products/" + productId.ToString();

                        HttpResponseMessage response = await client.HttpClient.GetAsync(uri);
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ProductCatalogModel>(content);
                    });
            }
            catch (Exception ex)
            {
                // Sample code: print exception
                //ServiceEventSource.Current.OperationFailed(ex.Message, "Count - run web request");
                throw;
            }

        }

        public async Task<ProductCatalogModel> Update(ProductCatalogModel product)
        {
            try
            {
                ServicePartitionClient<HttpCommunicationClient> partitionClient = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri);

                return await partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                    {
                        var uri = client.Url.ToString() + "api/products/" + product.Id.ToString();

                        var reqContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.HttpClient.PutAsync(uri, reqContent);
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ProductCatalogModel>(content);
                    });
            }
            catch (Exception ex)
            {
                // Sample code: print exception
                //ServiceEventSource.Current.OperationFailed(ex.Message, "Count - run web request");
                throw;
            }
        }

        public async Task<ProductCatalogModel> Create(ProductCatalogModel product)
        {
            try
            {
                ServicePartitionClient<HttpCommunicationClient> partitionClient = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri);

                return await partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                    {
                        var uri = client.Url.ToString() + "api/products/" + product.Id.ToString();

                        var reqContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");

                        HttpResponseMessage response = await client.HttpClient.PostAsync(uri, reqContent);
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<ProductCatalogModel>(content);
                    });
            }
            catch (Exception ex)
            {
                // Sample code: print exception
                //ServiceEventSource.Current.OperationFailed(ex.Message, "Count - run web request");
                throw;
            }
        }
    }
}
