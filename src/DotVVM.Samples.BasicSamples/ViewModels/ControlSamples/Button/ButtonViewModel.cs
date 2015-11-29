using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Button
{
    public class ButtonViewModel : DotvvmViewModelBase
    {
        public string Title { get; set; }

        public ButtonViewModel()
        {
            Title = "Hello from DotVVM!";
        }
    }
}