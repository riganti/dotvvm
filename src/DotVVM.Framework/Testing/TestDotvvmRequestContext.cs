using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using Microsoft.AspNetCore.Http;

namespace DotVVM.Framework.Testing
{
    public class TestDotvvmRequestContext : IDotvvmRequestContext
    {
        public IHttpContext HttpContext { get; set; }
        public object ViewModel { get; set; }
        public DotvvmConfiguration Configuration { get; set; }
        public RouteBase Route { get; set; }
        public bool IsPostBack { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public ModelState ModelState { get; set; }
        public IDictionary<string, object> Query { get; set; }
        public bool IsCommandExceptionHandled { get; set; }
        public bool IsPageExceptionHandled { get; set; }
        public Exception CommandException { get; set; }
        public bool IsSpaRequest { get; set; }
        public bool IsInPartialRenderingMode { get; set; }
        public string ApplicationHostPath { get; set; }
        public string ResultIdFragment { get; set; }

        public DotvvmView View { get; set; }

        public void ChangeCurrentCulture(string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(cultureName);
        }

        public CultureInfo GetCurrentUICulture()
        {
            return CultureInfo.CurrentUICulture;
        }

        public CultureInfo GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture;
        }

        public void InterruptRequest()
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.Interrupt);
        }

        public void RedirectToUrl(string url, bool forceRefresh = false)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.Redirect, url);
        }

        public void RedirectToRoute(string routeName, object newRouteValues = null, bool forceRefresh = false)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrl(url);
        }

        public void RedirectToUrlPermanent(string url, bool forceRefresh = false)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.RedirectPermanent, url);
        }

        public void RedirectToRoutePermanent(string routeName, object newRouteValues = null, bool forceRefresh = false)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrlPermanent(url);
        }

        public void FailOnInvalidModelState()
        {
            if (!ModelState.IsValid)
            {
                throw new DotvvmInterruptRequestExecutionException(InterruptReason.ModelValidationFailed);
            }
        }

        public string TranslateVirtualPath(string virtualUrl)
        {
            if (virtualUrl.StartsWith("~/", System.StringComparison.Ordinal))
            {
                virtualUrl = ApplicationHostPath.TrimEnd('/') + virtualUrl.Substring(1);
            }
            return virtualUrl;
        }

        public void ReturnFile(byte[] bytes, string fileName, string mimeType, IHeaderDictionary additionalHeaders)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.ReturnFile, fileName);
        }

        public void ReturnFile(Stream stream, string fileName, string mimeType, IHeaderDictionary additionalHeaders)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.ReturnFile, fileName);
        }
    }
}