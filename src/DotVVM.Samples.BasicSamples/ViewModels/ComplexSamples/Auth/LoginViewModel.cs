using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    public class LoginViewModel : DotvvmViewModelBase
    {
        public string UserName { get; set; }

        public bool AdminRole { get; set; }

        public bool LoggedIn { get; set; }

        public LoginViewModel()
        {
            UserName = "";
            AdminRole = false;
        }

        public override Task Init()
        {
            if (!Context.IsPostBack && Context.OwinContext.Authentication.User.Identity.IsAuthenticated)
            {
                this.UserName = Context.OwinContext.Authentication.User.Identity.Name;
                this.AdminRole = Context.OwinContext.Authentication.User.IsInRole("admin");
            }
            return base.Init();
        }

        public void Login()
        {
            var auth = Context.OwinContext.Authentication;
            auth.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            var id = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, UserName)
                    }, DefaultAuthenticationTypes.ApplicationCookie);
            if (AdminRole)
            {
                id.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            }
            auth.SignIn(id);

            if (Context.Query.ContainsKey("ReturnUrl"))
            {
                Context.RedirectToUrl((string)Context.Query["ReturnUrl"]);
            }
        }
    }
}
