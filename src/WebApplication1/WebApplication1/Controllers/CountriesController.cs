using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowSpecificOrigin")]
    public class CountriesController : Controller
    {
        [HttpGet]
        public IEnumerable<Country> GetCountries()
        {
            return new[]
            {
                new Country(new[]
                {
                    new Region("JC"),
                    new Region("SC")
                }, "CR"),
                new Country(new[]
                {
                    new Region("JC"),
                    new Region("SC")
                }, "SVK")
            };
        }

        [HttpGet("{id}")]
        public Country GetCountry(int id)
        {
            return new Country(new[]
            {
                new Region("JC"),
                new Region("SC")
            }, "CR");
        }

        public class Country
        {
            public Country(IEnumerable<Region> region, string name)
            {
                Name = name;
                Region = region;
            }

            public string Name { get; set; }
            public IEnumerable<Region> Region { get; set; }
        }

        public class Region
        {
            public Region(string name)
            {

                Name = name;
                Id = 1;
            }
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}