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

namespace MicroWish.Consumer.OrderPaymentHandler
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderPaymentHandler : StatelessService
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;


        public OrderPaymentHandler(StatelessServiceContext context, ServiceBusConfiguration configuration)
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
            yield return new ServiceInstanceListener(context => new ServiceBusQueueListener(CreateOrderPaymentHandler, this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.VerifyOrderPaymentQueueName), $"Queue-Listener-{this.serviceBusConfiguration.VerifyOrderPaymentQueueName}");
        }

        private async Task CreateOrderPaymentHandler(BrokeredMessage message)
        {
            try
            {
                var command = message.GetBody<VerifyOrderPaymentCommand>();

                if (command.Order.State == OrderState.Created &&
                    command.Order.Payment != null &&
                    command.Order.Payment.Status == PaymentStatus.Pending)
                {
                    if (command.Order.Payment.ExpirationYear < DateTime.UtcNow.Year)
                    {
                        // failed
                        await OrderPaymentFailed(command.Order, "invalid payment year");
                    }
                    else
                    {
                        // succeeded
                        Random r = new Random();
                        await Task.Delay(1000 * r.Next(10));

                        await OrderSucceeded(command.Order);
                    }
                }
            }
            catch (Exception ex)
            {
                await message.AbandonAsync();
            }
        }

        private async Task OrderSucceeded(OrderModel order)
        {
            // Persist order
            var orderDataService = ServiceProxyUtils.GetOrderService(order.Id);
            var updatedOrder = order.Clone();
            updatedOrder.State = OrderState.Finalized;
            updatedOrder.Payment.ProcessingDate = DateTime.UtcNow;
            updatedOrder.Payment.Status = PaymentStatus.Paid;

            updatedOrder = await orderDataService.Update(updatedOrder);            

            // Send to topic "order finalized"
            await NotifyTopic(new OrderFinalizedEvent(updatedOrder), this.serviceBusConfiguration.OrderFinalizedTopicName);
        }

        private async Task OrderPaymentFailed(OrderModel order, string v)
        {
            // Persist order
            var orderDataService = ServiceProxyUtils.GetOrderService(order.Id);
            var updatedOrder = order.Clone();
            updatedOrder.State = OrderState.Failed;
            updatedOrder.Payment.ProcessingDate = DateTime.UtcNow;
            updatedOrder.Payment.Status = PaymentStatus.Failed;

            updatedOrder = await orderDataService.Update(updatedOrder);

            // Send to topic "order payment failed"
            await NotifyTopic(new OrderFinalizedEvent(updatedOrder), this.serviceBusConfiguration.OrderPaymentFailedTopicName);
        }

        private async Task SendCommand(string queueName, object command)
        {
            var queueClient = QueueClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, queueName);
            await queueClient.SendAsync(new BrokeredMessage(command));
        }

        private async Task NotifyTopic(object @event, string topicName)
        {
            var client = TopicClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, topicName);
            await client.SendAsync(new BrokeredMessage(@event));
        }
    }
}
