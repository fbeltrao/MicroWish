using MicroWish.Order.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Order.Contract.Data
{
    public interface IOrderRepository
    {
        Task<OrderModel> Get(Guid orderId);

        Task<OrderModel> Create(OrderModel orderModel);

        Task<OrderModel> Update(OrderModel orderModel);

        Task Delete(OrderModel orderModel);

        Task<IEnumerable<OrderModel>> GetPending();

        
    }
}
