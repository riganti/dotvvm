using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Utils
{
    internal static class DotvvmRequestContextUtils
    {
        internal static async Task InterruptRequestAsync(this IDotvvmRequestContext context, HttpStatusCode statusCode, string message)
        {
            context.HttpContext.Response.StatusCode = (int)statusCode;
            if (!string.IsNullOrEmpty(message))
            {
                await context.HttpContext.Response.WriteAsync(message);
            }

            context.InterruptRequest();
        }
    }
}
