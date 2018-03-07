using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Commands;
using MicroWish.Configuration;
using MicroWish.Consumer.Client;
using MicroWish.Consumer.Contracts;
using MicroWish.Events;
using MicroWish.Models;
using MicroWish.ServiceBus;
using MicroWish.ServiceFabric;

namespace MicroWish.Consumer.OrderCreatedHandler
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderCreatedHandler : StatelessService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;
        private readonly ICatalogProductAPIClient catalogProductAPIClient;

        public OrderCreatedHandler(StatelessServiceContext context, ServiceBusConfiguration configuration, ICatalogProductAPIClient catalogProductAPIClient)
            : base(context)
        {
            this.serviceBusConfiguration = configuration;
            this.catalogProductAPIClient = catalogProductAPIClient;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context => new ServiceBusQueueListener(CreateOrderHandler, this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.CreateOrderQueueName), $"Queue-Listener-{this.serviceBusConfiguration.CreateOrderQueueName}");

        }


        private async Task CreateOrderHandler(BrokeredMessage message)
        {
            try
            {
                var command = message.GetJsonBody<CreateOrderCommand>();

                var orderItems = new List<OrderItemModel>();
                foreach (var item in command.Items)
                {
                    var product = await this.catalogProductAPIClient.Get(item.ProductId);
                    if (product == null)
                    {
                        await NotifyTopic(this.serviceBusConfiguration.OrderCreationFailedTopicName, new OrderCreationFailedEvent()
                        {
                            OrderId = command.OrderId,
                            Reason = $"Product {item.ProductId} not found",
                        });
                    }

                    var orderItem = new OrderItemModel()
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price,
                        Price = product.Price * item.Quantity,
                        VendorId = product.VendorId
                    };
                    orderItems.Add(orderItem);
                }

                var order = new OrderModel()
                {
                    Id = command.OrderId,
                    CreationDate = DateTime.UtcNow,
                    State = OrderState.Created,
                    Items = orderItems,
                    DeliveryAddress = command.DeliveryAddress,
                };

                order.Total = order.Items.Sum(x => x.Price);
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
                var createdOrder = await orderDataService.Create(order);

                // Send to topic "ordercreated"
                await NotifyTopic(this.serviceBusConfiguration.OrderCreatedTopicName, new OrderCreatedEvent(createdOrder));

                // Send to queue "orderverifypayment"
                await SendCommand(this.serviceBusConfiguration.VerifyOrderPaymentQueueName, new VerifyOrderPaymentCommand(createdOrder));
            }
            catch (Exception ex)
            {
                await message.AbandonAsync();
            }
        }

        private async Task SendCommand(string queueName, object command)
        {
            var queueClient = QueueClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, queueName);
            await queueClient.SendAsync(BrokeredMessageFactory.CreateJsonMessage(command));
        }

        private async Task NotifyTopic(string topicName, object @event)
        {
            var client = TopicClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.OrderCreatedTopicName);
            var msg = BrokeredMessageFactory.CreateJsonMessage(@event);
                       
            await client.SendAsync(msg);
        }      
    }
}
