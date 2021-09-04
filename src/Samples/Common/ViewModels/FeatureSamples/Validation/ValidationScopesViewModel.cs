using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class ValidationScopesViewModel : DotvvmViewModelBase
    {
        public string Title { get; set; }

        [Required]
        public string Text { get; set; }

        public void Send()
        {

        }

        public SubModel SubModel { get; set; } = new SubModel();

        public override Task Init()
        {

            return base.Init();
        }

        public ValidationScopesViewModel()
        {
            Title = "Hello from DotVVM!";
        }
    }
    public class SubModel : DotvvmViewModelBase
    {
        [Required]
        public string Value { get; set; }

        public ComponentContextModel ComponentContext { get; set; }

        public override Task Load()
        {
            if (!Context.IsPostBack)
            {
                ComponentContext = new ComponentContextModel();
            }
            return base.Load();
        }

        public void DoSomething()
        {

        }
    }

    public class ComponentContextModel
    {
        public string ComponentData { get; set; }
    }


}

