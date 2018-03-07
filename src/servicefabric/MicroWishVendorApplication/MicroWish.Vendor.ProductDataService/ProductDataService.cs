using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Configuration;
using MicroWish.Events;
using MicroWish.Models;
using MicroWish.ServiceBus;
using MicroWish.Vendor.Contracts;

namespace MicroWish.Vendor.ProductDataService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductDataService : StatefulService, IProductDataService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;

        public ProductDataService(StatefulServiceContext context, ServiceBusConfiguration serviceBusConfiguration)
            : base(context)
        {
            this.serviceBusConfiguration = serviceBusConfiguration;
        }

        async Task<IReliableDictionary<Guid, ProductModel>> ProductDictionary() => await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, ProductModel>>("products");


        public async Task<ProductModel> Create(ProductModel product)
        {
            var products = await ProductDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {

                var createdProduct = product.Clone();
                if (createdProduct.Id == Guid.Empty)
                    createdProduct.Id = Guid.NewGuid();
                createdProduct.CreationDate = DateTime.UtcNow;

                await products.AddAsync(tx, createdProduct.Id, createdProduct);

                await tx.CommitAsync();

                await this.RaiseEvent(serviceBusConfiguration.ProductChangedTopicName, new ProductChangedEvent() { Current = createdProduct });

                return product;
            }
        }

        private async Task RaiseEvent(string productChangedTopicName, ProductChangedEvent productChangedEvent)
        {
            var topicClient = TopicClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.ProductChangedTopicName);
            await topicClient.SendAsync(BrokeredMessageFactory.CreateJsonMessage(productChangedEvent));
        }

        public async Task<ProductModel> Update(ProductModel product)
        {
            var products = await ProductDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var existingProduct = await products.TryGetValueAsync(tx, product.Id);

                var updatedProduct = product.Clone();
                if (existingProduct.HasValue)
                {
                    updatedProduct.VendorId = existingProduct.Value.VendorId;
                    updatedProduct.Id = existingProduct.Value.Id;
                }

                updatedProduct.LastUpdate = DateTime.UtcNow;
                await products.SetAsync(tx, updatedProduct.Id, updatedProduct);

                await tx.CommitAsync();

                await this.RaiseEvent(serviceBusConfiguration.ProductChangedTopicName, new ProductChangedEvent() { Previous = existingProduct.Value, Current = updatedProduct });


                return updatedProduct;
            }
        }

        public async Task<ProductModel> Get(Guid productId)
        {
            var products = await ProductDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var product = await products.TryGetValueAsync(tx, productId);
                if (!product.HasValue)
                    throw new ProductNotFoundException();

                return product.Value;
            }
        }

        public async Task<IEnumerable<ProductModel>> List(Guid? vendorId)
        {
            var result = new List<ProductModel>();
            var cts = new CancellationToken();
            var products = await ProductDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var enumerable = await products.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                var enumerator = enumerable.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(cts))
                {
                    var product = enumerator.Current.Value;
                    if (product != null && (!vendorId.HasValue || vendorId.Value == product.VendorId))
                    {
                        result.Add(product);
                    }
                }
            }

            return result;
        }

        #region Not important code       
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            // enable remoting in this service
            yield return new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context));
        }
        #endregion
    }
}
