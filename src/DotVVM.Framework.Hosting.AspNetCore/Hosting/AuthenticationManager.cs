using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Hosting.AspNetCore.Hosting
{
    /// <summary>
    /// Provides various authentication and authorization methods.
    /// </summary>
    public class AuthenticationManager
    {
        public HttpContext Context { get; }

        public AuthenticationManager(HttpContext context)
        {
            Context = context;
        }

        public async Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal)
            => await Context.SignInAsync(authenticationScheme, principal);

        public async Task SignInAsync(string authenticationScheme, ClaimsPrincipal principal,
            AuthenticationProperties properties)
            => await Context.SignInAsync(authenticationScheme, principal, properties);

        public async Task ChallengeAsync(AuthenticationProperties properties)
            => await Context.ChallengeAsync(properties);

        public async Task ForbidAsync() 
            => await Context.ForbidAsync();

        public async Task ForbidAsync(string authenticationScheme) 
            => await Context.ForbidAsync(authenticationScheme);

        public async Task<ClaimsPrincipal> AuthenticateAsync(string authenticationScheme) =>
            (await Context.AuthenticateAsync(authenticationScheme)).Principal;

        public async Task ChallengeAsync() 
            => await Context.ChallengeAsync();

        public async Task ChallengeAsync(string authenticationScheme) 
            => await Context.ChallengeAsync(authenticationScheme);

        public async Task ChallengeAsync(string authenticationScheme, AuthenticationProperties properties) 
            => await Context.ChallengeAsync(authenticationScheme, properties);

        public async Task ForbidAsync(string authenticationScheme, AuthenticationProperties properties) 
            => await Context.ForbidAsync(authenticationScheme, properties);

        public async Task ForbidAsync(AuthenticationProperties properties) 
            => await Context.ForbidAsync(properties);

        public async Task SignOutAsync(string authenticationScheme) 
            => await Context.SignOutAsync(authenticationScheme);

        public async Task SignOutAsync(string authenticationScheme, AuthenticationProperties properties) 
            => await Context.SignOutAsync(authenticationScheme, properties);
    }
}
