using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using swag.DataStore;
using swag.Model;

namespace swag.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        

        [Route("{companyId}")]
        public List<Order> Get(int companyId, int pageIndex = 0, int pageSize = 20)
        {
            lock (Database.Instance)
            {
                return Database.Instance
                    .Orders
                    .Where(o => o.CompanyId == companyId)
                    .Skip(pageIndex)
                    .Take(pageSize)
                    .ToList();
            }
        }

        [Route("{orderId}", Name = nameof(GetItem))]
        public Order GetItem(int orderId)
        {
            lock (Database.Instance)
            {
                return Database.Instance.Orders.First(o => o.Id == orderId);
            }
        }

        public IActionResult Post(Order order)
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

        [Route("{orderId}")]
        public IActionResult Put(int orderId, Order order)
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

        [Route("delete-{orderId}")]
        public void Delete(int orderId)
        {
            lock (Database.Instance)
            {
                Database.Instance.Orders.RemoveAll(o => o.Id == orderId);
            }
        }

    }
    
}
