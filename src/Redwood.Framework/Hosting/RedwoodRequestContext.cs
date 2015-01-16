using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.Owin;
using Redwood.Framework.Configuration;
using Redwood.Framework.Routing;
using Redwood.Framework.ResourceManagement;

namespace Redwood.Framework.Hosting
{
    public class RedwoodRequestContext
    {


        public IOwinContext OwinContext { get; internal set; }
        
        public IRedwoodPresenter Presenter { get; internal set; }

        public RedwoodConfiguration Configuration { get; internal set; }

        public RouteBase Route { get; internal set; }

        public bool IsPostBack { get; internal set; }

        public IDictionary<string, object> Parameters { get; set; }

        public ResourceManager ResourceManager { get; set; }

        public object ViewModel { get; internal set; }

        internal string CsrfToken { get; set; }

        public string CommandValidationPath { get; set; }


        public IReadableStringCollection Query
        {
            get
            {
                return OwinContext.Request.Query;
            }
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
        public void InterruptExecution()
        {
            throw new RedwoodInterruptRequestExecutionException();    
        }

        /// <summary>
        /// Returns the redirect response and interrupts the execution of current request.
        /// </summary>
        public void Redirect(string url)
        {
            SetRedirectResponse(url, (int)HttpStatusCode.Redirect);

        }

        /// <summary>
        /// Returns the permanent redirect response and interrupts the execution of current request.
        /// </summary>
        public void RedirectPermanent(string url)
        {
            SetRedirectResponse(url, (int)HttpStatusCode.MovedPermanently);
        }

        /// <summary>
        /// Renders the redirect response.
        /// </summary>
        private void SetRedirectResponse(string url, int statusCode)
        {
            if (!IsPostBack)
            {
                OwinContext.Response.Headers["Location"] = url;
                OwinContext.Response.StatusCode = statusCode;
            }
            else
            {
                OwinContext.Response.ContentType = "application/json";
                OwinContext.Response.Write(Presenter.ViewModelSerializer.SerializeRedirectAction(this, url));
            }
            InterruptExecution();
        }

    }
}
