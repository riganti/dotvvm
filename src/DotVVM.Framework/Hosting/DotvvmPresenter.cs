using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Owin;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;
using DotVVM.Framework.Parser;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.Runtime.Filters;
using DotVVM.Framework.Security;

namespace DotVVM.Framework.Hosting
{
    public class DotvvmPresenter : IDotvvmPresenter
    {
        public IDotvvmViewBuilder DotvvmViewBuilder { get; private set; }

        public IViewModelLoader ViewModelLoader { get; private set; }

        public IViewModelSerializer ViewModelSerializer { get; private set; }

        public IOutputRenderer OutputRenderer { get; private set; }

        public ICsrfProtector CsrfProtector { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter"/> class.
        /// </summary>
        public DotvvmPresenter(DotvvmConfiguration configuration)
        {
            DotvvmViewBuilder = configuration.ServiceLocator.GetService<IDotvvmViewBuilder>();
            ViewModelLoader = configuration.ServiceLocator.GetService<IViewModelLoader>();
            ViewModelSerializer = configuration.ServiceLocator.GetService<IViewModelSerializer>();
            OutputRenderer = configuration.ServiceLocator.GetService<IOutputRenderer>();
            CsrfProtector = configuration.ServiceLocator.GetService<ICsrfProtector>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DotvvmPresenter"/> class.
        /// </summary>
        public DotvvmPresenter(
            IDotvvmViewBuilder dotvvmViewBuilder,
            IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRenderer,
            ICsrfProtector csrfProtector
        )
        {
            DotvvmViewBuilder = dotvvmViewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRenderer;
            CsrfProtector = csrfProtector;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public async Task ProcessRequest(DotvvmRequestContext context)
        {
            try
            {
                await ProcessRequestCore(context);
            }
            catch (DotvvmInterruptRequestExecutionException)
            {
                // the response has already been generated, do nothing
                return;
            }
            catch (UnauthorizedAccessException)
            {
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }

        public async Task ProcessRequestCore(DotvvmRequestContext context)
        {
            if (context.OwinContext.Request.Method != "GET" && context.OwinContext.Request.Method != "POST")
            {
                // unknown HTTP method
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                throw new DotvvmHttpException("Only GET and POST methods are supported!");
            }
            var isPostBack = DetermineIsPostBack(context.OwinContext);
            context.IsPostBack = isPostBack;
            context.ChangeCurrentCulture(context.Configuration.DefaultCulture);

            // build the page view
            var page = DotvvmViewBuilder.BuildView(context);
            
            // run the preinit phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreInit(context));

            // run the init phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnInit(context));

            // locate and create the view model
            context.ViewModel = ViewModelLoader.InitializeViewModel(context, page);
            page.DataContext = context.ViewModel;

            // get action filters
            var globalFilters = context.Configuration.Runtime.GlobalFilters.ToList();
            var viewModelFilters = context.ViewModel.GetType().GetCustomAttributes<ActionFilterAttribute>(true).ToList();

            // run OnViewModelCreated on action filters
            foreach (var filter in globalFilters.Concat(viewModelFilters))
            {
                filter.OnViewModelCreated(context);
            }

            // init the view model lifecycle
            if (context.ViewModel is IDotvvmViewModel)
            {
                ((IDotvvmViewModel)context.ViewModel).Context = context;
                await ((IDotvvmViewModel)context.ViewModel).Init();
            }

            if (!isPostBack)
            {
                // perform standard get
                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).Load();
                }

                // run the load phase in the page
                InvokePageLifeCycleEventRecursive(page, c => c.OnLoad(context));
            }
            else
            {
                // perform the postback
                string postData;
                using (var sr = new StreamReader(context.OwinContext.Request.Body))
                {
                    postData = await sr.ReadToEndAsync();
                }
                ViewModelSerializer.PopulateViewModel(context, page, postData);
                if (context.ViewModel is IDotvvmViewModel)
                {
                    await ((IDotvvmViewModel)context.ViewModel).Load();
                }

                // validate CSRF token 
                CsrfProtector.VerifyToken(context, context.CsrfToken);

                // run the load phase in the page
                InvokePageLifeCycleEventRecursive(page, c => c.OnLoad(context));

                // invoke the postback command
                ActionInfo actionInfo;
                ViewModelSerializer.ResolveCommand(context, page, postData, out actionInfo);

                if (actionInfo != null)
                {
                    // get filters
                    var methodFilters = actionInfo.MethodInfo.GetCustomAttributes<ActionFilterAttribute>(true).ToList();

                    // run OnCommandExecuting on action filters
                    foreach (var filter in globalFilters.Concat(viewModelFilters).Concat(methodFilters))
                    {
                        filter.OnCommandExecuting(context, actionInfo);
                    }

                    Exception commandException = null;
                    try
                    {
                        var invokedAction = actionInfo.GetAction();
                        invokedAction();
                    }
                    catch (Exception ex)
                    {
                        if (ex is TargetInvocationException)
                        {
                            ex = ex.InnerException;
                        }
                        if (ex is DotvvmInterruptRequestExecutionException)
                        {
                            throw new DotvvmInterruptRequestExecutionException("The request execution was interrupted in the command!", ex);
                        }
                        commandException = ex;
                    }

                    // run OnCommandExecuted on action filters
                    foreach (var filter in globalFilters.Concat(viewModelFilters).Concat(methodFilters))
                    {
                        filter.OnCommandExecuted(context, actionInfo, commandException);
                    }

                    if (commandException != null && !context.IsCommandExceptionHandled)
                    {
                        throw new Exception("Unhandled exception occured in the command!", commandException);
                    }
                }
            }

            if (context.ViewModel is IDotvvmViewModel)
            {
                await ((IDotvvmViewModel)context.ViewModel).PreRender();
            }

            // run the prerender phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreRender(context));

            // run the prerender complete phase in the page
            InvokePageLifeCycleEventRecursive(page, c => c.OnPreRenderComplete(context));

            // generate CSRF token if required
            if (string.IsNullOrEmpty(context.CsrfToken))
            {
                context.CsrfToken = CsrfProtector.GenerateToken(context);
            }

            // run OnResponseRendering on action filters
            foreach (var filter in globalFilters.Concat(viewModelFilters))
            {
                filter.OnResponseRendering(context);
            }

            // render the output
            ViewModelSerializer.BuildViewModel(context, page);
            OutputRenderer.RenderPage(context, page);
            if (!context.IsInPartialRenderingMode)
            {
                // standard get
                await OutputRenderer.WriteHtmlResponse(context);
            }
            else
            {
                // postback or SPA content
                ViewModelSerializer.AddPostBackUpdatedControls(context);
                await OutputRenderer.WriteViewModelResponse(context, page);
            }

            if (context.ViewModel != null)
            {
                ViewModelLoader.DisposeViewModel(context.ViewModel);
            }
        }

        public static bool DetermineIsPostBack(IOwinContext context)
        {
            return context.Request.Method == "POST";
        }

        public static bool DetermineSpaRequest(IOwinContext context)
        {
            return !string.IsNullOrEmpty(context.Request.Headers[Constants.SpaContentPlaceHolderHeaderName]);
        }

        public static bool DeterminePartialRendering(IOwinContext context)
        {
            return DetermineIsPostBack(context) || DetermineSpaRequest(context);
        }

        public static string DetermineSpaContentPlaceHolderUniqueId(IOwinContext context)
        {
            return context.Request.Headers[Constants.SpaContentPlaceHolderHeaderName];
        }


        /// <summary>
        /// Invokes the specified method on all controls in the page control tree.
        /// </summary>
        private void InvokePageLifeCycleEventRecursive(DotvvmControl control, Action<DotvvmControl> action)
        {
            foreach (var child in control.GetThisAndAllDescendants())
            {
                action(child);
            }
        }
    }
}
