using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class ClientSideValidationDisablingViewModel : DotvvmViewModelBase, IDisposable
    {
        [Required]
        public string RequiredString { get; set; }

        [EmailAddress]
        public string EmailString { get; set; }

        public bool ClientSideValidationEnabled => Context.Configuration.ClientSideValidation;

        public ClientSideValidationDisablingViewModel()
        {
        }

        public override Task Init()
        {
            bool value;
            //if there`s parameter ClientSideValidationEnabled which controls if client side valid validation will be active than set it according to it
            //else disable client side validation
            if (Context.Parameters.ContainsKey("ClientSideValidationEnabled") &&
                Boolean.TryParse(Context.Parameters["ClientSideValidationEnabled"].ToString(), out value))
            {
                Context.Configuration.ClientSideValidation = value;
            }
            else
            {
                Context.Configuration.ClientSideValidation = false;
            }

            return base.Init();
        }

        public void Dispose()
        {
            Context.Configuration.ClientSideValidation = true;
        }
    }
}