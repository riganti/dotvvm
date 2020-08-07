using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCoreLatest.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        [HttpGet]
        public List<Order> Get(int companyId, int pageIndex = 0, int pageSize = 20)
        {
            lock (Database.Instance)
            {
                return Database.Instance
                    .Orders
                    .Where(o => o.CompanyId == companyId)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();
            }
        }

        [HttpGet]
        [Route("{orderId}")]
        public Order GetItem(int orderId)
        {
            lock (Database.Instance)
            {
                return Database.Instance.Orders.First(o => o.Id == orderId);
            }
        }

        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            lock (Database.Instance)
            {
                order.Id = (Database.Instance.Orders.Max(o => (int?) o.Id) ?? 0) + 1;

                if (!Database.Instance.Companies.Any(c => c.Id == order.CompanyId))
                {
                    return BadRequest();
                }

                Database.Instance.Orders.Add(order);
            }

            return CreatedAtRoute("GetItem", new { orderId = order.Id });
        }

        [HttpPut]
        [Route("{orderId}")]
        public IActionResult Put(int orderId, [FromBody]Order order)
        {
            lock (Database.Instance)
            {
                var index = Database.Instance.Orders.FindIndex(o => o.Id == orderId);
                if (index < 0)
                {
                    return NotFound();
                }

                if (!Database.Instance.Companies.Any(c => c.Id == order.CompanyId))
                {
                    return BadRequest();
                }

                Database.Instance.Orders[index] = order;
                return NoContent();
            }
        }

        [HttpDelete]
        [Route("delete/{orderId}")]
        public void Delete(int orderId)
        {
            lock (Database.Instance)
            {
                Database.Instance.Orders.RemoveAll(o => o.Id == orderId);
            }
        }
    }
}
