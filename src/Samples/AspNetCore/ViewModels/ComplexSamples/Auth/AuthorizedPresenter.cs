using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    public class AuthorizedPresenter : IDotvvmPresenter
    {
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            await context.Authorize(authenticationSchemes: new [] { "Scheme1" });
            await context.HttpContext.Response.WriteAsync("Secret Text");
        }
    }
}
