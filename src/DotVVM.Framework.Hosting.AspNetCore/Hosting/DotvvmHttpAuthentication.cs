using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpAuthentication : IAuthentication
    {
        public DotvvmHttpAuthentication(AuthenticationManager originalAuthentication)
        {
            OriginalAuthentication = originalAuthentication;
        }

        public AuthenticationManager OriginalAuthentication { get; }

        public Task ChallengeAsync(string authenticationScheme)
        {
            return OriginalAuthentication.ChallengeAsync(authenticationScheme);
        }

        public async Task<ClaimsPrincipal> TryAuthenticateAsync(string authScheme)
        {
            return (await OriginalAuthentication.GetAuthenticateInfoAsync(authScheme)).Principal;
        }
    }
}