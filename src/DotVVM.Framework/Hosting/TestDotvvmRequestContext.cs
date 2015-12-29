using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using Microsoft.Owin;

namespace DotVVM.Framework.Hosting
{
    public class TestDotvvmRequestContext : IDotvvmRequestContext
    {
        public IOwinContext OwinContext { get; set; }
        public object ViewModel { get; set; }
        public DotvvmConfiguration Configuration { get; set; }
        public RouteBase Route { get; set; }
        public bool IsPostBack { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public ModelState ModelState { get; set; }
        public IDictionary<string, object> Query { get; set; }
        public bool IsCommandExceptionHandled { get; set; }
        public bool IsSpaRequest { get; set; }
        public bool IsInPartialRenderingMode { get; set; }
        public string ApplicationHostPath { get; set; }

        public void ChangeCurrentCulture(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }

        public CultureInfo GetCurrentUICulture()
        {
            return Thread.CurrentThread.CurrentUICulture;
        }

        public CultureInfo GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
        }

        public void InterruptRequest()
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.Interrupt);
        }

        public void Redirect(string url)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.Redirect, url);
        }

        public void Redirect(string routeName, object newRouteValues)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            Redirect(url);
        }

        public void RedirectPermanent(string url)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.RedirectPermanent, url);
        }

        public void RedirectPermanent(string routeName, object newRouteValues)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectPermanent(url);
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