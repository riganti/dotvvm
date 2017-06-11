using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Validation;

namespace DotVVM.Samples.Common.ViewModels.ComplexSamples.CustomValidationError
{
    public class CustomValidationErrorViewModel : DotvvmViewModelBase
    {
        public LoginInfo Login { get; set; } = new LoginInfo();

        public void LogIn()
        {
            if (Login.Nick == "Mike" && Login.Password == "1234")
            {
                Login.IsLoggedIn = true;
            }
            else
            {
                Context.ModelState.Errors.Add(new ViewModelValidationError()
                {
                    ErrorMessage = "Wrong Nick or Password.",
                    PropertyPath = GetPropertyPath()
                });
                Context.FailOnInvalidModelState();
            }
        }

        public void LogOut()
        {
            Login.IsLoggedIn = false;
        }

        public virtual string GetPropertyPath()
        {
            return null;
        }

    }
}
