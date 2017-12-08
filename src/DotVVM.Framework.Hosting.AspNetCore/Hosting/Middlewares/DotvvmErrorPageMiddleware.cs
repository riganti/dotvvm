using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace DotVVM.Framework.Hosting.Middlewares
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
                    .ErrorHtml(error, DotvvmMiddleware.ConvertHttpContext(context));
                return context.Response.WriteAsync(text);
            }
            catch (Exception exc)
            {
                context.Response.ContentType = "text/plain";
                try
                {
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        writer.WriteLine("Error in Dotvvm Application:");
                        writer.WriteLine(error.ToString());
                        writer.WriteLine();
                        writer.WriteLine("Error occurred while displaying the error page. This is internal error and should not happened, please report it:");
                        writer.WriteLine(exc.ToString());
                    }
                }
                catch { }
                throw new Exception("Error occurred inside dotvvm error handler, this is internal error and should not happen; \n Original error:" + error.ToString(), exc);
            }
        }

    }
}
