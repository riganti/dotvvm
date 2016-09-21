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


namespace DotVVM.Framework.Hosting
{
    public class DotvvmRequestContext : IDotvvmRequestContext
    {
        public string CsrfToken { get; set; }

        /// <summary>
        /// Gets the underlying object for this HTTP request.
        /// </summary>
        public IHttpContext HttpContext { get;  set; }

        /// <summary>
        /// Gets the <see cref="IDotvvmPresenter"/> that is responsible for handling this HTTP request.
        /// </summary>
        public IDotvvmPresenter Presenter { get; set; }

        /// <summary>
        /// Gets the global configuration of DotVVM.
        /// </summary>
        public DotvvmConfiguration Configuration { get;  set; }

        /// <summary>
        /// Gets the route that was used for this request.
        /// </summary>
        public RouteBase Route { get;  set; }

        /// <summary>
        /// Determines whether this HTTP request is a postback or a classic GET request.
        /// </summary>
        public bool IsPostBack { get;  set; }

        /// <summary>
        /// Gets the values of parameters specified in the <see cref="P:Route" /> property.
        /// </summary>
        public IDictionary<string, object> Parameters { get;  set; }

        /// <summary>
        /// Gets the resource manager that is responsible for rendering script and stylesheet resources.
        /// </summary>
        public ResourceManager ResourceManager { get;  set; }

        /// <summary>
        /// Gets the view model object for the current HTTP request.
        /// </summary>
        public object ViewModel { get; set; }

        /// <summary>
        /// Gets the top-level control representing the whole view for the current request.
        /// </summary>
        public DotvvmView View { get; set; }

        /// <summary>
        /// Gets the <see cref="ModelState"/> object that manages validation errors for the viewmodel.
        /// </summary>
        public ModelState ModelState { get; private set; }

        public Dictionary<string, string> PostBackUpdatedControls { get; private set; }

        public JObject ViewModelJson { get; set; }

        public JObject ReceivedViewModelJson { get; set; }

        /// <summary>
        /// Gets the query string parameters specified in the URL of the current HTTP request.
        /// </summary>
        public IDictionary<string, object> Query { get;  set; }

        /// <summary>
        /// Gets or sets the value indiciating whether the exception that occured in the command execution was handled. 
        /// This property is typically set from the exception filter's OnCommandException method.
        /// </summary>
        public bool IsCommandExceptionHandled { get; set; }

		internal void RedirectToUrl(object p)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets or sets the value indiciating whether the exception that occured during the page execution was handled and that the OnPageExceptionHandled will not be called on the next action filters. 
		/// This property is typically set from the action filter's OnPageExceptionHandled method.
		/// </summary>
		public bool IsPageExceptionHandled { get; set; }


        /// <summary>
        /// Gets or sets the exception that occured when the command was executed.
        /// </summary>
        public Exception CommandException { get; set; }

        /// <summary>
        /// Gets or sets new url fragment (tha part after #) to be set on client
        /// </summary>
        public string ResultIdFragment { get; set; }


        /// <summary>
        /// Gets a value indicating whether the HTTP request wants to render only content of a specific SpaContentPlaceHolder.
        /// </summary>
        public bool IsSpaRequest
        {
            get { return DotvvmPresenter.DetermineSpaRequest(HttpContext); }
        }

        /// <summary>
        /// Gets a value indicating whether this HTTP request is made from single page application and only the SpaContentPlaceHolder content will be rendered.
        /// </summary>
        public bool IsInPartialRenderingMode
        {
            get { return DotvvmPresenter.DeterminePartialRendering(HttpContext); }
        }

        public IViewModelSerializer ViewModelSerializer { get; set; }



        /// <summary>
        /// Gets the unique id of the SpaContentPlaceHolder that should be loaded.
        /// </summary>
        public string GetSpaContentPlaceHolderUniqueId()
        {
            return DotvvmPresenter.DetermineSpaContentPlaceHolderUniqueId(HttpContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRequestContext"/> class.
        /// </summary>
        public DotvvmRequestContext()
        {
            ModelState = new ModelState();
            PostBackUpdatedControls = new Dictionary<string, string>();
        }
        private CultureInfo currentCulture;
        /// <summary>
        /// Changes the current culture of this HTTP request.
        /// </summary>
        public void ChangeCurrentCulture(string cultureName)
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = currentCulture = new CultureInfo(cultureName);
        }
        /// <summary>
        /// WORKAROUND: .NET for some reason resets CurrentCulture when exited viewModel method, this is changing it back
        /// </summary>
        public void ResetCulture()
        {
            if (currentCulture != null) CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = currentCulture;
        }

        /// <summary>
        /// Gets the current UI culture of this HTTP request.
        /// </summary>
        public CultureInfo GetCurrentUICulture()
        {
            return CultureInfo.CurrentUICulture;
        }

        /// <summary>
        /// Gets the current culture of this HTTP request.
        /// </summary>
        public CultureInfo GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture;
        }
        /// <summary>
        /// Interrupts the execution of the current request.
        /// </summary>
        [DebuggerHidden]
        public void InterruptRequest()
        {
            throw new DotvvmInterruptRequestExecutionException();
        }

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToUrl(string url, bool forceRefresh = false)
        {
            SetRedirectResponse(HttpContext, TranslateVirtualPath(url), (int)HttpStatusCode.Redirect, forceRefresh);
            InterruptRequest();
        }

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToRoute(string routeName, object newRouteValues = null, bool forceRefresh = false)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrl(url, forceRefresh);
        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToUrlPermanent(string url, bool forceRefresh = false)
        {
            SetRedirectResponse(HttpContext, TranslateVirtualPath(url), (int)HttpStatusCode.MovedPermanently, forceRefresh);
            InterruptRequest();
        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToRoutePermanent(string routeName, object newRouteValues = null, bool forceRefresh = false)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrlPermanent(url, forceRefresh);
        }

