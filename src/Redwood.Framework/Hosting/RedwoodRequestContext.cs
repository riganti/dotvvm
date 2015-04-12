using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Owin;
using Newtonsoft.Json.Linq;
using Redwood.Framework.Configuration;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.Routing;
using Redwood.Framework.ResourceManagement;
using Redwood.Framework.Runtime;

namespace Redwood.Framework.Hosting
{
    public class RedwoodRequestContext
    {
        internal string CsrfToken { get; set; }



        public IOwinContext OwinContext { get; internal set; }
        
        public IRedwoodPresenter Presenter { get; internal set; }

        public RedwoodConfiguration Configuration { get; internal set; }

        public RouteBase Route { get; internal set; }

        public bool IsPostBack { get; internal set; }

        public IDictionary<string, object> Parameters { get; set; }

        public ResourceManager ResourceManager { get; internal set; }

        public object ViewModel { get; internal set; }
        
        public ModelState ModelState { get; private set; }

        internal Dictionary<string, string> PostBackUpdatedControls { get; private set; }

        internal string RenderedHtml { get; set; }

        public JObject ViewModelJson { get; set; }

        public JObject ReceivedViewModelJson { get; set; }


        public IReadableStringCollection Query
        {
            get
            {
                return OwinContext.Request.Query;
            }
        }

        /// <summary>
        /// Gets or sets the value indiciating whether the exception that occured in the command execution was handled. 
        /// This property is typically set from the exception filter.
        /// </summary>
        public bool IsCommandExceptionHandled { get; set; }

        /// <summary>
        /// Gets a value indicating whether the HTTP request wants to render only content of a specific SpaContentPlaceHolder.
        /// </summary>
        public bool IsSpaRequest
        {
            get { return RedwoodPresenter.DetermineSpaRequest(OwinContext); }
        }

        public bool IsInPartialRenderingMode
        {
            get { return RedwoodPresenter.DeterminePartialRendering(OwinContext); }
        }

        /// <summary>
        /// Gets the unique id of the SpaContentPlaceHolder that should be loaded.
        /// </summary>
        public string GetSpaContentPlaceHolderUniqueId()
        {
            return RedwoodPresenter.DetermineSpaContentPlaceHolderUniqueId(OwinContext);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodRequestContext"/> class.
        /// </summary>
        public RedwoodRequestContext()
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
        /// Interrupts the execution of the current request.
        /// </summary>
        public void InterruptRequest()
        {
            throw new RedwoodInterruptRequestExecutionException();    
        }

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        public void Redirect(string url)
        {
            SetRedirectResponse(OwinContext, url, (int)HttpStatusCode.Redirect);
            InterruptRequest();

        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectPermanent(string url)
        {
            SetRedirectResponse(OwinContext, url, (int)HttpStatusCode.MovedPermanently);
            InterruptRequest();
        }

        /// <summary>
        /// Renders the redirect response.
        /// </summary>
        public static void SetRedirectResponse(IOwinContext OwinContext, string url, int statusCode)
        {
            if (!RedwoodPresenter.DeterminePartialRendering(OwinContext))
            {
                OwinContext.Response.Headers["Location"] = url;
                OwinContext.Response.StatusCode = statusCode;
            }
            else
            {
                OwinContext.Response.StatusCode = 200;
                OwinContext.Response.ContentType = "application/json";
                OwinContext.Response.Write(DefaultViewModelSerializer.GenerateRedirectActionResponse(url));
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
                OwinContext.Response.Write(Presenter.ViewModelSerializer.SerializeModelState(this));
                throw new RedwoodInterruptRequestExecutionException("The ViewModel contains validation errors!");
            }
        }

        /// <summary>
        /// Gets the serialized view model.
        /// </summary>
        public string GetSerializedViewModel()
        {
            return Presenter.ViewModelSerializer.SerializeViewModel(this);
        }
    }
}
