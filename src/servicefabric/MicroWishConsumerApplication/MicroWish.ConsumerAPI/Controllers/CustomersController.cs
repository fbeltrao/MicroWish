using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using MicroWish.Consumer.Models;
using Newtonsoft.Json;

namespace MicroWish.ConsumerAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/customers")]
    public class CustomersController : Controller
    {
        private readonly string collectionName;
        private readonly string databaseId;
        private readonly Uri databaseEndpoint;
        private readonly string authKey;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public CustomersController(StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var customerDatabaseConfigSection = configurationPackage.Settings.Sections.Contains("CustomerDatabase") ? configurationPackage.Settings.Sections["CustomerDatabase"] : null;
            this.databaseEndpoint = new Uri(customerDatabaseConfigSection?.Parameters["Endpoint"]?.Value ?? "https://localhost:8081");
            this.databaseId = customerDatabaseConfigSection?.Parameters["DatabaseId"]?.Value ?? "microwish";
            this.collectionName = customerDatabaseConfigSection?.Parameters["CollectionName"]?.Value ?? "customer";
            this.authKey = customerDatabaseConfigSection?.Parameters["AccessKey"]?.Value ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        }


        #region Helpers

        DocumentClient CreateConnection()
        {
            DocumentClient client = new DocumentClient(
                this.databaseEndpoint,
                this.authKey
                );

            return client;
        }
        #endregion

       

        // GET: api/Customers/5
        [HttpGet("{customerId}")]
        public async Task<CustomerModel> Get(Guid customerId)
        {

            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, customerId.ToString());
                using (var client = CreateConnection())
                {
                    return await client.ReadDocumentAsync<CustomerModel>(documentUri, new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(customerId.ToString())
                    }
                    );
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // GET: api/customers
        [HttpGet]
        public async Task<IEnumerable<CustomerModel>> List()
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
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }


            return customers;
        }
                
        
        // POST: api/Customers
        [HttpPost("{customerId?}")]
        public async Task<IActionResult> Create(Guid? customerId, [FromBody] CustomerModel customer)
        {
            try
            {
                if (!customerId.HasValue)
                    customerId = Guid.NewGuid();

                var createdCustomer = customer.Clone();
                createdCustomer.CreationDate = DateTime.UtcNow;
                createdCustomer.LastUpdate = null;
                createdCustomer.Id = customerId.Value;

                Uri collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionName);
                using (var client = CreateConnection())
                {
                    await client.CreateDocumentAsync(collectionUri, createdCustomer);
                }

                return CreatedAtAction(nameof(Get), new { customerId = createdCustomer.Id }, createdCustomer);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // PUT: api/Customers/5
        [HttpPut("{customerId}")]
        public async Task<IActionResult> Update(Guid customerId, [FromBody] CustomerModel customer)
        {
            try
            {
                Uri documentUri = UriFactory.CreateDocumentUri(databaseId, collectionName, customerId.ToString());
                using (var client = CreateConnection())
                {
                    var updatedCustomer = customer.Clone();
                    updatedCustomer.Id = customerId;
                    updatedCustomer.LastUpdate = DateTime.UtcNow;
                    var response = await client.ReplaceDocumentAsync(documentUri, updatedCustomer, new RequestOptions()
                    {
                        PartitionKey = new PartitionKey(customerId.ToString())
                    });

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        CustomerModel returnCustomer = null;
                        using (var reader = new StreamReader(response.ResponseStream))
                        {
                            returnCustomer = JsonConvert.DeserializeObject<CustomerModel>(await reader.ReadToEndAsync());
                            return AcceptedAtAction(nameof(Get), new { customerId = updatedCustomer.Id }, updatedCustomer);
                        }                        
                    }
                    
                    return new ContentResult()
                    {
                        StatusCode = (int)response.StatusCode
                    };
                }
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{customerId}")]
        public void Delete(Guid customerId)
        {
            throw new NotImplementedException();
        }
    }
}
