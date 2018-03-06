using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using MicroWish.Consumer.Contracts;
using MicroWish.Models;

namespace MicroWish.Consumer.OrderDataService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class OrderDataService : StatefulService, IOrderDataService
    {        
        public OrderDataService(StatefulServiceContext context)
            : base(context)
        {
        }


        async Task<IReliableDictionary<Guid, OrderModel>> OrderDictionary() => await this.StateManager.GetOrAddAsync<IReliableDictionary<Guid, OrderModel>>("orders");


        public async Task<OrderModel> Create(OrderModel order)
        {
            var orders = await OrderDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {
                await orders.AddAsync(tx, order.Id, order);

                await tx.CommitAsync();

                return order;
            }
        }

        public async Task<OrderModel> Update(OrderModel order)
        {
            var orders = await OrderDictionary();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var updatedOrder = order.Clone();
                updatedOrder.LastUpdate = DateTime.UtcNow;
                await orders.SetAsync(tx, updatedOrder.Id, updatedOrder);

                await tx.CommitAsync();

                return updatedOrder;
            }
        }

        public async Task<OrderModel> Get(Guid orderId)
        {
            var orders = await OrderDictionary();
            using (var tx = this.StateManager.CreateTransaction())
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
            using (var tx = this.StateManager.CreateTransaction())
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

        #region Not important code       
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            // enable remoting in this service
            yield return new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context));
        }
        #endregion
    }
}
