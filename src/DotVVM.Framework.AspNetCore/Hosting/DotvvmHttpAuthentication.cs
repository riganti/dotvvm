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

        public Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme)
        {
            return OriginalAuthentication.GetAuthenticateInfoAsync(authenticationScheme);
        }
    }
}