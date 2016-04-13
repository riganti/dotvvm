using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Storage;
using System.Diagnostics;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmRequestContext : IDotvvmRequestContext
    {
        internal string CsrfToken { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="IOwinContext"/> object for this HTTP request.
        /// </summary>
        public IOwinContext OwinContext { get; internal set; }

        /// <summary>
        /// Gets the <see cref="IDotvvmPresenter"/> that is responsible for handling this HTTP request.
        /// </summary>
        public IDotvvmPresenter Presenter { get; internal set; }

        /// <summary>
        /// Gets the global configuration of DotVVM.
        /// </summary>
        public DotvvmConfiguration Configuration { get; internal set; }

        /// <summary>
        /// Gets the route that was used for this request.
        /// </summary>
        public RouteBase Route { get; internal set; }

        /// <summary>
        /// Determines whether this HTTP request is a postback or a classic GET request.
        /// </summary>
        public bool IsPostBack { get; internal set; }

        /// <summary>
        /// Gets the values of parameters specified in the <see cref="P:Route" /> property.
        /// </summary>
        public IDictionary<string, object> Parameters { get; internal set; }

        /// <summary>
        /// Gets the resource manager that is responsible for rendering script and stylesheet resources.
        /// </summary>
        public ResourceManager ResourceManager { get; internal set; }

        /// <summary>
        /// Gets the view model object for the current HTTP request.
        /// </summary>
        public object ViewModel { get; internal set; }

        /// <summary>
        /// Gets the top-level control representing the whole view for the current request.
        /// </summary>
        public DotvvmView View { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ModelState"/> object that manages validation errors for the viewmodel.
        /// </summary>
        public ModelState ModelState { get; private set; }

        internal Dictionary<string, string> PostBackUpdatedControls { get; private set; }

        internal JObject ViewModelJson { get; set; }

        internal JObject ReceivedViewModelJson { get; set; }

        /// <summary>
        /// Gets the query string parameters specified in the URL of the current HTTP request.
        /// </summary>
        public IDictionary<string, object> Query { get; internal set; }

        /// <summary>
        /// Gets or sets the value indiciating whether the exception that occured in the command execution was handled. 
        /// This property is typically set from the exception filter.
        /// </summary>
        public bool IsCommandExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the exception that occured when the command was executed.
        /// </summary>
        public Exception CommandException { get; internal set; }

        /// <summary>
        /// Gets or sets new url fragment (tha part after #) to be set on client
        /// </summary>
        public string ResultIdFragment { get; set; }

        /// <summary>
        /// Gets a value indicating whether the HTTP request wants to render only content of a specific SpaContentPlaceHolder.
        /// </summary>
        public bool IsSpaRequest
        {
            get { return DotvvmPresenter.DetermineSpaRequest(OwinContext); }
        }

        /// <summary>
        /// Gets a value indicating whether this HTTP request is made from single page application and only the SpaContentPlaceHolder content will be rendered.
        /// </summary>
        public bool IsInPartialRenderingMode
        {
            get { return DotvvmPresenter.DeterminePartialRendering(OwinContext); }
        }

        public IViewModelSerializer ViewModelSerializer { get; set; }


        /// <summary>
        /// Gets the unique id of the SpaContentPlaceHolder that should be loaded.
        /// </summary>
        public string GetSpaContentPlaceHolderUniqueId()
        {
            return DotvvmPresenter.DetermineSpaContentPlaceHolderUniqueId(OwinContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmRequestContext"/> class.
        /// </summary>
        public DotvvmRequestContext()
        {
            ModelState = new ModelState();
            PostBackUpdatedControls = new Dictionary<string, string>();
        }

        /// <summary>
        /// Changes the current culture of this HTTP request.
        /// </summary>
        public void ChangeCurrentCulture(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
        }

        /// <summary>
        /// Changes the current UI culture of this HTTP request.
        /// </summary>
        public CultureInfo GetCurrentUICulture()
        {
            return Thread.CurrentThread.CurrentUICulture;
        }

        /// <summary>
        /// Changes the current culture of this HTTP request.
        /// </summary>
        public CultureInfo GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture;
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
        public void RedirectToUrl(string url)
        {
            SetRedirectResponse(OwinContext, TranslateVirtualPath(url), (int)HttpStatusCode.Redirect);
            InterruptRequest();
        }

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToRoute(string routeName, object newRouteValues = null)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrl(url);
        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToUrlPermanent(string url)
        {
            SetRedirectResponse(OwinContext, TranslateVirtualPath(url), (int)HttpStatusCode.MovedPermanently);
            InterruptRequest();
        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectToRoutePermanent(string routeName, object newRouteValues = null)
        {
            var route = Configuration.RouteTable[routeName];
            var url = route.BuildUrl(Parameters, newRouteValues);
            RedirectToUrlPermanent(url);
        }

        /// <summary>
        /// Renders the redirect response.
        /// </summary>
        public static void SetRedirectResponse(IOwinContext owinContext, string url, int statusCode)
        {
            if (!DotvvmPresenter.DeterminePartialRendering(owinContext))
            {
                owinContext.Response.Headers["Location"] = url;
                owinContext.Response.StatusCode = statusCode;
            }
            else
            {
                owinContext.Response.StatusCode = 200;
                owinContext.Response.ContentType = "application/json";
                owinContext.Response.Write(DefaultViewModelSerializer.GenerateRedirectActionResponse(url));
            }
        }

        /// <summary>
        /// Ends the request execution when the <see cref="ModelState"/> is not valid and displays the validation errors in <see cref="ValidationSummary"/> control.
        /// If it is, it does nothing.
        /// </summary>
        public void FailOnInvalidModelState()
        {
            if (!ModelState.IsValid)
            {
                OwinContext.Response.ContentType = "application/json";
                OwinContext.Response.Write(ViewModelSerializer.SerializeModelState(this));
                throw new DotvvmInterruptRequestExecutionException("The ViewModel contains validation errors!");
            }
        }

        /// <summary>
        /// Gets the serialized view model.
        /// </summary>
        internal string GetSerializedViewModel()
        {
            return ViewModelSerializer.SerializeViewModel(this);
        }

        /// <summary>
        /// Translates the virtual path (~/something) to the domain relative path (/virtualDirectory/something). 
        /// For example, when the app is configured to run in a virtual directory '/virtDir', the URL '~/myPage.dothtml' will be translated to '/virtDir/myPage.dothtml'.
        /// </summary>
        public string TranslateVirtualPath(string virtualUrl)
        {
            return TranslateVirtualPath(virtualUrl, OwinContext);
        }

        /// <summary>
        /// Translates the virtual path (~/something) to the domain relative path (/virtualDirectory/something). 
        /// For example, when the app is configured to run in a virtual directory '/virtDir', the URL '~/myPage.dothtml' will be translated to '/virtDir/myPage.dothtml'.
        /// </summary>
        public static string TranslateVirtualPath(string virtualUrl, IOwinContext owinContext)
        {
            if (virtualUrl.StartsWith("~/", StringComparison.Ordinal))
            {
                var url = DotvvmMiddleware.GetVirtualDirectory(owinContext) + "/" + virtualUrl.Substring(2);
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
        public void ReturnFile(byte[] bytes, string fileName, string mimeType, IHeaderDictionary additionalHeaders = null)
        {
            var returnedFileStorage = Configuration.ServiceLocator.GetService<IReturnedFileStorage>();
            var metadata = new ReturnedFileMetadata()
            {
                FileName = fileName,
                MimeType = mimeType,
                AdditionalHeaders = additionalHeaders
            };

            var generatedFileId = returnedFileStorage.StoreFile(bytes, metadata).Result;
            RedirectToUrl("~/dotvvmReturnedFile?id=" + generatedFileId);
        }

        /// <summary>
        /// Redirects the client to the specified file.
        /// </summary>
        public void ReturnFile(Stream stream, string fileName, string mimeType, IHeaderDictionary additionalHeaders = null)
        {
            var returnedFileStorage = Configuration.ServiceLocator.GetService<IReturnedFileStorage>();
            var metadata = new ReturnedFileMetadata()
            {
                FileName = fileName,
                MimeType = mimeType,
                AdditionalHeaders = additionalHeaders
            };

            var generatedFileId = returnedFileStorage.StoreFile(stream, metadata).Result;
            RedirectToUrl("~/dotvvmReturnedFile?id=" + generatedFileId);
        }

    }
}
