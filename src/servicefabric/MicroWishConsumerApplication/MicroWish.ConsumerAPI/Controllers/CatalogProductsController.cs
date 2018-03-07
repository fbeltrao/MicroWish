using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MicroWish.Commands;
using MicroWish.Configuration;
using MicroWish.Consumer.Contracts;
using MicroWish.Consumer.Models;
using MicroWish.ConsumerAPI.Data;
using MicroWish.Models;
using MicroWish.ServiceBus;
using MicroWish.ServiceFabric;

namespace MicroWish.ConsumerAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/products")]
    public class CatalogProductsController : Controller
    {
        ProductCatalogRepository repository;

        public CatalogProductsController(StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var vendorDatabaseConfigSection = configurationPackage.Settings.Sections.Contains("VendorDatabase") ? configurationPackage.Settings.Sections["VendorDatabase"] : null;
            var databaseEndpoint = new Uri(vendorDatabaseConfigSection?.Parameters["Endpoint"]?.Value ?? "https://localhost:8081");
            var databaseId = vendorDatabaseConfigSection?.Parameters["DatabaseId"]?.Value ?? "microwish";
            var collectionName = vendorDatabaseConfigSection?.Parameters["CollectionName"]?.Value ?? "catalog";
            var authKey = vendorDatabaseConfigSection?.Parameters["AccessKey"]?.Value ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            this.repository = new ProductCatalogRepository(databaseEndpoint, databaseId, collectionName, authKey);          
        }
               

        // GET: api/products/5
        [HttpGet("{productId:guid}")]
        public async Task<IActionResult> Get(Guid productId)
        {
            try
            {
                var result = await this.repository.GetSingle(productId);

                return (result == null) ?
                    (IActionResult)this.NotFound() :
                    new ObjectResult(result);

            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }


        // GET: api/products
        [HttpGet]
        public async Task<IEnumerable<ProductCatalogModel>> List()
        {
            try
            {
                return await this.repository.List();
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // POST: api/products
        [HttpPost("{productId?}")]
        public async Task<IActionResult> Create(Guid? productId, [FromBody] ProductCatalogModel product)
        {
            try
            {
                if (!productId.HasValue)
                    productId = Guid.NewGuid();

                var createdVendor = product.Clone();
                createdVendor.CreationDate = DateTime.UtcNow;
                createdVendor.LastUpdate = null;
                createdVendor.Id = productId.Value;

                var savedVendor = await repository.Create(createdVendor);

                return CreatedAtAction(nameof(Get), new { vendorId = savedVendor.Id }, savedVendor);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // PUT: api/products/5
        [HttpPut("{productId}")]
        public async Task<IActionResult> Update(Guid productId, [FromBody] ProductCatalogModel product)
        {
            try
            {
                var updatedProduct = product.Clone();
                updatedProduct.Id = productId;
                updatedProduct.LastUpdate = DateTime.UtcNow;

                var savedVendor = await this.repository.Update(productId.ToString(), updatedProduct);

                return new OkObjectResult(savedVendor);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

    }
}
