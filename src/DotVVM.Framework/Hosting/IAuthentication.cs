using System.Security.Claims;
using System.Threading.Tasks;

namespace DotVVM.Framework.Hosting
{
    public interface IAuthentication
    {
        Task ChallengeAsync(string authenticationScheme);
        //Task<AuthenticateInfo> GetAuthenticateInfoAsync(string authenticationScheme);
        Task<ClaimsPrincipal> TryAuthenticateAsync(string authScheme);
    }
}