using MicroWish.Common.Models;
using MicroWish.Order.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Order.Contract.Client
{
    public interface IOrderServiceClient
    {
        Task<OrderModel> CreateOrder(Guid orderId, Guid customerId, IEnumerable<OrderItemModel> items);

        Task<OrderModel> Book(Guid orderId, AddressModel deliveryAddress);

        Task<IEnumerable<OrderModel>> GetPending();

        Task<OrderModel> Get(Guid orderId);
    }
}
