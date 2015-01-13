using Redwood.Framework.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace Redwood.Samples.BasicSamples.AuthSample
{
    public class LoginViewModel : RedwoodViewModelBase
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
                Context.OwinContext.Authentication.SignOut(Context.OwinContext.Authentication.User.Identity.AuthenticationType);
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

            if(Context.Query["ReturnUrl"] != null)
            {
                Context.Redirect(Context.Query["ReturnUrl"]);
            }
        }
    }
}