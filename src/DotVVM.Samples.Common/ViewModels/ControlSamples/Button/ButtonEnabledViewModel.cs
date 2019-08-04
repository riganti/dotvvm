using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Button
{
    public class ButtonEnabledViewModel : DotvvmViewModelBase
    {
        public string CommandResult { get; set; }
        public string ClientStaticCommandResult { get; set; }
        public string StaticCommandResult { get; set; }
        public bool Enabled { get; set; }

        public void ChangeResult()
        {
            CommandResult = "Changed from command binding";
        }

        [AllowStaticCommand]
        public static string StaticChangeResult()
        {
            return "Changed from static command on server";
        }
    }
}

