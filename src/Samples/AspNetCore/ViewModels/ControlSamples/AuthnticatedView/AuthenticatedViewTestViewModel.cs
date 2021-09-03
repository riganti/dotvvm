using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.Security.Claims;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Authentication;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.AuthenticatedView
{
	public class AuthenticatedViewTestViewModel : DotvvmViewModelBase
	{

        public async Task SignIn()
        {
            var identity = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "test")
                },
                "ApplicationCookie");

            await Context.GetAuthentication().SignInAsync("Scheme3", new ClaimsPrincipal(identity));
            Context.RedirectToRoute(Context.Route.RouteName);
        }

        public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync("Scheme3");
            Context.RedirectToRoute(Context.Route.RouteName);
        }


    }
}

