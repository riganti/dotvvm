using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationErrorsCount
{
    public class BasicViewModel : DotvvmViewModelBase
    {
        public BasicModel Basic { get; set; } = new();

        public ContactModel Contact { get; set; } = new();


        public class BasicModel
        {
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
        }

        public class ContactModel
        {
            [Required]
            [Phone]
            public string Phone { get; set; }
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
    }
}

