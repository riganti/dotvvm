using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ResourceManagement;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientExtenders
{
    public class PasswordStrengthViewModel : DotvvmViewModelBase
    {
        [ClientExtender("passwordStrength", false)]
        public string Password { get; set; }

    }

   
}

