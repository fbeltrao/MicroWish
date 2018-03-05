using Microsoft.ServiceFabric.Services.Remoting;
using MicroWish.Customer.Contract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroWish.Customer.Contract.Services
{
    public interface ICustomerService : Microsoft.ServiceFabric.Services.Remoting.IService
    {
        Task<CustomerModel> Get(Guid id);

        Task<CustomerModel> Create(Guid id, CustomerModel customer);

        Task<CustomerModel> Update(CustomerModel customer);

        Task<IList<CustomerModel>> List();
    }
}
