using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using MicroWish.Common.Models;
using MicroWish.Order.Contract.Data;
using MicroWish.Order.Contract.Models;
using MicroWish.Order.Contract.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicroWish.Order.Business.Services
{
    public class ReliableCollectionOrderService : IOrderService
    {
        private readonly IReliableStateManager stateManager;

        public ReliableCollectionOrderService(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        async Task<IReliableDictionary<Guid, OrderModel>> OrderDictionary() => await stateManager.GetOrAddAsync<IReliableDictionary<Guid, OrderModel>>("orders");


        public async Task<OrderModel> Book(Guid orderId, AddressModel deliveryAddress)
        {
            var orders = await OrderDictionary();
            using (var tx = stateManager.CreateTransaction())
            {
                var order = await orders.TryGetValueAsync(tx, orderId);
                if (!order.HasValue)
                    throw new OrderNotFoundException();

                if (order.Value.State != OrderState.Created)
                    throw new OrderInvalidStateException($"invalid order state {order.Value.State}");

                if (!order.Value.Items.Any())
                    throw new OrderInvalidStateException("order has no items");

                var updatedOrder = order.Value.Clone();
                updatedOrder.LastUpdate = DateTime.UtcNow;
                updatedOrder.DeliveryAddress = deliveryAddress;
                updatedOrder.Total = updatedOrder.Items.Sum(x => x.Quantity * x.UnitPrice);
                updatedOrder.State = OrderState.Finalized;

                await orders.SetAsync(tx, orderId, updatedOrder);
                await tx.CommitAsync();
                return updatedOrder;
            }                
        }

        public async Task<OrderModel> CreateOrder(Guid orderId, Guid customerId, IEnumerable<OrderItemModel> items)
        {
            var orders = await OrderDictionary();
            using (var tx = stateManager.CreateTransaction())
            {
                var newOrder = new OrderModel()
                {
                    Id = orderId,
                    CreationDate = DateTime.UtcNow,
                    State = OrderState.Created,
                    CustomerId = customerId,
                };

                newOrder.Items.AddRange(items);
                newOrder.Total = newOrder.Items.Sum(x => x.UnitPrice * x.Quantity);

                await orders.AddAsync(tx, newOrder.Id, newOrder);

                await tx.CommitAsync();

                return newOrder;
            }
        }

        public async Task<OrderModel> Get(Guid orderId)
        {
            var orders = await OrderDictionary();
            using (var tx = stateManager.CreateTransaction())
            {
                var order = await orders.TryGetValueAsync(tx, orderId);
                if (!order.HasValue)
                    throw new OrderNotFoundException();

                return order.Value;
            }
        }

        public async Task<IEnumerable<OrderModel>> GetPending()
        {
            var pendingOrders = new List<OrderModel>();
            var cts = new CancellationToken();
            var orders = await OrderDictionary();
            using (var tx = stateManager.CreateTransaction())
            {
                var enumerable = await orders.CreateEnumerableAsync(tx, EnumerationMode.Unordered);
                var enumerator = enumerable.GetAsyncEnumerator();
                while (await enumerator.MoveNextAsync(cts))
                {
                    var order = enumerator.Current.Value;
                    if (order != null && order.State == OrderState.Created)
                    {
                        pendingOrders.Add(order);
                    }
                }                
            }

            return pendingOrders;
        }
    }
}
