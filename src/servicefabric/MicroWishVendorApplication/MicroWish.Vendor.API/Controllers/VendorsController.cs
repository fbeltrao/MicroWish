using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroWish.Models;
using MicroWish.Vendor.API.Data;
using MicroWish.Vendor.Contracts;

namespace MicroWish.Vendor.API.Controllers
{
    [Produces("application/json")]
    [Route("api/vendors")]
    public class VendorsController : Controller
    {
        private readonly VendorRepository repository;
        private readonly FabricClient fabricClient;

        public VendorsController(StatelessServiceContext context, FabricClient fabricClient)
        {
            var configurationPackage = context.CodePackageActivationContext.GetConfigurationPackageObject("Config");

            var vendorDatabaseConfigSection = configurationPackage.Settings.Sections.Contains("VendorDatabase") ? configurationPackage.Settings.Sections["VendorDatabase"] : null;
            var databaseEndpoint = new Uri(vendorDatabaseConfigSection?.Parameters["Endpoint"]?.Value ?? "https://localhost:8081");
            var databaseId = vendorDatabaseConfigSection?.Parameters["DatabaseId"]?.Value ?? "microwish";
            var collectionName = vendorDatabaseConfigSection?.Parameters["CollectionName"]?.Value ?? "vendor";
            var authKey = vendorDatabaseConfigSection?.Parameters["AccessKey"]?.Value ?? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";


            this.repository = new VendorRepository(databaseEndpoint, databaseId, collectionName, authKey);
            this.fabricClient = fabricClient;
        }

        #region Vendors
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
        #endregion

        #region Products

        // GET: api/vendors/5/products
        [HttpGet("{vendorId}/products/{productId}")]
        public async Task<IActionResult> Product(Guid vendorId, Guid productId)
        {

            try
            {
                var product = await ServiceProxyUtils.GetProductService(productId).Get(productId);
                return product == null ?
                    (IActionResult)NotFound() :
                    new OkObjectResult(product);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // GET: api/vendors/5/products
        [HttpGet("{vendorId}/products")]
        public async Task<IActionResult> Products(Guid vendorId)
        {

            try
            {
                var products = new List<ProductModel>();
                var productServices = await ServiceProxyUtils.GetProductServicePartitions(fabricClient);
                foreach (var productService in productServices)
                {
                    products.AddRange(await productService.List(vendorId));
                }


                return new OkObjectResult(products);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // POST: api/vendors/1/product/3
        [HttpPost("{vendorId}/products/{productId?}")]
        public async Task<IActionResult> CreateProduct(Guid vendorId, Guid? productId, [FromBody] ProductModel productModel)
        {
            try
            {
                if (!productId.HasValue)
                    productId = Guid.NewGuid();

                var createdProduct = productModel.Clone();
                createdProduct.VendorId = vendorId;
                createdProduct.CreationDate = DateTime.UtcNow;
                createdProduct.LastUpdate = null;
                createdProduct.Id = productId.Value;

                var savedProduct = await ServiceProxyUtils.GetProductService(productId.Value).Create(createdProduct);

                return CreatedAtAction(nameof(Product), new { vendorId = vendorId, productId = savedProduct.Id }, savedProduct);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        // PUT: api/vendors/5/products/
        [HttpPut("{vendorId}/products/{productId}")]
        public async Task<IActionResult> UpdateProduct(Guid vendorId, Guid productId, [FromBody] ProductModel product)
        {
            try
            {
                var updatedProduct = product.Clone();
                updatedProduct.Id = productId;
                updatedProduct.VendorId = vendorId;
                updatedProduct.LastUpdate = DateTime.UtcNow;

                var savedProduct = await ServiceProxyUtils.GetProductService(productId).Update(updatedProduct);

                return new OkObjectResult(savedProduct);
            }
            catch (Exception ex)
            {
                // TODO: handle error
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion

    }
}
