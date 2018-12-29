using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVM.Samples.BasicSamples.Api.Common.Model
{
    public class Order
    {
        public int Id { get; set; }

        [JsonProperty(PropertyName = "Number")]
        public string Number { get; set; }

        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "companyId")]
        public int CompanyId { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
