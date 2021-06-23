using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DotVVM.Samples.BasicSamples.Api.Common.DataStore;
using DotVVM.Samples.BasicSamples.Api.Common.Model;
using DotVVM.Samples.BasicSamples.Api.Owin;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.AspNetCore.Controllers
{
    [RoutePrefix("api/orders")]
    public class OrdersController : ApiController
    {

        [HttpGet]
        [Route("")]
        [NoCache]
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

        [HttpGet]
        [Route("{orderId}", Name = nameof(GetItem))]
        [NoCache]
        public Order GetItem(int orderId = 0)
        {
            lock (Database.Instance)
            {
                return Database.Instance.Orders.First(o => o.Id == orderId);
            }
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult Post([FromBody]Order order)
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

            return CreatedAtRoute("GetItem", new { orderId = order.Id }, order);
        }

        [HttpPut]
        [Route("{orderId}")]
        public IHttpActionResult Put(int orderId, [FromBody]Order order)
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
                return Ok(HttpStatusCode.NoContent);
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
