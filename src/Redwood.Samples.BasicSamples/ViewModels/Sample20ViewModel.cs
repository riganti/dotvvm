using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Redwood.Framework.ViewModel;

namespace Redwood.Samples.BasicSamples.ViewModels
{
    public class Sample20ViewModel : RedwoodViewModelBase
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




        public class CustomerData
        {
            [Required]
            public string Name { get; set; }

            [EmailAddress]
            public string Email { get; set; }
        }
    }
}