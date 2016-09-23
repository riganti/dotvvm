using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.RoleView
{
	public class RoleViewTestViewModel : DotvvmViewModelBase
	{
        public List<string> DesiredRoles { get; set; } = new List<string>();

        public void SignIn()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
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

