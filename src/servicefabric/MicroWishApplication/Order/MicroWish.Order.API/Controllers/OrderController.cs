using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MicroWish.Common.Models;
using MicroWish.Order.Contract.Models;
using MicroWish.Order.Contract.Services;

namespace MicroWish.Order.API.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderService service;

        public OrderController(IOrderService service)
        {
            this.service = service;
        }


        [HttpGet("pending")]
        public async Task<IActionResult> Pending()
        {
            var orders = await service.GetPending();
            if (orders != null)
                return new OkObjectResult(orders);

            return NotFound();
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> Get(Guid orderId)
        {
            var order = await service.Get(orderId);
            if (order != null)
                return new OkObjectResult(order);

            return NotFound();
        }      


        [HttpPost("{orderId}")]
        public async Task<IActionResult> Create(Guid orderId, Guid customerId, [FromBody] IEnumerable<OrderItemModel> orderItems)
        {
            if (ModelState.IsValid)
            {
                var order = await service.CreateOrder(orderId, customerId, orderItems);
                return CreatedAtAction("Get", new { id = order.Id }, order);
            }

            return BadRequest();
        }

        [HttpPost("{orderId}/book")]
        public async Task<IActionResult> Book(Guid orderId, [FromBody]AddressModel deliveryAddress)
        {
            if (ModelState.IsValid)
            {
                var order = await service.Book(orderId, deliveryAddress);
                return AcceptedAtAction("Get", new { id = order.Id }, order);
            }

            return BadRequest();
        }
    }
}
