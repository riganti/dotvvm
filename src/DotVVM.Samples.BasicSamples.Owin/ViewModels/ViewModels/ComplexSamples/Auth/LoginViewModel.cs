using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Net;
using DotVVM.Framework.AspNetCore.Hosting;

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
            if (!Context.IsPostBack && Context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                this.UserName = Context.HttpContext.User.Identity.Name;
                this.AdminRole = Context.HttpContext.User.IsInRole("admin");
            }
            return base.Init();
        }

        public async Task Login()
        {
            var auth = Context.GetAuthentication();
            auth.SignOut();
            var id = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, UserName)
                    }, "ApplicationCookie");
            if (AdminRole)
            {
                id.AddClaim(new Claim(ClaimTypes.Role, "admin"));
            }
            auth.SignIn(id);
        }
    }
}
