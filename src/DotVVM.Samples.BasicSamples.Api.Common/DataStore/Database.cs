using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using DotVVM.Samples.BasicSamples.Api.Common.Model;

namespace DotVVM.Samples.BasicSamples.Api.Common.DataStore
{
    public class Database
    {
        public List<Order> Orders { get; set; }

        public List<Company<string>> Companies { get; set; }

        public List<Company<bool>> Companies2 { get; set; }

        public static Database Instance { get; set; }

        public void SeedData()
        {
            Randomizer.Seed = new Random(1);

            var companyId = 1;
            var companiesFaker = new Faker<Company<string>>()
                .RuleFor(c => c.Id, f => companyId++)
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Owner, f => f.Name.FullName());

            Companies2 = new Faker<Company<bool>>()
              .RuleFor(c => c.Id, f => companyId++)
              .RuleFor(c => c.Name, f => f.Company.CompanyName())
              .RuleFor(c => c.Owner, f => f.Name.FullName())
              .RuleFor(c => c.Department, f => f.PickRandom(true, false))
              .Generate(10)
              .ToList();

            var orderItemId = 1;
            var orderItemsFaker = new Faker<OrderItem>()
                .RuleFor(i => i.Id, f => orderItemId++)
                .RuleFor(i => i.Amount, f => f.Random.Decimal(0, 1000))
                .RuleFor(i => i.Discount, f => f.Random.Decimal(0, 50))
                .RuleFor(i => i.IsOnStock, f => f.Random.Bool())
                .RuleFor(i => i.Text, f => f.Lorem.Lines(1));

            var orderId = 1;
            var ordersFaker = new Faker<Order>()
                .RuleFor(o => o.Id, f => orderId++)
                .RuleFor(o => o.CompanyId, f => f.PickRandom(Companies.Select(c => c.Id)))
                .RuleFor(o => o.Date, f => f.Date.Between(new DateTime(2010, 1, 1), new DateTime(2012, 1, 1)))
                .RuleFor(o => o.Number, f => f.Random.AlphaNumeric(8))
                .RuleFor(o => o.OrderItems, f => orderItemsFaker.Generate(f.Random.Number(1, 10)));

            Companies = companiesFaker.Generate(35).ToList();
            Orders = ordersFaker.Generate(300).ToList();
        }
    }
}
