using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroWish.Common.Models;
using MicroWish.Order.Contract.Client;
using MicroWish.Order.Contract.Models;

namespace MicroWish.Shop.Web.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderServiceClient orderServiceClient;

        public OrderController(IOrderServiceClient orderServiceClient)
        {
            this.orderServiceClient = orderServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            
            var items = new List<OrderItemModel>()
            {
                new OrderItemModel()
                {
                    Price = 10,
                    Quantity = 2,
                    UnitPrice = 20,
                    ProductId = Guid.NewGuid(),
                    VendorId = Guid.NewGuid(),
                }
            };

            var order = await orderServiceClient.CreateOrder(Guid.NewGuid(), Guid.NewGuid(), items);
            return new ObjectResult(order)
            {
                StatusCode = (int)HttpStatusCode.Created,
            };
        }

        public async Task<IActionResult> Get(Guid orderId)
        {

            var items = new List<OrderItemModel>()
            {
                new OrderItemModel()
                {
                    Price = 10,
                    Quantity = 2,
                    UnitPrice = 20,
                    ProductId = Guid.NewGuid(),
                    VendorId = Guid.NewGuid(),
                }
            };

            var order = await orderServiceClient.Get(orderId);
            return order == null ?
                (IActionResult)this.NotFound() :
                new ObjectResult(order);
        }

        public async Task<IActionResult> Book(Guid orderId)
        {
            var r = new Random();
            var cities = new string[] { "Zurich", "Amsterdam", "Berlin", "Wien" };
            var address = new AddressModel()
            {
                Address = $"Street {100 + r.Next(100)}",
                ZipCode = (1000 + r.Next(9999 + 1)).ToString(),
                City = cities[r.Next(cities.Length)],
            };

            var order = await orderServiceClient.Book(orderId, address);
            return order == null ?
                (IActionResult)this.NotFound() :
                new ObjectResult(order);
        }

        public async Task<IActionResult> Pending()
        {
            var orders = await orderServiceClient.GetPending();
            return new ObjectResult(orders);
        }

    }
}
