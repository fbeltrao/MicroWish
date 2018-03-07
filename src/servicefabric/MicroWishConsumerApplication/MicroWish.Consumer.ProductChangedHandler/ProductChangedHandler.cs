using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Configuration;
using MicroWish.Consumer.Client;
using MicroWish.Consumer.Contracts;
using MicroWish.Consumer.Models;
using MicroWish.Contracts;
using MicroWish.Events;
using MicroWish.ServiceBus;
using MicroWish.ServiceFabric;

namespace MicroWish.Consumer.ProductChangedHandler
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ProductChangedHandler : StatelessService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;
        private readonly IVendorAPIClient vendorClient;
        private readonly ICatalogProductAPIClient catalogProductClient;

        public ProductChangedHandler(StatelessServiceContext context, ServiceBusConfiguration serviceBusConfiguration, IVendorAPIClient vendorClient, ICatalogProductAPIClient catalogProductClient)
            : base(context)
        {
            this.serviceBusConfiguration = serviceBusConfiguration;
            this.vendorClient = vendorClient;
            this.catalogProductClient = catalogProductClient;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context =>
                new ServiceBusTopicListener(Handler, this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.ProductChangedTopicName, "MicroWish.Consumer.ProductChangedHandler"), $"Consumer-Topic-Listener-{this.serviceBusConfiguration.ProductChangedTopicName}");
        }


        private async Task Handler(BrokeredMessage message)
        {
            try
            {
                var productChanged = message.GetJsonBody<ProductChangedEvent>();

                // Start catalog update

                // 1. Get vendor name from VendorApplication
                var vendor = await vendorClient.Get(productChanged.Current.VendorId);

                // 2. Send message to update catalog
                var isNewProductCatalog = false;
                var catalogProduct = await catalogProductClient.Get(productChanged.Current.Id);
                if (catalogProduct == null)
                {
                    isNewProductCatalog = true;
                    catalogProduct = new ProductCatalogModel()
                    {
                        Id = productChanged.Current.Id,                                                
                    };                    
                }               

                catalogProduct.Name = productChanged.Current.Name;
                catalogProduct.Enabled = productChanged.Current.Enabled;
                catalogProduct.VendorId = productChanged.Current.VendorId;
                catalogProduct.VendorName = vendor.Name;
                catalogProduct.Price = productChanged.Current.Price;
                catalogProduct.MembersOnly = productChanged.Current.MembersOnly;

                if (productChanged.Previous != null && productChanged.Previous.Price != productChanged.Current.Price)
                    catalogProduct.PreviousPrice = productChanged.Previous.Price;

                if (isNewProductCatalog)
                {
                    await catalogProductClient.Create(catalogProduct);
                }
                else
                {
                    await catalogProductClient.Update(catalogProduct);
                }


            }
            catch (Exception ex)
            {
            }
        }

    }
}
