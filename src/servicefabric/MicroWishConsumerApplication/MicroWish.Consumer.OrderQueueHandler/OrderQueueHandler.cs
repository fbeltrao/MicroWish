using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Consumer.Commands;
using MicroWish.Consumer.Configuration;
using MicroWish.Consumer.Contracts;
using MicroWish.Consumer.Events;
using MicroWish.Consumer.Models;
using MicroWish.Consumer.ServiceFabric;

namespace MicroWish.Consumer.OrderQueueHandler
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderQueueHandler : StatelessService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;

        public OrderQueueHandler(StatelessServiceContext context, ServiceBusConfiguration configuration)
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
            yield return new ServiceInstanceListener(context => new ServiceBusQueueListener(CreateOrderHandler, this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.CreateOrderQueueName), $"Queue-Listener-{this.serviceBusConfiguration.CreateOrderQueueName}");           
        }

        private void CreateOrderHandler(BrokeredMessage message)
        {
            try
            {
                var command = message.GetBody<CreateOrderCommand>();

                var order = new OrderModel()
                {
                    Id = command.OrderId,
                    CreationDate = DateTime.UtcNow,
                    State = OrderState.Created,
                    Items = command.Items,
                    DeliveryAddress = command.DeliveryAddress,                    
                };

                order.Total = order.Items.Sum(x => x.UnitPrice * x.Price);
                order.Payment = new PaymentModel()
                {
                    CreditCardNumber = command.Payment.CreditCardNumber,
                    CreditCardType = command.Payment.CreditCardType,
                    ExpirationMonth = command.Payment.ExpirationMonth,
                    ExpirationYear = command.Payment.ExpirationYear,
                    Status = PaymentStatus.Pending,
                    Value = order.Total,
                };

                // Persist order
                var orderDataService = ServiceProxyUtils.GetOrderService(order.Id);
                var createdOrder = orderDataService.Create(order);

                // Send to topic "ordercreated"
                NotifyTopic(new OrderCreatedEvent(createdOrder));
                

                // Send to queue "orderverifyinventory"
            }
            catch (Exception ex)
            {

            }
        }

        private void NotifyTopic(OrderCreatedEvent orderCreatedEvent)
        {
            //throw new NotImplementedException();
        }



        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
