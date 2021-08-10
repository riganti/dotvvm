using DotVVM.Framework.Hosting;
using DotVVM.Framework.Runtime.Filters;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.Auth
{
    [Authorize]
    public class AuthorizedPresenter : IDotvvmPresenter
    {
        public async Task ProcessRequest(IDotvvmRequestContext context)
        {
            await context.HttpContext.Response.WriteAsync("Secret Text");
        }
    }
}
