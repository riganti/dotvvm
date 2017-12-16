using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Samples.BasicSamples.Api.AspNetCore.Model;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.DataStore
{
    public class Database
    {

        public List<Order> Orders { get; set; }

        public List<Company> Companies { get; set; }


        public static Database Instance { get; set; }
    }
}
