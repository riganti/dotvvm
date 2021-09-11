using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ValidationSummary
{
    public class IncludeErrorsFromTargetViewModel : DotvvmViewModelBase
    {
        public LoginInfo Login { get; set; } = new LoginInfo();

        public string PropertyPath { get; set; } = null;

        public void LogIn()
        {
            if (Login.Nick == "Mike" && Login.Password == "1234")
            {
                Login.IsLoggedIn = true;
            }
            else
            {
                this.UnsafeAddModelError(PropertyPath, "Wrong Nick or Password.");
                Context.FailOnInvalidModelState();
            }
        }

        public void LogOut()
        {
            Login.IsLoggedIn = false;
        }
    }
}
