using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace DotVVM.Samples.SeleniumGenerator.ANC.ViewModels
{
    public class MasterPageViewModel : DotvvmViewModelBase
    {
		public async Task SignOut()
        {
            await Context.GetAuthentication().SignOutAsync(IdentityConstants.ApplicationScheme);
            Context.RedirectToRoute("Default", null, false, false);
        }
    }
}
