using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmErrorPageMiddleware : OwinMiddleware
    {
        public ErrorFormatter Formatter { get; set; }

        public DotvvmErrorPageMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            Exception error = null;
            try
            {
                await Next.Invoke(context);
            }
            catch (Exception ex)
            {
                error = ex;
            }

            if (error != null)
            {
                context.Response.StatusCode = 500;
                await RenderErrorResponse(context, error);
            }
        }

        /// <summary>
        /// Renders the error response.
        /// </summary>
        public Task RenderErrorResponse(IOwinContext context, Exception error)
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
