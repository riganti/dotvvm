using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;

namespace DotVVM.Framework.Hosting
{
    public interface IAuthentication
    {
        Task ChallengeAsync(string authenticationScheme);
        Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme);
    }
}