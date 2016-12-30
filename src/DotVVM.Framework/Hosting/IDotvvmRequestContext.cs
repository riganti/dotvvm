using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Routing;
using Newtonsoft.Json.Linq;

namespace DotVVM.Framework.Hosting
{
    public interface IDotvvmRequestContext
    {
        /// <summary>
        /// Gets the underlying object for this HTTP request.
        /// </summary>
        IHttpContext HttpContext { get; }

        /// <summary>
        /// Csrf protection token.
        /// </summary>
        string CsrfToken { get; set; }

        JObject ReceivedViewModelJson { get; set; }


        /// <summary>
        /// Gets the unique id of the SpaContentPlaceHolder that should be loaded.
        /// </summary>
        string GetSpaContentPlaceHolderUniqueId();

        /// <summary>
        /// Gets the view model for the current request.
        /// </summary>
        object ViewModel { get; set; }

        JObject ViewModelJson { get; set; }

        Dictionary<string, string> PostBackUpdatedControls { get; }

        /// <summary>
        /// Gets the top-level control representing the whole view for the current request.
        /// </summary>
        DotvvmView View { get; set; }

        /// <summary>
        /// Gets the global configuration of DotVVM.
        /// </summary>
        DotvvmConfiguration Configuration { get; }

        /// <summary>
        /// Gets the <see cref="IDotvvmPresenter"/> that is responsible for handling this HTTP request.
        /// </summary>
        IDotvvmPresenter Presenter { get; set; }

        /// <summary>
        /// Gets the route that was used for this request.
        /// </summary>
        RouteBase Route { get; set; }

        /// <summary>
        /// Determines whether this HTTP request is a postback or a classic GET request.
        /// </summary>
        bool IsPostBack { get; set; }

        /// <summary>
        /// Gets the values of parameters specified in the <see cref="P:Route" /> property.
        /// </summary>
        IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets the resource manager that is responsible for rendering script and stylesheet resources.
        /// </summary>
        ResourceManager ResourceManager { get; }

        /// <summary>
        /// Gets the <see cref="ModelState"/> object that manages validation errors for the viewmodel.
        /// </summary>
        ModelState ModelState { get; }

        /// <summary>
        /// Gets the query string parameters specified in the URL of the current HTTP request.
        /// </summary>
        IQueryCollection Query { get; }

        /// <summary>
        /// Gets or sets the value indiciating whether the exception that occured in the command execution was handled. 
        /// This property is typically set from the exception filter.
        /// </summary>
        bool IsCommandExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the value indiciating whether the exception that occured in the command execution was handled. 
        /// This property is typically set from the action filter's OnPageExceptionHandled method.
        /// </summary>
        bool IsPageExceptionHandled { get; set; }

        /// <summary>
        /// Gets or sets the exception that occured when the command was executed.
        /// </summary>
        Exception CommandException { get; set; }

        /// <summary>
        /// Gets a value indicating whether the HTTP request wants to render only content of a specific SpaContentPlaceHolder.
        /// </summary>
        bool IsSpaRequest { get; }

        /// <summary>
        /// Gets a value indicating whether this HTTP request is made from single page application and only the SpaContentPlaceHolder content will be rendered.
        /// </summary>
        bool IsInPartialRenderingMode { get; }

        /// <summary>
        /// Gets or sets new url fragment (the part after #) to be set on client. Use this to refer to element Ids on the page
        /// </summary>
        string ResultIdFragment { get; set; }

		/// <summary>
		/// Changes the current culture of this HTTP request.
		/// </summary>
		void ChangeCurrentCulture(string cultureName);

        /// <summary>
        /// Changes the current culture of this HTTP request.
        /// </summary>
        void ChangeCurrentCulture(string cultureName, string uiCultureName);

        /// <summary>
        /// Returns current UI culture of this HTTP request.
        /// </summary>
        CultureInfo GetCurrentUICulture();

        /// <summary>
        /// Returns current culture of this HTTP request.
        /// </summary>
        CultureInfo GetCurrentCulture();

        /// <summary>
        /// Interrupts the execution of the current request.
        /// </summary>
        void InterruptRequest();

        /// <summary>
        /// Gets the serialized view model.
        /// </summary>
        string GetSerializedViewModel();

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        void RedirectToUrl(string url, bool replaceInHistory = false, bool allowSpaRedirect = false);

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        void RedirectToRoute(string routeName, object newRouteValues = null, bool replaceInHistory = false, bool allowSpaRedirect = true, string urlSuffix = null);

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        void RedirectToUrlPermanent(string url, bool replaceInHistory = false, bool allowSpaRedirect = false);

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        void RedirectToRoutePermanent(string routeName, object newRouteValues = null, bool replaceInHistory = false, bool allowSpaRedirect = true);

        /// <summary>
        /// Ends the request execution when the <see cref="DotvvmRequestContext.ModelState"/> is not valid and displays the validation errors in <see cref="ValidationSummary"/> control.
        /// If it is, it does nothing.
        /// </summary>
        void FailOnInvalidModelState();

        /// <summary>
        /// Translates the virtual path (~/something) to the domain relative path (/virtualDirectory/something). 
        /// For example, when the app is configured to run in a virtual directory '/virtDir', the URL '~/myPage.dothtml' will be translated to '/virtDir/myPage.dothtml'.
        /// </summary>
        string TranslateVirtualPath(string virtualUrl);

        /// <summary>
        /// Sends data stream to client.
        /// </summary>
        /// <param name="bytes">Data to be sent.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="additionalHeaders">Additional headers.</param>
        void ReturnFile(byte[] bytes, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders = null);

        /// <summary>
        /// Sends data stream to client.
        /// </summary>
        /// <param name="stream">Data to be sent.</param>
        /// <param name="fileName">Name of file.</param>
        /// <param name="mimeType">MIME type.</param>
        /// <param name="additionalHeaders">Additional headers.</param>
        void ReturnFile(Stream stream, string fileName, string mimeType, IEnumerable<KeyValuePair<string, string>> additionalHeaders = null);
    }
}