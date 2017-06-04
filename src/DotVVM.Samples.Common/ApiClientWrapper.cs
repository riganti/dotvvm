using System;
using System.Collections.Generic;
using System.Text;
using MyNamespace;

namespace DotVVM.Samples.BasicSamples
{
    public class ApiClientWrapper
    {
        public CompaniesClient Companies { get; set; }
        public OrdersClient Orders { get; set; }
    }
}
