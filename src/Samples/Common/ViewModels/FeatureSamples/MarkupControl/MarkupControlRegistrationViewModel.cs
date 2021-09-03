using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.MarkupControl
{
    public class MarkupControlRegistrationViewModel : DotvvmViewModelBase
    {

        public MarkupControlTestControlViewModel ViewModel1 { get; set; }

        public MarkupControlTestControlViewModel ViewModel2 { get; set; }

        public string SecondControlName { get; set; }



        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                ViewModel1 = new MarkupControlTestControlViewModel() { Value = 15 };
                ViewModel2 = new MarkupControlTestControlViewModel() { Value = 25 };
                SecondControlName = "Second control name was set from the binding";
            }
            return base.Init();
        }

    }
    public class MarkupControlTestControlViewModel
    {
        public int Value { get; set; }

    }
}