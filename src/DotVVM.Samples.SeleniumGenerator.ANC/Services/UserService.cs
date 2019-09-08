using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DotVVM.Samples.SeleniumGenerator.ANC.Services
{
    public class UserService
    {
        private readonly UserManager<IdentityUser> userManager;
		
        public UserService(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<ClaimsIdentity> SignInAsync(string userName, string password)
        {
            var user = await userManager.FindByNameAsync(userName);
            if (user != null)
            {
                var result = await userManager.CheckPasswordAsync(user, password);
                if (result)
                {
                    return CreateIdentity(user);
                }
            }
            return null;
        }

        public async Task<IdentityResult> RegisterAsync(string userName, string password)
        {
            var user = new IdentityUser(userName);
            return await userManager.CreateAsync(user, password);
        }

        private ClaimsIdentity CreateIdentity(IdentityUser user)
        {
            var identity = new ClaimsIdentity(
                new []
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.NameIdentifier, user.Id), 
                    new Claim(ClaimTypes.Role, "administrator"), 
                },"Cookie");
            return identity;
        }

    }
}
