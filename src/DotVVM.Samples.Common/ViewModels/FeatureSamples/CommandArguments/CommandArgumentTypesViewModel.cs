using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.Controls;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.CommandArguments
{
    public class CommandArgumentTypesViewModel : DotvvmViewModelBase
    {
        public string Value { get; set; } = "Nothing here";

        [Bind(Direction.ServerToClientFirstRequest)]
        public ButtonParameter Parameter { get; set; } = new ButtonParameter {
            MyProperty = "Sample text"
        };

        public void Command(string arg)
        {
            Value = arg;
        }
    }
}

