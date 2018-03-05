using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using MicroWish.Customer.Contract.Models;
using MicroWish.Customer.Contract.Services;

namespace MicroWish.Shop.Web.Controllers
{
    public class CustomerController : Controller
    {
        public async Task<IActionResult> Create()
        {
            var proxy = ServiceProxy.Create<ICustomerService>(new Uri("fabric:/MicroWishApplication/MicroWish.Customer.Service"));
            var customer = new CustomerModel()
            {
                FirstName = "Francisco",
                LastName = "Beltrao",
                Email = "fbeltra@microsoft.com",                
            };

            var result = await proxy.Create(Guid.NewGuid(), customer);
            return new OkObjectResult(result);
        }


        public async Task<IActionResult> Get(Guid customerId)
        {
            var proxy = ServiceProxy.Create<ICustomerService>(new Uri("fabric:/MicroWishApplication/MicroWish.Customer.Service"));

            var result = await proxy.Get(customerId);
            return new OkObjectResult(result);
        }


        public async Task<IActionResult> List()
        {
            var proxy = ServiceProxy.Create<ICustomerService>(new Uri("fabric:/MicroWishApplication/MicroWish.Customer.Service"));

            var result = await proxy.List();
            return new OkObjectResult(result);
        }

    }
}
