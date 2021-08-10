using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    [Authorize(AuthenticationSchemes = "Scheme1")]
    public class AuthorizedPresenter : IDotvvmPresenter
    {
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            await context.HttpContext.Response.WriteAsync("Secret Text");
        }
    }
}