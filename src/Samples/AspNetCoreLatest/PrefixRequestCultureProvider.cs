using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace DotVVM.Samples.BasicSamples
{
    public class PrefixRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext.Request.Path.StartsWithSegments("/cs"))
            {
                return Task.FromResult(new ProviderCultureResult("cs-CZ"));
            }
            else if (httpContext.Request.Path.StartsWithSegments("/de"))
            {
                return Task.FromResult(new ProviderCultureResult("de"));
            }
            else
            {
                return Task.FromResult(new ProviderCultureResult("en-US"));
            }
        }
    }
}
