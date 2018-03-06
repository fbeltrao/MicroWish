using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroWish.Models;
using MicroWish.Vendor.API.Data;

namespace MicroWish.Vendor.API.Controllers
{
    [Produces("application/json")]
    [Route("api/vendors")]
    public class VendorsController : Controller
    {
        private readonly VendorRepository repository;

        public VendorsController(StatelessServiceContext context)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var vendorDatabaseConfigSection = configurationPackage.Settings.Sections.Contains("VendorDatabase") ? configurationPackage.Settings.Sections["VendorDatabase"] : null;
            var databaseEndpoint = new Uri(vendorDatabaseConfigSection?.Parameters["Endpoint"]?.Value ?? "https://localhost:8081");
            var databaseId = vendorDatabaseConfigSection?.Parameters["DatabaseId"]?.Value ?? "microwish";
            var collectionName = vendorDatabaseConfigSection?.Parameters["CollectionName"]?.Value ?? "vendor";
            var authKey = vendorDatabaseConfigSection?.Parameters["AccessKey"]?.Value ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";


            this.repository = new VendorRepository(databaseEndpoint, databaseId, collectionName, authKey);

        }

        // GET: api/vendors/5
        [HttpGet("{vendorId}")]
        public async Task<IActionResult> Get(Guid vendorId)
        {

            try
            {
                var result = await this.repository.GetSingle(vendorId, vendorId.ToString());

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


        // GET: api/customers
        [HttpGet]
        public async Task<IEnumerable<VendorModel>> List()
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

        // POST: api/vendor
        [HttpPost("{vendorId?}")]
        public async Task<IActionResult> Create(Guid? vendorId, [FromBody] VendorModel vendor)
        {
            try
            {
                if (!vendorId.HasValue)
                    vendorId = Guid.NewGuid();

                var createdVendor = vendor.Clone();
                createdVendor.CreationDate = DateTime.UtcNow;
                createdVendor.LastUpdate = null;
                createdVendor.Id = vendorId.Value;

                var savedVendor = await repository.Create(createdVendor, vendorId.Value.ToString());                

                return CreatedAtAction(nameof(Get), new { vendorId = savedVendor.Id }, savedVendor);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // PUT: api/Customers/5
        [HttpPut("{vendorId}")]
        public async Task<IActionResult> Update(Guid vendorId, [FromBody] VendorModel vendor)
        {
            try
            {
                var updatedCustomer = vendor.Clone();
                updatedCustomer.Id = vendorId;
                updatedCustomer.LastUpdate = DateTime.UtcNow;

                var savedVendor = await this.repository.Update(vendorId.ToString(), vendorId.ToString(), updatedCustomer);

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
