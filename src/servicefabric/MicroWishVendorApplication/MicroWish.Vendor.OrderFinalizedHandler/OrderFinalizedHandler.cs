using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Configuration;
using MicroWish.Events;
using MicroWish.ServiceBus;
using MicroWish.ServiceFabric;

namespace MicroWish.Vendor.OrderFinalizedHandler
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderFinalizedHandler : StatelessService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;

        public OrderFinalizedHandler(StatelessServiceContext context, ServiceBusConfiguration configuration)
            : base(context)
        {
            this.serviceBusConfiguration = configuration;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {

            yield return new ServiceInstanceListener(context => 
                new ServiceBusTopicListener(Handler, this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.OrderFinalizedTopicName, "MicroWish.Vendor.OrderFinalizedHandler"), $"Vendor-Topic-Listener-{this.serviceBusConfiguration.OrderFinalizedTopicName}");

        }

        private Task Handler(BrokeredMessage message)
        {
            try
            {
                var orderFinalized = message.GetJsonBody<OrderFinalizedEvent>();
                
            }
            catch (Exception ex)
            {
            }

            return Task.CompletedTask;

        }
  
    }
}
