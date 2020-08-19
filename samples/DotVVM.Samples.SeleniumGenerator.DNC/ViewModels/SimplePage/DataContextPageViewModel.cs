using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace SampleApp1.ViewModels.SimplePage
{
    public class DataContextPageViewModel : DotvvmViewModelBase
    {
        public Address Address { get; set; }

        public DataContextPageViewModel()
        {
            Address = new Address
            {
                City = "Brno",
                Street = "Vojtova",
                PostalCode = "58712"
            };
        }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}

