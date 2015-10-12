using Microsoft.Owin;
using DotVVM.Framework.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting.ErrorPages;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmErrorPageMiddleware: OwinMiddleware
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

            //var template = new ErrorPageTemplate()
            //{
            //    Exception = error,
            //    ErrorCode = context.Response.StatusCode,
            //    ErrorDescription = ((HttpStatusCode)context.Response.StatusCode).ToString(),
            //    IpAddress = context.Request.RemoteIpAddress,
            //    CurrentUserName = context.Request.User != null ? context.Request.User.Identity.Name : "",
            //    Url = context.Request.Uri.ToString(),
            //    Verb = context.Request.Method
            //};
            //if (error is ParserException)
            //{
            //    template.FileName = ((ParserException)error).FileName;
            //    template.LineNumber = ((ParserException)error).LineNumber;
            //    template.PositionOnLine = ((ParserException)error).PositionOnLine;
            //}

            var text = (Formatter ?? (Formatter = ErrorFormatter.CreateDefault())).ErrorHtml(error, context);
            return context.Response.WriteAsync(text);
        }
    }
}
