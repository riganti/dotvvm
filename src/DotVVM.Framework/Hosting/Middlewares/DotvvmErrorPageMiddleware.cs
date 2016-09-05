using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;

namespace DotVVM.Framework.Hosting.Middlewares
{
    public class DotvvmErrorPageMiddleware : IMiddleware
    {
        public ErrorFormatter Formatter { get; set; }

        public async Task Handle(IDotvvmRequestContext request, Func<IDotvvmRequestContext, Task> next)
        {
            Exception error = null;
            try
            {
                await next(request);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                request.HttpContext.Response.StatusCode = 500;
                await RenderErrorResponse(request.HttpContext, error);
            }
        }

        /// <summary>
        /// Renders the error response.
        /// </summary>
        public Task RenderErrorResponse(IHttpContext context, Exception error)
        {
            context.Response.ContentType = "text/html";

            try
            {

                var text = (Formatter ?? (Formatter = ErrorFormatter.CreateDefault()))
                    .ErrorHtml(error, context);
                return context.Response.WriteAsync(text);
            }
            catch (Exception exc)
            {
                throw new Exception("Error occured inside dotvvm error handler, this is internal error and should not happen; \n Original error:" + error.ToString(), exc);
            }
        }
    }
}
