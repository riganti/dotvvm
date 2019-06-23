using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.SimplePage
{
    public class PageViewModel : DotvvmViewModelBase
    {
        public AddressType AddressType { get; set; }

        public string Address { get; set; }

        public bool IsEuVatPayer { get; set; }

        public List<string> Countries { get; set; } = Data.Countries;

        public string CountryCode { get; set; }

        public string StatusMessage { get; set; }

        public NameData Name { get; set; } = new NameData();

        public void CreateCompany()
        {
            StatusMessage = $"The company {Name} was created.";
        }

        public void ResetForm()
        {
            AddressType = AddressType.Person;
            Name = new NameData();
            Address = "";
            IsEuVatPayer = false;
            CountryCode = "";
            StatusMessage = "";
        }

    }

    public class NameData
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
    }
}

