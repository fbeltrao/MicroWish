using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using MicroWish.Contracts;
using MicroWish.Models;
using Newtonsoft.Json;
using System;
using System.Fabric;
using System.Net.Http;
using System.Threading.Tasks;

namespace MicroWish.Client
{
    public class VendorAPIClient : IVendorAPIClient
    {
        private readonly FabricClient fabricClient;
        private const int MaxQueryRetryCount = 20;
        private readonly Uri serviceUri;
        private readonly HttpCommunicationClientFactory communicationFactory;
        private readonly TimeSpan backoffQueryDelay;


        public VendorAPIClient(FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            backoffQueryDelay = TimeSpan.FromSeconds(3);

            this.serviceUri = new Uri("fabric:/MicroWishVendorApplication/MicroWish.Vendor.API");
            communicationFactory = new HttpCommunicationClientFactory(new ServicePartitionResolver(() => fabricClient));

        }

        public async Task<VendorModel> Get(Guid vendorId)
        {
            try
            {
                ServicePartitionClient<HttpCommunicationClient> partitionClient = new ServicePartitionClient<HttpCommunicationClient>(communicationFactory, serviceUri);

                return await partitionClient.InvokeWithRetryAsync(
                    async (client) =>
                    {
                        var uri = client.Url.ToString() + "api/vendors/" + vendorId.ToString();

                        HttpResponseMessage response = await client.HttpClient.GetAsync(uri);
                        string content = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<VendorModel>(content);
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
