using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.Model
{
    public class Order
    {

        public int Id { get; set; }

        public string Number { get; set; }

        public DateTime Date { get; set; }

        public int CompanyId { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}
