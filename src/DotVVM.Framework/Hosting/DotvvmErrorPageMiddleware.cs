using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;
using Microsoft.AspNetCore.Http;
//#if DotNetCore
//using AppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
//using Context = Microsoft.AspNetCore.Http.HttpContext;
//#else
//using AppBuilder = Owin.IApplicationBuilder;
//using Context = Microsoft.Owin.HttpContext;
//#endif

namespace DotVVM.Framework.Hosting
{
    public class DotvvmErrorPageMiddleware
    {
        private RequestDelegate next;

        public ErrorFormatter Formatter { get; set; }

        public DotvvmErrorPageMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            Exception error = null;
            try
            {
                await next(context);
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
        public Task RenderErrorResponse(HttpContext context, Exception error)
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
