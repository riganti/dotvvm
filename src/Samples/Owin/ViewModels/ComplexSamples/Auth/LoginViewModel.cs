using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    public class LoginViewModel : DotvvmViewModelBase
    {
        public LoginViewModel()
        {
            UserName = "";
            AdminRole = false;
        }

        public string UserName { get; set; }

        public bool AdminRole { get; set; }

        public bool LoggedIn { get; set; }

        public override Task Init()
        {
            if (!Context.IsPostBack && Context.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                UserName = Context.HttpContext.User.Identity.Name;
                AdminRole = Context.HttpContext.User.IsInRole("admin");
            }
            return base.Init();
        }

        public void Login()
        {
            var auth = Context.GetAuthentication();
            auth.SignOut();

            var id = new ClaimsIdentity(new[] {
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