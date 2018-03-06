using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MicroWish.Consumer.Models;

namespace MicroWish.Consumer.Contracts
{
    /// <summary>
    /// Defines the Basket data Service
    /// </summary>
    public interface IOrderDataService : Microsoft.ServiceFabric.Services.Remoting.IService
    {
        Task<OrderModel> Create(OrderModel order);

        Task<OrderModel> Update(OrderModel order);

        Task<OrderModel> Get(Guid orderId);

        Task<IEnumerable<OrderModel>> GetPending();
    }
}
