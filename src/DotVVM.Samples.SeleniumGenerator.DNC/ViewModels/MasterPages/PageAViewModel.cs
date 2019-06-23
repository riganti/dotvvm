using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using SampleApp1.Models;

namespace SampleApp1.ViewModels.MasterPages
{
    public class PageAViewModel : SiteViewModel
    {

        public LoginDTO Login { get; set; } = new LoginDTO();

        public RegisterDTO Register { get; set; } = new RegisterDTO();

        public string StatusMessage { get; set; }

        public void SignIn()
        {
            StatusMessage = "Sign in clicked!";
        }

        public void SignUp()
        {
            StatusMessage = "Sign up clicked!";
        }
    }
}

