using Microsoft.Owin.Security;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmHttpAuthentication : IAuthentication
    {
        public DotvvmHttpAuthentication(IAuthenticationManager originalAuthentication)
        {
            OriginalAuthentication = originalAuthentication;
        }

        public IAuthenticationManager OriginalAuthentication { get; }

        public async Task ChallengeAsync(string authenticationScheme)
        {
            // TODO auth schemes
            if (authenticationScheme != "Automatic") throw new NotImplementedException();
            OriginalAuthentication.Challenge();
        }

        public async Task<ClaimsPrincipal> TryAuthenticateAsync(string authScheme)
        {
            if (authScheme != "Automatic") throw new NotImplementedException();
            return OriginalAuthentication.User;
        }
    }
}