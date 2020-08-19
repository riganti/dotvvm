using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace SampleApp1.ViewModels.Forms
{
    public class SignInViewModel : DotvvmViewModelBase
    {
        public string Email { get; set; }

        public string Password { get; set; }

        public bool IsLoginSuccessful { get; set; }

        public void OnSubmitClicked()
        {
            IsLoginSuccessful = true;
        }
    }
}

