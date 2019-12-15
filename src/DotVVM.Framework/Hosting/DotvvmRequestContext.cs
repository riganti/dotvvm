using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Routing;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmRequestContext : IDotvvmRequestContext
    {
        public string CsrfToken { get; set; }
        public JObject ReceivedViewModelJson { get; set; }

        public JObject ViewModelJson { get; set; }

        /// <summary>
        /// Gets the route that was used for this request.
        /// </summary>
        public RouteBase Route { get; set; }

        /// <summary>
        /// Determines whether this HTTP request is a postback or a classic GET request.
        /// </summary>
        public bool IsPostBack { get; set; }

        /// <summary>
        /// Gets the view model object for the current HTTP request.
        /// </summary>
        public object ViewModel { get; set; }

        /// <summary>
        /// Gets the top-level control representing the whole view for the current request.
        /// </summary>
        public DotvvmView View { get; set; }

        /// <summary>
        /// Gets the values of parameters specified in the <see cref="P:Route" /> property.
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }

        /// <summary>
        /// Gets the <see cref="IDotvvmPresenter"/> that is responsible for handling this HTTP request.
        /// </summary>
        public IDotvvmPresenter Presenter { get; set; }

        /// <summary>
        /// Gets the global configuration of DotVVM.
        /// </summary>
        public DotvvmConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets the resource manager that is responsible for rendering script and stylesheet resources.
        /// </summary>
        private ResourceManager _resourceManager;
        public ResourceManager ResourceManager => _resourceManager ?? (_resourceManager = Services.GetRequiredService<ResourceManager>());

        /// <summary>
        /// Gets the <see cref="ModelState"/> object that manages validation errors for the viewmodel.
        /// </summary>
        public ModelState ModelState { get; } = new ModelState();

        /// <summary>
        /// Gets the query string parameters specified in the URL of the current HTTP request.
        /// </summary>
        public IQueryCollection Query => HttpContext.Request.Query;

        /// <summary>
        /// Gets or sets the value indicating whether the exception that occurred in the command execution was handled. 
        /// This property is typically set from the exception filter's OnCommandException method.
        /// </summary>
        public bool IsCommandExceptionHandled { get; set; }


        /// <summary>
        /// Gets or sets the value indicating whether the exception that occurred during the page execution was handled and that the OnPageExceptionHandled will not be called on the next action filters. 
        /// This property is typically set from the action filter's OnPageExceptionHandled method.
        /// </summary>
        public bool IsPageExceptionHandled { get; set; }


        /// <summary>
        /// Gets or sets the exception that occurred when the command was executed.
        /// </summary>
        public Exception CommandException { get; set; }

        /// <summary>
        /// Gets or sets new url fragment (the part after #) to be set on client
        /// </summary>
        public string ResultIdFragment { get; set; }


        /// <summary>
        /// Gets a value indicating whether the HTTP request wants to render only content of a specific SpaContentPlaceHolder.
        /// </summary>
        public bool IsSpaRequest => DotvvmPresenter.DetermineSpaRequest(HttpContext);

        /// <summary>
        /// Gets a value indicating whether this HTTP request is made from single page application and only the SpaContentPlaceHolder content will be rendered.
        /// </summary>
        public bool IsInPartialRenderingMode => DotvvmPresenter.DeterminePartialRendering(HttpContext);

        [Obsolete("Get the IViewModelSerializer from IServiceProvider")]
        public IViewModelSerializer ViewModelSerializer => Services.GetRequiredService<IViewModelSerializer>();

        private IServiceProvider _services;
        public IServiceProvider Services
        {
            get => _services ?? (_services = Configuration.ServiceProvider ?? throw new NotSupportedException());
            set => _services = value;
        }

        public IHttpContext HttpContext { get; set; }

        /// <summary>
        /// Gets the current DotVVM context.
        /// </summary>
        public static DotvvmRequestContext GetCurrent(IHttpContext httpContext)
        {
            return httpContext.GetItem<DotvvmRequestContext>(HostingConstants.DotvvmRequestContextOwinKey);
        }
    }
}
