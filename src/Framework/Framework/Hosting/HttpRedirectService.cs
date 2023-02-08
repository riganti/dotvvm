using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Storage;
using System.Diagnostics;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting.Middlewares;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;
using DotVVM.Framework.Runtime.Tracing;

namespace DotVVM.Framework.Hosting
{
    public interface IHttpRedirectService
    {
        void WriteRedirectResponse(IHttpContext httpContext, string url, int statusCode = (int)HttpStatusCode.Redirect, bool replaceInHistory = false, bool allowSpaRedirect = false);
    }

    public class DefaultHttpRedirectService: IHttpRedirectService
    {
        public void WriteRedirectResponse(IHttpContext httpContext, string url, int statusCode = (int)HttpStatusCode.Redirect, bool replaceInHistory = false, bool allowSpaRedirect = false)
        {
            
            if (DotvvmRequestContext.DetermineRequestType(httpContext) is DotvvmRequestType.Navigate)
            {
                httpContext.Response.Headers["Location"] = url;
                httpContext.Response.StatusCode = statusCode;
            }
            else
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response
                    .WriteAsync(DefaultViewModelSerializer.GenerateRedirectActionResponse(url, replaceInHistory, allowSpaRedirect))
                    .GetAwaiter().GetResult();
               //   ^ we just wait for this Task. Redirect API never was async and the response size is small enough that we can't quite safely wait for the result
               //     .GetAwaiter().GetResult() preserves stack traces across async calls, thus I like it more that .Wait()
            }
        }
    }
}
