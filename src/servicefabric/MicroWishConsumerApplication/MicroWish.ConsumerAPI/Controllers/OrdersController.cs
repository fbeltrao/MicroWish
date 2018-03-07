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
using MicroWish.Models;
using MicroWish.ServiceBus;
using MicroWish.ServiceFabric;

namespace MicroWish.ConsumerAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/orders")]
    public class OrdersController : Controller
    {
        private readonly ServiceBusConfiguration serviceBusConfiguration;
        private readonly FabricClient fabricClient;

        public OrdersController(ServiceBusConfiguration serviceBusConfiguration, FabricClient fabricClient)
        {
            this.serviceBusConfiguration = serviceBusConfiguration;
            this.fabricClient = fabricClient;
        }
        

        // POST: api/orders
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            if (command == null)
                return this.BadRequest("Invalid commmand, ensure body has json payload");

            if (command.OrderId == Guid.Empty)
                command.OrderId = Guid.NewGuid();

            var queueClient = QueueClient.CreateFromConnectionString(this.serviceBusConfiguration.ConnectionString, this.serviceBusConfiguration.CreateOrderQueueName);
            await queueClient.SendAsync(BrokeredMessageFactory.CreateJsonMessage(command));
            return CreatedAtAction(nameof(Get), new { orderId = command.OrderId }, command.OrderId);
        }

        // GET: api/orders/5
        [HttpGet("{orderId:guid}")]
        public async Task<IActionResult> Get(Guid orderId)
        {
            try
            {
                var orderDataService = ServiceProxyUtils.GetOrderService(orderId);
                var order = await orderDataService.Get(orderId);
                return order == null ?
                    (IActionResult)this.NotFound() :
                    this.Ok(order);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is OrderNotFoundException)
                    return this.NotFound();

                throw;
            }
        }

        // GET: api/orders/pending
        [HttpGet("pending")]
        public async Task<IActionResult> PendingOrders()
        {
            var pendingOrders = new List<OrderModel>();

            try
            {
                foreach (var orderDataService in await ServiceProxyUtils.GetOrderServicePartitions(this.fabricClient))
                {
                    var orders = await orderDataService.GetPending();
                    if (orders != null)
                        pendingOrders.AddRange(orders);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return new OkObjectResult(pendingOrders);
        }
    }
}