        /// <summary>
        /// Renders the redirect response.
        /// </summary>
        /// <param name="forceRefresh"></param>
        public static void SetRedirectResponse(IHttpContext httpContext, string url, int statusCode, bool forceRefresh = false)
        {
            if (!DotvvmPresenter.DeterminePartialRendering(httpContext))
            {
                httpContext.Response.Headers["Location"] = url;
                httpContext.Response.StatusCode = statusCode;
            }
            else
            {
                if (DotvvmPresenter.DetermineIsPostBack(httpContext) && DotvvmPresenter.DetermineSpaRequest(httpContext) && !forceRefresh && !url.Contains("//"))
                {
                    // if we are in SPA postback, redirect should point at #! URL
                    url = "#!" + url;
                }

                httpContext.Response.StatusCode = 200;
                httpContext.Response.ContentType = "application/json";
                httpContext.Response.Write(DefaultViewModelSerializer.GenerateRedirectActionResponse(url, forceRefresh));
            }
        }

        /// <summary>
        /// Gets the current DotVVM context.
        /// </summary>
        public static DotvvmRequestContext GetCurrent(IHttpContext httpContext)
        {
            return httpContext.GetItem<DotvvmRequestContext>(HostingConstants.DotvvmRequestContextOwinKey);
        }

        /// <summary>
        /// Ends the request execution when the <see cref="ModelState"/> is not valid and displays the validation errors in <see cref="ValidationSummary"/> control.
        /// If it is, it does nothing.
        /// </summary>
        public void FailOnInvalidModelState()
        {
            if (!ModelState.IsValid)
            {
                HttpContext.Response.ContentType = "application/json";
                HttpContext.Response.Write(ViewModelSerializer.SerializeModelState(this));
                throw new DotvvmInterruptRequestExecutionException("The ViewModel contains validation errors!");
            }
        }

        /// <summary>
        /// Gets the serialized view model.
        /// </summary>
        public string GetSerializedViewModel()
        {
            return ViewModelSerializer.SerializeViewModel(this);
        }

        /// <summary>
        /// Translates the virtual path (~/something) to the domain relative path (/virtualDirectory/something). 
        /// For example, when the app is configured to run in a virtual directory '/virtDir', the URL '~/myPage.dothtml' will be translated to '/virtDir/myPage.dothtml'.
        /// </summary>
        public string TranslateVirtualPath(string virtualUrl)
        {
            return TranslateVirtualPath(virtualUrl, HttpContext);
        }

        /// <summary>
        /// Translates the virtual path (~/something) to the domain relative path (/virtualDirectory/something). 
        /// For example, when the app is configured to run in a virtual directory '/virtDir', the URL '~/myPage.dothtml' will be translated to '/virtDir/myPage.dothtml'.
        /// </summary>
        public static string TranslateVirtualPath(string virtualUrl, IHttpContext httpContext)
        {
            if (virtualUrl.StartsWith("~/", StringComparison.Ordinal))
            {
                var url = DotvvmMiddlewareBase.GetVirtualDirectory(httpContext) + "/" + virtualUrl.Substring(2);
                if (!url.StartsWith("/", StringComparison.Ordinal))
                {
                    url = "/" + url;
                }
                return url;
            }
            else
            {
                return virtualUrl;
            }
        }

        /// <summary>
        /// Redirects the client to the specified file.
        /// </summary>
        public void ReturnFile(byte[] bytes, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders = null)
        {
            var returnedFileStorage = Configuration.ServiceLocator.GetService<IReturnedFileStorage>();
            var metadata = new ReturnedFileMetadata()
            {
                FileName = fileName,
                MimeType = mimeType,
                AdditionalHeaders = additionalHeaders?.GroupBy(k => k.Key, k => k.Value)?.ToDictionary(k => k.Key, k => k.ToArray())
            };

            var generatedFileId = returnedFileStorage.StoreFile(bytes, metadata).Result;
            RedirectToUrl("~/dotvvmReturnedFile?id=" + generatedFileId);
        }

        /// <summary>
        /// Redirects the client to the specified file.
        /// </summary>
        public void ReturnFile(Stream stream, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders = null)
        {
            var returnedFileStorage = Configuration.ServiceLocator.GetService<IReturnedFileStorage>();
            var metadata = new ReturnedFileMetadata()
            {
                FileName = fileName,
                MimeType = mimeType,
                AdditionalHeaders = additionalHeaders?.GroupBy(k => k.Key, k => k.Value)?.ToDictionary(k => k.Key, k => k.ToArray())
            };

            var generatedFileId = returnedFileStorage.StoreFile(stream, metadata).Result;
            RedirectToUrl("~/dotvvmReturnedFile?id=" + generatedFileId);
        }

    }
}
