using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Validation
{
    public class LocalizationViewModel : DotvvmViewModelBase
    {
        public override Task Init()
        {
            if (Context.Query.ContainsKey("lang") && Context.Query["lang"] == "cs-CZ")
            {
                Context.ChangeCurrentCulture("cs-CZ");
            }
            else
            {
                Context.ChangeCurrentCulture("en-US");
            }
            return base.Init();
        }

        [Required(ErrorMessageResourceType = typeof(DotVVM.Samples.BasicSamples.Localization_Resources), ErrorMessageResourceName = "LocalizedString1")]
        public string Email { get; set; }
        public void Submit()
        {

        }
    }
}

