using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Redwood.Framework.Controls;
using Redwood.Framework.Parser;
using Redwood.Framework.ViewModel;
using Redwood.Framework.Runtime;
using Redwood.Framework.Runtime.Filters;
using Redwood.Framework.Security;
using Microsoft.Owin;

namespace Redwood.Framework.Hosting
{
    public class RedwoodPresenter : IRedwoodPresenter
    {
        public IRedwoodViewBuilder RedwoodViewBuilder { get; private set; }

        public IViewModelLoader ViewModelLoader { get; private set; }

        public IViewModelSerializer ViewModelSerializer { get; private set; }

        public IOutputRenderer OutputRenderer { get; private set; }

        public ICsrfProtector CsrfProtector { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedwoodPresenter"/> class.
        /// </summary>
        public RedwoodPresenter(
            IRedwoodViewBuilder redwoodViewBuilder,
            IViewModelLoader viewModelLoader,
            IViewModelSerializer viewModelSerializer,
            IOutputRenderer outputRenderer,
            ICsrfProtector csrfProtector
        )
        {
            RedwoodViewBuilder = redwoodViewBuilder;
            ViewModelLoader = viewModelLoader;
            ViewModelSerializer = viewModelSerializer;
            OutputRenderer = outputRenderer;
            CsrfProtector = csrfProtector;
        }

        /// <summary>
        /// Processes the request.
        /// </summary>
        public async Task ProcessRequest(RedwoodRequestContext context)
        {
            Exception exceptionToRender = null;
            try
            {
                await ProcessRequestCore(context);
            }
            catch (RedwoodInterruptRequestExecutionException)
            {
                // the response has already been generated, do nothing
            }
            catch (UnauthorizedAccessException ex)
            {
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                exceptionToRender = ex;
            }

            if (exceptionToRender != null)
            {
                await RedwoodErrorPageMiddleware.RenderErrorResponse(context.OwinContext, exceptionToRender);                
            }
        }

        public async Task ProcessRequestCore(RedwoodRequestContext context)
        {
            if (context.OwinContext.Request.Method != "GET" && context.OwinContext.Request.Method != "POST")
            {
                // unknown HTTP method
                context.OwinContext.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                throw new RedwoodHttpException("Only GET and POST methods are supported!");
            }
            var isPostBack = context.OwinContext.Request.Method == "POST";
            context.IsPostBack = isPostBack;
            context.ChangeCurrentCulture(context.Configuration.DefaultCulture);

            // build the page view
            var page = RedwoodViewBuilder.BuildView(context);

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
            if (context.ViewModel is IRedwoodViewModel)
            {
                ((IRedwoodViewModel)context.ViewModel).Context = context;
                await ((IRedwoodViewModel)context.ViewModel).Init();
            }

            if (!isPostBack)
            {
                // perform standard get
                if (context.ViewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)context.ViewModel).Load();
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
                if (context.ViewModel is IRedwoodViewModel)
                {
                    await ((IRedwoodViewModel)context.ViewModel).Load();
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
                        if (ex is RedwoodInterruptRequestExecutionException)
                        {
                            throw new RedwoodInterruptRequestExecutionException("The request execution was interrupted in the command!", ex);
                        }
                        commandException = ex;
                    }

                    // run OnCommandExecuted on action filters
                    foreach (var filter in globalFilters.Concat(viewModelFilters).Concat(methodFilters))
                    {
                        filter.OnCommandExecuted(context, actionInfo, commandException);
                    }

                    if (commandException != null)
                    {
                        throw new Exception("Unhandled exception occured in the command!", commandException);
                    }
                }
            }

            if (context.ViewModel is IRedwoodViewModel)
            {
                await ((IRedwoodViewModel)context.ViewModel).PreRender();
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
            var serializedViewModel = ViewModelSerializer.SerializeViewModel(context, page);
            if (!isPostBack)
            {
                // standard get
                await OutputRenderer.RenderPage(context, page, serializedViewModel);
            }
            else
            {
                // postback
                await OutputRenderer.RenderViewModel(context, page, serializedViewModel);
            }

        }

        /// <summary>
        /// Invokes the specified method on all controls in the page control tree.
        /// </summary>
        private void InvokePageLifeCycleEventRecursive(RedwoodControl control, Action<RedwoodControl> action)
        {
            foreach (var child in control.GetThisAndAllDescendants())
            {
                action(child);
            }
        }
    }
}
