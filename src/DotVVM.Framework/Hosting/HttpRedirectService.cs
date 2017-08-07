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
        void WriteRedirectReponse(IHttpContext httpContext, string url, int statusCode = (int)HttpStatusCode.Redirect, bool replaceInHistory = false, bool allowSpaRedirect = false);
    }

    public class DefaultHttpRedirectService: IHttpRedirectService
    {
        public void WriteRedirectReponse(IHttpContext httpContext, string url, int statusCode = (int)HttpStatusCode.Redirect, bool replaceInHistory = false, bool allowSpaRedirect = false)
        {
            if (!DotvvmPresenter.DeterminePartialRendering(httpContext))
            {
                httpContext.Response.Headers["Location"] = url;
                httpContext.Response.StatusCode = statusCode;
            }
            else
            {
                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Write(DefaultViewModelSerializer.GenerateRedirectActionResponse(url, replaceInHistory, allowSpaRedirect));
            }
        }
    }
}
