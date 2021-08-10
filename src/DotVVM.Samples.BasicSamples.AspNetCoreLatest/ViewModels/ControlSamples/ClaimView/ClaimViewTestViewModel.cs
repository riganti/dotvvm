using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ClaimView
{
    public class ClaimViewTestViewModel : DotvvmViewModelBase
    {
        private const string AuthenticationScheme = "Scheme3";

        public List<string> DesiredRoles { get; set; } = new List<string>();

        public async Task SignIn()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "test")
                }
                .Concat(DesiredRoles.Select(role => new Claim(ClaimTypes.Role, role))),
                "ApplicationCookie");

            await Context.GetAuthentication().SignInAsync(AuthenticationScheme, new ClaimsPrincipal(identity));
            Context.RedirectToRoute(Context.Route.RouteName);
        }

        public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync(AuthenticationScheme);
            Context.RedirectToRoute(Context.Route.RouteName);
        }
    }
}
