using System.Collections.Generic;
using System.Security.Claims;
using DotVVM.Framework.ViewModel;
using Microsoft.AspNet.Identity;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPARedirect
{
	public class LoginViewModel : DotvvmViewModelBase
	{

	    public string UserName { get; set; }

	    public string Password { get; set; }

	    public void Login()
	    {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, "Brock"));
            claims.Add(new Claim(ClaimTypes.Email, "brockallen@gmail.com"));
            var id = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);
            Context.OwinContext.Authentication.SignIn(id);

            Context.RedirectToRoute("ComplexSamples_SPARedirect_home", allowSpaRedirect: false);
	    }

	}
}

