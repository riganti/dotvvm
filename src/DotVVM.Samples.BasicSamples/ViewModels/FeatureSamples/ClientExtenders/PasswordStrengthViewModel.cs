using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.ClientExtenders
{
    public class PasswordStrengthViewModel : DotvvmViewModelBase
	{
        [ClientExtender("passwordStrength", false)]
        public string Password { get; set; }


        public override Task Init()
        {
            this.Context.ResourceManager.AddRequiredScriptFile("extenders", "~/Scripts/ClientExtenders.js", DotVVM.Framework.ResourceManagement.ResourceConstants.KnockoutJSResourceName);
            return base.Init();
        }
	}

   
}

