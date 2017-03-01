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
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Testing
{
    public class TestDotvvmRequestContext : IDotvvmRequestContext
    {
        public IHttpContext HttpContext { get; set; }
        public string CsrfToken { get; set; }

        public JObject ReceivedViewModelJson { get; set; }
        public string GetSpaContentPlaceHolderUniqueId()
        {
            throw new NotImplementedException();
        }

        public object ViewModel { get; set; }
        public JObject ViewModelJson { get; set; }
        public DotvvmConfiguration Configuration { get; set; }
        public IDotvvmPresenter Presenter { get; set; }
        public RouteBase Route { get; set; }
        public bool IsPostBack { get; set; }
        public IDictionary<string, object> Parameters { get; set; }
        public ResourceManager ResourceManager { get; set; }
        public ModelState ModelState { get; set; }
        public IQueryCollection Query { get; set; }
        public bool IsCommandExceptionHandled { get; set; }
        public bool IsPageExceptionHandled { get; set; }
        public Exception CommandException { get; set; }
        public bool IsSpaRequest { get; set; }
        public bool IsInPartialRenderingMode { get; set; }
        public string ApplicationHostPath { get; set; }
        public string ResultIdFragment { get; set; }
        public Dictionary<string, string> PostBackUpdatedControls { get; }
        public DotvvmView View { get; set; }

        private IServiceProvider _services;
        public IServiceProvider Services
        {
            get => _services ?? Configuration?.ServiceLocator?.GetServiceProvider() ?? throw new NotSupportedException();
            set => _services = value;
        }

        public void ChangeCurrentCulture(string cultureName)
            => ChangeCurrentCulture(cultureName, cultureName);

        public void ChangeCurrentCulture(string cultureName, string uiCultureName)
        {
#if DotNetCore
            CultureInfo.CurrentCulture = new CultureInfo(cultureName);
            CultureInfo.CurrentUICulture = new CultureInfo(uiCultureName);
#else
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(uiCultureName);
#endif
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

        public string GetSerializedViewModel()
        {
            throw new NotImplementedException();
        }

        public void RedirectToUrl(string url, bool replaceInHistory = false, bool allowSpaRedirect = false)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.Redirect, url);
        }


        public void RedirectToRoute(string routeName, object newRouteValues = null, bool replaceInHistory = false, bool allowSpaRedirect = true, string urlSuffix = null)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);

            if (!string.IsNullOrEmpty(urlSuffix))
            {
                url += urlSuffix;
            }

            RedirectToUrl(url, replaceInHistory, allowSpaRedirect);
        }

        public void RedirectToUrlPermanent(string url, bool replaceInHistory = false, bool allowSpaRedirect = false)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.RedirectPermanent, url);
        }

        public void RedirectToRoutePermanent(string routeName, object newRouteValues = null, bool replaceInHistory = false, bool allowSpaRedirect = true)
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

        public void ReturnFile(byte[] bytes, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.ReturnFile, fileName);
        }

        public void ReturnFile(Stream stream, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders)
        {
            throw new DotvvmInterruptRequestExecutionException(InterruptReason.ReturnFile, fileName);
        }
    }
}