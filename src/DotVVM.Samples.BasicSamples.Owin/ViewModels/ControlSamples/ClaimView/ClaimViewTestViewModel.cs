using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ClaimView
{
    public class ClaimViewTestViewModel : DotvvmViewModelBase
    {
        public List<string> DesiredRoles { get; set; } = new List<string>();

        public void SignIn()
        {

            var identity = new ClaimsIdentity(
                new[] {
                        new Claim(ClaimTypes.Name, "test")
                    }
                    .Concat(DesiredRoles.Select(r => new Claim(ClaimTypes.Role, r))),
                "ApplicationCookie");

            Context.GetAuthentication().SignIn(identity);
            Context.RedirectToRoute(Context.Route.RouteName);
        }

        public void SignOut()
        {
            Context.GetAuthentication().SignOut();
            Context.RedirectToRoute(Context.Route.RouteName);
        }
    }
}
