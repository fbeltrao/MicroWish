using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using MicroWish.Customer.Contract.Models;
using MicroWish.Customer.Contract.Services;

namespace MicroWish.Customer.Service
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class Service : StatelessService, ICustomerService
    {
        private readonly string collectionName;
        private readonly string databaseId;
        private readonly Uri databaseEndpoint;
        private readonly string authKey;

        public Service(StatelessServiceContext context)
            : base(context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var customerDatabaseConfigSection = configurationPackage.Settings.Sections.Contains("CustomerDatabase") ? configurationPackage.Settings.Sections["CustomerDatabase"] : null;
            this.databaseEndpoint = new Uri(customerDatabaseConfigSection?.Parameters["Endpoint"]?.Value ?? "https://localhost:8081");
            this.databaseId = customerDatabaseConfigSection?.Parameters["DatabaseId"]?.Value ?? "microwish";
            this.collectionName = customerDatabaseConfigSection?.Parameters["CollectionName"]?.Value ?? "customer";
            this.authKey = customerDatabaseConfigSection?.Parameters["AccessKey"]?.Value ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        }
        
        DocumentClient CreateConnection()
        {
            DocumentClient client = new DocumentClient(
                this.databaseEndpoint,
                this.authKey
                );

            return client;
        }

        public async Task<CustomerModel> Create(Guid id, CustomerModel customer)
        {
            try
            {
                var createdCustomer = customer.Clone();
                createdCustomer.CreationDate = DateTime.UtcNow;
                createdCustomer.Id = id;

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
                using (var client = CreateConnection())
                {
                    await client.CreateDocumentAsync(collectionUri, createdCustomer);
                }

                return createdCustomer;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CustomerModel> Get(Guid id)
        {

            Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, id.ToString());
            using (var client = CreateConnection())
            {
                return await client.ReadDocumentAsync<CustomerModel>(documentUri);
            }
        }

        public async Task<IList<CustomerModel>> List()
        {
            var customers = new List<CustomerModel>();


            try
            {
                using (var client = CreateConnection())
                {
                    var query = client.CreateDocumentQuery<CustomerModel>(UriFactory.CreateDocumentCollectionUri(databaseId, collectionName)).AsDocumentQuery();
                    while (query.HasMoreResults)
                    {
                        customers.AddRange(await query.ExecuteNextAsync<CustomerModel>());
                    }
                }
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Write(ex.Message);
            }


            return customers;
        }

        public async Task<CustomerModel> Update(CustomerModel customer)
        {
            Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, customer.Id.ToString());
            using (var client = CreateConnection())
            {
                var updatedCustomer = customer.Clone();
                updatedCustomer.LastUpdate = DateTime.UtcNow;
                await client.ReplaceDocumentAsync(documentUri, updatedCustomer);

                return updatedCustomer;
            }
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[] { new ServiceInstanceListener(this.CreateServiceRemotingListener) };
        }

        ///// <summary>
        ///// This is the main entry point for your service instance.
        ///// </summary>
        ///// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        //protected override async Task RunAsync(CancellationToken cancellationToken)
        //{
        //    // TODO: Replace the following sample code with your own logic 
        //    //       or remove this RunAsync override if it's not needed in your service.

        //    long iterations = 0;

        //    while (true)
        //    {
        //        cancellationToken.ThrowIfCancellationRequested();

        //        ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

        //        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        //    }
        //}
    }
}
