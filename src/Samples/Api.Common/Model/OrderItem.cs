using Newtonsoft.Json;

namespace DotVVM.Samples.BasicSamples.Api.Common.Model
{
    public class OrderItem
    {
        public int Id { get; set; }

        public string Text { get; set; }

        public decimal Amount { get; set; }

        public decimal? Discount { get; set; }

        public bool IsOnStock { get; set; }
    }
}
