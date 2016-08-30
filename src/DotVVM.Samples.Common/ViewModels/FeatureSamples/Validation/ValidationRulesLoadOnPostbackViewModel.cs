using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class ValidationRulesLoadOnPostbackViewModel: DotvvmViewModelBase
    {
        public CustomerData Customer { get; set; }

        public bool IsValid { get; set; }


        public void Validate()
        {
            IsValid = Context.ModelState.IsValid;
        }

        public void LoadCustomer()
        {
            Customer = new CustomerData()
            {
                Name = "Test",
                Email = "Test2"
            };
        }
    }

    public class CustomerData
    {
        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; }
    }
}
